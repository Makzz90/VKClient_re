using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.BLExtensions;


namespace VKClient.Common.Library
{
  public class WallPostItem : VirtualizableItemBase, IHaveUniqueKey, ISupportChildHeightChange, IHaveNewsfeedItemId, ICanHideFromNewsfeed
  {
    private readonly Dictionary<int, WallPostItem.WallPostHistoryVirtItems> _historyVirtItemsDict = new Dictionary<int, WallPostItem.WallPostHistoryVirtItems>();
    private readonly WallPost _wallPost;
    private readonly List<User> _users;
    private readonly List<Group> _groups;
    private readonly User _fromUser;
    private readonly Group _fromGroup;
    private readonly bool _preview;
    //private bool _showHideFromNews;
    private readonly bool _suppressRepostButton;
    private double _height;
    private readonly bool _showDivideLine;
    private readonly bool _showBackground;
    private readonly Action<WallPostItem> _deletedItemCallback;
    private static int _instanceCount;
    private readonly bool _isFeedbackItem;
    private readonly bool _showEarlierReplies;
    private readonly NewsFeedAdsItem _parentAdItem;
    private readonly Func<List<MenuItem>> _getExtraMenuItemsFunc;
    private const double MARGIN_MIN_VALUE = 8.0;
    private const double EXTRA_MARGIN_LEFT_RIGHT = 16.0;
    private WallRepostInfo _wallRepostInfo;
    private const bool _isCommentsAttachments = false;
    private const bool _isMessage = false;
    private const bool _isHorizontal = false;
    private const double _horizontalWidth = 0.0;
    private const bool _rightAlign = false;
    private UserOrGroupHeaderItem _originalHeaderItem;
    private NewsTextItem _originalTextItem;
    private AttachmentsItem _originalAttachmentsItem;
    private LinkToUserOrGroupItem _originalSignerItem;
    private UCItem _publishRejectItem;
    private bool _isPinning;

    private bool IsGroupPost
    {
      get
      {
        return this._wallPost.from_id < 0L;
      }
    }

    public int AdminLevel { get; private set; }

    public bool AllowPinUnpin
    {
      get
      {
        return !this.IsSuggestedPostponed;
      }
    }

    public bool IsSuggestedPostponed
    {
      get
      {
        return this._wallPost.IsSuggestedPostponed;
      }
    }

    public WallPost WallPost
    {
      get
      {
        return this._wallPost;
      }
    }

    public Action<long, User, Group> HideSourceItemsCallback { get; set; }

    public Action<NewsFeedIgnoreItemData> IgnoreNewsfeedItemCallback { get; set; }

    public string TextToCopy
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this._wallPost.text))
          return this._wallPost.text;
        if (this._wallPost.IsRepost())
          return this._wallPost.copy_history[0].text;
        return "";
      }
    }

    private string ExtraText
    {
      get
      {
        bool isMale = true;
        bool isGroup = true;
        if (this._fromUser != null)
        {
          isMale = this._fromUser.sex == 2;
          isGroup = false;
        }
        return WallPostItem.GetExtraText(this.IsProfilePhoto, this._wallPost.IsRepost(), isMale, isGroup).ToLowerInvariant();
      }
    }

    private string ExtraTextEnd
    {
      get
      {
        return WallPostItem.GetExtraTextEnd(this._wallPost.IsReply);
      }
    }

    private PostIconType IconType
    {
      get
      {
        if (this._wallPost.friends_only == 1)
          return PostIconType.Private;
        return this._wallPost.fixed_post == 1 ? PostIconType.Pinned : PostIconType.None;
      }
    }

    private PostSourcePlatform PostSourcePlatform
    {
      get
      {
        if (this._wallPost.post_source == null)
          return PostSourcePlatform.None;
        return this._wallPost.post_source.GetPlatform();
      }
    }

    private bool IsProfilePhoto
    {
      get
      {
        return WallPostItem.GetIsProfilePhoto(this._wallPost.attachments, this._wallPost.post_source);
      }
    }

    private double DividerHeight
    {
      get
      {
        if (!this._showDivideLine)
          return 0.0;
        return this._isFeedbackItem ? (this._showEarlierReplies ? 48.0 : 2.0) : 16.0;
      }
    }

    private string AdData
    {
      get
      {
        if (this._parentAdItem == null)
          return "";
        return this._parentAdItem.NewsItem.ads[0].ad_data;
      }
    }

    public LikesAndCommentsItem LikesAndCommentsItem { get; private set; }

    public bool CanShowLikesSeparator { get; private set; }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public bool CanDelete
    {
      get
      {
        return this._wallPost.CanDelete(this._groups, false);
      }
    }

    public bool CanPin
    {
      get
      {
        return this._wallPost.CanPin(this._groups);
      }
    }

    public bool CanUnpin
    {
      get
      {
        return this._wallPost.CanUnpin(this._groups);
      }
    }

    public bool CanReport
    {
      get
      {
        return this._wallPost.CanReport();
      }
    }

    public bool CanEdit
    {
      get
      {
        if (this._wallPost.CanEdit(this._groups))
          return !this.IsProfilePhoto;
        return false;
      }
    }

    public bool IsSuggested
    {
      get
      {
        return this._wallPost.IsSuggested;
      }
    }

    public bool IsPostponed
    {
      get
      {
        return this._wallPost.IsPostponed;
      }
    }

    public string NewsfeedItemId
    {
      get
      {
        return string.Format("post{0}_{1}", (object) this._wallPost.owner_id, (object) this._wallPost.id);
      }
    }

    public WallPostItem(double width, Thickness margin, bool preview, NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo, Action<WallPostItem> deletedItemCallback = null, bool suppressRepostButton = false, Action<long, User, Group> hideSourceItemsCallback = null, bool isFeedbackItem = false, bool showEarlierReplies = false, bool showDivideLine = true, bool showBackground = true, NewsFeedAdsItem parentAdItem = null, Func<List<MenuItem>> getExtraMenuItemsFunc = null)
      : base(width, margin, new Thickness())
    {
      WallPostItem.UpdateInstanceCount(true);
      this._parentAdItem = parentAdItem;
      this._getExtraMenuItemsFunc = getExtraMenuItemsFunc;
      this._isFeedbackItem = isFeedbackItem;
      this._showEarlierReplies = showEarlierReplies;
      if (wallPostWithInfo.NewsItem != null)
        this._wallPost = WallPostItem.GetWallPostFromNewsItem(wallPostWithInfo.NewsItem);
      if (wallPostWithInfo.WallPost != null)
        this._wallPost = wallPostWithInfo.WallPost;
      this._users = wallPostWithInfo.Profiles;
      this._groups = wallPostWithInfo.Groups;
      this._preview = preview;
      this.AdminLevel = 0;
      if (this.IsGroupPost)
        this._fromGroup = this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._wallPost.from_id)) ?? GroupsService.Current.GetCachedGroup(-this._wallPost.from_id) ?? new Group();
      else
        this._fromUser = this._users.FirstOrDefault<User>((Func<User, bool>) (p => p.uid == this._wallPost.from_id)) ?? new User();
      if (this._wallPost.to_id < 0L)
        this.AdminLevel = (this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._wallPost.to_id)) ?? GroupsService.Current.GetCachedGroup(-this._wallPost.to_id) ?? new Group()).admin_level;
      this._deletedItemCallback = deletedItemCallback;
      this.HideSourceItemsCallback = hideSourceItemsCallback;
      this._suppressRepostButton = suppressRepostButton;
      this._showDivideLine = showDivideLine;
      this._showBackground = showBackground;
      this.GenerateLayoutForWallPost();
    }

    ~WallPostItem()
    {
      WallPostItem.UpdateInstanceCount(false);
    }

    private static string GetExtraText(bool isProfilePhoto, bool isRepost, bool isMale, bool isGroup)
    {
      if (!isProfilePhoto || isRepost)
        return "";
      if (isGroup)
        return CommonResources.Photo_UpdatedProfileCommunity;
      if (!isMale)
        return CommonResources.Photo_UpdatedProfileFemale;
      return CommonResources.Photo_UpdatedProfileMale;
    }

    private static string GetExtraTextEnd(bool isReply)
    {
      if (!isReply)
        return "";
      return CommonResources.OnPost;
    }

    private static bool GetIsProfilePhoto(List<Attachment> attachments, PostSource postSource)
    {
      Photo photo;
      if (attachments == null)
      {
        photo = (Photo) null;
      }
      else
      {
        Attachment attachment = attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>) (a => a.type == "photo"));
        photo = attachment != null ? attachment.photo : (Photo) null;
      }
      if (photo == null)
        return false;
      PostSource postSource1 = postSource;
      return photo.IsProfilePhoto(postSource1);
    }

    private static void UpdateInstanceCount(bool increase)
    {
      if (increase)
        ++WallPostItem._instanceCount;
      else
        --WallPostItem._instanceCount;
    }

    public NewsFeedIgnoreItemData GetIgnoreItemData()
    {
      return new NewsFeedIgnoreItemData("wall", this.WallPost.to_id, this.WallPost.id);
    }

    private static WallPost GetWallPostFromNewsItem(NewsItem newsItem)
    {
      return WallPost.CreateFromNewsItem(newsItem);
    }

    private void View_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this._preview)
        return;
      this.NavigateToWallPostWithComments(false);
      e.Handled = true;
    }

    private void NavigateToWallPostWithComments(bool toComments = false)
    {
      long postId = this._wallPost.id;
      long toId = this._wallPost.to_id;
      if (!this._wallPost.IsReply)
        ParametersRepository.SetParameterForId("WallPost", (object) new NewsItemDataWithUsersAndGroupsInfo()
        {
          WallPost = this._wallPost,
          Profiles = this._users,
          Groups = this._groups
        });
      else if (this._wallPost.post_id != 0L)
        postId = this._wallPost.post_id;
      Navigator.Current.NavigateToWallPostComments(postId, toId, toComments, 0L, 0L, this.AdData);
    }

    private void GenerateLayoutForWallPost()
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      Thickness margin = new Thickness();
      Action action = null;
      if (this._preview)
        action = new Action(this.OnMoreOptionsTap);
      if (this._originalHeaderItem == null)
      {
        this._originalHeaderItem = new UserOrGroupHeaderItem(this.Width, new Thickness(), this.IsGroupPost, this._wallPost.date, this._fromUser, this._fromGroup, this.ExtraText, this.IconType, this.PostSourcePlatform, this._isFeedbackItem ? null : action, new Action(this.onNavigatedToUserOrGroup), this.ExtraTextEnd);
        this.VirtualizableChildren.Add((IVirtualizable) this._originalHeaderItem);
      }
      this._originalHeaderItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
      margin.Top += this._originalHeaderItem.FixedHeight + 8.0;
      if (!string.IsNullOrEmpty(this._wallPost.text))
      {
        if (this._originalTextItem == null)
        {
          this._originalTextItem = new NewsTextItem(this.Width - 32.0, new Thickness(16.0, 0.0, 16.0, 0.0), this._wallPost.text, this._preview, (Action) (() => this.NavigateToWallPostWithComments(false)), 21.3, new FontFamily("Segoe WP"), 28.0, (Brush) null, false, 0.0, HorizontalAlignment.Left, this._wallPost.PostId, TextAlignment.Left, true);
          this.VirtualizableChildren.Add((IVirtualizable) this._originalTextItem);
        }
        this._originalTextItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
        margin.Top += this._originalTextItem.FixedHeight + 8.0;
      }
      this.CanShowLikesSeparator = true;
      if ((!this._wallPost.attachments.IsNullOrEmpty() ? 1 : (this._wallPost.geo != null ? 1 : 0)) != 0)
      {
        if (this._originalAttachmentsItem == null)
        {
          this._originalAttachmentsItem = new AttachmentsItem(this.Width, new Thickness(), this._wallPost.attachments, this._wallPost.geo, this._wallPost.from_id == 0L || this._wallPost.id <= 0L ? "" : string.Format("{0}_{1}", (object) this._wallPost.from_id, (object) this._wallPost.id), this._wallPost.friends_only == 1, false, false, false, 0.0, false, !this._preview, this._wallPost.PostId);
          this.VirtualizableChildren.Add((IVirtualizable) this._originalAttachmentsItem);
        }
        this._originalAttachmentsItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
        margin.Top += this._originalAttachmentsItem.FixedHeight;
        this.CanShowLikesSeparator = !this._originalAttachmentsItem.IsLastAttachmentMedia;
      }
      if (this._wallPost.signer_id != 0L)
      {
        margin.Top += 8.0;
        if (this._originalSignerItem == null)
        {
          this._originalSignerItem = new LinkToUserOrGroupItem(this.Width, new Thickness(), new long?(this._wallPost.signer_id), this._users, this._groups, null);
          this.VirtualizableChildren.Add((IVirtualizable) this._originalSignerItem);
        }
        this._originalSignerItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
        margin.Top += this._originalSignerItem.FixedHeight;
        this.CanShowLikesSeparator = true;
      }
      if (!this._wallPost.copy_history.IsNullOrEmpty())
      {
        int index = 0;
        foreach (WallPost wallPost in this._wallPost.copy_history)
        {
          int currentInd = index;
          WallPostItem.WallPostHistoryVirtItems historyVirtItems = this._historyVirtItemsDict.ContainsKey(currentInd) ? this._historyVirtItemsDict[currentInd] : new WallPostItem.WallPostHistoryVirtItems();
          bool isMale;
          string pic;
          string name;
          this.GetNamePicAndSex(wallPost.from_id, out name, out pic, out isMale);
          int num1 = WallPostItem.GetIsProfilePhoto(wallPost.attachments, wallPost.post_source) ? 1 : 0;
          bool flag = wallPost.owner_id < 0L;
          int num2 = wallPost.IsRepost() ? 1 : 0;
          int num3 = isMale ? 1 : 0;
          int num4 = flag ? 1 : 0;
          string extraText = WallPostItem.GetExtraText(num1 != 0, num2 != 0, num3 != 0, num4 != 0);
          string extraTextEnd = WallPostItem.GetExtraTextEnd(wallPost.IsReply);
          PostSource postSource = wallPost.post_source;
          PostSourcePlatform postSourcePlatform = (PostSourcePlatform) (postSource != null ? (int) postSource.GetPlatform() : 0);
          string subtitle = "";
          if (!string.IsNullOrEmpty(extraText))
            subtitle = extraText.ToLowerInvariant();
          else if (wallPost.date != 0)
            subtitle = UIStringFormatterHelper.FormatDateTimeForUI(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double) wallPost.date, true));
          if (!string.IsNullOrEmpty(extraTextEnd))
            subtitle += string.Format(" {0}", (object) extraTextEnd);
          margin.Top += 8.0;
          if (historyVirtItems.HeaderItem == null)
          {
            historyVirtItems.HeaderItem = new UCItem(this.Width, new Thickness(), (Func<UserControlVirtualizable>) (() =>
            {
              RepostHeaderUC repostHeaderUc = new RepostHeaderUC();
              this._wallRepostInfo = new WallRepostInfo()
              {
                Pic = pic,
                Name = name,
                Subtitle = subtitle,
                PostSourcePlatform = postSourcePlatform,
                Width = this.Width - 16.0
              };
              WallRepostInfo configuration = this._wallRepostInfo;
              repostHeaderUc.Configure(configuration, (Action) (() => this.LinkToUserOrGroupTap(currentInd)));
              return (UserControlVirtualizable) repostHeaderUc;
            }), (Func<double>) (() => 56.0), (Action<UserControlVirtualizable>) null, 0.0, false);
            this.VirtualizableChildren.Add((IVirtualizable) historyVirtItems.HeaderItem);
          }
          historyVirtItems.HeaderItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
          margin.Top += historyVirtItems.HeaderItem.FixedHeight + 8.0;
          if (!string.IsNullOrWhiteSpace(wallPost.text))
          {
            if (historyVirtItems.TextItem == null)
            {
              historyVirtItems.TextItem = new NewsTextItem(this.Width - 32.0, new Thickness(16.0, 0.0, 16.0, 0.0), wallPost.text, this._preview, (Action) (() => this.NavigateToWallPostWithComments(false)), 21.3, new FontFamily("Segoe WP"), 28.0, (Brush) null, false, 0.0, HorizontalAlignment.Left, wallPost.PostId, TextAlignment.Left, true);
              this.VirtualizableChildren.Add((IVirtualizable) historyVirtItems.TextItem);
            }
            historyVirtItems.TextItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
            margin.Top += historyVirtItems.TextItem.FixedHeight + 8.0;
          }
          if ((!wallPost.attachments.IsNullOrEmpty() ? 1 : (wallPost.geo != null ? 1 : 0)) != 0)
          {
            if (historyVirtItems.AttachmentsItem == null)
            {
              string itemId = wallPost.from_id == 0L || wallPost.id <= 0L ? "" : string.Format("{0}_{1}", (object) wallPost.from_id, (object) wallPost.id);
              historyVirtItems.AttachmentsItem = new AttachmentsItem(this.Width, new Thickness(), wallPost.attachments, wallPost.geo, itemId, wallPost.friends_only == 1, false, false, false, 0.0, false, !this._preview, wallPost.PostId);
              this.VirtualizableChildren.Add((IVirtualizable) historyVirtItems.AttachmentsItem);
            }
            historyVirtItems.AttachmentsItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
            margin.Top += historyVirtItems.AttachmentsItem.FixedHeight;
            this.CanShowLikesSeparator = !historyVirtItems.AttachmentsItem.IsLastAttachmentMedia;
          }
          if (wallPost.signer_id != 0L)
          {
            margin.Top += 8.0;
            if (historyVirtItems.SignerItem == null)
            {
              historyVirtItems.SignerItem = new LinkToUserOrGroupItem(this.Width, new Thickness(), new long?(wallPost.signer_id), this._users, this._groups, null);
              this.VirtualizableChildren.Add((IVirtualizable) historyVirtItems.SignerItem);
            }
            historyVirtItems.SignerItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
            margin.Top += historyVirtItems.SignerItem.FixedHeight;
            this.CanShowLikesSeparator = true;
          }
          this._historyVirtItemsDict[index] = historyVirtItems;
          ++index;
        }
      }
      if (this._preview)
      {
        bool isReply = this._wallPost.IsReply;
        if (!this.IsSuggestedPostponed && !this._isFeedbackItem && !isReply)
        {
          if (this.CanShowLikesSeparator)
            margin.Top += 8.0;
          margin.Top = Math.Round(margin.Top);
          if (this.LikesAndCommentsItem == null)
          {
            this.LikesAndCommentsItem = new LikesAndCommentsItem(this.Width, new Thickness(), this._wallPost, (Action) (() => this.NavigateToWallPostWithComments(true)), this._suppressRepostButton, this.CanShowLikesSeparator);
            this.VirtualizableChildren.Add((IVirtualizable) this.LikesAndCommentsItem);
          }
          this.LikesAndCommentsItem.ViewMargin = new Thickness(0.0, margin.Top, 0.0, 0.0);
          margin.Top += this.LikesAndCommentsItem.FixedHeight;
        }
        else if (isReply)
          margin.Top += 8.0;
      }
      else
        margin.Top += 8.0;
      if (this._preview && this.IsSuggested && this._wallPost.can_publish == 1)
      {
        margin.Top += 8.0;
        if (this._publishRejectItem == null)
        {
          this._publishRejectItem = new UCItem(this.Width, margin, (Func<UserControlVirtualizable>) (() =>
          {
            PublicRejectUC publicRejectUc = new PublicRejectUC();
            publicRejectUc.buttonPublish.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.buttonPublish_Tap);
            publicRejectUc.buttonDelete.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.buttonDelete_Tap);
            return (UserControlVirtualizable) publicRejectUc;
          }), (Func<double>) (() => 60.0), (Action<UserControlVirtualizable>) null, 0.0, false);
          this.VirtualizableChildren.Add((IVirtualizable) this._publishRejectItem);
        }
        margin.Top += this._publishRejectItem.FixedHeight;
      }
      this._height = margin.Top;
      if (this._preview && this._showDivideLine)
        this._height = this._height + this.DividerHeight;
      stopwatch.Stop();
    }

    private void onNavigatedToUserOrGroup()
    {
      TransitionFromPostEvent transitionFromPostEvent = new TransitionFromPostEvent()
      {
        post_id = this._wallPost.PostId
      };
      if (this._wallPost.CopyPostIds.Any<string>())
        transitionFromPostEvent.parent_id = this._wallPost.CopyPostIds.First<string>();
      EventAggregator.Current.Publish((object) transitionFromPostEvent);
    }

    private void buttonDelete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.InitiateDelete();
      e.Handled = true;
    }

    private void buttonPublish_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this._wallPost.NavigateToPublishWallPost(this.AdminLevel);
      e.Handled = true;
    }

    private void LinkToUserOrGroupTap(int depthInd = 0)
    {
      if (!this._wallPost.copy_history.IsNullOrEmpty() && this._wallPost.copy_history.Count == depthInd + 1 && (this._wallPost.copy_history[depthInd].post_type == "photo" || this._wallPost.copy_history[depthInd].post_type == "video"))
        (this.VirtualizableChildren.LastOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (v => v is AttachmentsItem)) as AttachmentsItem).ProcessTapOnThumbsItem();
      else
        this.GoToOriginal(depthInd);
    }

    private void GetNamePicAndSex(long userOrGroupId, out string name, out string pic, out bool isMale)
    {
      name = "";
      pic = "";
      isMale = false;
      if (userOrGroupId > 0L)
      {
        User user = this._users.FirstOrDefault<User>((Func<User, bool>) (p => p.uid == userOrGroupId));
        if (user == null)
          return;
        name = user.Name;
        pic = user.photo_max;
        isMale = user.sex != 1;
      }
      else
      {
        Group group = this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -userOrGroupId));
        if (group == null || group.name == null)
          return;
        name = group.name;
        pic = group.photo_200;
        isMale = true;
      }
    }

    public List<MenuItem> GenerateMenuItems()
    {
      List<MenuItem> menuItemList = new List<MenuItem>();
      if (this.CanEdit)
      {
        MenuItem menuItem1 = new MenuItem();
        string edit = CommonResources.Edit;
        menuItem1.Header = (object) edit;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.editMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (this._wallPost.can_publish == 1 || this.IsPostponed)
      {
        MenuItem menuItem = new MenuItem();
        if (this.IsPostponed)
        {
          menuItem.Click += new RoutedEventHandler(this.publishNowMenuItem_Click);
          menuItem.Header = (object) CommonResources.PublishNow;
        }
        else
        {
          menuItem.Click += new RoutedEventHandler(this.publishMenuItem_Click);
          menuItem.Header = (object) CommonResources.SuggestedNews_Publish;
        }
        menuItemList.Add(menuItem);
      }
      if (this.AllowPinUnpin && this.CanPin)
      {
        MenuItem menuItem1 = new MenuItem();
        string pinPost = CommonResources.PinPost;
        menuItem1.Header = (object) pinPost;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.pinMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (this.AllowPinUnpin && this.CanUnpin)
      {
        MenuItem menuItem1 = new MenuItem();
        string unpinPost = CommonResources.UnpinPost;
        menuItem1.Header = (object) unpinPost;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.unpinMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (this.CanDelete)
      {
        MenuItem menuItem1 = new MenuItem();
        string delete = CommonResources.Delete;
        menuItem1.Header = (object) delete;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this._deleteMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (!string.IsNullOrWhiteSpace(this.TextToCopy))
      {
        MenuItem menuItem1 = new MenuItem();
        string conversationCopy = CommonResources.Conversation_Copy;
        menuItem1.Header = (object) conversationCopy;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.copyMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (!this._wallPost.IsReply)
      {
        MenuItem menuItem1 = new MenuItem();
        string copyLink = CommonResources.CopyLink;
        menuItem1.Header = (object) copyLink;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.CopyLinkMI_OnClick);
        menuItemList.Add(menuItem2);
      }
      if (this.WallPost.CanGoToOriginal())
      {
        MenuItem menuItem1 = new MenuItem();
        string goToOriginal = CommonResources.GoToOriginal;
        menuItem1.Header = (object) goToOriginal;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.goToOriginal_Click);
        menuItemList.Add(menuItem2);
      }
      if (this.IgnoreNewsfeedItemCallback != null && this.GetIgnoreItemData() != null)
      {
        MenuItem menuItem1 = new MenuItem();
        string lowerInvariant = CommonResources.HideThisPost.ToLowerInvariant();
        menuItem1.Header = (object) lowerInvariant;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.HidePostItem_OnClick);
        menuItemList.Add(menuItem2);
      }
      if (!this.CanDelete && this.HideSourceItemsCallback != null)
      {
        MenuItem menuItem1 = new MenuItem();
        string hideFromNews = CommonResources.HideFromNews;
        menuItem1.Header = (object) hideFromNews;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.HideFromNewsMenuItem_OnClick);
        menuItemList.Add(menuItem2);
      }
      if (this.CanReport)
      {
        MenuItem menuItem1 = new MenuItem();
        string report = CommonResources.Report;
        menuItem1.Header = (object) report;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler(this.reportMenuItem_Click);
        menuItemList.Add(menuItem2);
      }
      if (this._getExtraMenuItemsFunc != null)
        menuItemList.AddRange((IEnumerable<MenuItem>) this._getExtraMenuItemsFunc());
      return menuItemList;
    }

    private void HidePostItem_OnClick(object sender, RoutedEventArgs e)
    {
      this.IgnoreNewsfeedItemCallback(this.GetIgnoreItemData());
    }

    private void publishMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this._wallPost.NavigateToPublishWallPost(this.AdminLevel);
    }

    private void publishNowMenuItem_Click(object sender, RoutedEventArgs e)
    {
      WallPostViewModel wallPostViewModel = new WallPostViewModel(this._wallPost, this.AdminLevel, (WallRepostInfo) null);
      wallPostViewModel.WMMode = WallPostViewModel.Mode.PublishWallPost;
      wallPostViewModel.IsPublishSuggestedSuppressed = true;
      IOutboundAttachment timerAttachment = wallPostViewModel.OutboundAttachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>) (a => a.AttachmentId == "timestamp"));
      if (timerAttachment != null)
        wallPostViewModel.OutboundAttachments.Remove(timerAttachment);
      wallPostViewModel.Publish((Action<ResultCode>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (res == ResultCode.Succeeded)
        {
          if (this._wallPost.IsFromGroup())
          {
            long groupId = -this._wallPost.owner_id;
            Group group = this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == groupId));
            if (group == null)
              return;
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, groupId, group.name);
          }
          else if (this._wallPost.owner_id >= 0L)
          {
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, 0L, "");
          }
          else
          {
            long communityId = -this._wallPost.owner_id;
            Group group = this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == communityId));
            if (group == null)
              return;
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, communityId, group.name);
          }
        }
        else if (res == ResultCode.PostsLimitOrAlreadyScheduled)
        {
          if (timerAttachment != null)
            new GenericInfoUC(2000).ShowAndHideLater(CommonResources.ScheduledForExistingTime, null);
          else
            new GenericInfoUC(2000).ShowAndHideLater(CommonResources.PostsLimitReached, null);
        }
        else
          new GenericInfoUC(2000).ShowAndHideLater(CommonResources.Error, null);
      }))));
    }

    private void CreateMenu()
    {
      List<MenuItem> menuItems = this.GenerateMenuItems();
      if (this._preview)
        this._originalHeaderItem.SetMenu(menuItems);
      else
        this.SetMenu(menuItems);
    }

    private void unpinMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.PinUnpin();
    }

    private void pinMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.PinUnpin();
    }

    private void PinUnpin()
    {
      if (this._isPinning)
        return;
      this._isPinning = true;
      this._wallPost.PinUnpin((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this._isPinning = false;
        this.ReleaseMenu();
        this.CreateMenu();
      }))));
    }

    private void reportMenuItem_Click(object sender, RoutedEventArgs e)
    {
      ReportContentHelper.ReportWallPost(this._wallPost, this.AdData);
    }

    private void copyMenuItem_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(this.TextToCopy);
    }

    private void goToOriginal_Click(object sender, RoutedEventArgs e)
    {
      this.GoToOriginal(0);
    }

    private void GoToOriginal(int depthInd = 0)
    {
      if (this.WallPost.copy_history.IsNullOrEmpty() || this.WallPost.copy_history.Count <= depthInd)
        return;
      Navigator.Current.NavigateToWallPostComments(this.WallPost.copy_history[depthInd].WallPostOrReplyPostId, this.WallPost.copy_history[depthInd].owner_id, false, 0L, 0L, "");
    }

    private void CopyLinkMI_OnClick(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(string.Format("http://vk.com/wall{0}_{1}", (object) this.WallPost.to_id, (object) this.WallPost.id));
    }

    private void HideFromNewsMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
      this.HideSourceItemsCallback(this.WallPost.to_id, this._fromUser, this._fromGroup);
    }

    private void editMenuItem_Click(object sender, RoutedEventArgs e)
    {
      ParametersRepository.SetParameterForId("WallRepostInfo", (object) this._wallRepostInfo);
      this._wallPost.NavigateToEditWallPost(this.AdminLevel);
    }

    protected override void NotifyAboutImpression()
    {
      base.NotifyAboutImpression();
      this.TrackStats();
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this.CreateMenu();
      this.HookUpEvents();
      if (this._showBackground)
      {
        Rectangle rect = new Rectangle();
        double fixedHeight = this.FixedHeight;
        rect.Height = fixedHeight;
        Thickness thickness = new Thickness();
        rect.Margin = thickness;
        double num = 480.0;
        rect.Width = num;
        SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneNewsBackgroundBrush"];
        rect.Fill = (Brush) solidColorBrush;
        foreach (FrameworkElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
          this.Children.Add(coverByRectangle);
      }
      if (!this._preview || !this._showDivideLine)
        return;
      Rectangle rectangle = new Rectangle();
      SolidColorBrush solidColorBrush1 = this._isFeedbackItem ? Application.Current.Resources["PhoneTableSeparatorBrush"] as SolidColorBrush : Application.Current.Resources["PhoneNewsDividerBrush"] as SolidColorBrush;
      rectangle.Fill = (Brush) solidColorBrush1;
      double num1 = 480.0;
      rectangle.Width = num1;
      double dividerHeight = this.DividerHeight;
      rectangle.Height = dividerHeight;
      Thickness thickness1 = new Thickness(0.0, this.FixedHeight - this.DividerHeight, 0.0, 0.0);
      rectangle.Margin = thickness1;
      this.Children.Add((FrameworkElement) rectangle);
      if (!this._showEarlierReplies)
        return;
      EarlierRepliesUC earlierRepliesUc = new EarlierRepliesUC();
      Thickness thickness2 = new Thickness(0.0, this.FixedHeight - this.DividerHeight, 0.0, 0.0);
      earlierRepliesUc.Margin = thickness2;
      this.Children.Add((FrameworkElement) earlierRepliesUc);
    }

    private void TrackStats()
    {
      if (this._wallPost == null || this.Parent == null)
        return;
      EventAggregator.Current.Publish((object) new ViewPostEvent()
      {
        PostId = this.WallPost.PostId,
        CopyPostIds = this.WallPost.CopyPostIds,
        ItemType = NewsFeedItemType.WallPost,
        Position = this.Parent.VirtualizableItems.IndexOf((IVirtualizable) this)
      });
    }

    private void HookUpEvents()
    {
      this._view.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.View_OnTap);
    }

    private void UnHookEvents()
    {
      this._view.Tap -= new EventHandler<System.Windows.Input.GestureEventArgs>(this.View_OnTap);
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this.ReleaseMenu();
      this.UnHookEvents();
    }

    private void ReleaseMenu()
    {
      this.ResetMenu();
      this._originalHeaderItem.ResetMenu();
    }

    private void _deleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.InitiateDelete();
    }

    private void InitiateDelete()
    {
      if (!this._wallPost.AskConfirmationAndDelete() || this._deletedItemCallback == null)
        return;
      this._deletedItemCallback(this);
    }

    public string GetKey()
    {
      if (this._wallPost.id != 0L)
        return this._wallPost.id.ToString();
      return "";
    }

    public void OnMoreOptionsTap()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this._originalHeaderItem.View);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    public void RespondToChildHeightChange(IVirtualizable child)
    {
      this.GenerateLayoutForWallPost();
      this.RegenerateChildren();
      this.NotifyHeightChanged();
    }

    private class WallPostHistoryVirtItems
    {
      public UCItem HeaderItem { get; set; }

      public NewsTextItem TextItem { get; set; }

      public AttachmentsItem AttachmentsItem { get; set; }

      public LinkToUserOrGroupItem SignerItem { get; set; }
    }
  }
}
