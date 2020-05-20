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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Common;
using VKClient.Common.Backend;
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
using VKClient.Video.Library;
using VKClient.Video.Localization;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Video
{
    public class VideoCommentsPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, ISupportShare
    {
        private bool _isInitialized;
        private bool _textsGenerated;
        //private readonly List<PhotoVideoTag> _tags;
        private readonly PhotoChooserTask _photoChooserTask;
        private WallPostViewModel _commentVM;
        private readonly ApplicationBar _appBar;
        private const string LikeHeartImagePath = "Resources/appbar.heart2.rest.png";
        private const string UnlikeHeartImagePath = "Resources/appbar.heart2.broken.rest.png";
        private readonly ApplicationBarIconButton _appBarButtonComment;
        private readonly ApplicationBarIconButton _appBarButtonEmojiToggle;
        private readonly ApplicationBarIconButton _appBarButtonAttachments;
        private readonly ApplicationBarIconButton _appBarButtonLikeUnlike;
        private readonly ApplicationBarMenuItem _appBarMenuItemAddDelete;
        private readonly ApplicationBarMenuItem _appBarMenuItemReport;
        private readonly ApplicationBarMenuItem _appBarMenuItemShare;
        private readonly ApplicationBarMenuItem _appBarMenuItemEdit;
        //private bool _isAddedOrDeleted;
        private DialogService _ds;
        private SharePostUC _sharePostUC;
        private ViewportScrollableAreaAdapter _adapter;
        private long _ownerId;
        private long _videoId;
        internal ViewportControl scroll;
        internal StackPanel stackPanel;
        internal Canvas canvasBackground;
        internal TextBlock textBlockMetaData;
        internal CommentsGenericUC ucCommentGeneric;
        internal NewMessageUC ucNewMessage;
        internal MoreActionsUC ucMoreActions;
        private bool _contentLoaded;

        private VideoCommentsViewModel VM
        {
            get
            {
                return base.DataContext as VideoCommentsViewModel;
            }
        }

        private bool IsSelfOwner
        {
            get
            {
                return this.VM.OwnerId == AppGlobalStateManager.Current.LoggedInUserId;
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
                    return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
                return false;
            }
        }

        public VideoCommentsPage()
        {
            PhotoChooserTask photoChooserTask = new PhotoChooserTask();
            int num = 1;
            photoChooserTask.ShowCamera = (num != 0);
            this._photoChooserTask = photoChooserTask;
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = (appBarBgColor);
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = (appBarFgColor);
            this._appBar = applicationBar;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri1 = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = (uri1);
            string commentsPageAppBarSend = CommonResources.PostCommentsPage_AppBar_Send;
            applicationBarIconButton1.Text = (commentsPageAppBarSend);
            this._appBarButtonComment = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            Uri uri2 = new Uri("Resources/appbar.smile.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = (uri2);
            string str1 = "emoji";
            applicationBarIconButton2.Text = (str1);
            this._appBarButtonEmojiToggle = applicationBarIconButton2;
            ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
            Uri uri3 = new Uri("Resources/attach.png", UriKind.Relative);
            applicationBarIconButton3.IconUri = (uri3);
            string barAddAttachment = CommonResources.NewPost_AppBar_AddAttachment;
            applicationBarIconButton3.Text = (barAddAttachment);
            this._appBarButtonAttachments = applicationBarIconButton3;
            ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
            Uri uri4 = new Uri("Resources/appbar.heart2.rest.png", UriKind.Relative);
            applicationBarIconButton4.IconUri = (uri4);
            string commentsPageAppBarLike = CommonResources.PostCommentsPage_AppBar_Like;
            applicationBarIconButton4.Text = (commentsPageAppBarLike);
            this._appBarButtonLikeUnlike = applicationBarIconButton4;
            ApplicationBarMenuItem applicationBarMenuItem1 = new ApplicationBarMenuItem();
            string appBarAddToVideos = VideoResources.VideoComments_AppBar_AddToVideos;
            applicationBarMenuItem1.Text = (appBarAddToVideos);
            this._appBarMenuItemAddDelete = applicationBarMenuItem1;
            ApplicationBarMenuItem applicationBarMenuItem2 = new ApplicationBarMenuItem();
            string str2 = CommonResources.Report.ToLowerInvariant() + "...";
            applicationBarMenuItem2.Text = (str2);
            this._appBarMenuItemReport = applicationBarMenuItem2;
            ApplicationBarMenuItem applicationBarMenuItem3 = new ApplicationBarMenuItem();
            string commentsPageAppBarShare = CommonResources.PostCommentsPage_AppBar_Share;
            applicationBarMenuItem3.Text = (commentsPageAppBarShare);
            this._appBarMenuItemShare = applicationBarMenuItem3;
            ApplicationBarMenuItem applicationBarMenuItem4 = new ApplicationBarMenuItem();
            string edit = CommonResources.Edit;
            applicationBarMenuItem4.Text = (edit);
            this._appBarMenuItemEdit = applicationBarMenuItem4;
            this._ds = new DialogService();
            // ISSUE: explicit constructor call
            //  base.\u002Ector();
            this.InitializeComponent();
            this.ucMoreActions.SetBlue();
            this.ucMoreActions.TapCallback = new Action(this.ShowContextMenu);
            this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
            this.ucCommentGeneric.InitializeWithScrollViewer((IScrollableArea)this._adapter);
            this.ucCommentGeneric.UCNewComment = this.ucNewMessage;
            this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
            this.ucNewMessage.OnAddAttachTap = (Action)(() => this.AddAttachTap());
            this.ucNewMessage.OnSendTap = (Action)(() => this._appBarButtonSend_Click(null, null));
            this.ucNewMessage.UCNewPost.OnImageDeleteTap = (Action<object>)(sender =>
            {
                FrameworkElement frameworkElement = sender as FrameworkElement;
                if (frameworkElement != null)
                    this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
                this.UpdateAppBar();
            });
            this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = (CommonResources.Comment);
            Binding binding = new Binding("OutboundAttachments");
            ((FrameworkElement)this.ucNewMessage.UCNewPost.ItemsControlAttachments).SetBinding((DependencyProperty)ItemsControl.ItemsSourceProperty, binding);
            this.scroll.BindViewportBoundsTo((FrameworkElement)this.stackPanel);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.ucCommentGeneric.Panel);
            this.CreateAppBar();
            // ISSUE: method pointer
            this.ucCommentGeneric.UCNewComment.TextBoxNewComment.TextChanged += (new TextChangedEventHandler(this.TextBoxNewComment_TextChanged));
            ((ChooserBase<PhotoResult>)this._photoChooserTask).Completed += (new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
            EventAggregator.Current.Subscribe(this);
        }

        public void CreateAppBar()
        {
            this._appBarButtonComment.Click += (new EventHandler(this._appBarButtonSend_Click));
            this._appBarButtonAttachments.Click += (new EventHandler(this._appBarButtonAttachments_Click));
            this._appBarButtonLikeUnlike.Click += (new EventHandler(this._appBarButtonLikeUnlike_Click));
            this._appBarMenuItemReport.Click += (new EventHandler(this._appBarMenuItemReport_Click));
            this._appBarMenuItemShare.Click += (new EventHandler(this._appBarButtonShare_Click));
            this._appBarButtonEmojiToggle.Click += (new EventHandler(this._appBarButtonEmojiToggle_Click));
            this._appBarMenuItemEdit.Click += (new EventHandler(this._appBarMenuItemEdit_Click));
            this._appBar.Buttons.Add(this._appBarButtonComment);
            this._appBar.Buttons.Add(this._appBarButtonEmojiToggle);
            this._appBar.Buttons.Add(this._appBarButtonAttachments);
            this._appBar.Buttons.Add(this._appBarButtonLikeUnlike);
            this._appBar.MenuItems.Add(this._appBarMenuItemShare);
            this._appBar.MenuItems.Add(this._appBarMenuItemAddDelete);
            this._appBar.Opacity = (0.9);
        }

        private void _appBarMenuItemEdit_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToEditVideo(this.VM.OwnerId, this.VM.VideoId, this.VM.Video);
        }

        private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
        {
        }

        private void _appBarMenuItemReport_Click(object sender, EventArgs e)
        {
            ReportContentHelper.ReportVideo(this.VM.OwnerId, this.VM.VideoId);
        }

        private void _appBarButtonAttachments_Click(object sender, EventArgs e)
        {
            if (this._commentVM.OutboundAttachments.Count == 0)
            {
                PickerUC.PickAttachmentTypeAndNavigate(AttachmentTypes.AttachmentTypesWithPhotoFromGallery, null, (() => Navigator.Current.NavigateToPhotoPickerPhotos(2, false, false)));
            }
            else
            {
                ParametersRepository.SetParameterForId("NewCommentVM", this._commentVM);
                Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
            }
        }

        private void _appBarButtonShare_Click(object sender, EventArgs e)
        {
            this._ds = new DialogService();
            this._ds.SetStatusBarBackground = false;
            this._ds.HideOnNavigation = false;
            this._sharePostUC = new SharePostUC(0L);
            this._sharePostUC.SendTap += new EventHandler(this.ButtonSend_Click);
            this._sharePostUC.ShareTap += new EventHandler(this.ButtonShare_Click);
            this._ds.Child = (FrameworkElement)this._sharePostUC;
            this._ds.AnimationType = DialogService.AnimationTypes.None;
            this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
            this._ds.Show(null);
        }

        private void ButtonShare_Click(object sender, EventArgs eventArgs)
        {
            this.Share(0, "");
        }

        private void Share(long gid = 0, string groupName = "")
        {
            if (this.VM.Video == null)
                return;
            this._ds.Hide();
            this.VM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
        }

        private void ButtonSend_Click(object sender, EventArgs eventArgs)
        {
            if (this.VM.Video == null)
                return;
            this._ds.Hide();
            ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
            contentDataProvider.Message = this._sharePostUC.Text;
            contentDataProvider.Video = this.VM.Video;
            contentDataProvider.StoreDataToRepository();
            ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
            Navigator.Current.NavigateToPickConversation();
        }

        private void _appBarButtonLikeUnlike_Click(object sender, EventArgs e)
        {
            this.VM.LikeUnlike();
            this.ucCommentGeneric.UpdateLikesItem(this.VM.UserLiked);
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
            }))), null, "");
        }

        private void UpdateAppBar()
        {
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
                return;
            if (this.VM.UserLiked)
            {
                this._appBarButtonLikeUnlike.IconUri = (new Uri("Resources/appbar.heart2.broken.rest.png", UriKind.Relative));
                this._appBarButtonLikeUnlike.Text = (CommonResources.PostCommentsPage_AppBar_Unlike);
            }
            else
            {
                this._appBarButtonLikeUnlike.IconUri = (new Uri("Resources/appbar.heart2.rest.png", UriKind.Relative));
                this._appBarButtonLikeUnlike.Text = (CommonResources.PostCommentsPage_AppBar_Like);
            }/*
      if (!this._isAddedOrDeleted)
      {
        if (this.IsSelfOwner)
          this._appBarMenuItemAddDelete.Text=(CommonResources.Delete);
        else
          this._appBarMenuItemAddDelete.Text=(CommonResources.AppBar_Add);
        if (!this._appBar.MenuItems.Contains(this._appBarMenuItemAddDelete))
          this._appBar.MenuItems.Add(this._appBarMenuItemAddDelete);
      }
      else */
            if (this._appBar.MenuItems.Contains(this._appBarMenuItemAddDelete))
                this._appBar.MenuItems.Remove(this._appBarMenuItemAddDelete);
            if (this.VM.CanReport && !this._appBar.MenuItems.Contains(this._appBarMenuItemReport))
                this._appBar.MenuItems.Add(this._appBarMenuItemReport);
            if (this.VM.CanEdit && !this._appBar.MenuItems.Contains(this._appBarMenuItemEdit))
            {
                if (this._appBar.MenuItems.Count >= 1)
                    this._appBar.MenuItems.Insert(1, this._appBarMenuItemEdit);
                else
                    this._appBar.MenuItems.Add(this._appBarMenuItemEdit);
            }
            this._appBarButtonComment.IsEnabled = (this.VM.CanComment && this.ReadyToSend);
            this.ucNewMessage.UpdateSendButton(this._appBarButtonComment.IsEnabled);
            this._appBarButtonAttachments.IsEnabled = (this.VM.CanComment);
            int count = this._commentVM.OutboundAttachments.Count;
            if (count > 0)
                this._appBarButtonAttachments.IconUri = (new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", Math.Min(count, 10)), UriKind.Relative));
            else
                this._appBarButtonAttachments.IconUri = (new Uri("Resources/attach.png", UriKind.Relative));
        }

        private void GridContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size newSize1 = e.NewSize;
            // ISSUE: explicit reference operation
            double height = ((Size)@newSize1).Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            ((FrameworkElement)this.canvasBackground).Height = (height);
            ((PresentationFrameworkCollection<UIElement>)((Panel)this.canvasBackground).Children).Clear();
            Rectangle rect = new Rectangle();
            double num = height;
            ((FrameworkElement)rect).Height = (num);
            Thickness thickness = new Thickness(0.0);
            ((FrameworkElement)rect).Margin = (thickness);
            Size newSize2 = e.NewSize;
            // ISSUE: explicit reference operation
            double width = ((Size)@newSize2).Width;
            ((FrameworkElement)rect).Width = (width);
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            ((Shape)rect).Fill = ((Brush)solidColorBrush);
            using (List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(rect).GetEnumerator())
            {
                while (enumerator.MoveNext())
                    ((PresentationFrameworkCollection<UIElement>)((Panel)this.canvasBackground).Children).Add((UIElement)enumerator.Current);
            }
        }

        private void StackPanelProductInfo_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size newSize = e.NewSize;
            // ISSUE: explicit reference operation
            double height = ((Size)@newSize).Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            this.ucCommentGeneric.Panel.DeltaOffset = -height;
        }

        private void ShowContextMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            if (this.VM.CanEdit)
            {
                MenuItem menuItem1 = new MenuItem();
                string lowerInvariant = CommonResources.Edit.ToLowerInvariant();
                menuItem1.Header = lowerInvariant;
                MenuItem menuItem2 = menuItem1;
                // ISSUE: method pointer
                menuItem2.Click += new RoutedEventHandler((this.mItemEdit_Click));
                menuItems.Add(menuItem2);
            }
            if (this.VM.CanAddToMyVideos)
            {
                MenuItem menuItem1 = new MenuItem();
                string newAddToMyVideos = CommonResources.VideoNew_AddToMyVideos;
                menuItem1.Header = newAddToMyVideos;
                MenuItem menuItem2 = menuItem1;
                // ISSUE: method pointer
                menuItem2.Click += new RoutedEventHandler((this.mItemAddToMyVideos_Click));
                menuItems.Add(menuItem2);
            }
            MenuItem menuItem3 = new MenuItem();
            string lowerInvariant1 = CommonResources.VideoNew_AddToAlbum.ToLowerInvariant();
            menuItem3.Header = lowerInvariant1;
            MenuItem menuItem4 = menuItem3;
            // ISSUE: method pointer
            menuItem4.Click += new RoutedEventHandler((this.mItemAddToAlbum_Click));
            menuItems.Add(menuItem4);
            MenuItem menuItem5 = new MenuItem();
            string lowerInvariant2 = CommonResources.OpenInBrowser.ToLowerInvariant();
            menuItem5.Header = lowerInvariant2;
            MenuItem menuItem6 = menuItem5;
            // ISSUE: method pointer
            menuItem6.Click += new RoutedEventHandler((this.mItemOpenInBrowser_Click));
            menuItems.Add(menuItem6);
            MenuItem menuItem7 = new MenuItem();
            string lowerInvariant3 = CommonResources.CopyLink.ToLowerInvariant();
            menuItem7.Header = lowerInvariant3;
            MenuItem menuItem8 = menuItem7;
            // ISSUE: method pointer
            menuItem8.Click += new RoutedEventHandler((this.mItemCopyLink_Click));
            menuItems.Add(menuItem8);
            MenuItem menuItem9 = new MenuItem();
            string str = CommonResources.Report.ToLowerInvariant() + "...";
            menuItem9.Header = str;
            MenuItem menuItem10 = menuItem9;
            // ISSUE: method pointer
            menuItem10.Click += new RoutedEventHandler((this.mItemReport_Click));
            menuItems.Add(menuItem10);
            if (this.VM.CanRemoveFromMyVideos)
            {
                MenuItem menuItem1 = new MenuItem();
                string removedFromMyVideos = CommonResources.VideoNew_RemovedFromMyVideos;
                menuItem1.Header = removedFromMyVideos;
                MenuItem menuItem2 = menuItem1;
                // ISSUE: method pointer
                menuItem2.Click += new RoutedEventHandler((this.mItemRemoveFromMyVideos_Click));
                menuItems.Add(menuItem2);
            }
            if (this.VM.CanDelete)
            {
                MenuItem menuItem1 = new MenuItem();
                string lowerInvariant4 = CommonResources.Delete.ToLowerInvariant();
                menuItem1.Header = lowerInvariant4;
                MenuItem menuItem2 = menuItem1;
                // ISSUE: method pointer
                menuItem2.Click += new RoutedEventHandler((this.mItemDelete_Click));
                menuItems.Add(menuItem2);
            }
            this.ucMoreActions.SetMenu(menuItems);
            this.ucMoreActions.ShowMenu();
        }

        private void mItemAddToAlbum_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToAddVideoToAlbum(this._ownerId, this._videoId);
        }

        private void mItemReport_Click(object sender, RoutedEventArgs e)
        {
            this.ReportVideo();
        }

        private void ReportVideo()
        {
            ReportContentHelper.ReportVideo(this.VM.OwnerId, this.VM.VideoId);
        }

        private void mItemCopyLink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.VM.VideoUri);
        }

        private void mItemOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToWebUri(this.VM.VideoUri, true, false);
        }

        private void mItemRemoveFromMyVideos_Click(object sender, RoutedEventArgs e)
        {
            this.VM.AddRemoveToMyVideos();
        }

        private void mItemAddToMyVideos_Click(object sender, RoutedEventArgs e)
        {
            this.VM.AddRemoveToMyVideos();
        }

        private void mItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(CommonResources.DeleteConfirmation, VideoResources.DeleteVideo, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this.VM.Delete((Action<ResultCode>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (res == ResultCode.Succeeded)
                    Navigator.Current.GoBack();
                else
                    GenericInfoUC.ShowBasedOnResult((int)res, "", (VKRequestsDispatcher.Error)null);
            }))));
        }

        private void mItemEdit_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToEditVideo(this.VM.OwnerId, this.VM.VideoId, this.VM.Video);
        }

        private void PanelIsOpenedChanged(object sender, bool e)
        {
            if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
                this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
            else
                this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
        }

        private void AddAttachTap()
        {
            AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action)(() =>
            {
                PostCommentsPage.HandleInputParams(this._commentVM);
                this.UpdateAppBar();
            }), true, 0, 0, null);
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
                this._ownerId = long.Parse(((Page)this).NavigationContext.QueryString["OwnerId"]);
                this._videoId = long.Parse(((Page)this).NavigationContext.QueryString["VideoId"]);
                string accessKey = ((Page)this).NavigationContext.QueryString["AccessKey"];
                string videoContext = "";
                if (((Page)this).NavigationContext.QueryString.ContainsKey("VideoContext"))
                    videoContext = ((Page)this).NavigationContext.QueryString["VideoContext"];
                VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("Video") as VKClient.Common.Backend.DataObjects.Video;
                StatisticsActionSource actionSource = (StatisticsActionSource)Enum.Parse(typeof(StatisticsActionSource), ((Page)this).NavigationContext.QueryString["VideoSource"]);
                this.InitializeCommentVM();
                VideoCommentsViewModel commentsViewModel = new VideoCommentsViewModel(this._ownerId, this._videoId, accessKey, parameterForIdAndReset, actionSource, videoContext);
                commentsViewModel.PageLoadInfoViewModel.LoadingStateChangedCallback = new Action(this.OnLoadingStateChanged);
                base.DataContext = (commentsViewModel);
                commentsViewModel.Reload(true);
                this.RestoreUnboundState();
                this._isInitialized = true;
                flag = false;
            }
            if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
                WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
            this.ProcessInputData();
            this.UpdateAppBar();
        }

        private void OnLoadingStateChanged()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.VM.PageLoadInfoViewModel.LoadingState != PageLoadingState.Loaded)
                    return;
                this.VM.LoadMoreComments(7, new Action<bool>(this.CommentsAreLoadedCallback));
            }));
        }

        private void ProcessInputData()
        {
            Group parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (parameterForIdAndReset1 != null)
                this.Share(parameterForIdAndReset1.id, parameterForIdAndReset1.name);
            Photo parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (parameterForIdAndReset2 != null)
                this._commentVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForChoosingExistingPhoto(parameterForIdAndReset2, 0, false, PostType.WallPost));
            VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
            if (parameterForIdAndReset3 != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundVideoAttachment(parameterForIdAndReset3));
            AudioObj parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (parameterForIdAndReset4 != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundAudioAttachment(parameterForIdAndReset4));
            Doc parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (parameterForIdAndReset5 != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundDocumentAttachment(parameterForIdAndReset5));
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
                    this._commentVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType)num2));
                }
                this.VM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
                this._commentVM.UploadAttachments();
            }
            FileOpenPickerContinuationEventArgs parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if ((parameterForIdAndReset8 == null || !parameterForIdAndReset8.Files.Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocuments"))
                return;
            object parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
            IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset8 != null ? parameterForIdAndReset8.Files : (IReadOnlyList<StorageFile>)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
            AttachmentType result;
            if (parameterForIdAndReset9 == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset9.ToString(), out result))
                return;
            foreach (StorageFile file in (IEnumerable<StorageFile>)storageFileList)
            {
                if (result != AttachmentType.VideoFromPhone)
                {
                    if (result == AttachmentType.DocumentFromPhone || result == AttachmentType.DocumentPhoto)
                    {
                        this._commentVM.AddAttachment((IOutboundAttachment)new OutboundUploadDocumentAttachment(file));
                        this._commentVM.UploadAttachments();
                    }
                }
                else
                {
                    this._commentVM.AddAttachment((IOutboundAttachment)new OutboundUploadVideoAttachment(file, true, 0L));
                    this._commentVM.UploadAttachments();
                }
            }
        }

        private void InitializeCommentVM()
        {
            this._commentVM = WallPostViewModel.CreateNewVideoCommentVM(this._ownerId, this._videoId);
            this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
            ((FrameworkElement)this.ucNewMessage).DataContext = (this._commentVM);
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
            this.VM.SetInProgress(false, "");
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

        private void CommentsAreLoadedCallback(bool success)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.VM.PageLoadInfoViewModel.LoadingState == PageLoadingState.LoadingFailed)
                    return;
                if (!this._textsGenerated)
                    this._textsGenerated = true;
                this.ucCommentGeneric.ProcessLoadedComments(true);
                this.UpdateAppBar();
                if (this._ownerId >= 0L)
                    return;
                Group cachedGroup = GroupsService.Current.GetCachedGroup(-this._ownerId);
                if (cachedGroup == null)
                    return;
                this.ucNewMessage.SetAdminLevel(cachedGroup.admin_level);
            }));
        }

        private void Play_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.PlayVideo();
        }

        protected override void TextBoxPanelIsOpenedChanged(object sender, bool e)
        {
            this.UpdateAppBar();
        }

        public void Handle(SpriteElementTapEvent data)
        {
            if (!this._isCurrentPage)
                return;
            base.Dispatcher.BeginInvoke((Action)(() =>
            {
                TextBox textBoxNewComment = this.ucCommentGeneric.UCNewComment.TextBoxNewComment;
                int selectionStart = textBoxNewComment.SelectionStart;
                string str = textBoxNewComment.Text.Insert(selectionStart, data.Data.ElementCode);
                textBoxNewComment.Text = (str);
                int num1 = selectionStart + data.Data.ElementCode.Length;
                int num2 = 0;
                textBoxNewComment.Select(num1, num2);
            }));
        }

        public void Handle(StickerItemTapEvent message)
        {
            if (!this._isCurrentPage)
                return;
            this.ucCommentGeneric.AddComment(new List<IOutboundAttachment>(), (Action<bool>)(res => { }), message.StickerItem, message.Referrer);
        }

        public void InitiateShare()
        {
            this._appBarButtonShare_Click(this, null);
        }

        private void TextBlockMetaData_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.VM == null || string.IsNullOrEmpty(this.VM.MetaDataStr) || string.IsNullOrEmpty(this.textBlockMetaData.Text))
                return;
            Size newSize = e.NewSize;
            // ISSUE: explicit reference operation
            if (newSize.Height <= this.textBlockMetaData.LineHeight)
                return;
            this.textBlockMetaData.Text = (this.VM.MetaDataStr.Replace(" · ", "\n"));
        }

        private void Description_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.ExpandDescription();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCommentsPage.xaml", UriKind.Relative));
            this.scroll = (ViewportControl)base.FindName("scroll");
            this.stackPanel = (StackPanel)base.FindName("stackPanel");
            this.canvasBackground = (Canvas)base.FindName("canvasBackground");
            this.textBlockMetaData = (TextBlock)base.FindName("textBlockMetaData");
            this.ucCommentGeneric = (CommentsGenericUC)base.FindName("ucCommentGeneric");
            this.ucNewMessage = (NewMessageUC)base.FindName("ucNewMessage");
            this.ucMoreActions = (MoreActionsUC)base.FindName("ucMoreActions");
        }
    }
}
