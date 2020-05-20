using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Photos
{
  public class PhotoCommentsPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, ISupportShare
  {
    private static readonly string LikeHeartImagePath = "Resources/appbar.heart2.rest.png";
    private static readonly string UnlikeHeartImagePath = "Resources/appbar.heart2.broken.rest.png";
    private DelayedExecutor _de = new DelayedExecutor(250);
    private List<Hyperlink> _tagHyperlinks=new List<Hyperlink>();
    private List<PhotoVideoTag> _photoTags = new List<PhotoVideoTag>();
    private bool _isInitialized;
    private PhotoChooserTask _photoChooserTask;
    private WallPostViewModel _commentVM;
    private ViewportScrollableAreaAdapter _adapter;
    private long _ownerId;
    private long _pid;
    private bool _friendsOnly;
    private bool _fromDialog;
    private ApplicationBar _appBar;
    private ApplicationBarIconButton _appBarButtonAttachments;
    private ApplicationBarIconButton _appBarButtonComment;
    private ApplicationBarIconButton _appBarButtonEmojiToggle;
    private ApplicationBarIconButton _appBarButtonLikeUnlike;
    private ApplicationBarMenuItem _appBarMenuItemSave;
    private ApplicationBarMenuItem _appBarMenuItemReport;
    private ApplicationBarMenuItem _appBarMenuItemShare;
    private DialogService _ds;
    private SharePostUC _sharePostUC;
    private int _selectedTagInd;
    internal Grid LayoutRoot;
    internal ViewportControl scroll;
    internal StackPanel stackPanel;
    internal UserOrGroupHeaderUC UserHeader;
    internal Image image;
    internal TextBlock textBlockImageSaved;
    internal StackPanel stackPanelInfo;
    internal RichTextBox textPhotoText;
    internal RichTextBox textTags;
    internal CommentsGenericUC ucCommentGeneric;
    internal TextBlock textBlockError;
    internal NewMessageUC ucNewMessage;
    internal GenericHeaderUC Header;
    internal MoreActionsUC ucMoreActions;
    private bool _contentLoaded;

    private PhotoViewModel PhotoVM
    {
      get
      {
        return base.DataContext as PhotoViewModel;
      }
    }

    public bool ReadyToSend
    {
      get
      {
        string text = this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
        ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
        if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
          return true;
        if (outboundAttachments.Count > 0)
          return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Completed));
        return false;
      }
    }

    public PhotoCommentsPage()
    {
      PhotoChooserTask photoChooserTask = new PhotoChooserTask();
      int num = 1;
      photoChooserTask.ShowCamera=(num != 0);
      this._photoChooserTask = photoChooserTask;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor=(appBarBgColor);
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor=(appBarFgColor);
      this._appBar = applicationBar;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/attach.png", UriKind.Relative);
      applicationBarIconButton1.IconUri=(uri1);
      string barAddAttachment = CommonResources.NewPost_AppBar_AddAttachment;
      applicationBarIconButton1.Text=(barAddAttachment);
      this._appBarButtonAttachments = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri=(uri2);
      string commentsPageAppBarSend = CommonResources.PostCommentsPage_AppBar_Send;
      applicationBarIconButton2.Text=(commentsPageAppBarSend);
      this._appBarButtonComment = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("Resources/appbar.smile.png", UriKind.Relative);
      applicationBarIconButton3.IconUri=(uri3);
      string str1 = "emoji";
      applicationBarIconButton3.Text=(str1);
      this._appBarButtonEmojiToggle = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri(PhotoCommentsPage.LikeHeartImagePath, UriKind.Relative);
      applicationBarIconButton4.IconUri=(uri4);
      string commentsPageAppBarLike = CommonResources.PostCommentsPage_AppBar_Like;
      applicationBarIconButton4.Text=(commentsPageAppBarLike);
      this._appBarButtonLikeUnlike = applicationBarIconButton4;
      ApplicationBarMenuItem applicationBarMenuItem1 = new ApplicationBarMenuItem();
      string viewerAppBarSave = PhotoResources.ImageViewer_AppBar_Save;
      applicationBarMenuItem1.Text=(viewerAppBarSave);
      this._appBarMenuItemSave = applicationBarMenuItem1;
      ApplicationBarMenuItem applicationBarMenuItem2 = new ApplicationBarMenuItem();
      string str2 = CommonResources.Report.ToLowerInvariant() + "...";
      applicationBarMenuItem2.Text=(str2);
      this._appBarMenuItemReport = applicationBarMenuItem2;
      ApplicationBarMenuItem applicationBarMenuItem3 = new ApplicationBarMenuItem();
      string commentsPageAppBarShare = CommonResources.PostCommentsPage_AppBar_Share;
      applicationBarMenuItem3.Text=(commentsPageAppBarShare);
      this._appBarMenuItemShare = applicationBarMenuItem3;
      this._ds = new DialogService();
      this._selectedTagInd = -1;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.Header.TextBlockTitle.Text=(PhotoResources.PhotoCommentsPage_PHOTO);
      this.Header.OnHeaderTap = new Action(this.HandleOnHeaderTap);
      this.scroll.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.CreateAppBar();
      this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
      this.ucCommentGeneric.InitializeWithScrollViewer((IScrollableArea) this._adapter);
      this.ucCommentGeneric.UCNewComment = this.ucNewMessage;
      this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
      this.ucMoreActions.SetBlue();
      this.ucMoreActions.TapCallback = new Action(this.ShowContextMenu);
      this.ucNewMessage.OnAddAttachTap = (Action) (() => this.AddAttachTap());
      this.ucNewMessage.OnSendTap = (Action) (() => this._appBarButtonSend_Click(null,  null));
      this.ucNewMessage.UCNewPost.OnImageDeleteTap = (Action<object>) (sender =>
      {
        FrameworkElement frameworkElement = sender as FrameworkElement;
        if (frameworkElement != null)
          this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
        this.UpdateAppBar();
      });
      this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text=(CommonResources.Comment);
      Binding binding = new Binding("OutboundAttachments");
      ((FrameworkElement) this.ucNewMessage.UCNewPost.ItemsControlAttachments).SetBinding((DependencyProperty) ItemsControl.ItemsSourceProperty, binding);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.ucCommentGeneric.Panel);
      ((ChooserBase<PhotoResult>) this._photoChooserTask).Completed+=(new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
      // ISSUE: method pointer
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.TextChanged+=(new TextChangedEventHandler(this.TextBoxNewComment_TextChanged));
      // ISSUE: method pointer
      ((UIElement) this.ucCommentGeneric.UCNewComment.TextBoxNewComment).GotFocus+=(new RoutedEventHandler(this.textBoxGotFocus));
      // ISSUE: method pointer
      ((UIElement) this.ucCommentGeneric.UCNewComment.TextBoxNewComment).LostFocus+=(new RoutedEventHandler(this.textBoxLostFocus));
      EventAggregator.Current.Subscribe(this);
    }

    private void PanelIsOpenedChanged(object sender, bool e)
    {
      if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
        this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
      else
        this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
    }

    private void ShowContextMenu()
    {
        List<MenuItem> list = new List<MenuItem>();
        MenuItem menuItem = new MenuItem
        {
            Header = PhotoResources.ImageViewer_AppBar_Save
        };
        menuItem.Click += delegate(object s, RoutedEventArgs e)
        {
            this._appBarButtonSave_Click(this, null);
        };
        list.Add(menuItem);
        MenuItem menuItem2 = new MenuItem
        {
            Header = CommonResources.AppBarMenu_SaveInAlbum
        };
        menuItem2.Click += delegate(object s, RoutedEventArgs e)
        {
            this.SavePhotoToAlbum();
        };
        list.Add(menuItem2);
        if (this.PhotoVM.PhotoWithInfo != null && this.PhotoVM.PhotoWithInfo.Photo != null && this.PhotoVM.PhotoWithInfo.Photo.album_id != -8L && this.PhotoVM.PhotoWithInfo.Photo.album_id != -12L && this.PhotoVM.PhotoWithInfo.Photo.album_id != -3L && this.PhotoVM.PhotoWithInfo.Photo.album_id != -10L && this.PhotoVM.PhotoWithInfo.Photo.album_id != -5L)
        {
            MenuItem menuItem3 = new MenuItem
            {
                Header = CommonResources.Photos_GoToAlbum
            };
            menuItem3.Click += delegate(object s, RoutedEventArgs e)
            {
                this.GoToAlbum();
            };
            list.Add(menuItem3);
        }
        this.ucMoreActions.SetMenu(list);
        this.ucMoreActions.ShowMenu();
    }


    private void GoToAlbum()
    {
      if (this.PhotoVM.PhotoWithInfo == null || this.PhotoVM.PhotoWithInfo.Photo == null)
        return;
      Photo photo = this.PhotoVM.PhotoWithInfo.Photo;
      AlbumType albumType = AlbumTypeHelper.GetAlbumType(photo.aid);
      Navigator.Current.NavigateToPhotoAlbum(photo.owner_id > 0L ? photo.owner_id : -photo.owner_id, photo.owner_id < 0, albumType.ToString(), photo.aid.ToString(), "", 0, "", "", false, 0, false);
    }

    private void SavePhotoToAlbum()
    {
      this.PhotoVM.SaveInSavedPhotosAlbum();
    }

    private void AddAttachTap()
    {
        AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action)(() =>
        {
            PostCommentsPage.HandleInputParams(this._commentVM);
            this.UpdateAppBar();
        }), true, 0, 0, (ConversationInfo)null);
    }

    private void HandleOnHeaderTap()
    {
      this.ucCommentGeneric.Panel.ScrollToBottom(false);
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
        if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", e.ChosenPhoto);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      bool flag = true;
      if (!this._isInitialized)
      {
        this._ownerId = long.Parse(((Page) this).NavigationContext.QueryString["ownerId"]);
        this._pid = long.Parse(((Page) this).NavigationContext.QueryString["pid"]);
        string accessKey = ((Page) this).NavigationContext.QueryString["accessKey"];
        Photo parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("Photo") as Photo;
        PhotoWithFullInfo parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("PhotoWithFullInfo") as PhotoWithFullInfo;
        this._friendsOnly = ((Page) this).NavigationContext.QueryString["FriendsOnly"] == bool.TrueString;
        this._fromDialog = ((Page) this).NavigationContext.QueryString["FromDialog"] == bool.TrueString;
        PhotoViewModel photoViewModel;
        if (parameterForIdAndReset1 == null)
        {
          photoViewModel = new PhotoViewModel(this._ownerId, this._pid, accessKey);
        }
        else
        {
          if (string.IsNullOrEmpty(parameterForIdAndReset1.access_key))
            parameterForIdAndReset1.access_key = accessKey;
          photoViewModel = new PhotoViewModel(parameterForIdAndReset1, parameterForIdAndReset2);
        }
        this.InitializeCommentVM();
        base.DataContext = photoViewModel;
        // ISSUE: method pointer
        photoViewModel.LoadInfoWithComments(new Action<bool, int>( this.OnPhotoInfoLoaded));
        this.RestoreUnboundState();
        this._isInitialized = true;
        flag = false;
      }
      if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
        WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
      this.ProcessInputData();
      this.UpdateAppBar();
    }

    private void InitializeCommentVM()
    {
      this._commentVM = WallPostViewModel.CreateNewPhotoCommentVM(this._ownerId, this._pid);
      this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
      ((FrameworkElement) this.ucNewMessage).DataContext = this._commentVM;
    }

    private void _commentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender != this._commentVM || !(e.PropertyName == "CanPublish"))
            return;
        this.UpdateAppBar();
        ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
        Func<IOutboundAttachment, bool> func = (Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Uploading);
        if (outboundAttachments.Any<IOutboundAttachment>(func))
            return;
        this.PhotoVM.SetInProgress(false, "");
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
        this.PhotoVM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
        this._commentVM.UploadAttachments();
      }
      FileOpenPickerContinuationEventArgs parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if ((parameterForIdAndReset8 == null || !((IEnumerable<StorageFile>) parameterForIdAndReset8.Files).Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocuments"))
        return;
      object parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
      IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset8 != null ? parameterForIdAndReset8.Files : (IReadOnlyList<StorageFile>) ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
      AttachmentType attachmentType;
      // ISSUE: explicit reference operation
      // ISSUE: cast to a reference type
      if (parameterForIdAndReset9 == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset9.ToString(), out attachmentType))
        return;
      foreach (StorageFile file in (IEnumerable<StorageFile>) storageFileList)
      {
        if (attachmentType != AttachmentType.VideoFromPhone)
        {
          if (attachmentType == AttachmentType.DocumentFromPhone || attachmentType == AttachmentType.DocumentPhoto)
          {
            this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadDocumentAttachment(file));
            this._commentVM.UploadAttachments();
          }
        }
        else
        {
          this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadVideoAttachment(file, true, 0L));
          this._commentVM.UploadAttachments();
        }
      }
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      if (e.NavigationMode != NavigationMode.Back)
        WallPostVMCacheManager.RegisterForDelayedSerialization(this._commentVM);
      if (e.NavigationMode == NavigationMode.Back)
        WallPostVMCacheManager.ResetInstance();
      this.SaveUnboundState();
    }

    private void SaveUnboundState()
    {
      this.State["CommentText"] = this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
    }

    private void RestoreUnboundState()
    {
      if (!this.State.ContainsKey("CommentText"))
        return;
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text = (this.State["CommentText"].ToString());
    }

    public void CreateAppBar()
    {
      this._appBarButtonComment.Click+=(new EventHandler(this._appBarButtonSend_Click));
      this._appBarButtonEmojiToggle.Click+=(new EventHandler(this._appBarButtonEmojiToggle_Click));
      this._appBarButtonAttachments.Click+=(new EventHandler(this._appBarButtonAttachments_Click));
      this._appBarButtonLikeUnlike.Click+=(new EventHandler(this._appBarButtonLikeUnlike_Click));
      this._appBarMenuItemSave.Click+=(new EventHandler(this._appBarButtonSave_Click));
      this._appBarMenuItemReport.Click+=(new EventHandler(this._appBarMenuItemReport_Click));
      this._appBarMenuItemShare.Click+=(new EventHandler(this._appBarButtonShare_Click));
      this._appBar.Buttons.Add(this._appBarButtonComment);
      this._appBar.Buttons.Add(this._appBarButtonEmojiToggle);
      this._appBar.Buttons.Add(this._appBarButtonAttachments);
      this._appBar.Buttons.Add(this._appBarButtonLikeUnlike);
      this._appBar.MenuItems.Add(this._appBarMenuItemShare);
      this._appBar.MenuItems.Add(this._appBarMenuItemSave);
      this._appBar.MenuItems.Add(this._appBarMenuItemReport);
      this._appBar.Opacity = 0.9;
      this._appBar.StateChanged += (new EventHandler<ApplicationBarStateChangedEventArgs>(this._appBar_StateChanged));
    }

    private void _appBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
    {
    }

    private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
    {
    }

    private void textBoxGotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void textBoxLostFocus(object sender, RoutedEventArgs e)
    {
    }

    private void _appBarMenuItemReport_Click(object sender, EventArgs e)
    {
      ReportContentHelper.ReportPhoto(this.PhotoVM.OwnerId, this.PhotoVM.Pid);
    }

    private void _appBarButtonAttachments_Click(object sender, EventArgs e)
    {
    }

    private void _appBarButtonShare_Click(object sender, EventArgs e)
    {
      this._ds = new DialogService()
      {
        SetStatusBarBackground = true,
        HideOnNavigation = false
      };
      this._sharePostUC = new SharePostUC(0L);
      this._sharePostUC.SendTap += new EventHandler(this.ButtonSend_Click);
      this._sharePostUC.ShareTap += new EventHandler(this.ButtonShare_Click);
      if (this._fromDialog || this._friendsOnly)
      {
        this._sharePostUC.SetShareCommunityEnabled(false);
        this._sharePostUC.SetShareCommunityEnabled(false);
      }
      this._ds.Child = (FrameworkElement) this._sharePostUC;
      this._ds.AnimationType = DialogService.AnimationTypes.None;
      this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
      this._ds.Show( null);
    }

    private void ButtonShare_Click(object sender, EventArgs eventArgs)
    {
      this.Share(0, "");
    }

    private void Share(long gid = 0, string groupName = "")
    {
      this._ds.Hide();
      this.PhotoVM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
    }

    private void ButtonSend_Click(object sender, EventArgs eventArgs)
    {
      if (this.PhotoVM.Photo == null)
        return;
      this._ds.Hide();
      ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
      contentDataProvider.Message = this._sharePostUC.Text;
      contentDataProvider.Photo = this.PhotoVM.Photo;
      contentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
      Navigator.Current.NavigateToPickConversation();
    }

    private void _appBarButtonSave_Click(object sender, EventArgs e)
    {
      ImageHelper.SaveImage(this.image.Source as BitmapImage);
    }

    private void UpdateAppBar()
    {
      if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
        return;
      if (this.PhotoVM.UserLiked)
      {
        this._appBarButtonLikeUnlike.IconUri=(new Uri(PhotoCommentsPage.UnlikeHeartImagePath, UriKind.Relative));
        this._appBarButtonLikeUnlike.Text = CommonResources.PostCommentsPage_AppBar_Unlike;
      }
      else
      {
        this._appBarButtonLikeUnlike.IconUri=(new Uri(PhotoCommentsPage.LikeHeartImagePath, UriKind.Relative));
        this._appBarButtonLikeUnlike.Text = CommonResources.PostCommentsPage_AppBar_Like;
      }
      this._appBarButtonComment.IsEnabled = (this.PhotoVM.CanComment && this.ReadyToSend);
      this.ucNewMessage.UpdateSendButton(this._appBarButtonComment.IsEnabled);
      this._appBarButtonAttachments.IsEnabled = this.PhotoVM.CanComment;
      int count = ((Collection<IOutboundAttachment>) this._commentVM.OutboundAttachments).Count;
      if (count > 0)
        this._appBarButtonAttachments.IconUri=(new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", Math.Min(count, 10)), UriKind.Relative));
      else
        this._appBarButtonAttachments.IconUri=(new Uri("Resources/attach.png", UriKind.Relative));
      if (this._appBar.MenuItems.Contains(this._appBarMenuItemReport) || !this.PhotoVM.CanReport)
        return;
      this._appBar.MenuItems.Add(this._appBarMenuItemReport);
    }

    private void _appBarButtonLikeUnlike_Click(object sender, EventArgs e)
    {
      this.PhotoVM.LikeUnlike();
      this.ucCommentGeneric.UpdateLikesItem(this.PhotoVM.UserLiked);
      this.UpdateAppBar();
    }

    private void _appBarButtonSend_Click(object sender, EventArgs e)
    {
        this.ucCommentGeneric.AddComment(this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (!res)
                return;
            this.InitializeCommentVM();
            this.UpdateAppBar();
        }))), (StickerItemData)null, "");
    }

    private void OnPhotoInfoLoaded(bool result, int adminLevel)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            this.GenerateAuthorText();
            this.GeneratePhotoText();
            this.GenerateTextForTags();
            this.ucCommentGeneric.ProcessLoadedComments(result);
            this.ucNewMessage.SetAdminLevel(adminLevel);
            ((UIElement)this.stackPanelInfo).Visibility=(result ? Visibility.Visible : Visibility.Collapsed);
            this.UpdateAppBar();
        }));
    }

    private void GeneratePhotoText()
    {
      if (!string.IsNullOrEmpty(this.PhotoVM.Text))
      {
        BrowserNavigationService.SetText((DependencyObject) this.textPhotoText, this.PhotoVM.Text);
        ((UIElement) this.textPhotoText).Visibility = Visibility.Visible;
      }
      else
      {
        BrowserNavigationService.SetText((DependencyObject) this.textPhotoText, "");
        ((UIElement) this.textPhotoText).Visibility = Visibility.Collapsed;
      }
    }

    private void GenerateAuthorText()
    {
      string date = "";
      if (this.PhotoVM.Photo != null)
        date = UIStringFormatterHelper.FormatDateTimeForUI(this.PhotoVM.Photo.created);
      this.UserHeader.Initilize(this.PhotoVM.OwnerImageUri, this.PhotoVM.OwnerName ?? "", date, this.PhotoVM.AuthorId);
    }

    private void GenerateTextForTags()
    {
        ((PresentationFrameworkCollection<Block>)this.textTags.Blocks).Clear();
        this._tagHyperlinks.Clear();
        this._photoTags.Clear();
        this._photoTags.AddRange((IEnumerable<PhotoVideoTag>)this.PhotoVM.PhotoTags);
        ((UIElement)this.textTags).Visibility=(this._photoTags.Count > 0 ? Visibility.Visible : Visibility.Collapsed);
        if (this._photoTags.Count <= 0)
            return;
        Paragraph paragraph = new Paragraph();
        Run run1 = new Run();
        string str1 = PhotoResources.PhotoUC_OnThisPhoto + " ";
        run1.Text=(str1);
        SolidColorBrush solidColorBrush = Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush;
        ((TextElement)run1).Foreground=((Brush)solidColorBrush);
        Run run2 = run1;
        ((PresentationFrameworkCollection<Inline>)paragraph.Inlines).Add((Inline)run2);
        for (int index = 0; index < this._photoTags.Count; ++index)
        {
            Hyperlink hyperlink = HyperlinkHelper.GenerateHyperlink(this._photoTags[index].tagged_name ?? "", index.ToString(), (Action<Hyperlink, string>)((h, t) =>
            {
                int state = (int)HyperlinkHelper.GetState(h);
                int ind = int.Parse(t);
                PhotoVideoTag photoTag = this._photoTags[int.Parse(t)];
                this.SelectTaggedUser(ind);
            }), (Brush)null, HyperlinkState.Normal);
            HyperlinkHelper.SetState(hyperlink, HyperlinkState.Accent, (Brush)null);
            this._tagHyperlinks.Add(hyperlink);
            ((PresentationFrameworkCollection<Inline>)paragraph.Inlines).Add((Inline)hyperlink);
            if (index < this.PhotoVM.PhotoTags.Count - 1)
            {
                Run run3 = new Run();
                string str2 = ", ";
                run3.Text=(str2);
                Run run4 = run3;
                ((PresentationFrameworkCollection<Inline>)paragraph.Inlines).Add((Inline)run4);
            }
        }
        ((PresentationFrameworkCollection<Block>)this.textTags.Blocks).Add((Block)paragraph);
    }

    private void SelectTaggedUser(int ind)
    {
      PhotoVideoTag photoTag = this._photoTags[ind];
      if (this._selectedTagInd == ind)
      {
        if (photoTag.uid == 0L)
          return;
        Navigator.Current.NavigateToUserProfile(photoTag.uid, photoTag.tagged_name, "", false);
      }
      else
      {
        for (int index = 0; index < this._tagHyperlinks.Count; ++index)
        {
          Hyperlink tagHyperlink = this._tagHyperlinks[index];
          if (index == ind)
          {
            if (photoTag.uid != 0L)
              HyperlinkHelper.SetState(tagHyperlink, HyperlinkState.Normal,  null);
          }
          else
            HyperlinkHelper.SetState(tagHyperlink, HyperlinkState.Accent,  null);
        }
        WriteableBitmap opacityMask = this.GenerateOpacityMask(((FrameworkElement) this.image).ActualWidth, ((FrameworkElement) this.image).ActualHeight, photoTag.x, photoTag.x2, photoTag.y, photoTag.y2);
        Image image = this.image;
        ImageBrush imageBrush = new ImageBrush();
        WriteableBitmap writeableBitmap = opacityMask;
        imageBrush.ImageSource=((ImageSource) writeableBitmap);
        int num = 1;
        ((TileBrush) imageBrush).Stretch=((Stretch) num);
        ((UIElement) image).OpacityMask=((Brush) imageBrush);
        this._selectedTagInd = ind;
      }
    }

    private void ResetTaggedUsersSelection()
    {
      using (List<Hyperlink>.Enumerator enumerator = this._tagHyperlinks.GetEnumerator())
      {
        while (enumerator.MoveNext())
          HyperlinkHelper.SetState(enumerator.Current, HyperlinkState.Accent,  null);
      }
      ((UIElement) this.image).OpacityMask=( null);
      this._selectedTagInd = -1;
    }

    private WriteableBitmap GenerateOpacityMask(double totalWidth, double totalHeight, double x1, double x2, double y1, double y2)
    {
      int num1 = (int) (100.0 * (totalHeight / totalWidth));
      int num2 = (int) (100.0 * x1 / 100.0);
      int num3 = (int) (100.0 * x2 / 100.0);
      int num4 = (int) ((double) num1 * y1 / 100.0);
      int num5 = (int) ((double) num1 * y2 / 100.0);
      WriteableBitmap writeableBitmap = new WriteableBitmap(100, num1);
      for (int index = 0; index < writeableBitmap.Pixels.Length; ++index)
      {
        int num6 = index % ((BitmapSource) writeableBitmap).PixelWidth;
        int num7 = index / ((BitmapSource) writeableBitmap).PixelWidth;
        writeableBitmap.Pixels[index] = num6 < num2 || num6 > num3 || (num7 < num4 || num7 > num5) ? int.MinValue : -16777216;
      }
      return writeableBitmap;
    }

    private void image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Point position = e.GetPosition((UIElement) this.image);
      if (((FrameworkElement) this.image).ActualHeight == 0.0 || ((FrameworkElement) this.image).ActualWidth == 0.0)
        return;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      int relativePosition = this.GetTagIndForRelativePosition((int) (((Point) @position).X * 100.0 / ((FrameworkElement) this.image).ActualWidth), (int) (((Point) @position).Y * 100.0 / ((FrameworkElement) this.image).ActualHeight));
      if (relativePosition >= 0)
        this.SelectTaggedUser(relativePosition);
      else
        this.ResetTaggedUsersSelection();
    }

    private int GetTagIndForRelativePosition(int x, int y)
    {
      if (this._photoTags != null)
      {
        for (int index = 0; index < this._photoTags.Count; ++index)
        {
          PhotoVideoTag photoTag = this._photoTags[index];
          if ((double) x >= photoTag.x && (double) x <= photoTag.x2 && ((double) y >= photoTag.y && (double) y <= photoTag.y2))
            return index;
        }
      }
      return -1;
    }

    public void Handle(SpriteElementTapEvent data)
    {
        if (!this._isCurrentPage)
            return;
        ((DependencyObject)this).Dispatcher.BeginInvoke((Action)(() =>
        {
            TextBox textBoxNewComment = this.ucCommentGeneric.UCNewComment.TextBoxNewComment;
            int selectionStart = textBoxNewComment.SelectionStart;
            string str = textBoxNewComment.Text.Insert(selectionStart, data.Data.ElementCode);
            textBoxNewComment.Text=(str);
            int num1 = selectionStart + data.Data.ElementCode.Length;
            int num2 = 0;
            textBoxNewComment.Select(num1, num2);
        }));
    }

    public void Handle(StickerItemTapEvent message)
    {
      if (!this._isCurrentPage)
        return;
      this.ucCommentGeneric.AddComment(new List<IOutboundAttachment>(), (Action<bool>) (res => {}), message.StickerItem, message.Referrer);
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
      Application.LoadComponent(this, new Uri("/VKClient.Photos;component/PhotoCommentsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.scroll = (ViewportControl) base.FindName("scroll");
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.UserHeader = (UserOrGroupHeaderUC) base.FindName("UserHeader");
      this.image = (Image) base.FindName("image");
      this.textBlockImageSaved = (TextBlock) base.FindName("textBlockImageSaved");
      this.stackPanelInfo = (StackPanel) base.FindName("stackPanelInfo");
      this.textPhotoText = (RichTextBox) base.FindName("textPhotoText");
      this.textTags = (RichTextBox) base.FindName("textTags");
      this.ucCommentGeneric = (CommentsGenericUC) base.FindName("ucCommentGeneric");
      this.textBlockError = (TextBlock) base.FindName("textBlockError");
      this.ucNewMessage = (NewMessageUC) base.FindName("ucNewMessage");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucMoreActions = (MoreActionsUC) base.FindName("ucMoreActions");
    }
  }
}
