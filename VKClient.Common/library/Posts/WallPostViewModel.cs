using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Social;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class WallPostViewModel : ViewModelBase, IBinarySerializable
  {
    private string _text = string.Empty;
    private readonly ObservableCollection<IOutboundAttachment> _outboundAttachments = new ObservableCollection<IOutboundAttachment>();
    private readonly ObservableCollection<IOutboundAttachment> _outboundAttachmentsWithAdd = new ObservableCollection<IOutboundAttachment>();
    private List<Attachment> _uneditableAttachments = new List<Attachment>();
    private readonly ObservableCollection<IVirtualizable> _repostItems = new ObservableCollection<IVirtualizable>();
    private bool _friendsOnly;
    private bool _publishOnTwitter;
    private bool _publishOnFacebook;
    private long _userOrGroupId;
    private bool _isGroup;
    private WallPostViewModel.Mode _mode;
    private Dictionary<long, long> _cidToAuthorIdDict;
    private bool _editWallRepost;
    private WallRepostInfo _wallRepostInfo;
    private string _topicTitle;
    private bool _isDirty;
    private bool _isAdmin;
    private int _adminLevel;
    private bool _isPublicPage;
    private bool _fromGroup;
    private bool _signature;
    private long _postId;
    private Comment _comment;
    private bool _goToPhotoChooser;
    private bool _isPublishing;
    private bool _isOnPostPage;

    public WallPostViewModel.Mode WMMode
    {
      get
      {
        return this._mode;
      }
      set
      {
        this._mode = value;
        if (this.CanAddMoreAttachments)
          return;
        IOutboundAttachment outboundAttachment = (IOutboundAttachment) Enumerable.FirstOrDefault<IOutboundAttachment>(this._outboundAttachmentsWithAdd, (Func<IOutboundAttachment, bool>) (a => a is OutboundAddAttachment));
        if (outboundAttachment == null)
          return;
        ((Collection<IOutboundAttachment>) this._outboundAttachmentsWithAdd).Remove(outboundAttachment);
      }
    }

    public bool IsDirty
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.Text) || ((Collection<IOutboundAttachment>) this._outboundAttachments).Count > 0)
          return this._isDirty;
        return false;
      }
    }

    public string UniqueId
    {
      get
      {
        string str = string.Concat(this._mode, "_", this.IsNewPostSuggested.ToString());
        switch (this._mode)
        {
          case WallPostViewModel.Mode.EditWallPost:
          case WallPostViewModel.Mode.EditWallComment:
          case WallPostViewModel.Mode.EditPhotoComment:
          case WallPostViewModel.Mode.EditVideoComment:
          case WallPostViewModel.Mode.EditDiscussionComment:
          case WallPostViewModel.Mode.PublishWallPost:
          case WallPostViewModel.Mode.EditProductComment:
            return str;
          default:
            return string.Concat( new object[5]{ str, "_", this.UserOrGroupId, "_", this.IsGroup.ToString() });
        }
      }
    }

    public bool IsNewPostPostponedOrSuggested
    {
      get
      {
        if (!this.IsNewPostPostponed)
          return this.IsNewPostSuggested;
        return true;
      }
    }

    public bool IsNewPostSuggested
    {
      get
      {
        if (this._mode == WallPostViewModel.Mode.NewWallPost && this._isPublicPage)
          return this._adminLevel < 2;
        return false;
      }
    }

    public bool IsNewPostPostponed
    {
      get
      {
        if (this._mode == WallPostViewModel.Mode.NewWallPost && this._outboundAttachments != null)
          return Enumerable.FirstOrDefault<IOutboundAttachment>(this._outboundAttachments, (Func<IOutboundAttachment, bool>) (a => a.AttachmentId == "timestamp")) != null;
        return false;
      }
    }

    private bool IsPublishPostPostponed
    {
      get
      {
        if (this._mode == WallPostViewModel.Mode.PublishWallPost && this._outboundAttachments != null)
          return Enumerable.FirstOrDefault<IOutboundAttachment>(this._outboundAttachments, (Func<IOutboundAttachment, bool>) (a => a.AttachmentId == "timestamp")) != null;
        return false;
      }
    }

    public bool IsPostponed { get; set; }

    public bool IsSuggested { get; set; }

    public bool IsPublishSuggestedSuppressed { get; set; }

    public string Title
    {
      get
      {
        switch (this._mode)
        {
          case WallPostViewModel.Mode.NewWallPost:
            if (this._isPublicPage && this._adminLevel < 2)
              return ((string) CommonResources.SuggestedNews_SuggestAPost).ToUpperInvariant();
            return CommonResources.NewPost_NewPost;
          case WallPostViewModel.Mode.EditWallPost:
            return CommonResources.EditPost;
          case WallPostViewModel.Mode.NewWallComment:
          case WallPostViewModel.Mode.NewPhotoComment:
          case WallPostViewModel.Mode.NewVideoComment:
          case WallPostViewModel.Mode.NewDiscussionComment:
          case WallPostViewModel.Mode.NewProductComment:
            return ( UIStringFormatterHelper.FormatNumberOfSomething(((Collection<IOutboundAttachment>) this._outboundAttachments).Count, CommonResources.OneAttachmentFrm, CommonResources.ManageAttachments_TwoFourAttachmentsFrm, CommonResources.ManageAttachments_FiveMoreAttachmentsFrm, true,  null, false)).ToUpperInvariant();
          case WallPostViewModel.Mode.EditWallComment:
          case WallPostViewModel.Mode.EditPhotoComment:
          case WallPostViewModel.Mode.EditVideoComment:
          case WallPostViewModel.Mode.EditDiscussionComment:
          case WallPostViewModel.Mode.EditProductComment:
            return CommonResources.EditComment;
          case WallPostViewModel.Mode.NewTopic:
            return CommonResources.NewTopic;
          case WallPostViewModel.Mode.PublishWallPost:
            return ((string) CommonResources.SuggestedNews_Publish).ToUpperInvariant();
          default:
            return "";
        }
      }
    }

    public bool IsInNewTopicMode
    {
      get
      {
        return this._mode == WallPostViewModel.Mode.NewTopic;
      }
    }

    public Visibility IsInNewTopicModeVisibility
    {
      get
      {
        if (!this.IsInNewTopicMode)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool IsNewCommentMode
    {
      get
      {
        switch (this._mode)
        {
          case WallPostViewModel.Mode.NewWallComment:
          case WallPostViewModel.Mode.NewPhotoComment:
          case WallPostViewModel.Mode.NewVideoComment:
          case WallPostViewModel.Mode.NewDiscussionComment:
          case WallPostViewModel.Mode.NewProductComment:
            return true;
          default:
            return false;
        }
      }
    }

    public Comment Comment
    {
      get
      {
        return this._comment;
      }
    }

    public bool EditWallRepost
    {
      get
      {
        return this._editWallRepost;
      }
    }

    public WallRepostInfo WallRepostInfo
    {
      get
      {
        return this._wallRepostInfo;
      }
    }

    public Visibility HaveRepostVisibility
    {
      get
      {
        if (!this._editWallRepost)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public ObservableCollection<IVirtualizable> RepostItems
    {
      get
      {
        return this._repostItems;
      }
    }

    public string TextWatermarkText
    {
      get
      {
        if (this.IsInNewTopicMode)
          return CommonResources.NewTopicTextLbl;
        if (this._editWallRepost)
          return CommonResources.NewsPage_EnterComment;
        return CommonResources.NewsPage_WhatsNew;
      }
    }

    public string TopicTitle
    {
      get
      {
        return this._topicTitle;
      }
      set
      {
        bool canPublish = this.CanPublish;
        this._topicTitle = value;
        this._isDirty = true;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.TopicTitle));
                if (this.CanPublish == canPublish)
                    return;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanPublish));
            }
    }

    public bool? FriendsOnly
    {
      get
      {
        return new bool?(this._friendsOnly);
      }
      set
      {
        this._friendsOnly = value.Value;
        this.NotifyPropertyChanged<bool?>((System.Linq.Expressions.Expression<Func<bool?>>)(() => this.FriendsOnly));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.FacebookVisibility));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.TwitterVisibility));
            }
    }

    public bool? PublishOnTwitter
    {
      get
      {
        return new bool?(this._publishOnTwitter);
      }
      set
      {
        this._publishOnTwitter = value.Value;
        this.NotifyPropertyChanged<bool?>((System.Linq.Expressions.Expression<Func<bool?>>)(() => this.PublishOnTwitter));
            }
    }

    public bool? PublishOnFacebook
    {
      get
      {
        return new bool?(this._publishOnFacebook);
      }
      set
      {
        this._publishOnFacebook = value.Value;
        this.NotifyPropertyChanged<bool?>((System.Linq.Expressions.Expression<Func<bool?>>)(() => this.PublishOnFacebook));
            }
    }

    public string Text
    {
      get
      {
        return this._text;
      }
      set
      {
        bool canPublish = this.CanPublish;
        this._text = value;
        this._isDirty = true;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Text));
                if (this.CanPublish == canPublish)
                    return;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanPublish));
            }
    }

    public Visibility OwnPostVisibility
    {
      get
      {
        if (this._userOrGroupId != AppGlobalStateManager.Current.LoggedInUserId || !this.IsInNewWallPostMode)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility TwitterVisibility
    {
      get
      {
        if (this._userOrGroupId != AppGlobalStateManager.Current.LoggedInUserId || this._friendsOnly || (AppGlobalStateManager.Current.GlobalState.LoggedInUser.exports.twitter != 1 || !this.IsInNewWallPostMode))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility FacebookVisibility
    {
      get
      {
        if (this._userOrGroupId != AppGlobalStateManager.Current.LoggedInUserId || this._friendsOnly || (AppGlobalStateManager.Current.GlobalState.LoggedInUser.exports.facebook != 1 || !this.IsInNewWallPostMode))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public long UserOrGroupId
    {
      get
      {
        return this._userOrGroupId;
      }
    }

    public bool IsGroup
    {
      get
      {
        return this._isGroup;
      }
    }

    public ObservableCollection<IOutboundAttachment> OutboundAttachments
    {
      get
      {
        return this._outboundAttachments;
      }
    }

    public ObservableCollection<IOutboundAttachment> Attachments
    {
      get
      {
        return this._outboundAttachmentsWithAdd;
      }
    }

    public bool CanPublish
    {
      get
      {
        ObservableCollection<IOutboundAttachment> outboundAttachments1 = this.OutboundAttachments;
        Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>) (a => a.AttachmentId != "timestamp");
        if (Enumerable.Any<IOutboundAttachment>(outboundAttachments1, (Func<IOutboundAttachment, bool>) func1) || !string.IsNullOrWhiteSpace(this._text))
        {
          ObservableCollection<IOutboundAttachment> outboundAttachments2 = this.OutboundAttachments;
          Func<IOutboundAttachment, bool> func3 = (Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Completed);
          if (Enumerable.All<IOutboundAttachment>(outboundAttachments2, (Func<IOutboundAttachment, bool>) func3) && this._mode != WallPostViewModel.Mode.NewTopic)
            goto label_6;
        }
        if (this._mode != WallPostViewModel.Mode.EditWallPost || !this._editWallRepost)
        {
          if (this._mode == WallPostViewModel.Mode.NewTopic && !string.IsNullOrWhiteSpace(this._topicTitle) && !string.IsNullOrWhiteSpace(this._text))
            return Enumerable.All<IOutboundAttachment>(this.OutboundAttachments, (Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Completed));
          return false;
        }
label_6:
        return true;
      }
    }

    public bool GoDirectlyToPhotoChooser
    {
      get
      {
        return this._goToPhotoChooser;
      }
      set
      {
        this._goToPhotoChooser = value;
      }
    }

    public bool FromGroup
    {
      get
      {
        return this._fromGroup;
      }
      set
      {
        if (this._fromGroup == value)
          return;
        this._fromGroup = value;
        if (!this._fromGroup)
          this.Signature = false;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FromGroup));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FromGroupIsEnabled));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.SignatureVisibility));
            }
    }

    public bool FromGroupIsEnabled
    {
      get
      {
          return Enumerable.FirstOrDefault<IOutboundAttachment>(this._outboundAttachmentsWithAdd, (Func<IOutboundAttachment, bool>)(a => a.AttachmentId == "timestamp")) == null;
      }
    }

    public bool Signature
    {
      get
      {
        return this._signature;
      }
      set
      {
        this._signature = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.Signature));
            }
    }

    public Visibility FromGroupVisibility
    {
      get
      {
        if ((this._adminLevel <= 1 || !this._isGroup || (this._isPublicPage || !this.IsInNewWallPostMode)) && (this._adminLevel <= 1 || !this.IsInNewTopicMode))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SignatureVisibility
    {
      get
      {
        if ((this._adminLevel <= 1 || !this._isGroup || !this.IsInNewWallPostMode || !this.FromGroup && !this._isPublicPage) && ((!this.IsPostponed || !this._isGroup) && this.WMMode != WallPostViewModel.Mode.PublishWallPost))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool IsInNewWallPostMode
    {
      get
      {
        return this._mode == WallPostViewModel.Mode.NewWallPost;
      }
    }

    public int NumberOfAttAllowedToAdd
    {
      get
      {
        switch (this._mode)
        {
          case WallPostViewModel.Mode.EditWallPost:
            if (!this._editWallRepost)
              return Math.Max(0, 10 - ((Collection<IOutboundAttachment>) this._outboundAttachments).Count);
            return 0;
          case WallPostViewModel.Mode.NewWallComment:
          case WallPostViewModel.Mode.EditWallComment:
          case WallPostViewModel.Mode.EditPhotoComment:
          case WallPostViewModel.Mode.EditVideoComment:
          case WallPostViewModel.Mode.NewPhotoComment:
          case WallPostViewModel.Mode.NewVideoComment:
          case WallPostViewModel.Mode.NewProductComment:
          case WallPostViewModel.Mode.EditProductComment:
            return Math.Max(0, 2 - ((Collection<IOutboundAttachment>) this._outboundAttachments).Count);
          default:
            return Math.Max(0, 10 - Enumerable.Count<IOutboundAttachment>(this._outboundAttachments, (Func<IOutboundAttachment, bool>) (a => a.AttachmentId != "timestamp")));
        }
      }
    }

    public bool CanAddMoreAttachments
    {
      get
      {
        return this.NumberOfAttAllowedToAdd > 0;
      }
    }

    public bool IsOnPostPage
    {
      get
      {
        return this._isOnPostPage;
      }
      set
      {
        this._isOnPostPage = value;
        IEnumerator<IOutboundAttachment> enumerator = ((Collection<IOutboundAttachment>) this.Attachments).GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
            enumerator.Current.IsOnPostPage = value;
        }
        finally
        {
          if (enumerator != null)
            enumerator.Dispose();
        }
      }
    }

    public bool CanAddPollAttachment
    {
      get
      {
        if (this._mode == WallPostViewModel.Mode.EditWallPost || this._mode == WallPostViewModel.Mode.NewWallPost || this._mode == WallPostViewModel.Mode.PublishWallPost)
          return !Enumerable.Any<IOutboundAttachment>(this.OutboundAttachments, (Func<IOutboundAttachment, bool>) (a => a is OutboundPollAttachment));
        return false;
      }
    }

    public bool CannAddTimerAttachment
    {
      get
      {
        if ((this.IsInNewWallPostMode || this.IsPostponed || this.IsSuggested) && ((this.IsGroupAdmin || this.IsCurrentUser) && !this.IsNewPostSuggested))
          return !Enumerable.Any<IOutboundAttachment>(this.OutboundAttachments, (Func<IOutboundAttachment, bool>) (a => a is OutboundTimerAttachment));
        return false;
      }
    }

    private bool IsGroupAdmin
    {
      get
      {
        if (this._isGroup)
          return this._adminLevel > 1;
        return false;
      }
    }

    private bool IsCurrentUser
    {
      get
      {
        return this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public WallPostViewModel(WallPost wallPost, int adminLevel, WallRepostInfo wallRepostInfo = null)
      : this()
    {
      this.InitializeWithWallPost(wallPost, adminLevel, wallRepostInfo);
    }

    public WallPostViewModel(long userOrGroupId, bool isGroup, int adminLevel, bool isPublicPage, bool isNewTopicMode)
      : this()
    {
      this._userOrGroupId = userOrGroupId;
      this._isGroup = isGroup;
      this._adminLevel = adminLevel;
      this._isPublicPage = isPublicPage;
      this._mode = isNewTopicMode ? WallPostViewModel.Mode.NewTopic : WallPostViewModel.Mode.NewWallPost;
      if (isNewTopicMode)
        return;
      if (this.TwitterVisibility == Visibility.Visible)
        this.PublishOnTwitter = new bool?(true);
      if (this.FacebookVisibility != Visibility.Visible)
        return;
      this.PublishOnFacebook = new bool?(true);
    }

    public WallPostViewModel()
    {
      this._outboundAttachments.CollectionChanged += new NotifyCollectionChangedEventHandler(this._outboundAttachments_CollectionChanged);
      ((Collection<IOutboundAttachment>) this._outboundAttachmentsWithAdd).Add((IOutboundAttachment) new OutboundAddAttachment());
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(4);
      writer.Write(this._friendsOnly);
      writer.Write(this._publishOnTwitter);
      writer.WriteString(this._text);
      List<OutboundAttachmentContainer> list = (List<OutboundAttachmentContainer>)Enumerable.ToList<OutboundAttachmentContainer>(Enumerable.Select<IOutboundAttachment, OutboundAttachmentContainer>(this._outboundAttachments, (Func<IOutboundAttachment, OutboundAttachmentContainer>)(a => new OutboundAttachmentContainer(a))));
      writer.WriteList<OutboundAttachmentContainer>((IList<OutboundAttachmentContainer>) list, 10000);
      writer.Write(this._userOrGroupId);
      writer.Write(this._isGroup);
      writer.Write(this._publishOnFacebook);
      writer.Write(this._isAdmin);
      writer.Write(this._isPublicPage);
      writer.Write(this._fromGroup);
      writer.Write(this._signature);
      writer.Write(this._postId);
      writer.WriteList<Attachment>((IList<Attachment>) this._uneditableAttachments, 10000);
      writer.Write((int) this._mode);
      writer.Write(this._editWallRepost);
      writer.Write(this._isDirty);
      writer.WriteString(this._topicTitle);
      BinarySerializerExtensions.WriteDictionary(writer, this._cidToAuthorIdDict);
      writer.Write<Comment>(this._comment, false);
      writer.Write<WallRepostInfo>(this._wallRepostInfo, false);
      writer.Write(this._adminLevel);
    }

    public void Read(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            if (num >= 1)
            {
                this._friendsOnly = reader.ReadBoolean();
                this._publishOnTwitter = reader.ReadBoolean();
                this._text = reader.ReadString();
                List<OutboundAttachmentContainer> source = reader.ReadList<OutboundAttachmentContainer>();
                this._outboundAttachments.Clear();
                foreach (IOutboundAttachment outboundAttachment in source.Select<OutboundAttachmentContainer, IOutboundAttachment>((Func<OutboundAttachmentContainer, IOutboundAttachment>)(c => c.OutboundAttachment)))
                    this._outboundAttachments.Add(outboundAttachment);
                this._userOrGroupId = reader.ReadInt64();
                this._isGroup = reader.ReadBoolean();
                this._publishOnFacebook = reader.ReadBoolean();
                this._isAdmin = reader.ReadBoolean();
                this._isPublicPage = reader.ReadBoolean();
                this._fromGroup = reader.ReadBoolean();
                this._signature = reader.ReadBoolean();
                this.UploadAttachments();
                this._postId = reader.ReadInt64();
                this._uneditableAttachments = reader.ReadList<Attachment>();
                this._mode = (WallPostViewModel.Mode)reader.ReadInt32();
                this._editWallRepost = reader.ReadBoolean();
                this._isDirty = reader.ReadBoolean();
            }
            if (num >= 2)
                this._topicTitle = reader.ReadString();
            if (num >= 3)
            {
                this._cidToAuthorIdDict = reader.ReadDictionaryLong();
                this._comment = reader.ReadGeneric<Comment>();
            }
            if (num < 4)
                return;
            this._wallRepostInfo = reader.ReadGeneric<WallRepostInfo>();
            this._adminLevel = reader.ReadInt32();
        }

    public static WallPostViewModel CreateEditWallCommentVM(Comment comment)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.EditWallComment;
      Comment comment1 = comment;
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment1, num != 0);
      return wallPostViewModel;
    }

    internal static WallPostViewModel CreateEditPhotoCommentVM(Comment comment)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.EditPhotoComment;
      Comment comment1 = comment;
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment1, num != 0);
      return wallPostViewModel;
    }

    internal static WallPostViewModel CreateEditVideoCommentVM(Comment comment)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.EditVideoComment;
      Comment comment1 = comment;
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment1, num != 0);
      return wallPostViewModel;
    }

    internal static WallPostViewModel CreateEditProductCommentVM(Comment comment)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.EditProductComment;
      Comment comment1 = comment;
      int num = 1;
      wallPostViewModel.InitializeWithComment(comment1, num != 0);
      return wallPostViewModel;
    }

    internal static WallPostViewModel CreateEditDiscussionCommentVM(Comment comment, Dictionary<long, long> cidToAuthorIdDict)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.EditDiscussionComment;
      wallPostViewModel._cidToAuthorIdDict = cidToAuthorIdDict;
      Comment comment1 = comment;
      int num = 1;
      wallPostViewModel.InitializeWithComment(comment1, num != 0);
      return wallPostViewModel;
    }

    internal static WallPostViewModel CreateNewWallCommentVM(long ownerId, long postId)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.NewWallComment;
      Comment comment1 = new Comment() { date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true), from_id = AppGlobalStateManager.Current.LoggedInUserId, text = "", likes = new Likes() { can_like = 1 } };
      comment1.owner_id = ownerId;
      comment1.PostId = postId;
      Comment comment2 = comment1;
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment2, num != 0);
      return wallPostViewModel;
    }

    public static WallPostViewModel CreateNewPhotoCommentVM(long ownerId, long photoId)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.NewPhotoComment;
      Comment comment1 = new Comment() { date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true), from_id = AppGlobalStateManager.Current.LoggedInUserId, text = "", likes = new Likes() { can_like = 1 } };
      comment1.owner_id = ownerId;
      comment1.PhotoId = photoId;
      Comment comment2 = comment1;
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment2, num != 0);
      return wallPostViewModel;
    }

    public static WallPostViewModel CreateNewVideoCommentVM(long ownerId, long videoId)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.NewVideoComment;
      Comment comment1 = new Comment() { date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true), from_id = AppGlobalStateManager.Current.LoggedInUserId, text = "", likes = new Likes() { can_like = 1 } };
      comment1.owner_id = ownerId;
      comment1.VideoId = videoId;
      Comment comment2 = comment1;
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment2, num != 0);
      return wallPostViewModel;
    }

    public static WallPostViewModel CreateNewProductCommentVM(long ownerId, long productId)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.NewProductComment;
      Comment comment = new Comment() { date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true), from_id = AppGlobalStateManager.Current.LoggedInUserId, text = "", likes = new Likes() { can_like = 1 } };
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment, num != 0);
      return wallPostViewModel;
    }

    public static WallPostViewModel CreateNewDiscussionCommentVM()
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel();
      wallPostViewModel._mode = WallPostViewModel.Mode.NewDiscussionComment;
      Comment comment = new Comment() { date = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true), from_id = AppGlobalStateManager.Current.LoggedInUserId, text = "", likes = new Likes() { can_like = 1 } };
      int num = 0;
      wallPostViewModel.InitializeWithComment(comment, num != 0);
      return wallPostViewModel;
    }

    private void _outboundAttachments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && !e.NewItems.IsNullOrEmpty())
            {
                IOutboundAttachment outboundAttachment = e.NewItems[0] as IOutboundAttachment;
                outboundAttachment.IsOnPostPage = this.IsOnPostPage;
                this._outboundAttachmentsWithAdd.Insert(e.NewStartingIndex, outboundAttachment);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && !e.OldItems.IsNullOrEmpty())
            {
                this._outboundAttachmentsWithAdd.Remove(e.OldItems[0] as IOutboundAttachment);
                if (e.OldItems[0] is OutboundTimerAttachment)
                    this.FromGroup = false;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
                this._outboundAttachmentsWithAdd.Clear();
            IOutboundAttachment outboundAttachment1 = this._outboundAttachmentsWithAdd.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(o => o is OutboundAddAttachment));
            if (!this.CanAddMoreAttachments && !this.CannAddTimerAttachment)
            {
                if (outboundAttachment1 == null)
                    return;
                this._outboundAttachmentsWithAdd.Remove(outboundAttachment1);
            }
            else
            {
                if (outboundAttachment1 != null)
                    return;
                this._outboundAttachmentsWithAdd.Add((IOutboundAttachment)new OutboundAddAttachment());
            }
        }

    private void InitializeWithComment(Comment comment, bool isGroupDiscussionComment = false)
    {
      this._comment = comment;
      this._userOrGroupId = Math.Abs(comment.owner_id);
      this._isGroup = comment.owner_id < 0L;
      this._text = comment.text;
      if (isGroupDiscussionComment)
        this._text = RegexHelper.SubstituteInTopicCommentExtendedToPost(this._text);
      this.InitializeAttachments(comment.Attachments);
    }

    private void InitializeWithWallPost(WallPost wallPost, int adminLevel, WallRepostInfo wallRepostInfo = null)
    {
      this._editWallRepost = wallPost.IsRepost();
      this._adminLevel = adminLevel;
      this._wallRepostInfo = wallRepostInfo;
      this._text = wallPost.text;
      this._userOrGroupId = Math.Abs(wallPost.to_id);
      this._isGroup = wallPost.to_id < 0L;
      this._postId = wallPost.id;
      this._signature = (ulong) wallPost.signer_id > 0UL;
      this.InitializeAttachments(wallPost.attachments);
      if (wallPost.geo != null)
      {
        double latitude;
        double longitude;
        wallPost.geo.coordinates.ParseCoordinates(out latitude, out longitude);
        ((Collection<IOutboundAttachment>) this.OutboundAttachments).Add((IOutboundAttachment) new OutboundGeoAttachment(latitude, longitude));
      }
      if (wallPost.IsPostponed)
      {
        this.IsPostponed = true;
        ((Collection<IOutboundAttachment>) this.OutboundAttachments).Add((IOutboundAttachment) new OutboundTimerAttachment(new TimerAttachment()
        {
          ScheduledPublishDateTime = Extensions.UnixTimeStampToDateTime((double) wallPost.date, true)
        }));
      }
      if (!wallPost.IsSuggested)
        return;
      this.IsSuggested = true;
    }

    private void InitializeAttachments(List<Attachment> attachments)
    {
      this._uneditableAttachments.Clear();
      ((Collection<IOutboundAttachment>) this._outboundAttachments).Clear();
      if (attachments == null)
        return;
      List<Attachment>.Enumerator enumerator = attachments.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Attachment current = enumerator.Current;
          IOutboundAttachment outboundAttachment =  null;
          if (current.photo != null)
            outboundAttachment = (IOutboundAttachment) OutboundPhotoAttachment.CreateForChoosingExistingPhoto(current.photo, 0, false, PostType.WallPost);
          if (current.video != null)
            outboundAttachment = (IOutboundAttachment) new OutboundVideoAttachment(current.video);
          if (current.audio != null)
            outboundAttachment = (IOutboundAttachment) new OutboundAudioAttachment(current.audio);
          if (current.doc != null)
            outboundAttachment = (IOutboundAttachment) new OutboundDocumentAttachment(current.doc);
          if (current.poll != null)
            outboundAttachment = (IOutboundAttachment) new OutboundPollAttachment(current.poll);
          if (current.link != null)
            outboundAttachment = (IOutboundAttachment) new OutboundLinkAttachment(current.link);
          if (current.note != null)
            outboundAttachment = (IOutboundAttachment) new OutboundNoteAttachment(current.note);
          if (current.market_album != null)
            outboundAttachment = (IOutboundAttachment) new OutboundMarketAlbumAttachment(current.market_album);
          if (current.album != null)
            outboundAttachment = (IOutboundAttachment) new OutboundAlbumAttachment(current.album);
          if (outboundAttachment != null)
            ((Collection<IOutboundAttachment>) this.OutboundAttachments).Add(outboundAttachment);
          else
            this._uneditableAttachments.Add(current);
        }
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    public void Publish(Action<ResultCode> callback)
    {
        if (!this.CanPublish || this._isPublishing)
            return;
        this._isPublishing = true;
        this.SetInProgress(true, "");
        WallPostRequestData postRequestData = new WallPostRequestData();
        postRequestData.AttachmentIds = this.OutboundAttachments.Where<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a =>
        {
            if (!a.IsGeo)
                return a.AttachmentId != "timestamp";
            return false;
        })).Select<IOutboundAttachment, string>((Func<IOutboundAttachment, string>)(a => a.AttachmentId)).ToList<string>();
        postRequestData.message = this.Text.Replace("\r\n", "\r").Replace("\r", "\r\n");
        if (this._userOrGroupId != 0L)
            postRequestData.owner_id = this._isGroup ? -this._userOrGroupId : this._userOrGroupId;
        postRequestData.post_id = this._postId;
        if (this._comment != null)
            postRequestData.comment_id = this._comment.cid;
        OutboundGeoAttachment outboundGeoAttachment = this.OutboundAttachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.IsGeo)) as OutboundGeoAttachment;
        if (outboundGeoAttachment != null)
        {
            postRequestData.latitude = new double?(outboundGeoAttachment.Latitude);
            postRequestData.longitude = new double?(outboundGeoAttachment.Longitude);
        }
        OutboundTimerAttachment timerAttachment = this.OutboundAttachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.AttachmentId == "timestamp")) as OutboundTimerAttachment;
        if (timerAttachment != null)
            postRequestData.publish_date = new long?((long)Extensions.DateTimeToUnixTimestamp(timerAttachment.Timer.ScheduledPublishDateTime.ToUniversalTime(), true));
        else if (this.IsPostponed)
            this._mode = WallPostViewModel.Mode.PublishWallPost;
        postRequestData.FriendsOnly = this.FriendsOnly.Value;
        postRequestData.PublishOnTwitter = this.PublishOnTwitter.Value;
        postRequestData.PublishOnFacebook = this.PublishOnFacebook.Value;
        if (this._adminLevel > 1 && this._isGroup || this._postId != 0L)
        {
            postRequestData.Sign = this.Signature;
            postRequestData.OnBehalfOfGroup = this.FromGroup;
        }
        switch (this._mode)
        {
            case WallPostViewModel.Mode.NewWallPost:
            case WallPostViewModel.Mode.PublishWallPost:
                WallService.Current.Post(postRequestData, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.PostsLimitOrAlreadyScheduled && res.Error.error_msg.ToLower().Contains("wall is closed"))
                        res.ResultCode = ResultCode.WallIsDisabled;
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        if (!this.IsNewPostPostponedOrSuggested)
                            this.FireWallPostAddedOrEditedEvent(res.ResultData.response, postRequestData.owner_id, true, (Action)(() => callback(res.ResultCode)));
                        else if (this.IsNewPostPostponed)
                            EventAggregator.Current.Publish((object)new WallPostPostponed(postRequestData.owner_id));
                        else if (this.IsNewPostSuggested)
                            EventAggregator.Current.Publish((object)new WallPostSuggested()
                            {
                                id = res.ResultData.response,
                                to_id = postRequestData.owner_id
                            });
                        if (this._mode == WallPostViewModel.Mode.PublishWallPost)
                        {
                            WallPost wallPost = new WallPost()
                            {
                                to_id = this._isGroup ? -this._userOrGroupId : this._userOrGroupId,
                                id = this._postId
                            };
                            EventAggregator current = EventAggregator.Current;
                            WallPostPublished wallPostPublished = new WallPostPublished();
                            wallPostPublished.WallPost = wallPost;
                            int num1 = timerAttachment != null ? 1 : 0;
                            wallPostPublished.IsPostponed = num1 != 0;
                            int num2 = !this.IsPublishSuggestedSuppressed ? 1 : 0;
                            wallPostPublished.IsSuggested = num2 != 0;
                            current.Publish((object)wallPostPublished);
                            if (this.IsPostponed)
                                EventAggregator.Current.Publish((object)new WallPostPostponedPublished()
                                {
                                    WallPost = wallPost
                                });
                        }
                        if (!this.IsNewPostPostponedOrSuggested)
                            return;
                    }
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.EditWallPost:
                WallService.Current.Edit(postRequestData, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.FireWallPostAddedOrEditedEvent(this._postId, postRequestData.owner_id, false, null);
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.EditWallComment:
                WallService.Current.EditComment(postRequestData, (Action<BackendResult<long, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.UpdateCommentAndFireTheCommentEditedEvent();
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.EditPhotoComment:
                PhotosService.Current.EditComment(this._comment.cid, postRequestData.message, postRequestData.owner_id, postRequestData.AttachmentIds, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.UpdateCommentAndFireTheCommentEditedEvent();
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.EditVideoComment:
                VideoService.Instance.EditComment(this._comment.cid, postRequestData.message, postRequestData.owner_id, postRequestData.AttachmentIds, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.UpdateCommentAndFireTheCommentEditedEvent();
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.EditDiscussionComment:
                GroupsService.Current.EditComment(this._comment.GroupId, this._comment.TopicId, this._comment.cid, postRequestData.message, postRequestData.AttachmentIds, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.UpdateCommentAndFireTheCommentEditedEvent();
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.NewTopic:
                GroupsService.Current.CreateTopic(this._userOrGroupId, this._topicTitle, this._text, postRequestData.AttachmentIds, this._fromGroup, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.FireTopicCreatedEvent(res.ResultData.response);
                    callback(res.ResultCode);
                }));
                break;
            case WallPostViewModel.Mode.EditProductComment:
                MarketService.Instance.EditComment(postRequestData.owner_id, this._comment.cid, postRequestData.message, postRequestData.AttachmentIds, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    this._isPublishing = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.UpdateCommentAndFireTheCommentEditedEvent();
                    callback(res.ResultCode);
                }));
                break;
        }
    }

    private void FireTopicCreatedEvent(long topidId)
    {
      EventAggregator.Current.Publish(new TopicCreated()
      {
        gid = this._userOrGroupId,
        tid = topidId,
        text = this.Text,
        title = this.TopicTitle
      });
    }

    private void UpdateCommentAndFireTheCommentEditedEvent()
    {
      if (this._comment == null)
        return;
      this._comment.text = this.Text;
      if (this._mode == WallPostViewModel.Mode.EditDiscussionComment)
        this._comment.text = RegexHelper.SubstituteInTopicCommentPostToExtended(this._comment.text, this.UserOrGroupId, this._cidToAuthorIdDict);
      this._comment.Attachments=(new List<Attachment>((IEnumerable<Attachment>)Enumerable.Select<IOutboundAttachment, Attachment>(this.OutboundAttachments, (Func<IOutboundAttachment, Attachment>)(oa => oa.GetAttachment()))));
      EventAggregator.Current.Publish(new CommentEdited()
      {
        Comment = this._comment
      });
      if (this._mode != WallPostViewModel.Mode.EditDiscussionComment)
        return;
      EventAggregator.Current.Publish(new TopicCommentAddedDeletedOrEdited()
      {
        tid = this._comment.TopicId,
        gid = this._comment.GroupId
      });
    }

    private void FireWallPostAddedOrEditedEvent(long postId, long ownerId, bool added = true, Action callback = null)
    {
      WallService.Current.GetWallPostByIdWithComments(postId, ownerId, 0, 0, 0, true, (Action<BackendResult<GetWallPostResponseData, ResultCode>>) (res =>
      {
        if (res.ResultData.WallPost == null || res.ResultData.Users == null || res.ResultData.Groups == null)
          return;
        WallPostAddedOrEdited postAddedOrEdited = new WallPostAddedOrEdited();
        postAddedOrEdited.NewlyAddedWallPost = res.ResultData.WallPost;
        postAddedOrEdited.Users = res.ResultData.Users;
        postAddedOrEdited.Groups = res.ResultData.Groups;
        postAddedOrEdited.Edited = !added;
        EventAggregator.Current.Publish(postAddedOrEdited);
        Action action = callback;
        if (action != null)
          action.Invoke();
        SocialDataManager.Instance.MarkFeedAsStale(ownerId);
      }), 0, 0, LikeObjectType.post);
    }

    public void AddAttachment(IOutboundAttachment attachment)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            IOutboundAttachment outboundAttachment = this._outboundAttachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.AttachmentId == "timestamp"));
            if (outboundAttachment != null)
                this._outboundAttachments.Insert(this._outboundAttachments.IndexOf(outboundAttachment), attachment);
            else
                this._outboundAttachments.Add(attachment);
            this._isDirty = true;
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Title));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanPublish));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanAddMoreAttachments));
        }));
    }

    public void InsertAttachment(int index, IOutboundAttachment attachment)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            this._outboundAttachments.Insert(index, attachment);
            this._isDirty = true;
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Title));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanPublish));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanAddMoreAttachments));
        }));
    }

    public void UploadAttachments()
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            IOutboundAttachment attachment = this._outboundAttachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.NotStarted));
            if (attachment == null)
                return;
            this.UploadAttachment(attachment, new Action(this.UploadAttachments));
        }));
    }

    public void UploadAttachment(IOutboundAttachment attachment, Action callback = null)
    {
        if (attachment.UploadState == OutboundAttachmentUploadState.Completed)
            return;
        this.UpdateUploadingStatus(true);
        attachment.Upload((Action)(() => Execute.ExecuteOnUIThread((Action)(() =>
        {
            this.UpdateUploadingStatus(false);
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanPublish));
            if (callback == null)
                return;
            callback();
        }))), (Action<double>)null);
    }

    internal void RemoveAttachment(IOutboundAttachment outboundAtt)
    {
        outboundAtt.RemoveAndCancelUpload();
        this._outboundAttachments.Remove(outboundAtt);
        this.UpdateUploadingStatus(false);
        this._isDirty = true;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanPublish));
    }

    public void UpdateUploadingStatus(bool forceTrue = false)
    {
      ObservableCollection<IOutboundAttachment> outboundAttachments = this.OutboundAttachments;
      Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>) (a => a.UploadState != OutboundAttachmentUploadState.Uploading);
     if (Enumerable.All<IOutboundAttachment>(outboundAttachments, (Func<IOutboundAttachment, bool>)func1) && !forceTrue)
        this.SetInProgress(false, "");
      else
        this.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
    }

    public enum Mode
    {
      NewWallPost,
      EditWallPost,
      NewWallComment,
      EditWallComment,
      EditPhotoComment,
      EditVideoComment,
      EditDiscussionComment,
      NewPhotoComment,
      NewVideoComment,
      NewDiscussionComment,
      NewTopic,
      PublishWallPost,
      NewProductComment,
      EditProductComment,
    }
  }
}
