using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Social;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

using VKClient.Common.BLExtensions;
using VKClient.Audio.Base.BLExtensions;

namespace VKClient.Common.Library
{
  public class PostCommentsViewModel : ViewModelBase, IHandle<WallPostAddedOrEdited>, IHandle, ILikeable
  {
    private readonly int _countToLoad = 4;
    private readonly int _countToReload = 20;
    private readonly double _width = 480.0;
    private int _commentsCount = -1;
    private List<Comment> _fetchedComments = new List<Comment>();
    private List<User> _user1List = new List<User>();
    private List<Group> _groupsList = new List<Group>();
    private List<User> _user2List = new List<User>();
    private bool _canLike = true;
    private bool _canRepost = true;
    private bool _canComment = true;
    private long _postId;
    private long _ownerId;
    private MyVirtualizingPanel2 _panel;
    private int _runningCountOfComments;
    private UCItem _reloadButtonItem;
    private UCItem _commentsCountItem;
    private NewsItemDataWithUsersAndGroupsInfo _wallPostData;
    private GetWallPostResponseData _wallResponseData;
    private Action _loadedCallback;
    private bool _liked;
    private bool _canPin;
    private bool _canUnpin;
    private Action<CommentItem> _replyCommentAction;
    private long _knownPollId;
    private long _knownPollOwnerId;
    private bool _canRepostCommunity;
    private bool _loadMoreInUIFlag;
    private bool _loadingInBuffer;
    private bool _postingComment;
    private TextSeparatorUC _commentsCountSeparatorUC;
    private ShowMoreCommentsUC _showMoreCommentsUC;
    private bool _isPinning;

    public NewsItemDataWithUsersAndGroupsInfo WallPostData
    {
      get
      {
        return this._wallPostData;
      }
    }

    public bool CanLike
    {
      get
      {
        return this._canLike;
      }
      set
      {
        this._canLike = value;
        base.NotifyPropertyChanged<bool>(() => this.CanLike);
      }
    }

    public List<User> Users2
    {
      get
      {
        return this._user2List;
      }
    }

    public List<Group> Groups
    {
      get
      {
        return this._groupsList;
      }
    }

    public WallPost WallPost
    {
      get
      {
        if (this._wallPostData == null)
          return  null;
        return this._wallPostData.WallPost;
      }
    }

    public bool CanRepost
    {
      get
      {
        return this._canRepost;
      }
      set
      {
        this._canRepost = value;
        base.NotifyPropertyChanged<bool>(() => this.CanRepost);
      }
    }

    public bool CanComment
    {
      get
      {
        return this._canComment;
      }
      set
      {
        this._canComment = value;
        base.NotifyPropertyChanged<bool>(() => this.CanComment);
      }
    }

    public bool CanPin
    {
      get
      {
        return this._canPin;
      }
      set
      {
        this._canPin = value;
        base.NotifyPropertyChanged<bool>(() => this.CanPin);
      }
    }

    public bool CanUnpin
    {
      get
      {
        return this._canUnpin;
      }
      set
      {
        this._canUnpin = value;
        base.NotifyPropertyChanged<bool>(() => this.CanUnpin);
      }
    }

    public bool Liked
    {
      get
      {
        return this._liked;
      }
      set
      {
        this._liked = value;
        base.NotifyPropertyChanged<bool>(() => this.Liked);
      }
    }

    private bool IsWallPostAddedToUI
    {
        get
			{
				IEnumerable<IVirtualizable> arg_2A_0 = this._panel.VirtualizableItems;
				Func<IVirtualizable, bool> arg_2A_1 = new Func<IVirtualizable, bool>((vi)=>{return vi is WallPostItem;});
				
				return Enumerable.FirstOrDefault<IVirtualizable>(arg_2A_0, arg_2A_1) != null;
			}
    }

    public WallPostItem WallPostItem
    {
        get
			{
				IEnumerable<IVirtualizable> arg_2A_0 = this._panel.VirtualizableItems;
				Func<IVirtualizable, bool> arg_2A_1 = new Func<IVirtualizable, bool>((vi)=>{return vi is WallPostItem;});
				
				return Enumerable.FirstOrDefault<IVirtualizable>(arg_2A_0, arg_2A_1) as WallPostItem;
			}
    }

    public string Title
    {
      get
      {
        return CommonResources.WallPost_Title;
      }
    }

    public long OwnerId
    {
      get
      {
        return this._ownerId;
      }
    }

    public bool CanRepostCommunity
    {
      get
      {
        return this._canRepostCommunity;
      }
      set
      {
        this._canRepostCommunity = value;
        base.NotifyPropertyChanged<bool>(() => this._canRepostCommunity);
      }
    }

    public long PollId
		{
			get
			{
				if (this._wallPostData != null)
				{
					List<Attachment> attachments = this._wallPostData.WallPost.attachments;
					if (attachments != null)
					{
						IEnumerable<Attachment> arg_3C_0 = attachments;
						Func<Attachment, bool> arg_3C_1 = new Func<Attachment, bool>((a)=>{return a.type == "poll";});
						
						Attachment attachment = Enumerable.FirstOrDefault<Attachment>(arg_3C_0, arg_3C_1);
						if (attachment != null && attachment.poll != null)
						{
							return attachment.poll.poll_id;
						}
					}
				}
				return this._knownPollId;
			}
		}

    public long PollOwnerId
    {
      get
      {
        if (this._wallPostData == null || this._wallPostData.WallPost == null)
          return this._knownPollOwnerId;
        if (!((IList) this._wallPostData.WallPost.copy_history).IsNullOrEmpty())
          return this._wallPostData.WallPost.copy_history[0].owner_id;
        return this._wallPostData.WallPost.to_id;
      }
    }

    public PostCommentsViewModel(NewsItemDataWithUsersAndGroupsInfo wallPostData, long postId, long ownerId, MyVirtualizingPanel2 panel, Action loadedCallback, Action<CommentItem> replyCommentAction, long knownPollId, long knownPollOwnerId)
    {
      this._wallPostData = wallPostData;
      this._postId = postId;
      this._ownerId = ownerId;
      this._panel = panel;
      this._loadedCallback = loadedCallback;
      this._replyCommentAction = replyCommentAction;
      this._knownPollId = knownPollId;
      this._knownPollOwnerId = knownPollOwnerId;
      this.AddNewsItemIfItIsNotThere();
      EventAggregator.Current.Subscribe(this);
    }

    private void AddNewsItemIfItIsNotThere()
    {
      if (this._wallPostData == null)
        return;
      if (!this.IsWallPostAddedToUI)
      {
        GroupsService.Current.AddCachedGroups((IEnumerable<Group>) this._wallPostData.Groups);
        WallPostItem wallPostItem = new WallPostItem(this._width,  new Thickness(), false, this._wallPostData, new Action<WallPostItem>(this.DeletedWallPost), false,  null, false, false, true, true,  null,  null);
        MyVirtualizingPanel2 panel = this._panel;
        int index = 0;
        List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
        itemsToInsert.Add((IVirtualizable) wallPostItem);
        int num = 0;
        // ISSUE: variable of the null type
        
        panel.InsertRemoveItems(index, itemsToInsert, num != 0, null);
        this.UpdateCanSomethingProperties();
      }
      if (this._wallResponseData == null)
        return;
      EventAggregator current = EventAggregator.Current;
      OpenPostEvent openPostEvent = new OpenPostEvent();
      openPostEvent.PostId = this._wallResponseData.WallPost.PostId;
      List<string> copyPostIds = this._wallResponseData.WallPost.CopyPostIds;
      openPostEvent.CopyPostIds = copyPostIds;
      current.Publish(openPostEvent);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
     IEnumerable<IVirtualizable> arg_FD_0 = this._panel.VirtualizableItems;
				Func<IVirtualizable, bool> arg_FD_1 = new Func<IVirtualizable, bool>((vi)=>{return vi is LikesItem;});
				
				if (Enumerable.FirstOrDefault<IVirtualizable>(arg_FD_0, arg_FD_1) == null)
				{
      double width = this._width;
      Thickness margin =  new Thickness();
      WallPost wallPost = this._wallResponseData.WallPost;
      LikesInfo likesAll = this._wallResponseData.LikesAll;
      int num1 = this._wallResponseData.WallPost.likes.user_likes == 1 ? 1 : 0;
      User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
      List<User> users = this._wallResponseData.Users;
      WallPostItem wallPostItem1 = this.WallPostItem;
      int num2 = wallPostItem1 != null ? (wallPostItem1.CanShowLikesSeparator ? 1 : 0) : 0;
      LikesItem likesItem = new LikesItem(width, margin, wallPost, likesAll, num1 != 0, loggedInUser, users, num2 != 0);
      MyVirtualizingPanel2 panel1 = this._panel;
      int index1 = 1;
      List<IVirtualizable> itemsToInsert1 = new List<IVirtualizable>();
      itemsToInsert1.Add((IVirtualizable) likesItem);
      int num3 = 0;
      // ISSUE: variable of the null type
      panel1.InsertRemoveItems(index1, itemsToInsert1, num3 != 0, null);
                }
    }

    private void DeletedWallPost(WallPostItem obj)
    {
      Navigator.Current.GoBack();
    }

    private void UpdateCanSomethingProperties()
    {
      this.CanComment = this._wallPostData != null && this._wallPostData.WallPost.comments.can_post == 1;
      this.CanRepost = this._wallPostData != null && (this._wallPostData.WallPost.reposts.user_reposted == 1 || this._wallPostData.WallPost.likes.can_publish == 1);
      this.CanRepostCommunity = this._wallPostData != null && this._wallPostData.WallPost.CanRepostToCommunity();
      this.CanLike = this._wallPostData != null && (this._wallPostData.WallPost.likes.can_like == 1 || this._wallPostData.WallPost.likes.user_likes == 1);
      this.Liked = this._wallPostData != null && this._wallPostData.WallPost.likes.user_likes == 1;
      this.CanPin = this._wallPostData != null && this._wallPostData.WallPost.CanPin(this._wallPostData.Groups);
      this.CanUnpin = this._wallPostData != null && this._wallPostData.WallPost.CanUnpin(this._wallPostData.Groups);
    }

    public bool CanPostComment(string text, List<IOutboundAttachment> attachments, StickerItemData stickerItemData = null)
    {
        if ((string.IsNullOrWhiteSpace(text) || text.Length < 2) && stickerItemData == null)
        {
            if (attachments.Count != 0)
            {
                List<IOutboundAttachment> source = attachments;

                Func<IOutboundAttachment, bool> predicate = new Func<IOutboundAttachment, bool>(a => { return a.UploadState != OutboundAttachmentUploadState.Completed; });//Func<IOutboundAttachment, bool> func = (Func<IOutboundAttachment, bool>) (a => a.UploadState != OutboundAttachmentUploadState.Completed);

                if (!source.Any<IOutboundAttachment>(predicate))
                    goto label_4;
            }
            return false;
        }
    label_4:
        return true;
    }

    public void PostComment(string text, long replyCid, long replyUid, bool fromGroup, List<IOutboundAttachment> attachments, Action<bool> callback, StickerItemData stickerItemData = null, string stickerReferrer = "")
    {
      if (!this.CanPostComment(text, attachments, stickerItemData))
        callback(false);
      else if (this._postingComment)
      {
        callback(false);
      }
      else
      {
        this._postingComment = true;
        this.SetInProgress(true, "");
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: method pointer
        WallService.Current.AddComment(this._postId, this._ownerId, text, replyCid, fromGroup, attachments.Select<IOutboundAttachment, string>((Func<IOutboundAttachment, string>)(a => a.AttachmentId)).ToList<string>(), (Action<BackendResult<Comment, ResultCode>>)(res =>
        {
          this.SetInProgress(false, "");
          this._postingComment = false;
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            bool flag = res.ResultCode == ResultCode.Succeeded;
            if (flag)
            {
              CommentItem commentItem = this.CreateCommentItem(res.ResultData);
              this._runningCountOfComments = this._runningCountOfComments + 1;
              this.UpdateCommentsCountItem();
              MyVirtualizingPanel2 panel = this._panel;
              int count = this._panel.VirtualizableItems.Count;
              List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
              itemsToInsert.Add((IVirtualizable) commentItem);
              int num = 0;
              // ISSUE: variable of the null type
              
              panel.InsertRemoveItems(count, itemsToInsert, num != 0, null);
              this._fetchedComments.Add(res.ResultData);
              EventAggregator current = EventAggregator.Current;
              WallCommentIsAddedDeleted commentIsAddedDeleted = new WallCommentIsAddedDeleted();
              commentIsAddedDeleted.Added = true;
              long toId = this._wallPostData.WallPost.to_id;
              commentIsAddedDeleted.OwnerId = toId;
              long id = this._wallPostData.WallPost.id;
              commentIsAddedDeleted.WallPostId = id;
              current.Publish(commentIsAddedDeleted);
            }
            callback(flag);
          }));
        }), stickerItemData == null ? 0 : stickerItemData.StickerId, stickerReferrer);
      }
    }

    public void LoadMoreCommentsInUI()
    {
      if (this._wallResponseData != null)
      {
        if (this._wallPostData == null)
        {
          NewsItemDataWithUsersAndGroupsInfo usersAndGroupsInfo = new NewsItemDataWithUsersAndGroupsInfo();
          usersAndGroupsInfo.WallPost = this._wallResponseData.WallPost;
          List<User> users = this._wallResponseData.Users;
          usersAndGroupsInfo.Profiles = users;
          List<Group> groups = this._wallResponseData.Groups;
          usersAndGroupsInfo.Groups = groups;
          this._wallPostData = usersAndGroupsInfo;
        }
        this.AddNewsItemIfItIsNotThere();
      }
      if (this.AllLoaded())
        this.CallLoadedCallback();
      else if (this.HaveInBuffer())
      {
        this.LoadCommentsFromBuffer();
        if (this._loadingInBuffer)
          return;
        this._loadingInBuffer = true;
        this.LoadMoreCommentsInBuffer((Action<bool>) (success => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this._loadingInBuffer = false;
          if (!this._loadMoreInUIFlag)
            return;
          this._loadMoreInUIFlag = false;
          if (!success)
            return;
          this.LoadMoreCommentsInUI();
        }))));
      }
      else if (this._loadingInBuffer)
      {
        this._loadMoreInUIFlag = true;
      }
      else
      {
        this._loadingInBuffer = true;
        this.LoadMoreCommentsInBuffer((Action<bool>) (success => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this._loadingInBuffer = false;
          if (!success)
            return;
          this.LoadMoreCommentsInUI();
        }))));
      }
    }

    private bool AllLoaded()
    {
      if (this._commentsCount != -1)
        return this.GetCountLoadedOnUI() == this._commentsCount;
      return false;
    }

    private bool HaveInBuffer()
    {
      if (this._commentsCount == -1)
        return false;
      int countLoadedOnUi = this.GetCountLoadedOnUI();
      if (countLoadedOnUi == this._commentsCount)
        return true;
      return this._fetchedComments.Count > countLoadedOnUi;
    }

    private void LoadMoreCommentsInBuffer(Action<bool> completionCallback)
    {
      int num = this._commentsCount != -1 ? this._countToReload : this._countToLoad;
      bool flag = this._wallPostData == null;
      LikeObjectType likeObjectType = LikeObjectType.post;
      if (this._wallPostData != null && this._wallPostData.WallPost != null)
        likeObjectType = this._wallPostData.WallPost.GetLikeObjectType();
      if (flag || !this.IsWallPostAddedToUI)
        this.SetInProgress(true, CommonResources.Loading);
      WallService.Current.GetWallPostByIdWithComments(this._postId, this._ownerId, this._fetchedComments.Count, num, this._commentsCount, flag, (Action<BackendResult<GetWallPostResponseData, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.SetInProgress(false, "");
        if (res.ResultCode != ResultCode.Succeeded)
        {
          completionCallback(false);
        }
        else
        {
          if (this._commentsCount == -1)
          {
            EventAggregator.Current.Publish(new WallCommentsLikesUpdated()
            {
              OwnerId = this._ownerId,
              WallPostId = this._postId,
              CommentsCount = res.ResultData.Count,
              LikesCount = res.ResultData.LikesAll.count
            });
            this._commentsCount = res.ResultData.Count;
            this._runningCountOfComments = this._commentsCount;
          }
          this._user1List.AddRange((IEnumerable<User>) res.ResultData.Users);
          this._user2List.AddRange((IEnumerable<User>) res.ResultData.Users2);
          if (AppGlobalStateManager.Current.GlobalState.LoggedInUser != null)
          {
            this._user1List.Add(AppGlobalStateManager.Current.GlobalState.LoggedInUser);
            this._user2List.Add(AppGlobalStateManager.Current.GlobalState.LoggedInUser);
          }
          this._groupsList.AddRange((IEnumerable<Group>) res.ResultData.Groups);
          List<Comment> fetchedComments = this._fetchedComments;
          this._fetchedComments = res.ResultData.Comments;
          List<Comment>.Enumerator enumerator = fetchedComments.GetEnumerator();
          try
          {
              foreach (Comment comment1 in fetchedComments)
              {
                  Comment comment = comment1;
                  if (!this._fetchedComments.Any<Comment>((Func<Comment, bool>)(c => c.cid == comment.cid)))
                      this._fetchedComments.Add(comment);
              }
          }
          finally
          {
            enumerator.Dispose();
          }
          this._wallResponseData = res.ResultData;
          if (this._wallResponseData.WallPost.to_id == 0L && this._wallPostData != null)
            this._wallResponseData.WallPost = this._wallPostData.WallPost;
          completionCallback(true);
          if (!this._wallPostData.WallPost.IsNotExist)
            return;
          MessageBox.Show(CommonResources.WallPostIsNotAvailable, CommonResources.Error, (MessageBoxButton) 0);
          Navigator.Current.GoBack();
        }
      }))), this.PollId, this.PollOwnerId, likeObjectType);
    }

    private void LoadCommentsFromBuffer()
    {
      ((DependencyObject) Deployment.Current).Dispatcher.BeginInvoke((Action) (() =>
      {
        UCItem reloadButtonItem = this._reloadButtonItem;
        this._reloadButtonItem =  null;
        int countLoadedOnUi = this.GetCountLoadedOnUI();
        int num1 = 0;
        if (countLoadedOnUi == 0)
        {
          num1 = this._fetchedComments.Count;
        }
        else
        {
            Comment oldestLoaded = this.GetOldestLoadedComment();
            if (oldestLoaded != null)
            {
                Comment comment = this._fetchedComments.FirstOrDefault<Comment>((Func<Comment, bool>)(c =>
                {
                    if (c.cid == oldestLoaded.cid)
                        return c.from_id == oldestLoaded.from_id;
                    return false;
                }));
                if (comment != null)
                    num1 = this._fetchedComments.IndexOf(comment);
            }
        }
        int num2 = countLoadedOnUi == 0 ? this._countToLoad : this._countToReload;
        List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
        for (int index = num1 - 1; index >= 0 && num1 - index <= num2; --index)
        {
          CommentItem commentItem = this.CreateCommentItem(this._fetchedComments[index]);
          itemsToInsert.Add((IVirtualizable) commentItem);
        }
        int num3 = countLoadedOnUi + itemsToInsert.Count;
        if (num3 < this._commentsCount)
        {
          UCItem reloadButton = this.CreateReloadButton(Math.Min(this._countToReload, this._commentsCount - num3));
          itemsToInsert.Add((IVirtualizable) reloadButton);
        }
        if (this._commentsCountItem == null)
        {
          this._commentsCountItem = this.CreateCommentsCountItem();
          itemsToInsert.Add((IVirtualizable) this._commentsCountItem);
        }
        itemsToInsert.Reverse(0, itemsToInsert.Count);
        bool keepItemsBelowIndexFixed = true;
        if (this.GetCountLoadedOnUI() == 0)
          keepItemsBelowIndexFixed = false;
        this._panel.InsertRemoveItems(this.GetIndexToInsertItems(), itemsToInsert, keepItemsBelowIndexFixed, (IVirtualizable) reloadButtonItem);
        this.CallLoadedCallback();
      }));
    }

    private void CallLoadedCallback()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this._loadedCallback == null)
          return;
        this._loadedCallback();
      }));
    }

    private UCItem CreateCommentsCountItem()
    {
      this._commentsCountSeparatorUC = new TextSeparatorUC()
      {
        Text = this.GetTextForCommentsCount()
      };
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      this._commentsCountItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)this._commentsCountSeparatorUC), (Func<double>)(() => 56.0), (Action<UserControlVirtualizable>)null, 0.0, false);
      SocialDataManager.Instance.UpdateCommentsCount(this._ownerId, this._postId, this._runningCountOfComments);
      return this._commentsCountItem;
    }

    private void UpdateCommentsCountItem()
    {
      if (this._commentsCountItem != null)
      {
        if (this._runningCountOfComments != 0)
        {
          this._commentsCountSeparatorUC.Text = this.GetTextForCommentsCount();
        }
        else
        {
          this._panel.RemoveItem((IVirtualizable) this._commentsCountItem);
          this._commentsCountItem =  null;
        }
      }
      else
      {
        this._commentsCountItem = this.CreateCommentsCountItem();
        MyVirtualizingPanel2 panel = this._panel;
        int index = 2;
        List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
        itemsToInsert.Add((IVirtualizable) this._commentsCountItem);
        int num = 0;
        // ISSUE: variable of the null type
        
        panel.InsertRemoveItems(index, itemsToInsert, num != 0, null);
      }
      SocialDataManager.Instance.UpdateCommentsCount(this._ownerId, this._postId, this._runningCountOfComments);
    }

    private string GetTextForCommentsCount()
    {
      if (this._runningCountOfComments < 0)
        return "";
      if (this._runningCountOfComments == 0)
        return CommonResources.PostCommentsPage_NoComments;
      return UIStringFormatterHelper.FormatNumberOfSomething(this._runningCountOfComments, CommonResources.PostCommentPage_OneCommentFrm, CommonResources.PostCommentPage_TwoFourCommentsFrm, CommonResources.PostCommentPage_FiveCommentsFrm, true,  null, false);
    }

    private UCItem CreateReloadButton(int numberToReload)
    {
      ShowMoreCommentsUC showMoreCommentsUc = new ShowMoreCommentsUC();
      double num = 54.0;
      ((FrameworkElement) showMoreCommentsUc).Height = num;
      Action action = (Action) (() => this.reloadButtonItem_Tap(null,  null));
      showMoreCommentsUc.OnClickAction = action;
      string textFor = this.GetTextFor(numberToReload);
      showMoreCommentsUc.Text = textFor;
      this._showMoreCommentsUC = showMoreCommentsUc;
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      this._reloadButtonItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)this._showMoreCommentsUC), (Func<double>)(() => 54.0), (Action<UserControlVirtualizable>)null, 0.0, false);
      return this._reloadButtonItem;
    }

    private void reloadButtonItem_Tap(object sender, EventArgs e)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      this.LoadMoreCommentsInUI();
      stopwatch.Stop();
    }

    private string GetTextFor(int numberToReload)
    {
      if (numberToReload == 1)
        return CommonResources.PostCommentsPage_PreviousComment;
      return string.Format(UIStringFormatterHelper.FormatNumberOfSomething(numberToReload, CommonResources.PostCommentsPage_PreviousOneCommentsFrm, CommonResources.PostCommentsPage_PreviousTwoFourCommentsFrm, CommonResources.PostCommentsPage_PreviousFiveCommentsFrm, true,  null, false), numberToReload);
    }

    private int GetIndexToInsertItems()
    {
        IVirtualizable virtualizable = this._panel.VirtualizableItems.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(vi => vi is CommentItem));
        if (virtualizable == null)
            return this._panel.VirtualizableItems.Count;
        return this._panel.VirtualizableItems.IndexOf(virtualizable);
    }

    private Comment GetOldestLoadedComment()
    {
        CommentItem commentItem = this._panel.VirtualizableItems.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(vi => vi is CommentItem)) as CommentItem;
        if (commentItem != null)
            return commentItem.Comment;
        return (Comment)null;
    }

    private int GetCountLoadedOnUI()
    {
        return this._panel.VirtualizableItems.Count<IVirtualizable>((Func<IVirtualizable, bool>)(vi => vi is CommentItem));
    }

    private CommentItem CreateCommentItem(Comment comment)
    {
        User user = this._user1List.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == comment.from_id));
        User user2 = this._user2List.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == comment.reply_to_uid));
        Group group = this._groupsList.FirstOrDefault<Group>((Func<Group, bool>)(g => g.id == -comment.from_id));
        return new CommentItem(this._width - 32.0, new Thickness(8.0, 0.0, 0.0, 16.0), LikeObjectType.comment, new Action<CommentItem>(this.DeleteComment), new Action<CommentItem>(this.ReplyToComment), new Action<CommentItem>(this.EditComment), this._wallResponseData.WallPost.to_id, comment, user, user2, group, (Action<CommentItem>)null, "", "", new Action<CommentItem>(this.SeeAllLikes), false, false, "");
    }

    private void SeeAllLikes(CommentItem item)
    {
      Navigator.Current.NavigateToLikesPage(item.OwnerId, item.Comment.cid, 1, item.LikesCount, false);
    }

    private void EditComment(CommentItem commentItem)
    {
      commentItem.Comment.owner_id = this._wallPostData.WallPost.to_id;
      ParametersRepository.SetParameterForId("EditWallComment", commentItem.Comment);
      Navigator.Current.NavigateToNewWallPost(Math.Abs(this._wallPostData.WallPost.to_id), this._wallPostData.WallPost.to_id < 0, 0, false, false, false);
    }

    public void ReplyToComment(CommentItem commentItem)
    {
      this._replyCommentAction(commentItem);
    }

    public void DeleteComment(CommentItem commentItem)
    {
      this._panel.RemoveItem((IVirtualizable) commentItem);
      this._runningCountOfComments = this._runningCountOfComments - 1;
      this.UpdateCommentsCountItem();
      EventAggregator current = EventAggregator.Current;
      WallCommentIsAddedDeleted commentIsAddedDeleted = new WallCommentIsAddedDeleted();
      commentIsAddedDeleted.Added = false;
      long id = this._wallPostData.WallPost.id;
      commentIsAddedDeleted.WallPostId = id;
      long toId = this._wallPostData.WallPost.to_id;
      commentIsAddedDeleted.OwnerId = toId;
      current.Publish(commentIsAddedDeleted);
      WallService.Current.DeleteComment(this._ownerId, commentItem.Comment.cid, (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {}));
    }

    internal void Refresh()
    {
      if (this._loadingInBuffer)
        return;
      this._commentsCount = -1;
      this._fetchedComments = new List<Comment>();
      this._user1List = new List<User>();
      this._user2List = new List<User>();
      this._commentsCountItem =  null;
      this._panel.ClearItems();
      this._wallResponseData =  null;
      this.LoadMoreCommentsInUI();
    }

    internal void Like()
    {
      this.LikeImpl(true);
    }

    internal void Unlike()
    {
      this.LikeImpl(false);
    }

    private void LikeImpl(bool like)
    {
      if (this._wallPostData == null)
        return;
      LikesService.Current.AddRemoveLike(like, this._wallPostData.WallPost.to_id, this._wallPostData.WallPost.id, this._wallPostData.WallPost.GetLikeObjectType(), (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {}), "");
      this.Liked = like;
      EventAggregator current = EventAggregator.Current;
      WallItemLikedUnliked itemLikedUnliked = new WallItemLikedUnliked();
      itemLikedUnliked.OwnerId = this._wallPostData.WallPost.to_id;
      itemLikedUnliked.WallPostId = this._wallPostData.WallPost.id;
      int num = like ? 1 : 0;
      itemLikedUnliked.Liked = num != 0;
      current.Publish(itemLikedUnliked);
    }

    internal void Share(string text = "", long gid = 0, string groupName = "")
    {
      if (this._wallPostData == null)
        return;
      this._wallPostData.WallPost.reposts.user_reposted = 1;
      this.UpdateCanSomethingProperties();
      WallService.Current.Repost(this._ownerId, this._postId, text, this._wallPostData.WallPost.GetRepostObjectType(), gid, (Action<BackendResult<RepostResult, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, gid, groupName);
          if (gid != 0L || this.Liked)
            return;
          this.Liked = true;
          EventAggregator current = EventAggregator.Current;
          WallItemLikedUnliked itemLikedUnliked = new WallItemLikedUnliked();
          itemLikedUnliked.OwnerId = this._wallPostData.WallPost.to_id;
          itemLikedUnliked.WallPostId = this._wallPostData.WallPost.id;
          int num = 1;
          itemLikedUnliked.Liked = num != 0;
          current.Publish(itemLikedUnliked);
        }
        else
          new GenericInfoUC().ShowAndHideLater(CommonResources.Error,  null);
      }))));
    }

    public void Handle(WallPostAddedOrEdited message)
    {
      if (this._wallPostData == null || this._wallPostData.WallPost.to_id != message.NewlyAddedWallPost.to_id || this._wallPostData.WallPost.id != message.NewlyAddedWallPost.id)
        return;
      NewsItemDataWithUsersAndGroupsInfo usersAndGroupsInfo = new NewsItemDataWithUsersAndGroupsInfo();
      List<Group> groups = message.Groups;
      usersAndGroupsInfo.Groups = groups;
      List<User> users = message.Users;
      usersAndGroupsInfo.Profiles = users;
      WallPost newlyAddedWallPost = message.NewlyAddedWallPost;
      usersAndGroupsInfo.WallPost = newlyAddedWallPost;
      this._wallPostData = usersAndGroupsInfo;
      WallPostItem wallPostItem1 = new WallPostItem(this._width,  new Thickness(), false, this._wallPostData,  null, false,  null, false, false, true, true,  null,  null);
      WallPostItem wallPostItem2 = ((IEnumerable<IVirtualizable>) this._panel.VirtualizableItems).FirstOrDefault<IVirtualizable>() as WallPostItem;
      if (wallPostItem2 == null)
        return;
      this._panel.Substitute((IVirtualizable) wallPostItem2, (IVirtualizable) wallPostItem1);
    }

    internal void PinUnpin(Action<bool> callback)
    {
      if (this._isPinning)
        return;
      this._isPinning = true;
      this._wallPostData.WallPost.PinUnpin((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.UpdateCanSomethingProperties();
        this._isPinning = false;
        callback(res);
      }))));
    }

    public void LikeUnlike(bool like)
    {
      this.LikeImpl(like);
    }
  }
}
