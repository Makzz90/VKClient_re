using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.Utils;
using VKClient.Common.VideoCatalog;

namespace VKClient.Common.UC
{
  public class CommentsGenericUC : UserControl
  {
    public static readonly int CountToReload = 20;
    private DialogService _dialogService;
    private bool _commentsAreLoaded;
    private UCItem _commentsCountItem;
    private LikesItem _likesItem;
    private UCItem _loadMoreCommentsItem;
    private int _runningCommentsCount;
    private IScrollableArea _scrollViewer;
    private NewMessageUC ucNewComment;
    private bool _addingComment;
    private DelayedExecutor _de;
    private long _replyToCid;
    private long _replyToUid;
    private string _replyAutoForm;
    private TextSeparatorUC _commentsCountSeparatorUC;
    private UCItem _moreVideosUCItem;
    private const int OTHER_VIDEOS_MAX_COUNT = 3;
    internal Grid LayoutRoot;
    internal MyVirtualizingPanel2 virtPanel;
    internal TextBlock textBlockError;
    private bool _contentLoaded;

    private ISupportCommentsAndLikes VM
    {
      get
      {
        return base.DataContext as ISupportCommentsAndLikes;
      }
    }

    public ISupportOtherVideos OtherVideosVM
    {
      get
      {
        return base.DataContext as ISupportOtherVideos;
      }
    }

    public MyVirtualizingPanel2 Panel
    {
      get
      {
        return this.virtPanel;
      }
    }

    public IScrollableArea Scroll
    {
      get
      {
        return this._scrollViewer;
      }
    }

    public NewMessageUC UCNewComment
    {
      get
      {
        return this.ucNewComment;
      }
      set
      {
        this.ucNewComment = value;
        if (this.ucNewComment == null)
          return;
        this.ucNewComment.TextBoxNewComment.TextChanged += (new TextChangedEventHandler( this.TextBoxNewComment_TextChanged));
        ((UIElement) this.ucNewComment.ReplyUserUC).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.textBlockReplyToName_Tap_1));
      }
    }

    public ReplyUserUC ReplyUserUC
    {
      get
      {
        return this.ucNewComment.ReplyUserUC;
      }
    }

    public int CommentsCountForReload
    {
      get
      {
        int val2 = this.VM.TotalCommentsCount - this.VM.Comments.Count;
        return Math.Min(CommentsGenericUC.CountToReload, val2);
      }
    }

    private string ReplyAutoForm
    {
      get
      {
        return this._replyAutoForm;
      }
      set
      {
        this._replyAutoForm = value;
        this.ucNewComment.SetReplyAutoForm(value);
      }
    }

    public CommentsGenericUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
    }

    public void InitializeWithScrollViewer(IScrollableArea scrollViewer)
    {
      this._scrollViewer = scrollViewer;
      this.virtPanel.InitializeWithScrollViewer(this._scrollViewer, false);
      this.virtPanel.DeltaOffset = -550.0;
    }

    public void ProcessLoadedComments(bool result)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this._runningCommentsCount = this.VM.TotalCommentsCount;
        this.virtPanel.AddItems((IEnumerable<IVirtualizable>) this.GenereateVirtualizableItemsToAdd());
        this._commentsAreLoaded = true;
      }));
    }

    public void AddComment(List<IOutboundAttachment> attachments, Action<bool> resultCallback, StickerItemData stickerItemData = null, string stickerReferrer = "")
    {
      string str1 = this.ucNewComment.TextBoxNewComment.Text;
      if (this.ReplyAutoForm != null && str1.StartsWith(this.ReplyAutoForm))
      {
        string str2 = this.ReplyAutoForm.Remove(this.ReplyAutoForm.IndexOf(", "));
        string str3 = this._replyToUid > 0L ? "id" : "club";
        long num = this._replyToUid > 0L ? this._replyToUid : -this.VM.OwnerId;
        str1 = str1.Remove(0, this.ReplyAutoForm.Length).Insert(0, string.Format("[{0}{1}|{2}], ", str3, num, str2));
      }
      string str4 = str1.Replace("\r\n", "\r").Replace("\r", "\r\n");
      if ((string.IsNullOrWhiteSpace(str4) || str4.Length < 2) && stickerItemData == null)
      {
        if (attachments.Count != 0)
        {
          List<IOutboundAttachment> outboundAttachmentList = attachments;
          Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>) (a => a.UploadState != OutboundAttachmentUploadState.Completed);
          if (!Enumerable.Any<IOutboundAttachment>(outboundAttachmentList, (Func<IOutboundAttachment, bool>) func1))
            goto label_6;
        }
        // ISSUE: reference to a compiler-generated field
        resultCallback(false);
        return;
      }
label_6:
      if (str4.Length < 2 && attachments.Count <= 0 && stickerItemData == null)
        return;
      if (this._addingComment)
      {
        // ISSUE: reference to a compiler-generated field
        resultCallback(false);
      }
      else
      {
        this._addingComment = true;
        if (stickerItemData == null)
          this.ucNewComment.TextBoxNewComment.Text = string.Empty;
        Comment comment1 = new Comment();
        comment1.cid = 0L;
        comment1.date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
        comment1.text = stickerItemData == null ? str4 : "";
        comment1.reply_to_cid = this._replyToCid;
        comment1.reply_to_uid = this._replyToUid;
        comment1.likes = new Likes() { can_like = 0 };
        List<Attachment> m0List;
        if (stickerItemData != null)
          m0List = new List<Attachment>()
          {
            stickerItemData.CreateAttachment()
          };
        else
          m0List = Enumerable.ToList<Attachment>(Enumerable.Select<IOutboundAttachment, Attachment>(attachments, (Func<IOutboundAttachment, Attachment>) (a => a.GetAttachment())));
        comment1.Attachments=((List<Attachment>) m0List);
        int num = stickerItemData == null ? 0 : stickerItemData.StickerId;
        comment1.sticker_id = num;
        Comment comment2 = comment1;
        bool fromGroupChecked = this.ucNewComment.FromGroupChecked;
        comment2.from_id = !fromGroupChecked ? AppGlobalStateManager.Current.LoggedInUserId : this.VM.OwnerId;
        this.VM.AddComment(comment2, attachments.Select<IOutboundAttachment, string>((Func<IOutboundAttachment, string>)(a => a.AttachmentId)).ToList<string>(), fromGroupChecked, (Action<bool, Comment>)((res, createdComment) =>
        {
            if (res)
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    CommentItem commentItem = this.CreateCommentItem(createdComment);
                    this._addingComment = false;
                    MyVirtualizingPanel2 virtualizingPanel2 = this.virtPanel;
                    int count = this.virtPanel.VirtualizableItems.Count;
                    List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
                    itemsToInsert.Add((IVirtualizable)commentItem);
                    //int num = 0;
                    virtualizingPanel2.InsertRemoveItems(count, itemsToInsert, false, null);
                    this._runningCommentsCount = this._runningCommentsCount + 1;
                    this.KeepCommentsCountItemUpToDate();
                    this.ResetReplyFields();
                    resultCallback(true);
                    this.Panel.ScrollToBottom(true);
                }));
            else
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    GenericInfoUC.ShowBasedOnResult(1, "", null);
                    this._addingComment = false;
                    resultCallback(false);
                }));
        }), stickerReferrer);
      }
    }

    private void DeleteComment(CommentItem obj)
    {
      this.VM.DeleteComment(obj.Comment.cid);
      this.virtPanel.RemoveItem((IVirtualizable) obj);
      this._runningCommentsCount = this._runningCommentsCount - 1;
      this.KeepCommentsCountItemUpToDate();
    }

    private void _loadMoreCommentsItem_Tap(object sender, EventArgs e)
    {
      this.VM.LoadMoreComments(this.CommentsCountForReload, new Action<bool>(this.MoreCommentsAreLoaded));
    }

    private void MoreCommentsAreLoaded(bool result)
    {
      if (!result)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        UCItem moreCommentsItem = this._loadMoreCommentsItem;
        IVirtualizable virtualizable = (IVirtualizable)Enumerable.FirstOrDefault<IVirtualizable>(this.virtPanel.VirtualizableItems, (Func<IVirtualizable, bool>)(i => i is CommentItem));
        if (virtualizable == null)
          return;
        this.virtPanel.InsertRemoveItems(this.virtPanel.VirtualizableItems.IndexOf(virtualizable), this.GenereateVirtualizableItemsToAdd(), true, (IVirtualizable) moreCommentsItem);
      }));
    }

    private void KeepCommentsCountItemUpToDate()
    {
      this._commentsCountSeparatorUC.Text = CommentsItemsGeneratorHelper.GetTextForCommentsCount(this._runningCommentsCount);
    }

    public void UpdateLikesItem(bool liked)
    {
      if (this._likesItem == null)
        return;
      this._likesItem.Like(liked);
    }

    private List<IVirtualizable> GenereateVirtualizableItemsToAdd()
    {
      List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
      if (!this._commentsAreLoaded)
      {
        LikesInfo likesInfo1 = new LikesInfo();
        likesInfo1.count = this.VM.LikesCount;
        likesInfo1.repostsCount = this.VM.RepostsCount;
        List<long> likesAllIds = this.VM.LikesAllIds;
        List<UserLike> m0List = (likesAllIds != null ? Enumerable.ToList<UserLike>(Enumerable.Select<long, UserLike>(likesAllIds, (Func<long, UserLike>)(uid => new UserLike()
        {
          uid = uid
        }))) :  null) ??  new List<UserLike>();
        likesInfo1.users=((List<UserLike>) m0List);
        LikesInfo likesInfo2 = likesInfo1;
        double width1 = 480.0;
        Thickness margin1 =  new Thickness();
        LikedObjectData objectData = new LikedObjectData();
        objectData.OwnerId = this.VM.OwnerId;
        objectData.ItemId = this.VM.ItemId;
        objectData.Type = (int) this.VM.LikeObjectType;
        LikesInfo likesInfo3 = likesInfo2;
        int num1 = this.VM.CanRepost ? 1 : 0;
        int num2 = this.VM.UserLiked ? 1 : 0;
        User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
        List<User> users = this.VM.Users;
        this._likesItem = new LikesItem(width1, margin1, objectData, likesInfo3, num1 != 0, num2 != 0, loggedInUser, users);
        virtualizableList.Add((IVirtualizable) this._likesItem);
        ISupportOtherVideos otherVideosVm = this.OtherVideosVM;
        List<VKClient.Common.Backend.DataObjects.Video> videoList;
        if (otherVideosVm == null)
        {
          videoList =  null;
        }
        else
        {
            VKList<VKClient.Common.Backend.DataObjects.Video> otherVideos = otherVideosVm.OtherVideos;
          videoList = otherVideos != null ? otherVideos.items :  null;
        }
        if (videoList != null && otherVideosVm.OtherVideos.items.Count > 0)
        {
            VKList<VKClient.Common.Backend.DataObjects.Video> otherVideos = otherVideosVm.OtherVideos;
          List<Group> groupList = new List<Group>();
          List<User> userList = new List<User>();
          if (otherVideos.profiles != null)
              userList.AddRange((IEnumerable<User>)Enumerable.Select<User, User>(otherVideos.profiles, (Func<User, User>)(profile => new User()
            {
              id = profile.id,
              first_name = profile.first_name,
              last_name = profile.last_name
            })));
          if (otherVideos.groups != null)
              groupList.AddRange((IEnumerable<Group>)Enumerable.Select<Group, Group>(otherVideos.groups, (Func<Group, Group>)(profile => new Group()
            {
              id = profile.id,
              name = profile.name
            })));
          double width2 = 480.0;
          Thickness margin2 = new Thickness(0.0, 0.0, 0.0, 8.0);
          Func<UserControlVirtualizable> func1 = (Func<UserControlVirtualizable>) (() => (UserControlVirtualizable) new TextSeparatorUC()
          {
            Text = CommonResources.OtherVideos
          });
          // ISSUE: variable of the null type
          double landscapeWidth1 = 0.0;
          int num3 = 0;
          //Func<UserControlVirtualizable> getUserControlFunc1;
          UCItem ucItem1 = new UCItem(width2, margin2, func1, (Func<double>)(() => 56.0), null, landscapeWidth1, num3 != 0);
          virtualizableList.Add((IVirtualizable) ucItem1);
          IVideoCatalogItemUCFactory catalogItemFactory = ServiceLocator.Resolve<IVideoCatalogItemUCFactory>();
          IEnumerator<VKClient.Common.Backend.DataObjects.Video> enumerator = ((IEnumerable<VKClient.Common.Backend.DataObjects.Video>)Enumerable.Take<VKClient.Common.Backend.DataObjects.Video>(otherVideos.items, 3)).GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
                VKClient.Common.Backend.DataObjects.Video video = enumerator.Current;
              List<User> knownUsers = userList;
              List<Group> knownGroups = groupList;
              UCItem ucItem2 = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
              {
                UserControlVirtualizable controlVirtualizable = catalogItemFactory.Create(video, knownUsers, knownGroups, StatisticsActionSource.video_recommend, this.CreateVideoContext(otherVideos.context));
                ((System.Windows.Controls.Panel) (controlVirtualizable as CatalogItemUC).GridLayoutRoot).Background = ((Brush) (Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush));
                return controlVirtualizable;
              }), new Func<double> (() => catalogItemFactory.Height), null, 0.0, false);
              virtualizableList.Add((IVirtualizable) ucItem2);
            }
          }
          finally
          {
            if (enumerator != null)
              enumerator.Dispose();
          }
          double width3 = 480.0;
          Thickness margin3 =  new Thickness();
          Func<UserControlVirtualizable> func2 = (Func<UserControlVirtualizable>) (() => new UserControlVirtualizable());
          // ISSUE: variable of the null type
          double landscapeWidth2 = 0.0;
          int num4 = 0;
          //Func<UserControlVirtualizable> getUserControlFunc2;
          UCItem ucItem3 = new UCItem(width3, margin3, func2, (Func<double>)(() => 8.0), null, landscapeWidth2, num4 != 0);
          virtualizableList.Add((IVirtualizable) ucItem3);
          if (otherVideos.items.Count > 3)
          {
              this._moreVideosUCItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)new CategoryFooterShortUC()
            {
              TapAction = new Action(this.MoreVideos_OnTap)
            }), (Func<double>) (() => 64.0),  null, 0.0, false);
            virtualizableList.Add((IVirtualizable) this._moreVideosUCItem);
          }
        }
        int totalCommentsCount = this.VM.TotalCommentsCount;
        this._commentsCountSeparatorUC = new TextSeparatorUC()
        {
          Text = CommentsItemsGeneratorHelper.GetTextForCommentsCount(totalCommentsCount)
        };
        this._commentsCountItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)this._commentsCountSeparatorUC), (Func<double>)(() => 56.0), null, 0.0, false);
        virtualizableList.Add((IVirtualizable) this._commentsCountItem);
      }
      if (this.CommentsCountForReload > 0 && !ListExtensions.IsNullOrEmpty((IList) this.VM.Comments))
      {
        ShowMoreCommentsUC showMoreCommentsUc = new ShowMoreCommentsUC();
        double num = 54.0;
        ((FrameworkElement) showMoreCommentsUc).Height = num;
        Action action = (Action) (() => this._loadMoreCommentsItem_Tap(null,  null));
        showMoreCommentsUc.OnClickAction = action;
        string textFor = CommentsItemsGeneratorHelper.GetTextFor(this.CommentsCountForReload);
        showMoreCommentsUc.Text = textFor;
        ShowMoreCommentsUC showMoreCommentsUC = showMoreCommentsUc;
        this._loadMoreCommentsItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>)(() => (UserControlVirtualizable)showMoreCommentsUC), (Func<double>)(() => 54.0), null, 0.0, false);
        virtualizableList.Add((IVirtualizable) this._loadMoreCommentsItem);
      }
      long num5 = -1;
      CommentItem commentItem1 = Enumerable.FirstOrDefault<IVirtualizable>(this.virtPanel.VirtualizableItems, (Func<IVirtualizable, bool>)(i => i is CommentItem)) as CommentItem;
      if (commentItem1 != null)
        num5 = commentItem1.Comment.cid;
      List<Comment>.Enumerator enumerator1 = this.VM.Comments.GetEnumerator();
      try
      {
        while (enumerator1.MoveNext())
        {
          Comment current = enumerator1.Current;
          if (current.cid != num5)
          {
            CommentItem commentItem2 = this.CreateCommentItem(current);
            virtualizableList.Add((IVirtualizable) commentItem2);
          }
          else
            break;
        }
      }
      finally
      {
        enumerator1.Dispose();
      }
      ((UIElement) this.ucNewComment).Opacity = (this.VM.CanComment ? 1.0 : 0.6);
      ((UIElement) this.ucNewComment).IsHitTestVisible = this.VM.CanComment;
      return virtualizableList;
    }

    private string CreateVideoContext(string context)
    {
      string str = string.Format("{0}_{1}", this.VM.OwnerId, this.VM.ItemId);
      if (!string.IsNullOrEmpty(context))
        str += string.Format("|{0}", context);
      return str;
    }

    private void MoreVideos_OnTap()
    {
      Execute.ExecuteOnUIThread((Action) (() => this.virtPanel.InsertRemoveItems(this.virtPanel.VirtualizableItems.IndexOf((IVirtualizable) this._moreVideosUCItem) - 1, this.GetMoreOtherVideoItems(), false, (IVirtualizable) this._moreVideosUCItem)));
    }

    private List<IVirtualizable> GetMoreOtherVideoItems()
    {
      List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
      IVideoCatalogItemUCFactory catalogItemFactory = ServiceLocator.Resolve<IVideoCatalogItemUCFactory>();
      VKList<VKClient.Common.Backend.DataObjects.Video> otherVideos = this.OtherVideosVM.OtherVideos;
      List<Group> groupList = new List<Group>();
      List<User> userList = new List<User>();
      if (otherVideos.profiles != null)
          userList.AddRange((IEnumerable<User>)Enumerable.Select<User, User>(otherVideos.profiles, (Func<User, User>)(profile => new User()
        {
          id = profile.id,
          first_name = profile.first_name,
          last_name = profile.last_name
        })));
      if (otherVideos.groups != null)
          groupList.AddRange((IEnumerable<Group>)Enumerable.Select<Group, Group>(otherVideos.groups, (Func<Group, Group>)(profile => new Group()
        {
          id = profile.id,
          name = profile.name
        })));
      IEnumerator<VKClient.Common.Backend.DataObjects.Video> enumerator = ((IEnumerable<VKClient.Common.Backend.DataObjects.Video>)Enumerable.Skip<VKClient.Common.Backend.DataObjects.Video>(otherVideos.items, 3)).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
            VKClient.Common.Backend.DataObjects.Video video = enumerator.Current;
          List<User> knownUsers = userList;
          List<Group> knownGroups = groupList;
          UCItem ucItem = new UCItem(480.0, new Thickness(), (Func<UserControlVirtualizable>) (() =>
          {
            UserControlVirtualizable controlVirtualizable = catalogItemFactory.Create(video, knownUsers, knownGroups, StatisticsActionSource.video_recommend, this.CreateVideoContext(otherVideos.context));
            ((System.Windows.Controls.Panel) (controlVirtualizable as CatalogItemUC).GridLayoutRoot).Background = ((Brush) (Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush));
            return controlVirtualizable;
          }), new Func<double> (() => catalogItemFactory.Height),  null, 0.0, false);
          virtualizableList.Add((IVirtualizable) ucItem);
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
      return virtualizableList;
    }

    private CommentItem CreateCommentItem(Comment comment)
    {
        User user = Enumerable.FirstOrDefault<User>(this.VM.Users, (Func<User, bool>)(u => u.uid == comment.from_id));
        User user2 = Enumerable.FirstOrDefault<User>(this.VM.Users2, (Func<User, bool>)(u => u.uid == comment.reply_to_uid));
        Group group = Enumerable.FirstOrDefault<Group>(this.VM.Groups, (Func<Group, bool>)(g => g.id == -comment.from_id));
      if (user == null && comment.from_id == AppGlobalStateManager.Current.LoggedInUserId)
        user = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
      if (user2 == null && comment.reply_to_uid == AppGlobalStateManager.Current.LoggedInUserId)
        user2 = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
      Action<CommentItem> replyCallback = new Action<CommentItem>(this.ReplyToComment);
      LikeObjectType likeObjType = LikeObjectType.comment;
      if (this.VM.LikeObjectType == LikeObjectType.photo)
        likeObjType = LikeObjectType.photo_comment;
      if (this.VM.LikeObjectType == LikeObjectType.video)
        likeObjType = LikeObjectType.video_comment;
      if (this.VM.LikeObjectType == LikeObjectType.market)
        likeObjType = LikeObjectType.market_comment;
      return CommentsItemsGeneratorHelper.CreateCommentItem(480.0, comment, likeObjType, this.VM.OwnerId, user, user2, group, new Action<CommentItem>(this.DeleteComment), replyCallback, new Action<CommentItem>(this.EditComment),  null);
    }

    private void EditComment(CommentItem commentItem)
    {
      if (this.VM.LikeObjectType == LikeObjectType.photo)
      {
        commentItem.Comment.owner_id = this.VM.OwnerId;
        ParametersRepository.SetParameterForId("EditPhotoComment", commentItem.Comment);
      }
      else if (this.VM.LikeObjectType == LikeObjectType.video)
      {
        commentItem.Comment.owner_id = this.VM.OwnerId;
        ParametersRepository.SetParameterForId("EditVideoComment", commentItem.Comment);
      }
      else if (this.VM.LikeObjectType == LikeObjectType.market)
      {
        commentItem.Comment.owner_id = this.VM.OwnerId;
        ParametersRepository.SetParameterForId("EditProductComment", commentItem.Comment);
      }
      Navigator.Current.NavigateToNewWallPost(Math.Abs(this.VM.OwnerId), this.VM.OwnerId < 0, 0, false, false, false);
    }

    private void ReplyToComment(CommentItem commentItem)
    {
      this._replyToCid = commentItem.Comment.cid;
      this._replyToUid = commentItem.Comment.from_id;
      string str1 = "";
      string str2 = "";
      if (this._replyToUid > 0L)
      {
          User user = (User)Enumerable.FirstOrDefault<User>(this.VM.Users2, (Func<User, bool>)(u => u.uid == this._replyToUid));
        if (user == null && this._replyToUid == AppGlobalStateManager.Current.LoggedInUserId)
          user = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
        if (user != null)
        {
          str1 = user.first_name;
          str2 = user.first_name_dat;
        }
      }
      else
      {
          Group group = (Group)Enumerable.FirstOrDefault<Group>(this.VM.Groups, (Func<Group, bool>)(u => u.id == this.VM.OwnerId * -1L)) ?? GroupsService.Current.GetCachedGroup(-this.VM.OwnerId);
        if (group != null)
          str2 = str1 = group.name;
      }
      this.ReplyUserUC.Title = str2;
      ((UIElement) this.ReplyUserUC).Visibility = Visibility.Visible;
      if (this.ucNewComment.TextBoxNewComment.Text == "" || this.ucNewComment.TextBoxNewComment.Text == this.ReplyAutoForm)
      {
        this.ReplyAutoForm = str1 + ", ";
        this.ucNewComment.TextBoxNewComment.Text = this.ReplyAutoForm;
        this.ucNewComment.TextBoxNewComment.SelectionStart = this.ReplyAutoForm.Length;
      }
      else
        this.ReplyAutoForm = str1 + ", ";
      ((Control) this.ucNewComment.TextBoxNewComment).Focus();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      this.ResetReplyFields();
    }

    private void ResetReplyFields()
    {
      if (this.ucNewComment.TextBoxNewComment.Text == this.ReplyAutoForm)
        this.ucNewComment.TextBoxNewComment.Text = ("");
      this.ReplyAutoForm =  null;
      this._replyToUid = this._replyToCid = 0L;
      this.ReplyUserUC.Title = "";
      ((UIElement) this.ReplyUserUC).Visibility = Visibility.Collapsed;
      ((Control) this.UCNewComment.ucNewPost.TextBoxPost).Focus();
    }

    private void ShowHideErrorText(bool result)
    {
      this.textBlockError.Text = CommonResources.GenericErrorText;
      ((UIElement) this.textBlockError).Visibility = (result ? Visibility.Collapsed : Visibility.Visible);
    }

    private void textBlockReplyToName_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ResetReplyFields();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/CommentsGenericUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.virtPanel = (MyVirtualizingPanel2) base.FindName("virtPanel");
      this.textBlockError = (TextBlock) base.FindName("textBlockError");
    }
  }
}
