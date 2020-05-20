using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Groups.Library
{
    public class GroupDiscussionViewModel : ViewModelBase
    {
        public static readonly Regex PostMentionRegex = new Regex("\\[post\\d+.*?\\|.*?\\]");
        private readonly int LOAD_COUNT = 15;
        private readonly int RELOAD_COUNT = 30;
        private long _gid;
        private long _tid;
        private string _topicName;
        private int _knownCommentsCount;
        private bool _isLoaded;
        private bool _isLoading;
        private bool _canComment;
        private MyVirtualizingPanel2 _virtPanel;
        private Action<CommentItem> _replyCallback;
        private bool _loadFromEnd;
        private int _alreadyLoadedCommentsCount;
        private bool _isAddingComment;

        public bool CanComment
        {
            get
            {
                return this._canComment;
            }
        }

        public long GroupId
        {
            get
            {
                return this._gid;
            }
        }

        public int AlreadyLoadedCommentsCount
        {
            get
            {
                return this._alreadyLoadedCommentsCount;
            }
        }

        public bool LoadFromEnd
        {
            get
            {
                return this._loadFromEnd;
            }
        }

        public string Title
        {
            get
            {
                if (this._topicName == null)
                    return "";
                return this._topicName.ToUpperInvariant();
            }
        }

        public Visibility NewCommentVisibility
        {
            get
            {
                if (!this._canComment)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public GroupDiscussionViewModel(long gid, long tid, string topicName, int knownCommentsCount, bool canComment, MyVirtualizingPanel2 virtPanel, bool loadFromEnd, Action<CommentItem> replyCallback)
        {
            this._gid = gid;
            this._tid = tid;
            this._topicName = topicName;
            this._canComment = canComment;
            this._knownCommentsCount = knownCommentsCount;
            this._virtPanel = virtPanel;
            this._loadFromEnd = loadFromEnd;
            this._replyCallback = replyCallback;
        }

        public void LoadData(bool refresh, Action<bool> callback = null)
        {
            if (this._isLoading)
                return;
            if (this._isLoaded && !refresh && this._knownCommentsCount == this.AlreadyLoadedCommentsCount)
            {
                if (callback == null)
                    return;
                callback(true);
            }
            else
            {
                this._isLoading = true;
                if (!this._isLoaded | refresh || this._loadFromEnd)
                    this.SetInProgress(true, "");
                bool loadFromEndSaved = this._loadFromEnd;
                int offset;
                int count;
                this.CalculateOffsetAndCount(refresh, out offset, out count);
                GroupsService.Current.GetTopicComments(this._gid, this._tid, offset, count, (Action<BackendResult<CommentsInfo, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.ProcessReceivedData(loadFromEndSaved, refresh, res.ResultData);
                    else
                        this._isLoading = false;
                    if (callback == null)
                        return;
                    callback(res.ResultCode == ResultCode.Succeeded);
                }));
            }
        }

        public void EnsureLoadFromEnd()
        {
            if (this._loadFromEnd)
                return;
            this._loadFromEnd = true;
            this.LoadData(true, (Action<bool>)null);
        }

        private void ProcessReceivedData(bool loadFromEndSaved, bool refresh, CommentsInfo commentsInfo)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!this._isLoaded && !refresh && this._knownCommentsCount < commentsInfo.TotalCommentsCount || (loadFromEndSaved != this._loadFromEnd || refresh && this._knownCommentsCount < commentsInfo.TotalCommentsCount))
                {
                    this._knownCommentsCount = commentsInfo.TotalCommentsCount;
                    this._isLoading = false;
                    this.LoadData(true, (Action<bool>)null);
                }
                else
                {
                    List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
                    foreach (Comment comment in commentsInfo.comments)
                    {
                        Comment c = comment;
                        CommentItem commentItem = this.CreateCommentItem(commentsInfo.profiles.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == c.from_id)), c, commentsInfo.groups.FirstOrDefault<VKClient.Common.Backend.DataObjects.Group>((Func<VKClient.Common.Backend.DataObjects.Group, bool>)(g => g.id == -c.from_id)));
                        itemsToInsert.Add((IVirtualizable)commentItem);
                    }
                    if (refresh)
                    {
                        this._alreadyLoadedCommentsCount = 0;
                        this._virtPanel.ClearItems();
                    }
                    this.EnsurePollIsAdded(commentsInfo);
                    bool flag = (refresh || !this._isLoaded) && this._loadFromEnd;
                    if (this._loadFromEnd)
                    {
                        if (flag)
                            this._virtPanel.OnlyPartialLoad = true;
                        this._virtPanel.InsertRemoveItems(this.GetIndexOfFirstLoadedComment(), itemsToInsert, true, (IVirtualizable)null);
                    }
                    else
                        this._virtPanel.AddItems((IEnumerable<IVirtualizable>)itemsToInsert);
                    this._alreadyLoadedCommentsCount = this._alreadyLoadedCommentsCount + itemsToInsert.Count;
                    if (flag)
                        this._virtPanel.ScrollToBottom(true);
                    this._isLoading = false;
                    this._isLoaded = true;
                }
            }));
        }

        private void EnsurePollIsAdded(CommentsInfo commentsInfo)
        {
            if (this._loadFromEnd)
                return;
            List<IVirtualizable> virtualizableItems = this._virtPanel.VirtualizableItems;
            Func<IVirtualizable, bool> func = (Func<IVirtualizable, bool>)(vi => vi is PollItem);
            if (virtualizableItems.FirstOrDefault<IVirtualizable>(func) != null || commentsInfo.poll == null || commentsInfo.poll.poll_id == 0L)
                return;
            PollItem pollItem = new PollItem(480.0, new Thickness(0.0), commentsInfo.poll, this._tid);
            MyVirtualizingPanel2 virtPanel = this._virtPanel;
            int index = 0;
            List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
            itemsToInsert.Add((IVirtualizable)pollItem);
            int num = 0;
            virtPanel.InsertRemoveItems(index, itemsToInsert, num != 0, null);
        }

        private void editCommentCallback(CommentItem commentItem)
        {
            commentItem.Comment.GroupId = this._gid;
            commentItem.Comment.TopicId = this._tid;
            ParametersRepository.SetParameterForId("EditDiscussionComment", (object)commentItem.Comment);
            ParametersRepository.SetParameterForId("CidToAuthorIdDict", (object)this.GetCidToAuthorDict());
            Navigator.Current.NavigateToNewWallPost(Math.Abs(commentItem.Comment.owner_id), commentItem.Comment.owner_id < 0, 0, false, false, false);
        }

        private void deleteCommentCallback(CommentItem obj)
        {
            if (obj.Comment.cid == 0L)
                return;
            this._virtPanel.RemoveItem((IVirtualizable)obj);
            GroupsService.Current.DeleteComment(this._gid, this._tid, obj.Comment.cid, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => this.PublishChangedEvent()));
        }

        private void replyCommentCallback(CommentItem obj)
        {
            this._replyCallback(obj);
        }

        public bool CanPostComment(string commentText, List<IOutboundAttachment> attachments, StickerItemData stickerItemData = null)
        {
            if ((string.IsNullOrWhiteSpace(commentText) || commentText.Length < 2) && stickerItemData == null)
            {
                if (attachments.Count != 0)
                {
                    List<IOutboundAttachment> source = attachments;
                    Func<IOutboundAttachment, bool> func = (Func<IOutboundAttachment, bool>)(a => a.UploadState != OutboundAttachmentUploadState.Completed);
                    if (!source.Any<IOutboundAttachment>(func))
                        goto label_4;
                }
                return false;
            }
        label_4:
            return true;
        }

        internal void AddComment(string commentText, List<IOutboundAttachment> attachments, Action<bool> resultCallback, StickerItemData stickerItemData = null, bool fromGroup = false, string stickerReferrer = "")
        {
            if (!this.CanPostComment(commentText, attachments, stickerItemData))
                resultCallback(false);
            else if (this._isAddingComment)
            {
                resultCallback(false);
            }
            else
            {
                this._isAddingComment = true;
                GroupsService.Current.AddTopicComment(this._gid, this._tid, commentText, attachments.Select<IOutboundAttachment, string>((Func<IOutboundAttachment, string>)(a => a.AttachmentId)).ToList<string>(), (Action<BackendResult<Comment, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            CommentItem commentItem = this.CreateCommentItem(AppGlobalStateManager.Current.GlobalState.LoggedInUser, res.ResultData, null);
                            MyVirtualizingPanel2 virtPanel = this._virtPanel;
                            int count = this._virtPanel.VirtualizableItems.Count;
                            List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
                            itemsToInsert.Add((IVirtualizable)commentItem);
                            int num = 0;
                            // ISSUE: variable of the null type
                            virtPanel.InsertRemoveItems(count, itemsToInsert, num != 0, null);
                            this._virtPanel.ScrollToBottom(true);
                            this.PublishChangedEvent();
                            this._isAddingComment = false;
                            resultCallback(true);
                        }));
                    else
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            this._isAddingComment = false;
                            resultCallback(false);
                        }));
                }), stickerItemData == null ? 0 : stickerItemData.StickerId, fromGroup, stickerReferrer);
            }
        }

        private string ProcessMentions(string commentText)
        {
            Dictionary<long, long> cidToAuthorDict = this.GetCidToAuthorDict();
            return RegexHelper.SubstituteInTopicCommentPostToExtended(commentText, this._gid, cidToAuthorDict);
        }

        private Dictionary<long, long> GetCidToAuthorDict()
        {
            Dictionary<long, long> dictionary = new Dictionary<long, long>();
            foreach (IVirtualizable virtualizableItem in this._virtPanel.VirtualizableItems)
            {
                if (virtualizableItem is CommentItem)
                {
                    CommentItem commentItem = virtualizableItem as CommentItem;
                    dictionary[commentItem.Comment.cid] = commentItem.Comment.from_id;
                }
            }
            return dictionary;
        }

        private CommentItem CreateCommentItem(User user, Comment c, VKClient.Common.Backend.DataObjects.Group group)
        {
            return CommentsItemsGeneratorHelper.CreateCommentItem(480.0, c, LikeObjectType.topic_comment, -this._gid, user, (User)null, group, new Action<CommentItem>(this.deleteCommentCallback), new Action<CommentItem>(this.replyCommentCallback), new Action<CommentItem>(this.editCommentCallback), (Action<CommentItem>)null);
        }

        private void CalculateOffsetAndCount(bool refresh, out int offset, out int count)
        {
            int val1 = this._isLoaded ? this.RELOAD_COUNT : this.LOAD_COUNT;
            if (refresh)
            {
                count = Math.Min(val1, this._knownCommentsCount);
                if (this._loadFromEnd)
                    offset = this._knownCommentsCount - count;
                else
                    offset = 0;
            }
            else
            {
                count = Math.Min(val1, this._knownCommentsCount - this.AlreadyLoadedCommentsCount);
                if (this._loadFromEnd)
                    offset = this._knownCommentsCount - this.AlreadyLoadedCommentsCount - count;
                else
                    offset = this.AlreadyLoadedCommentsCount;
            }
        }

        private int GetIndexOfFirstLoadedComment()
        {
            IVirtualizable virtualizable = this._virtPanel.VirtualizableItems.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is CommentItem));
            if (virtualizable == null)
                return 0;
            return this._virtPanel.VirtualizableItems.IndexOf(virtualizable);
        }

        private void PublishChangedEvent()
        {
            EventAggregator.Current.Publish((object)new TopicCommentAddedDeletedOrEdited()
            {
                tid = this._tid,
                gid = this._gid
            });
        }
    }
}
