using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Profiles.Users.Views;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.Views
{
    public class ProfilePage : PageBase
    {
        private bool _isInitialized;
        private ProfileViewModel _viewModel;
        private readonly ApplicationBar _defaultAppBar = new ApplicationBar();////
        private readonly ApplicationBarIconButton _appBarButtonAddNews;
        private readonly ApplicationBarIconButton _appBarButtonManagement;
        private readonly ApplicationBarMenuItem _appBarMenuItemEditProfile;
        private readonly ApplicationBarMenuItem _appBarMenuItemBanUnban;
        private readonly ApplicationBarMenuItem _appBarMenuItemRemoveFromFriends;
        private readonly ApplicationBarMenuItem _appBarMenuItemFaveUnfave;
        private readonly ApplicationBarMenuItem _appBarMenuItemSubscribeUnsubscribe;
        private readonly ApplicationBarMenuItem _appBarMenuItemPinToStart;
        private readonly ApplicationBarMenuItem _appBarMenuItemCopyLink;
        private readonly ApplicationBarMenuItem _appBarMenuItemOpenInBrowser;
        internal VisualStateGroup Common;
        internal VisualState Loading;
        internal VisualState Reloading;
        internal VisualState Blocked;
        internal VisualState Private;
        internal VisualState LoadingFailed;
        internal VisualState Loaded;
        internal ViewportControl viewportControl;
        internal StackPanel stackPanelRoot;
        internal Canvas canvasBackground;
        internal Grid gridHeader;
        internal ProfileInfoHeaderUC ucProfileInfoHeader;
        internal ContextMenu PhotoMenu;
        internal Border borderHeaderPlaceholder;
        internal MediaItemsHorizontalUC ucMedia;
        internal StackPanel stackPanelNotLoaded;
        internal ProgressRing progressRing;
        internal TextBlock textBlockLoadingStatus;
        internal Button buttonTryAgain;
        internal MyVirtualizingPanel2 wallPanel;
        internal GenericHeaderUC ucHeader;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;

        public ProfileViewModel ViewModel
        {
            get
            {
                return this._viewModel;
            }
        }

        public ProfilePage()
        {
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            applicationBarIconButton1.Text = CommonResources.MainPage_News_AddNews;
            Uri uri1 = new Uri("/Resources/AppBarNewPost-WXGA.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            this._appBarButtonAddNews = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            applicationBarIconButton2.Text = CommonResources.Management;
            Uri uri2 = new Uri("/Resources/feature.settings.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            this._appBarButtonManagement = applicationBarIconButton2;
            this._appBarMenuItemEditProfile = new ApplicationBarMenuItem(CommonResources.EditProfile.ToLowerInvariant());
            this._appBarMenuItemBanUnban = new ApplicationBarMenuItem(CommonResources.BannedUsers_BanUser);
            this._appBarMenuItemRemoveFromFriends = new ApplicationBarMenuItem(CommonResources.Profile_RemoveFromFriends.ToLowerInvariant());
            this._appBarMenuItemFaveUnfave = new ApplicationBarMenuItem(CommonResources.AddToBookmarks);
            this._appBarMenuItemSubscribeUnsubscribe = new ApplicationBarMenuItem(CommonResources.SubscribeToNews);
            this._appBarMenuItemPinToStart = new ApplicationBarMenuItem(CommonResources.PinToStart);
            this._appBarMenuItemCopyLink = new ApplicationBarMenuItem(CommonResources.CopyLink);
            this._appBarMenuItemOpenInBrowser = new ApplicationBarMenuItem(CommonResources.OpenInBrowser.ToLowerInvariant());

            this.InitializeComponent();
            this.wallPanel.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.viewportControl), false);
            this.viewportControl.BindViewportBoundsTo((FrameworkElement)this.stackPanelRoot);
            this.ucPullToRefresh.Visibility = Visibility.Visible;
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.wallPanel);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.wallPanel);
            this.wallPanel.OnRefresh = (Action)(() => this._viewModel.LoadInfo(true));
            this.ucHeader.OnHeaderTap = (Action)(() =>
            {
                if (this.CommonParameters.UserOrGroupId > 0L)
                    this.UpdateHeaderOpacityWithScrollPosition(0.0);
                this.wallPanel.ScrollToBottom(false);
            });
            this.CreateAppBar();
        }

        private void CreateAppBar()
        {
            this._appBarButtonAddNews.Click += new EventHandler(this.AppBarButtonAddNews_OnClick);
            this._appBarButtonManagement.Click += new EventHandler(this.AppBarButtonManagement_OnClick);
            this._appBarMenuItemEditProfile.Click += new EventHandler(this.AppBarMenuItemEditProfile_OnClick);
            this._appBarMenuItemPinToStart.Click += new EventHandler(this.AppBarMenuItemPinToStart_OnClick);
            this._appBarMenuItemBanUnban.Click += new EventHandler(this.AppBarMenuItemBanUnban_OnClick);
            this._appBarMenuItemRemoveFromFriends.Click += new EventHandler(this.AppBarMenuItemRemoveFromFriends_OnClick);
            this._appBarMenuItemFaveUnfave.Click += new EventHandler(this.AppBarMenuItemFaveUnfave_OnClick);
            this._appBarMenuItemSubscribeUnsubscribe.Click += new EventHandler(this.AppBarMenuItemSubscribeUnsubscribe_OnClick);
            this._appBarMenuItemCopyLink.Click += new EventHandler(this.AppBarMenuItemCopyLink_OnClick);
            this._appBarMenuItemOpenInBrowser.Click += new EventHandler(this.AppBarMenuItemOpenInBrowser_OnClick);
            this._defaultAppBar.Opacity = 0.9;
        }

        private void AppBarMenuItemEditProfile_OnClick(object sender, EventArgs eventArgs)
        {
            Navigator.Current.NavigateToEditProfile();
        }

        private void AppBarButtonAddNews_OnClick(object sender, EventArgs e)
        {
            this._viewModel.NavigateToNewWallPost();
        }

        private void AppBarButtonManagement_OnClick(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagement(-this._viewModel.Id, this._viewModel.CommunityModel.GroupType, this._viewModel.IsAdministrator);
        }

        private void AppBarMenuItemPinToStart_OnClick(object sender, EventArgs e)
        {
            this._viewModel.PinToStart();
        }

        private void AppBarMenuItemBanUnban_OnClick(object sender, EventArgs e)
        {
            this._viewModel.BanUnban();
        }

        private void AppBarMenuItemRemoveFromFriends_OnClick(object sender, EventArgs e)
        {
            this._viewModel.RemoveFromFriends();
        }

        private void AppBarMenuItemFaveUnfave_OnClick(object sender, EventArgs e)
        {
            this._viewModel.FaveUnfave();
        }

        private void AppBarMenuItemSubscribeUnsubscribe_OnClick(object sender, EventArgs e)
        {
            this._viewModel.SubscribeUnsubscribe();
        }

        private void AppBarMenuItemCopyLink_OnClick(object sender, EventArgs e)
        {
            this._viewModel.CopyLink();
        }

        private void AppBarMenuItemOpenInBrowser_OnClick(object sender, EventArgs e)
        {
            this._viewModel.OpenInBrowser();
        }

        private void UpdateAppBar()
        {
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || (this.IsMenuOpen || this.Flyouts.Count > 0))
                return;
            this._defaultAppBar.MenuItems.Clear();
            if (this._viewModel.CanPost || this._viewModel.CanSuggestAPost)
            {
                if (!this._defaultAppBar.Buttons.Contains((object)this._appBarButtonAddNews))
                {
                    this._appBarButtonAddNews.Text = this._viewModel.CanPost ? CommonResources.MainPage_News_AddNews : CommonResources.SuggestedNews_SuggestAPost;
                    this._defaultAppBar.Buttons.Add((object)this._appBarButtonAddNews);
                }
            }
            else if (this._defaultAppBar.Buttons.Contains((object)this._appBarButtonAddNews))
                this._defaultAppBar.Buttons.Remove((object)this._appBarButtonAddNews);
            if (this._viewModel.CanManageCommunity && !this._defaultAppBar.Buttons.Contains((object)this._appBarButtonManagement))
                this._defaultAppBar.Buttons.Add((object)this._appBarButtonManagement);
            if (!this._viewModel.CanManageCommunity && this._defaultAppBar.Buttons.Contains((object)this._appBarButtonManagement))
                this._defaultAppBar.Buttons.Remove((object)this._appBarButtonManagement);
            if (this._viewModel.CanEditProfile)
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemEditProfile);
            if (this._viewModel.CanSubscribeUnsubscribe)
            {
                this._appBarMenuItemSubscribeUnsubscribe.Text = this._viewModel.IsSubscribed ? CommonResources.UnsubscribeFromNews : CommonResources.SubscribeToNews;
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemSubscribeUnsubscribe);
            }
            if (this._viewModel.CanPinToStart)
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemPinToStart);
            this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemCopyLink);
            this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemOpenInBrowser);
            if (this._viewModel.CanFaveUnfave)
            {
                this._appBarMenuItemFaveUnfave.Text = this._viewModel.IsFavorite ? CommonResources.RemoveFromBookmarks : CommonResources.AddToBookmarks;
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemFaveUnfave);
            }
            if (this._viewModel.CanBanUnban)
            {
                this._appBarMenuItemBanUnban.Text = this._viewModel.IsBlacklistedByMe ? CommonResources.BannedUsers_UnbanUser : CommonResources.BannedUsers_BanUser;
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemBanUnban);
            }
            if (this._defaultAppBar.MenuItems.Count > 0 || this._defaultAppBar.Buttons.Count > 0)
            {
                this.ApplicationBar = (IApplicationBar)this._defaultAppBar;
                this.ApplicationBar.Mode = this._defaultAppBar.Buttons.Count == 0 ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
            }
            else
                this.ApplicationBar = (IApplicationBar)null;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                string name = this.NavigationContext.QueryString.ContainsKey("Name") ? this.NavigationContext.QueryString["Name"] : "";
                string source = this.NavigationContext.QueryString.ContainsKey("Source") ? this.NavigationContext.QueryString["Source"] : "";
                if (this.CommonParameters.UserOrGroupId > 0L)
                {
                    this.ucPullToRefresh.ForegroundBrush = (Brush)Application.Current.Resources["PhoneNameBlueBrush"];
                    this.UpdateHeaderOpacity(0.0);
                    this.viewportControl.ViewportChanged += new EventHandler<ViewportChangedEventArgs>(this.ViewportControl_OnViewportChanged);
                }
                this._viewModel = new ProfileViewModel(this.CommonParameters.UserOrGroupId, name, source)
                {
                    LoadingStatusUpdated = new Action<ProfileLoadingStatus>(this.HandleLoadingStatusUpdated),
                    AppBarPropertyUpdated = new Action(this.UpdateAppBar)
                };
                this._viewModel.LoadInfo(false);
                this.DataContext = (object)this._viewModel;
                this._isInitialized = true;
            }
            else
            {
                this.ucProfileInfoHeader.Reload();
                this.ucMedia.Reload();
            }
            this.ProcessInputParameters();
            this.UpdateAppBar();
            if (this.CommonParameters.UserOrGroupId >= 0L)
            {
                CurrentMediaSource.AudioSource = StatisticsActionSource.wall_user;
                CurrentMediaSource.VideoSource = StatisticsActionSource.wall_user;
                CurrentMediaSource.GifPlaySource = StatisticsActionSource.wall_user;
                CurrentNewsFeedSource.Source = ViewPostSource.UserWall;
            }
            else
            {
                CurrentMediaSource.AudioSource = StatisticsActionSource.wall_group;
                CurrentMediaSource.VideoSource = StatisticsActionSource.wall_group;
                CurrentMediaSource.GifPlaySource = StatisticsActionSource.wall_group;
                CurrentNewsFeedSource.Source = ViewPostSource.GroupWall;
            }
            CurrentMediaSource.VideoContext = this.CommonParameters.UserOrGroupId.ToString();
            CurrentMarketItemSource.Source = MarketItemSource.wall;
            CurrentCommunitySource.Source = this.CommonParameters.UserOrGroupId > 0L ? (this.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId ? CommunityOpenSource.OwnProfileOrOwnMedia : CommunityOpenSource.UserProfileOrUserMedia) : CommunityOpenSource.OtherCommunityOrOtherCommunityMedia;
        }

        private void ProcessInputParameters()
        {
            Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (group != null)
            {
                foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>)this._viewModel.WallVM.Collection)
                {
                    WallPostItem wallPostItem = virtualizable as WallPostItem;
                    if (wallPostItem != null && wallPostItem.LikesAndCommentsItem != null)
                    {
                        if (wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(group.id, group.name))
                            break;
                    }
                }
            }
            List<Stream> streamList = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            Rect rect = new Rect();
            if (ParametersRepository.Contains("UserPicSquare"))
                rect = (Rect)ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
            if (streamList == null || streamList.Count <= 0)
                return;
            this._viewModel.UploadProfilePhoto(streamList[0], rect);
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.ucProfileInfoHeader.Unload();
            this.ucMedia.Unload();
            GC.Collect();
        }

        private void ViewportControl_OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            this.UpdateHeaderOpacityWithScrollPosition(this.viewportControl.Viewport.Y);
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            string stateName;
            switch (status)
            {
                case ProfileLoadingStatus.Loading:
                    stateName = "Loading";
                    break;
                case ProfileLoadingStatus.Reloading:
                    stateName = "Reloading";
                    break;
                case ProfileLoadingStatus.Loaded:
                    stateName = "Loaded";
                    break;
                case ProfileLoadingStatus.LoadingFailed:
                    stateName = "LoadingFailed";
                    break;
                case ProfileLoadingStatus.Deleted:
                case ProfileLoadingStatus.Banned:
                case ProfileLoadingStatus.Blacklisted:
                case ProfileLoadingStatus.Service:
                    stateName = "Blocked";
                    break;
                case ProfileLoadingStatus.Private:
                    stateName = "Private";
                    break;
                default:
                    return;
            }
            VisualStateManager.GoToState((Control)this, stateName, false);
        }

        private void UpdateHeaderOpacityWithScrollPosition(double scrollPosition)
        {
            this.UpdateHeaderOpacity(ProfilePage.CalculateOpacity(scrollPosition, 200.0, 224.0));
            this.ucProfileInfoHeader.SetOverlayOpacity(ProfilePage.CalculateOpacity(scrollPosition, 96.0, 200.0));
        }

        private void UpdateHeaderOpacity(double opacity)
        {
            this.ucHeader.rectBackground.Opacity = opacity;
            this.ucHeader.textBlockTitle.Opacity = opacity;
            this.ucHeader.borderCounter.Opacity = opacity;
        }

        private static double CalculateOpacity(double sp, double minSP, double maxSP)
        {
            double num1;
            if (sp < minSP)
                num1 = 0.0;
            else if (sp > maxSP)
            {
                num1 = 1.0;
            }
            else
            {
                double num2 = maxSP - minSP;
                num1 = 1.0 / num2 * sp - minSP / num2;
            }
            return num1;
        }

        private void TryAgainButton_OnClick(object sender, RoutedEventArgs e)
        {
            this._viewModel.LoadInfo(false);
        }

        private void StackPanelMainInfo_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = e.NewSize.Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            this.wallPanel.DeltaOffset = -height;
            this.canvasBackground.Height = height;
            this.canvasBackground.Children.Clear();
            Rectangle rect = new Rectangle();
            double num = height;
            rect.Height = num;
            Thickness thickness = new Thickness(0.0);
            rect.Margin = thickness;
            double width = e.NewSize.Width;
            rect.Width = width;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            rect.Fill = (Brush)solidColorBrush;
            foreach (UIElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
                this.canvasBackground.Children.Add(coverByRectangle);
        }

        private void UcProfileInfoHeader_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._viewModel.HeaderViewModel.HasAvatar)
            {
                if (this._viewModel.CanChangePhoto)
                    this.PhotoMenu.IsOpen = true;
                else
                    this._viewModel.OpenProfilePhotos();
            }
            else
            {
                if (!this._viewModel.CanChangePhoto)
                    return;
                this._viewModel.PickNewPhoto();
            }
        }

        private void UcProfileInfoHeader_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void AddPhoto_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._viewModel.PickNewPhoto();
        }

        private void ChoosePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this._viewModel.PickNewPhoto();
        }

        private void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(CommonResources.DeleteConfirmation, CommonResources.DeleteOnePhoto, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this._viewModel.DeletePhoto();
        }

        private void OpenPhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this._viewModel.OpenProfilePhotos();
        }

        private void SupportHyperlink_OnClicked(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToConversation(333L, false, false, "", 0L, false);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml", UriKind.Relative));
            this.Common = (VisualStateGroup)this.FindName("Common");
            this.Loading = (VisualState)this.FindName("Loading");
            this.Reloading = (VisualState)this.FindName("Reloading");
            this.Blocked = (VisualState)this.FindName("Blocked");
            this.Private = (VisualState)this.FindName("Private");
            this.LoadingFailed = (VisualState)this.FindName("LoadingFailed");
            this.Loaded = (VisualState)this.FindName("Loaded");
            this.viewportControl = (ViewportControl)this.FindName("viewportControl");
            this.stackPanelRoot = (StackPanel)this.FindName("stackPanelRoot");
            this.canvasBackground = (Canvas)this.FindName("canvasBackground");
            this.gridHeader = (Grid)this.FindName("gridHeader");
            this.ucProfileInfoHeader = (ProfileInfoHeaderUC)this.FindName("ucProfileInfoHeader");
            this.PhotoMenu = (ContextMenu)this.FindName("PhotoMenu");
            this.borderHeaderPlaceholder = (Border)this.FindName("borderHeaderPlaceholder");
            this.ucMedia = (MediaItemsHorizontalUC)this.FindName("ucMedia");
            this.stackPanelNotLoaded = (StackPanel)this.FindName("stackPanelNotLoaded");
            this.progressRing = (ProgressRing)this.FindName("progressRing");
            this.textBlockLoadingStatus = (TextBlock)this.FindName("textBlockLoadingStatus");
            this.buttonTryAgain = (Button)this.FindName("buttonTryAgain");
            this.wallPanel = (MyVirtualizingPanel2)this.FindName("wallPanel");
            this.ucHeader = (GenericHeaderUC)this.FindName("ucHeader");
            this.ucPullToRefresh = (PullToRefreshUC)this.FindName("ucPullToRefresh");
        }
    }
}
