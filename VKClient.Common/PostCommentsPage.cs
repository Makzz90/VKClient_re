using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.BLExtensions;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Common
{
  public class PostCommentsPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, IHandle<WallPostPinnedUnpinned>, ISupportShare
  {
    private bool _isInitialized;
    private SharePostUC _sharePostUC;
    private readonly PhotoChooserTask _photoChooserTask;
    private readonly ViewportScrollableAreaAdapter _adapter;
    private WallPostViewModel _commentVM;
    private readonly ApplicationBarMenuItem _appBarMenuItemGoToOriginal;
    private readonly ApplicationBarMenuItem _appBarMenuItemPin;
    private readonly ApplicationBarMenuItem _appBarMenuItemUnpin;
    private readonly ApplicationBarIconButton _appBarButtonSend;
    private readonly ApplicationBarIconButton _appBarButtonLike;
    private readonly ApplicationBarIconButton _appBarButtonUnlike;
    private readonly ApplicationBarIconButton _appBarButtonEmojiToggle;
    private readonly ApplicationBarMenuItem _appBarMenuItemShare;
    private readonly ApplicationBarIconButton _appBarButtonAttachments;
    private readonly ApplicationBarMenuItem _appBarMenuItemRefresh;
    private readonly ApplicationBarMenuItem _appBarMenuItemEdit;
    private readonly ApplicationBarMenuItem _appBarMenuItemDelete;
    private readonly ApplicationBarMenuItem _appBarMenuItemReport;
    private ApplicationBar _appBar;
    private DialogService _dc;
    private bool _addingComment;
    private bool _focusedComments;
    private long _replyToCid;
    private long _replyToUid;
    private string _replyAutoForm;
    private DelayedExecutor _de;
    private DelayedExecutor _de2;
    internal Grid LayoutRoot;
    internal ViewportControl scroll;
    internal StackPanel scrollableGrid;
    internal MyVirtualizingPanel2 panel;
    internal NewMessageUC ucNewMessage;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal MoreActionsUC ucMoreActions;
    private bool _contentLoaded;

    private PostCommentsViewModel PostCommentsVM
    {
      get
      {
        return base.DataContext as PostCommentsViewModel;
      }
    }

    private ReplyUserUC ReplyUserUC
    {
      get
      {
        return this.ucNewMessage.ReplyUserUC;
      }
    }

    public TextBox textBoxNewMessage
    {
      get
      {
        return this.ucNewMessage.ucNewPost.textBoxPost;
      }
    }

    private bool FocusComments
    {
      get
      {
        if (((Page) this).NavigationContext.QueryString.ContainsKey("FocusComments"))
          return ((Page) this).NavigationContext.QueryString["FocusComments"] == bool.TrueString;
        return false;
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
        this.ucNewMessage.SetReplyAutoForm(value);
      }
    }

    public bool ReadyToSend
    {
      get
      {
        string text = this.textBoxNewMessage.Text;
        ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
        if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
          return true;
        if (outboundAttachments.Count > 0)
            return Enumerable.All<IOutboundAttachment>(outboundAttachments, (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
        return false;
      }
    }

    public PostCommentsPage()
    {
      PhotoChooserTask photoChooserTask = new PhotoChooserTask();
      int num = 1;
      photoChooserTask.ShowCamera = (num != 0);
      this._photoChooserTask = photoChooserTask;
      ApplicationBarMenuItem applicationBarMenuItem1 = new ApplicationBarMenuItem();
      string goToOriginal = CommonResources.GoToOriginal;
      applicationBarMenuItem1.Text = goToOriginal;
      this._appBarMenuItemGoToOriginal = applicationBarMenuItem1;
      ApplicationBarMenuItem applicationBarMenuItem2 = new ApplicationBarMenuItem();
      string pinPost = CommonResources.PinPost;
      applicationBarMenuItem2.Text = pinPost;
      this._appBarMenuItemPin = applicationBarMenuItem2;
      ApplicationBarMenuItem applicationBarMenuItem3 = new ApplicationBarMenuItem();
      string unpinPost = CommonResources.UnpinPost;
      applicationBarMenuItem3.Text = unpinPost;
      this._appBarMenuItemUnpin = applicationBarMenuItem3;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string commentsPageAppBarSend = CommonResources.PostCommentsPage_AppBar_Send;
      applicationBarIconButton1.Text = commentsPageAppBarSend;
      this._appBarButtonSend = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("Resources/appbar.heart2.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string commentsPageAppBarLike = CommonResources.PostCommentsPage_AppBar_Like;
      applicationBarIconButton2.Text = commentsPageAppBarLike;
      this._appBarButtonLike = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("Resources/appbar.heart2.broken.rest.png", UriKind.Relative);
      applicationBarIconButton3.IconUri = uri3;
      string pageAppBarUnlike = CommonResources.PostCommentsPage_AppBar_Unlike;
      applicationBarIconButton3.Text = pageAppBarUnlike;
      this._appBarButtonUnlike = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri("Resources/appbar.smile.png", UriKind.Relative);
      applicationBarIconButton4.IconUri = uri4;
      string str = "emoji";
      applicationBarIconButton4.Text = str;
      this._appBarButtonEmojiToggle = applicationBarIconButton4;
      ApplicationBarMenuItem applicationBarMenuItem4 = new ApplicationBarMenuItem();
      string commentsPageAppBarShare = CommonResources.PostCommentsPage_AppBar_Share;
      applicationBarMenuItem4.Text = commentsPageAppBarShare;
      this._appBarMenuItemShare = applicationBarMenuItem4;
      ApplicationBarIconButton applicationBarIconButton5 = new ApplicationBarIconButton();
      Uri uri5 = new Uri("Resources/attach.png", UriKind.Relative);
      applicationBarIconButton5.IconUri = uri5;
      string barAddAttachment = CommonResources.NewPost_AppBar_AddAttachment;
      applicationBarIconButton5.Text = barAddAttachment;
      this._appBarButtonAttachments = applicationBarIconButton5;
      ApplicationBarMenuItem applicationBarMenuItem5 = new ApplicationBarMenuItem();
      string pageAppBarRefresh = CommonResources.PostCommentsPage_AppBar_Refresh;
      applicationBarMenuItem5.Text = pageAppBarRefresh;
      this._appBarMenuItemRefresh = applicationBarMenuItem5;
      ApplicationBarMenuItem applicationBarMenuItem6 = new ApplicationBarMenuItem();
      string edit = CommonResources.Edit;
      applicationBarMenuItem6.Text = edit;
      this._appBarMenuItemEdit = applicationBarMenuItem6;
      ApplicationBarMenuItem applicationBarMenuItem7 = new ApplicationBarMenuItem();
      string delete = CommonResources.Delete;
      applicationBarMenuItem7.Text = delete;
      this._appBarMenuItemDelete = applicationBarMenuItem7;
      ApplicationBarMenuItem applicationBarMenuItem8 = new ApplicationBarMenuItem();
      string report = CommonResources.Report;
      applicationBarMenuItem8.Text = report;
      this._appBarMenuItemReport = applicationBarMenuItem8;
      this._dc = new DialogService();
      this._de = new DelayedExecutor(200);
      this._de2 = new DelayedExecutor(550);
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.Header.OnHeaderTap = (Action) (() => this.panel.ScrollToBottom(false));
      this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
      this.panel.InitializeWithScrollViewer((IScrollableArea) this._adapter, false);
      TextBoxPanelControl panelControl = this.ucNewMessage.PanelControl;
      EventHandler<bool> eventHandler = (EventHandler<bool>) Delegate.Combine((Delegate) panelControl.IsOpenedChanged, (Delegate) new EventHandler<bool>(this.PanelIsOpenedChanged));
      panelControl.IsOpenedChanged = eventHandler;
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panel);
      this.panel.LoadedHeightDownwards = this.panel.LoadedHeightDownwardsNotScrolling = 1600.0;
      this.BuildAppBar();
      ((UIElement) this.ucNewMessage.ReplyUserUC).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.Button_Click_1));
      this.ucMoreActions.SetBlue();
      this.ucMoreActions.TapCallback = (Action) (() =>
      {
        WallPostItem wallPostItem = this.PostCommentsVM.WallPostItem;
        if (wallPostItem == null)
          return;
        ContextMenu menu = ContextMenuHelper.CreateMenu(wallPostItem.GenerateMenuItems());
        ContextMenuService.SetContextMenu((DependencyObject) this.ucMoreActions, menu);
        menu.IsOpen = true;
      });
      this.ucNewMessage.OnAddAttachTap = new Action(this.AddAttachTap);
      this.ucNewMessage.OnSendTap = (Action) (() => this._appBarButtonSend_Click(null,  null));
      this.ucNewMessage.UCNewPost.OnImageDeleteTap = (Action<object>) (sender =>
      {
        FrameworkElement frameworkElement = sender as FrameworkElement;
        if (frameworkElement != null)
          this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
        this.UpdateAppBar();
      });
      this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
      // ISSUE: method pointer
      this.ucNewMessage.TextBoxNewComment.TextChanged += (new TextChangedEventHandler( this.TextBoxNewComment_TextChanged));
      Binding binding = new Binding("OutboundAttachments");
      ((FrameworkElement) this.ucNewMessage.ucNewPost.ItemsControlAttachments).SetBinding((DependencyProperty) ItemsControl.ItemsSourceProperty, binding);
      this.scroll.BindViewportBoundsTo((FrameworkElement) this.scrollableGrid);
      ((ChooserBase<PhotoResult>) this._photoChooserTask).Completed += (new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
      EventAggregator.Current.Subscribe(this);
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void PanelIsOpenedChanged(object sender, bool e)
    {
      if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
        this.panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
      else
        this.panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
    }

    private void AddAttachTap()
    {
      AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action) (() =>
      {
        PostCommentsPage.HandleInputParams(this._commentVM);
        this.UpdateAppBar();
      }), true, 0, 0,  null);
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
        if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", e.ChosenPhoto);
    }

    private void BuildAppBar()
    {
      this._appBarButtonSend.Click+=(new EventHandler(this._appBarButtonSend_Click));
      this._appBarButtonAttachments.Click+=(new EventHandler(this._appBarButtonAttachments_Click));
      this._appBarButtonEmojiToggle.Click+=(new EventHandler(this._appBarButtonEmojiToggle_Click));
      this._appBarButtonLike.Click+=(new EventHandler(this._appBarButtonLike_Click));
      this._appBarMenuItemShare.Click+=(new EventHandler(this._appBarButtonShare_Click));
      this._appBarButtonUnlike.Click+=(new EventHandler(this._appBarButtonUnlike_Click));
      this._appBarMenuItemRefresh.Click+=(new EventHandler(this._appBarMenuItemRefresh_Click));
      this._appBarMenuItemGoToOriginal.Click+=(new EventHandler(this._appBarMenuItemGoToOriginal_Click));
      this._appBarMenuItemEdit.Click+=(new EventHandler(this._appBarMenuItemEdit_Click));
      this._appBarMenuItemDelete.Click+=(new EventHandler(this._appBarMenuItemDelete_Click));
      this._appBarMenuItemReport.Click+=(new EventHandler(this._appBarMenuItemReport_Click));
      ApplicationBar applicationBar = new ApplicationBar();
      applicationBar.BackgroundColor = VKConstants.AppBarBGColor;
      applicationBar.ForegroundColor = VKConstants.AppBarFGColor;
      this._appBar = applicationBar;
      this._appBar.StateChanged += (new EventHandler<ApplicationBarStateChangedEventArgs>(this._appBar_StateChanged));
      this._appBar.Buttons.Add(this._appBarButtonSend);
      this._appBar.Buttons.Add(this._appBarButtonEmojiToggle);
      this._appBar.Buttons.Add(this._appBarButtonAttachments);
      this._appBar.Buttons.Add(this._appBarButtonLike);
      this._appBar.MenuItems.Add(this._appBarMenuItemShare);
      this._appBar.MenuItems.Add(this._appBarMenuItemRefresh);
      this._appBarMenuItemPin.Click+=(new EventHandler(this._appBarMenuItemPin_Click));
      this._appBarMenuItemUnpin.Click+=(new EventHandler(this._appBarMenuItemUnpin_Click));
      this._appBar.Opacity = 0.9;
    }

    private void _appBarMenuItemUnpin_Click(object sender, EventArgs e)
    {
      this.PinUnpin();
    }

    private void _appBarMenuItemPin_Click(object sender, EventArgs e)
    {
      this.PinUnpin();
    }

    private void PinUnpin()
    {
      this.PostCommentsVM.PinUnpin((Action<bool>) (res => {}));
    }

    private void _appBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
    {
    }

    private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
    {
    }

    private void _appBarMenuItemReport_Click(object sender, EventArgs e)
    {
      if (this.PostCommentsVM.WallPost == null)
        return;
      ReportContentHelper.ReportWallPost(this.PostCommentsVM.WallPost, "");
    }

    private void _appBarMenuItemDelete_Click(object sender, EventArgs e)
    {
    }

    private void _appBarMenuItemEdit_Click(object sender, EventArgs e)
    {
      if (this.PostCommentsVM.WallPostData == null || this.PostCommentsVM.WallPostData.WallPost == null)
        return;
      this.PostCommentsVM.WallPostData.WallPost.NavigateToEditWallPost(this.PostCommentsVM.WallPostItem == null ? 3 : this.PostCommentsVM.WallPostItem.AdminLevel);
    }

    private void _appBarButtonAttachments_Click(object sender, EventArgs e)
    {
    }

    private void _appBarMenuItemGoToOriginal_Click(object sender, EventArgs e)
    {
      if (this.PostCommentsVM.WallPost == null || this.PostCommentsVM.WallPost.copy_history.IsNullOrEmpty())
        return;
      Navigator.Current.NavigateToWallPostComments(this.PostCommentsVM.WallPost.copy_history[0].WallPostOrReplyPostId, this.PostCommentsVM.WallPost.copy_history[0].owner_id, false, this.PostCommentsVM.PollId, this.PostCommentsVM.PollOwnerId, "");
    }

    private void _appBarMenuItemRefresh_Click(object sender, EventArgs e)
    {
      this.Refresh();
    }

    private void Refresh()
    {
      ((UIElement) this.ucNewMessage).Opacity = 0.0;
      this.PostCommentsVM.Refresh();
    }

    private void _appBarButtonUnlike_Click(object sender, EventArgs e)
    {
      this.PostCommentsVM.Unlike();
      this.UpdateAppBar();
    }

    private void _appBarButtonShare_Click(object sender, EventArgs e)
    {
      ((Control) this).Focus();
      this._dc = new DialogService();
      this._dc.SetStatusBarBackground = true;
      this._dc.HideOnNavigation = false;
      this._sharePostUC = new SharePostUC(-this.PostCommentsVM.OwnerId);
      this._sharePostUC.SetShareEnabled(this.PostCommentsVM.CanRepost);
      this._sharePostUC.SetShareCommunityEnabled(this.PostCommentsVM.CanRepostCommunity);
      this._sharePostUC.SendTap += new EventHandler(this.buttonSend_Click);
      this._sharePostUC.ShareTap += new EventHandler(this.buttonShare_Click);
      this._dc.Child = (FrameworkElement) this._sharePostUC;
      this._dc.AnimationType = DialogService.AnimationTypes.None;
      this._dc.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
      this._dc.Show((UIElement) this.scroll);
    }

    private void buttonShare_Click(object sender, EventArgs eventArgs)
    {
      this.Share(0, "");
    }

    private void Share(long gid = 0, string groupName = "")
    {
      this.PostCommentsVM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
      this.UpdateAppBar();
      this._dc.Hide();
    }

    private void buttonSend_Click(object sender, EventArgs eventArgs)
    {
      ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
      contentDataProvider.Message = this._sharePostUC.Text;
      contentDataProvider.WallPost = this.PostCommentsVM.WallPost;
      contentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
      this._dc.Hide();
      Navigator.Current.NavigateToPickConversation();
    }

    private void _appBarButtonLike_Click(object sender, EventArgs e)
    {
      this.PostCommentsVM.Like();
      this.UpdateAppBar();
    }

    private void _appBarButtonSend_Click(object sender, EventArgs e)
    {
      string str1 = this.textBoxNewMessage.Text;
      if (this.ReplyAutoForm != null && str1.StartsWith(this.ReplyAutoForm))
      {
        string str2 = this.ReplyAutoForm.Remove(this.ReplyAutoForm.IndexOf(", "));
        string str3 = this._replyToUid > 0L ? "id" : "club";
        long num = this._replyToUid > 0L ? this._replyToUid : -this.PostCommentsVM.WallPostData.WallPost.owner_id;
        str1 = str1.Remove(0, this.ReplyAutoForm.Length).Insert(0, string.Format("[{0}{1}|{2}], ", str3, num, str2));
      }
      string text = str1.Replace("\r\n", "\r").Replace("\r", "\r\n");
      if (!this.PostCommentsVM.CanPostComment(text, (List<IOutboundAttachment>) Enumerable.ToList<IOutboundAttachment>(this._commentVM.OutboundAttachments),  null))
        return;
      bool fromGroupChecked = this.ucNewMessage.FromGroupChecked;
      if (this._addingComment)
        return;
      this._addingComment = true;
      this.PostCommentsVM.PostComment(text, this._replyToCid, this._replyToUid, fromGroupChecked, (List<IOutboundAttachment>) Enumerable.ToList<IOutboundAttachment>(this._commentVM.OutboundAttachments), (Action<bool>) (result =>
      {
        this._addingComment = false;
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (!result)
          {
            ExtendedMessageBox.ShowSafe(CommonResources.Error);
          }
          else
          {
            this.textBoxNewMessage.Text = ("");
            this.InitializeCommentVM();
            this.UpdateAppBar();
            this.ScrollToBottom();
            this.textBoxNewMessage.Text = ("");
            this.ResetReplyFields();
          }
        }));
      }),  null, "");
    }

    private void ScrollToBottom()
    {
      Execute.ExecuteOnUIThread((Action) (() => this.panel.ScrollToBottom(true)));
    }

    private void UpdateAppBar()
    {
      this._appBarButtonSend.IsEnabled = (this.PostCommentsVM.CanComment && this.ReadyToSend);
      this.ucNewMessage.UpdateSendButton(this._appBarButtonSend.IsEnabled);
      ApplicationBarIconButton appBarButtonLike = this._appBarButtonLike;
      bool canLike;
      this._appBarButtonUnlike.IsEnabled = (canLike = this.PostCommentsVM.CanLike);
      int num = canLike ? 1 : 0;
      appBarButtonLike.IsEnabled = (num != 0);
      this._appBarButtonEmojiToggle.IsEnabled = this.PostCommentsVM.CanComment;
      this._appBarMenuItemShare.IsEnabled = true;
      this._appBarButtonAttachments.IsEnabled = this.PostCommentsVM.CanComment;
      int count = this._commentVM.OutboundAttachments.Count;
      if (count > 0)
        this._appBarButtonAttachments.IconUri=(new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", Math.Min(count, 10)), UriKind.Relative));
      else
        this._appBarButtonAttachments.IconUri=(new Uri("Resources/attach.png", UriKind.Relative));
      if (this.PostCommentsVM.Liked)
      {
        this._appBar.Buttons.Remove(this._appBarButtonLike);
        if (!this._appBar.Buttons.Contains(this._appBarButtonUnlike))
          this._appBar.Buttons.Insert(3, this._appBarButtonUnlike);
      }
      else
      {
        this._appBar.Buttons.Remove(this._appBarButtonUnlike);
        if (!this._appBar.Buttons.Contains(this._appBarButtonLike))
          this._appBar.Buttons.Insert(3, this._appBarButtonLike);
      }
      if (this.PostCommentsVM.WallPost != null && this.PostCommentsVM.WallPost.CanGoToOriginal() && !this._appBar.MenuItems.Contains(this._appBarMenuItemGoToOriginal))
        this._appBar.MenuItems.Add(this._appBarMenuItemGoToOriginal);
      if (this.PostCommentsVM.WallPostData != null && this.PostCommentsVM.WallPostData.WallPost != null && (this.PostCommentsVM.WallPostData.WallPost.CanEdit(this.PostCommentsVM.WallPostData.Groups) && !this._appBar.MenuItems.Contains(this._appBarMenuItemEdit)))
        this._appBar.MenuItems.Add(this._appBarMenuItemEdit);
      if (this.PostCommentsVM.WallPostData != null && this.PostCommentsVM.WallPostData.WallPost != null && (this.PostCommentsVM.WallPostData.WallPost.CanDelete(this.PostCommentsVM.WallPostData.Groups, false) && !this._appBar.MenuItems.Contains(this._appBarMenuItemDelete)))
        this._appBar.MenuItems.Add(this._appBarMenuItemDelete);
      if (this.PostCommentsVM.WallPost != null && this.PostCommentsVM.WallPost.CanReport() && !this._appBar.MenuItems.Contains(this._appBarMenuItemReport))
        this._appBar.MenuItems.Add(this._appBarMenuItemReport);
      if (this.PostCommentsVM.CanPin && !this._appBar.MenuItems.Contains(this._appBarMenuItemPin))
        this._appBar.MenuItems.Insert(0, this._appBarMenuItemPin);
      if (!this.PostCommentsVM.CanPin && this._appBar.MenuItems.Contains(this._appBarMenuItemPin))
        this._appBar.MenuItems.Remove(this._appBarMenuItemPin);
      if (this.PostCommentsVM.CanUnpin && !this._appBar.MenuItems.Contains(this._appBarMenuItemUnpin))
        this._appBar.MenuItems.Insert(0, this._appBarMenuItemUnpin);
      if (this.PostCommentsVM.CanUnpin || !this._appBar.MenuItems.Contains(this._appBarMenuItemUnpin))
        return;
      this._appBar.MenuItems.Remove(this._appBarMenuItemUnpin);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      bool flag = true;
      if (!this._isInitialized)
      {
        ((UIElement) this.ucNewMessage).Opacity = 0.0;
        NewsItemDataWithUsersAndGroupsInfo parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("WallPost") as NewsItemDataWithUsersAndGroupsInfo;
        long num1 = long.Parse(((Page) this).NavigationContext.QueryString["PollId"]);
        long num2 = long.Parse(((Page) this).NavigationContext.QueryString["PollOwnerId"]);
        if (this.FocusComments)
          this.panel.OnlyPartialLoad = true;
        long postId = this.CommonParameters.PostId;
        long ownerId = this.CommonParameters.OwnerId;
        MyVirtualizingPanel2 panel = this.panel;
        Action loadedCallback = new Action(this.ViewModelIsLoaded);
        Action<CommentItem> replyCommentAction = new Action<CommentItem>(this.ReplyToComment);
        long knownPollId = num1;
        long knownPollOwnerId = num2;
        PostCommentsViewModel commentsViewModel = new PostCommentsViewModel(parameterForIdAndReset, postId, ownerId, panel, loadedCallback, replyCommentAction, knownPollId, knownPollOwnerId);
        this.InitializeCommentVM();
        base.DataContext = commentsViewModel;
        commentsViewModel.LoadMoreCommentsInUI();
        this.UpdateAppBar();
        this.RestoreUnboundState();
        this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panel);
        this.panel.OnRefresh = new Action(this.Refresh);
        this._isInitialized = true;
        flag = false;
      }
      if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
        WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
      this.ProcessInputData();
      PostCommentsPage.HandleInputParams(this._commentVM);
      this.UpdateAppBar();
    }

    public static void HandleInputParams(WallPostViewModel wallPostVM)
    {
      GeoCoordinate parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("NewPositionToBeAttached") as GeoCoordinate;
      if (parameterForIdAndReset1 !=  null)
      {
        OutboundGeoAttachment outboundGeoAttachment = new OutboundGeoAttachment(parameterForIdAndReset1.Latitude, parameterForIdAndReset1.Longitude);
        wallPostVM.AddAttachment((IOutboundAttachment) outboundGeoAttachment);
      }
      List<Stream> parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      List<Stream> parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
      if (parameterForIdAndReset2 != null)
      {
        for (int index = 0; index < parameterForIdAndReset2.Count; ++index)
        {
          Stream stream1 = parameterForIdAndReset2[index];
          Stream stream2 = parameterForIdAndReset3[index];
          long userOrGroupId = wallPostVM.UserOrGroupId;
          int num1 = wallPostVM.IsGroup ? 1 : 0;
          Stream previewStream = stream2;
          int num2 = 0;
          OutboundPhotoAttachment forUploadNewPhoto = OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType) num2);
          wallPostVM.AddAttachment((IOutboundAttachment) forUploadNewPhoto);
        }
        wallPostVM.UploadAttachments();
      }
      Photo parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
      if (parameterForIdAndReset4 != null)
      {
        OutboundPhotoAttachment choosingExistingPhoto = OutboundPhotoAttachment.CreateForChoosingExistingPhoto(parameterForIdAndReset4, wallPostVM.UserOrGroupId, wallPostVM.IsGroup, PostType.WallPost);
        wallPostVM.AddAttachment((IOutboundAttachment) choosingExistingPhoto);
      }
      VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
      if (parameterForIdAndReset5 != null)
      {
        OutboundVideoAttachment outboundVideoAttachment = new OutboundVideoAttachment(parameterForIdAndReset5);
        wallPostVM.AddAttachment((IOutboundAttachment) outboundVideoAttachment);
      }
      AudioObj parameterForIdAndReset6 = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
      if (parameterForIdAndReset6 != null)
      {
        OutboundAudioAttachment outboundAudioAttachment = new OutboundAudioAttachment(parameterForIdAndReset6);
        wallPostVM.AddAttachment((IOutboundAttachment) outboundAudioAttachment);
      }
      Doc parameterForIdAndReset7 = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
      if (parameterForIdAndReset7 != null)
      {
        OutboundDocumentAttachment documentAttachment = new OutboundDocumentAttachment(parameterForIdAndReset7);
        wallPostVM.AddAttachment((IOutboundAttachment) documentAttachment);
      }
      FileOpenPickerContinuationEventArgs parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if ((parameterForIdAndReset8 == null || !Enumerable.Any<StorageFile>(parameterForIdAndReset8.Files)) && !ParametersRepository.Contains("PickedPhotoDocuments"))
        return;
      object parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
      IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset8 != null ? parameterForIdAndReset8.Files : (IReadOnlyList<StorageFile>) ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
      AttachmentType attachmentType;
      // ISSUE: explicit reference operation
      // ISSUE: cast to a reference type
      if (parameterForIdAndReset9 == null || !Enum.TryParse<AttachmentType>((parameterForIdAndReset9).ToString(), out attachmentType))
        return;
      IEnumerator<StorageFile> enumerator = ((IEnumerable<StorageFile>) storageFileList).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          StorageFile current = enumerator.Current;
          if (attachmentType != AttachmentType.VideoFromPhone)
          {
            if (attachmentType == AttachmentType.DocumentFromPhone || attachmentType == AttachmentType.DocumentPhoto)
            {
              OutboundUploadDocumentAttachment documentAttachment = new OutboundUploadDocumentAttachment(current);
              wallPostVM.AddAttachment((IOutboundAttachment) documentAttachment);
              wallPostVM.UploadAttachments();
            }
          }
          else
          {
            OutboundUploadVideoAttachment uploadVideoAttachment = new OutboundUploadVideoAttachment(current, true, 0L);
            wallPostVM.AddAttachment((IOutboundAttachment) uploadVideoAttachment);
            wallPostVM.UploadAttachments();
          }
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    private void InitializeCommentVM()
    {
      this._commentVM = WallPostViewModel.CreateNewWallCommentVM(this.CommonParameters.OwnerId, this.CommonParameters.PostId);
      this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
      ((FrameworkElement) this.ucNewMessage).DataContext = this._commentVM;
    }

    private void _commentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender != this._commentVM || !(e.PropertyName == "CanPublish"))
        return;
      this.UpdateAppBar();
      ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
      Func<IOutboundAttachment, bool> func1 = (Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Uploading);
      if (Enumerable.Any<IOutboundAttachment>(outboundAttachments, func1))
        return;
      this.PostCommentsVM.SetInProgress(false, "");
    }

    private void ProcessInputData()
    {
      Group parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
      if (parameterForIdAndReset1 != null)
        this.Share(parameterForIdAndReset1.id, parameterForIdAndReset1.name);
      Photo parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
      if (parameterForIdAndReset2 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForChoosingExistingPhoto(parameterForIdAndReset2, 0, false, PostType.WallPost));
      VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
      if (parameterForIdAndReset3 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundVideoAttachment(parameterForIdAndReset3));
      AudioObj parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
      if (parameterForIdAndReset4 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundAudioAttachment(parameterForIdAndReset4));
      Doc parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
      if (parameterForIdAndReset5 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundDocumentAttachment(parameterForIdAndReset5));
      List<Stream> parameterForIdAndReset6 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      List<Stream> parameterForIdAndReset7 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
      if (parameterForIdAndReset6 != null)
      {
        for (int index = 0; index < parameterForIdAndReset6.Count; ++index)
        {
          Stream stream1 = parameterForIdAndReset6[index];
          Stream stream2 = parameterForIdAndReset7[index];
          long userOrGroupId = 0;
          int num1 = 0;
          Stream previewStream = stream2;
          int num2 = 0;
          this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType) num2));
        }
        this.PostCommentsVM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
        this._commentVM.UploadAttachments();
      }
      FileOpenPickerContinuationEventArgs parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if ((parameterForIdAndReset8 == null || !Enumerable.Any<StorageFile>(parameterForIdAndReset8.Files)) && !ParametersRepository.Contains("PickedPhotoDocuments"))
        return;
      object parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
      IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset8 != null ? parameterForIdAndReset8.Files : (IReadOnlyList<StorageFile>) ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
      AttachmentType attachmentType;
      // ISSUE: explicit reference operation
      // ISSUE: cast to a reference type
      if (parameterForIdAndReset9 == null || !Enum.TryParse<AttachmentType>((parameterForIdAndReset9).ToString(), out attachmentType))
        return;
      IEnumerator<StorageFile> enumerator = ((IEnumerable<StorageFile>) storageFileList).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          StorageFile current = enumerator.Current;
          if (attachmentType != AttachmentType.VideoFromPhone)
          {
            if (attachmentType == AttachmentType.DocumentFromPhone || attachmentType == AttachmentType.DocumentPhoto)
            {
              this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadDocumentAttachment(current));
              this._commentVM.UploadAttachments();
            }
          }
          else
          {
            this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadVideoAttachment(current, true, 0L));
            this._commentVM.UploadAttachments();
          }
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      this.SaveUnboundState();
      if (e.NavigationMode != NavigationMode.Back)
        WallPostVMCacheManager.RegisterForDelayedSerialization(this._commentVM);
      if (e.NavigationMode != NavigationMode.Back)
        return;
      WallPostVMCacheManager.ResetInstance();
    }

    private void SaveUnboundState()
    {
      try
      {
        this.State["CommentText"] = this.textBoxNewMessage.Text;
      }
      catch (Exception )
      {
      }
    }

    private void RestoreUnboundState()
    {
      if (!this.State.ContainsKey("CommentText"))
        return;
      this.textBoxNewMessage.Text = ((this.State["CommentText"]).ToString());
    }

    private void ViewModelIsLoaded()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        ((UIElement) this.ucNewMessage).Opacity = (this.PostCommentsVM.CanComment ? 1.0 : 0.6);
        ((UIElement) this.ucNewMessage).IsHitTestVisible = this.PostCommentsVM.CanComment;
        if (this.PostCommentsVM.WallPostItem != null)
        {
          this.ucNewMessage.SetAdminLevel(this.PostCommentsVM.WallPostItem.AdminLevel);
          base.UpdateLayout();
        }
        if (this.FocusComments && !this._focusedComments)
        {
          this.ScrollToBottom();
          this._focusedComments = true;
        }
        this.UpdateAppBar();
      }));
    }

    private void ReplyToComment(CommentItem commentItem)
    {
      this._replyToCid = commentItem.Comment.cid;
      this._replyToUid = commentItem.Comment.from_id;
      string str1 = "";
      string str2 = "";
      if (this._replyToUid > 0L)
      {
          User user = (User)Enumerable.FirstOrDefault<User>(this.PostCommentsVM.Users2, (Func<User, bool>)(u => u.uid == this._replyToUid));
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
          Group group = (Group)Enumerable.FirstOrDefault<Group>(this.PostCommentsVM.Groups, (Func<Group, bool>)(u => u.id == this.PostCommentsVM.OwnerId * -1L)) ?? GroupsService.Current.GetCachedGroup(-this.PostCommentsVM.OwnerId);
        if (group != null)
          str2 = str1 = group.name;
      }
      ((UIElement) this.ReplyUserUC).Visibility = Visibility.Visible;
      this.ReplyUserUC.Title = str2;
      if (this.textBoxNewMessage.Text == "" || this.textBoxNewMessage.Text == this.ReplyAutoForm)
      {
        this.ReplyAutoForm = str1 + ", ";
        this.textBoxNewMessage.Text = this.ReplyAutoForm;
        this.textBoxNewMessage.SelectionStart = this.ReplyAutoForm.Length;
      }
      else
        this.ReplyAutoForm = str1 + ", ";
      ((Control) this.textBoxNewMessage).Focus();
    }

    private void ResetReplyFields()
    {
      if (this.textBoxNewMessage.Text == this.ReplyAutoForm)
        this.textBoxNewMessage.Text = ("");
      this.ReplyAutoForm =  null;
      this._replyToUid = this._replyToCid = 0L;
      ((UIElement) this.ReplyUserUC).Visibility = Visibility.Collapsed;
      this.ReplyUserUC.Title = "";
      ((Control) this.textBoxNewMessage).Focus();
    }

    private void Button_Click_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ResetReplyFields();
    }

    public void Handle(StickerItemTapEvent message)
    {
      if (!this._isCurrentPage)
        return;
      bool fromGroupChecked = this.ucNewMessage.FromGroupChecked;
      if (this._addingComment)
        return;
      this._addingComment = true;
      this.PostCommentsVM.PostComment("", this._replyToCid, this._replyToUid, fromGroupChecked, new List<IOutboundAttachment>(), (Action<bool>) (result =>
      {
        this._addingComment = false;
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (!result)
          {
            ExtendedMessageBox.ShowSafe(CommonResources.Error);
          }
          else
          {
            this.ScrollToBottom();
            this.ResetReplyFields();
          }
        }));
      }), message.StickerItem, message.Referrer);
    }

    public void Handle(SpriteElementTapEvent data)
    {
      if (!this._isCurrentPage)
        return;
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        int selectionStart = this.textBoxNewMessage.SelectionStart;
        this.textBoxNewMessage.Text = (this.textBoxNewMessage.Text.Insert(selectionStart, data.Data.ElementCode));
        this.textBoxNewMessage.Select(selectionStart + data.Data.ElementCode.Length, 0);
      }));
    }

    public void Handle(WallPostPinnedUnpinned message)
    {
      if (message.OwnerId != this.CommonParameters.OwnerId || message.PostId != this.CommonParameters.PostId)
        return;
      this.Refresh();
    }

    public void InitiateShare()
    {
      this._appBarButtonShare_Click(this,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/PostCommentsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.scroll = (ViewportControl) base.FindName("scroll");
      this.scrollableGrid = (StackPanel) base.FindName("scrollableGrid");
      this.panel = (MyVirtualizingPanel2) base.FindName("panel");
      this.ucNewMessage = (NewMessageUC) base.FindName("ucNewMessage");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.ucMoreActions = (MoreActionsUC) base.FindName("ucMoreActions");
    }
  }
}
