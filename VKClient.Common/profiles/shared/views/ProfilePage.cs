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
using System.Windows.Input;
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
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.Views
{
    public class ProfilePage : PageBase
    {
        private bool _isInitialized;
        private ProfileViewModel _viewModel;
        private readonly ApplicationBar _defaultAppBar;
        private readonly ApplicationBarIconButton _appBarButtonAddNews;
        private readonly ApplicationBarIconButton _appBarButtonManagement;
        private readonly ApplicationBarIconButton _appBarButtonSendGift;
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
        internal MyVirtualizingStackPanel stackPanelRoot;
        internal Canvas canvasBackground;
        internal Grid gridHeader;
        internal VKClient.Common.Profiles.Users.Views.ProfileInfoHeaderUC ucUserProfileInfoHeader;
        internal VKClient.Common.Profiles.Groups.Views.ProfileInfoHeaderUC ucGroupProfileInfoHeader;
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
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = appBarBgColor;
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = appBarFgColor;
            this._defaultAppBar = applicationBar;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            string mainPageNewsAddNews = CommonResources.MainPage_News_AddNews;
            applicationBarIconButton1.Text = mainPageNewsAddNews;
            Uri uri1 = new Uri("/Resources/AppBarNewPost-WXGA.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            this._appBarButtonAddNews = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            string management = CommonResources.Management;
            applicationBarIconButton2.Text = management;
            Uri uri2 = new Uri("/Resources/feature.settings.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            this._appBarButtonManagement = applicationBarIconButton2;
            ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
            string lowerInvariant = CommonResources.SendGift.ToLowerInvariant();
            applicationBarIconButton3.Text = lowerInvariant;
            Uri uri3 = new Uri("/Resources/AppBarGift.png", UriKind.Relative);
            applicationBarIconButton3.IconUri = uri3;
            this._appBarButtonSendGift = applicationBarIconButton3;
            this._appBarMenuItemEditProfile = new ApplicationBarMenuItem(CommonResources.EditProfile.ToLowerInvariant());
            this._appBarMenuItemBanUnban = new ApplicationBarMenuItem(CommonResources.BannedUsers_BanUser);
            this._appBarMenuItemRemoveFromFriends = new ApplicationBarMenuItem(CommonResources.Profile_RemoveFromFriends.ToLowerInvariant());
            this._appBarMenuItemFaveUnfave = new ApplicationBarMenuItem(CommonResources.AddToBookmarks);
            this._appBarMenuItemSubscribeUnsubscribe = new ApplicationBarMenuItem(CommonResources.SubscribeToNews);
            this._appBarMenuItemPinToStart = new ApplicationBarMenuItem(CommonResources.PinToStart);
            this._appBarMenuItemCopyLink = new ApplicationBarMenuItem(CommonResources.CopyLink);
            this._appBarMenuItemOpenInBrowser = new ApplicationBarMenuItem(CommonResources.OpenInBrowser.ToLowerInvariant());
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            this.InitializeComponent();
            this.wallPanel.ExtraOffsetY = 96.0;
            this.wallPanel.InitializeWithScrollViewer(new ViewportScrollableAreaAdapter(this.viewportControl), false);
            this.viewportControl.BindViewportBoundsTo(this.stackPanelRoot);
            this.ucPullToRefresh.Visibility = Visibility.Visible;
            this.ucPullToRefresh.TrackListBox(this.wallPanel);
            base.RegisterForCleanup(this.wallPanel);
            this.wallPanel.OnRefresh = delegate
            {
                this._viewModel.LoadInfo(true);
            };
            this.ucHeader.OnHeaderTap = delegate
            {
                this.wallPanel.ScrollToBottom(false);
            };
            this.CreateAppBar();
        }

        private void CreateAppBar()
        {
            this._appBarButtonAddNews.Click += (new EventHandler(this.AppBarButtonAddNews_OnClick));
            this._appBarButtonManagement.Click += (new EventHandler(this.AppBarButtonManagement_OnClick));
            this._appBarButtonSendGift.Click += (new EventHandler(this.AppBarButtonSendGift_OnClick));
            this._appBarMenuItemEditProfile.Click += (new EventHandler(this.AppBarMenuItemEditProfile_OnClick));
            this._appBarMenuItemPinToStart.Click += (new EventHandler(this.AppBarMenuItemPinToStart_OnClick));
            this._appBarMenuItemBanUnban.Click += (new EventHandler(this.AppBarMenuItemBanUnban_OnClick));
            this._appBarMenuItemRemoveFromFriends.Click += (new EventHandler(this.AppBarMenuItemRemoveFromFriends_OnClick));
            this._appBarMenuItemFaveUnfave.Click += (new EventHandler(this.AppBarMenuItemFaveUnfave_OnClick));
            this._appBarMenuItemSubscribeUnsubscribe.Click += (new EventHandler(this.AppBarMenuItemSubscribeUnsubscribe_OnClick));
            this._appBarMenuItemCopyLink.Click += (new EventHandler(this.AppBarMenuItemCopyLink_OnClick));
            this._appBarMenuItemOpenInBrowser.Click += (new EventHandler(this.AppBarMenuItemOpenInBrowser_OnClick));
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

        private void AppBarButtonSendGift_OnClick(object sender, EventArgs e)
        {
            EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.profile, GiftPurchaseStepsAction.store));
            Navigator.Current.NavigateToGiftsCatalog(this._viewModel.Id, false);
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
            if (base.ImageViewerDecorator != null && base.ImageViewerDecorator.IsShown || (base.IsMenuOpen || base.Flyouts.Count > 0))
                return;
            this._defaultAppBar.MenuItems.Clear();
            if (this._viewModel.CanPost || this._viewModel.CanSuggestAPost)
            {
                if (!this._defaultAppBar.Buttons.Contains(this._appBarButtonAddNews))
                {
                    this._appBarButtonAddNews.Text = (this._viewModel.CanPost ? CommonResources.MainPage_News_AddNews : CommonResources.SuggestedNews_SuggestAPost);
                    this._defaultAppBar.Buttons.Add(this._appBarButtonAddNews);
                }
            }
            else if (this._defaultAppBar.Buttons.Contains(this._appBarButtonAddNews))
                this._defaultAppBar.Buttons.Remove(this._appBarButtonAddNews);
            if (this._viewModel.CanSendGift)
            {
                if (!this._defaultAppBar.Buttons.Contains(this._appBarButtonSendGift))
                    this._defaultAppBar.Buttons.Add(this._appBarButtonSendGift);
            }
            else if (this._defaultAppBar.Buttons.Contains(this._appBarButtonSendGift))
                this._defaultAppBar.Buttons.Remove(this._appBarButtonSendGift);
            if (this._viewModel.CanManageCommunity && !this._defaultAppBar.Buttons.Contains(this._appBarButtonManagement))
                this._defaultAppBar.Buttons.Add(this._appBarButtonManagement);
            if (!this._viewModel.CanManageCommunity && this._defaultAppBar.Buttons.Contains(this._appBarButtonManagement))
                this._defaultAppBar.Buttons.Remove(this._appBarButtonManagement);
            if (this._viewModel.CanEditProfile)
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemEditProfile);
            if (this._viewModel.CanSubscribeUnsubscribe)
            {
                this._appBarMenuItemSubscribeUnsubscribe.Text = (this._viewModel.IsSubscribed ? CommonResources.UnsubscribeFromNews : CommonResources.SubscribeToNews);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemSubscribeUnsubscribe);
            }
            if (this._viewModel.CanPinToStart)
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemPinToStart);
            this._defaultAppBar.MenuItems.Add(this._appBarMenuItemCopyLink);
            this._defaultAppBar.MenuItems.Add(this._appBarMenuItemOpenInBrowser);
            if (this._viewModel.CanFaveUnfave)
            {
                this._appBarMenuItemFaveUnfave.Text = (this._viewModel.IsFavorite ? CommonResources.RemoveFromBookmarks : CommonResources.AddToBookmarks);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemFaveUnfave);
            }
            if (this._viewModel.CanBanUnban)
            {
                this._appBarMenuItemBanUnban.Text = (this._viewModel.IsBlacklistedByMe ? CommonResources.BannedUsers_UnbanUser : CommonResources.BannedUsers_BanUser);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemBanUnban);
            }
            if (this._defaultAppBar.MenuItems.Count > 0 || this._defaultAppBar.Buttons.Count > 0)
            {
                base.ApplicationBar = ((IApplicationBar)this._defaultAppBar);
                base.ApplicationBar.Mode = (this._defaultAppBar.Buttons.Count == 0 ? (ApplicationBarMode)1 : (ApplicationBarMode)0);
            }
            else
                base.ApplicationBar = (null);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                string name = base.NavigationContext.QueryString.ContainsKey("Name") ? base.NavigationContext.QueryString["Name"] : "";
                string source = base.NavigationContext.QueryString.ContainsKey("Source") ? base.NavigationContext.QueryString["Source"] : "";
                this.UpdateHeaderOpacity(0.0);
                this._viewModel = new ProfileViewModel(base.CommonParameters.UserOrGroupId, name, source)
                {
                    LoadingStatusUpdated = new Action<ProfileLoadingStatus>(this.HandleLoadingStatusUpdated),
                    AppBarPropertyUpdated = new Action(this.UpdateAppBar)
                };
                this._viewModel.LoadInfo(false);
                base.DataContext = this._viewModel;
                this._isInitialized = true;
            }
            else
            {
                this.ucUserProfileInfoHeader.Reload();
                this.ucMedia.Reload();
            }
            this.ProcessInputParameters();
            this.UpdateAppBar();
            base.IsMenuOpenChanged = base.IsMenuOpenChanged + (EventHandler)((o, args) => this.UpdateAppBar());
            if (base.CommonParameters.UserOrGroupId >= 0L)
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
            long userOrGroupId = base.CommonParameters.UserOrGroupId;
            CurrentMediaSource.VideoContext = (userOrGroupId > 0L ? userOrGroupId : -userOrGroupId).ToString();
            CurrentMarketItemSource.Source = MarketItemSource.wall;
            CurrentCommunitySource.Source = base.CommonParameters.UserOrGroupId > 0L ? (this.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId ? CommunityOpenSource.OwnProfileOrOwnMedia : CommunityOpenSource.UserProfileOrUserMedia) : CommunityOpenSource.OtherCommunityOrOtherCommunityMedia;
        }

        private void ProcessInputParameters()
        {
            Group parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (parameterForIdAndReset1 != null)
            {
                foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>)this._viewModel.WallVM.Collection)
                {
                    WallPostItem wallPostItem = virtualizable as WallPostItem;
                    if (wallPostItem != null && wallPostItem.LikesAndCommentsItem != null)
                    {
                        if (wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(parameterForIdAndReset1.id, parameterForIdAndReset1.name))
                            break;
                    }
                }
            }
            List<Stream> parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            Rect rect = new Rect();
            if (ParametersRepository.Contains("UserPicSquare"))
                rect = (Rect)ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
            if (parameterForIdAndReset2 == null || parameterForIdAndReset2.Count <= 0)
                return;
            this._viewModel.UploadProfilePhoto(parameterForIdAndReset2[0], rect);
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.ucUserProfileInfoHeader.Unload();
            this.ucMedia.Unload();
            GC.Collect();
        }

        private void ViewportControl_OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            Rect viewport = this.viewportControl.Viewport;
            // ISSUE: explicit reference operation
            this.UpdateHeaderOpacityWithScrollPosition(viewport.Y);
        }

        private void HandleLoadingStatusUpdated(ProfileLoadingStatus status)
        {
            this.UpdatePageState(status);
            if (status == ProfileLoadingStatus.Loading || status == ProfileLoadingStatus.Reloading)
                return;
            this.UpdateHeaderOpacity();
        }

        private void UpdatePageState(ProfileLoadingStatus status)
        {
            string str;
            switch (status)
            {
                case ProfileLoadingStatus.Loading:
                    str = "Loading";
                    break;
                case ProfileLoadingStatus.Reloading:
                    str = "Reloading";
                    break;
                case ProfileLoadingStatus.Loaded:
                    str = "Loaded";
                    break;
                case ProfileLoadingStatus.LoadingFailed:
                    str = "LoadingFailed";
                    break;
                case ProfileLoadingStatus.Deleted:
                case ProfileLoadingStatus.Banned:
                case ProfileLoadingStatus.Blacklisted:
                case ProfileLoadingStatus.Service:
                    str = "Blocked";
                    break;
                case ProfileLoadingStatus.Private:
                    str = "Private";
                    break;
                default:
                    return;
            }
            VisualStateManager.GoToState((Control)this, str, false);
        }

        private void UpdateHeaderOpacity()
        {
            if (this._viewModel != null && this._viewModel.CanAnimateHeaderOpacity)
            {
                this.UpdateHeaderOpacity(0.0);
                this.ucPullToRefresh.ForegroundBrush = (Brush)Application.Current.Resources["PhoneNameBlueBrush"];
                this.viewportControl.ViewportChanged -= (new EventHandler<ViewportChangedEventArgs>(this.ViewportControl_OnViewportChanged));
                this.viewportControl.ViewportChanged += (new EventHandler<ViewportChangedEventArgs>(this.ViewportControl_OnViewportChanged));
            }
            else
                this.UpdateHeaderOpacity(1.0);
        }

        private void UpdateHeaderOpacityWithScrollPosition(double scrollPosition)
        {
            bool num1 = this._viewModel.Id > 0L;
            int num2 = num1 ? 200 : 32;
            int num3 = num1 ? 224 : 64;
            int num4 = num1 ? 96 : 0;
            int num5 = num1 ? 200 : 32;
            this.UpdateHeaderOpacity(ProfilePage.CalculateOpacity(scrollPosition, (double)num2, (double)num3));
            double opacity = ProfilePage.CalculateOpacity(scrollPosition, (double)num4, (double)num5);
            if (num1)
                this.ucUserProfileInfoHeader.SetOverlayOpacity(opacity);
            else
                this.ucGroupProfileInfoHeader.SetOverlayOpacity(opacity);
        }

        private void UpdateHeaderOpacity(double opacity)
        {
            this.ucHeader.rectBackground.Opacity = opacity;
            this.ucHeader.textBlockTitle.Opacity = opacity;
            this.ucHeader.counterPanel.Opacity = opacity;
            //
            if (opacity == 1.0)
                this.ucHeader.Shadow.Opacity = 1.0;
            else
                this.ucHeader.Shadow.Opacity = 0.0;
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
            rect.Height = height;
            rect.Margin = new Thickness(0.0);
            rect.Width = e.NewSize.Width;
            rect.Fill = (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            using (List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(rect).GetEnumerator())
            {
                while (enumerator.MoveNext())
                    this.canvasBackground.Children.Add(enumerator.Current);
            }
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
            if (MessageBox.Show(CommonResources.DeleteConfirmation, CommonResources.DeleteOnePhoto, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this._viewModel.DeletePhoto();
        }

        private void OpenPhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this._viewModel.OpenProfilePhotos();
        }

        private void SupportHyperlink_OnClicked(object sender, RoutedEventArgs e)
        {
            ProfileViewModel viewModel = this._viewModel;
            if (viewModel == null)
                return;
            viewModel.NavigateToSupport();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/ProfilePage.xaml", UriKind.Relative));
            this.Common = (VisualStateGroup)base.FindName("Common");
            this.Loading = (VisualState)base.FindName("Loading");
            this.Reloading = (VisualState)base.FindName("Reloading");
            this.Blocked = (VisualState)base.FindName("Blocked");
            this.Private = (VisualState)base.FindName("Private");
            this.LoadingFailed = (VisualState)base.FindName("LoadingFailed");
            this.Loaded = (VisualState)base.FindName("Loaded");
            this.viewportControl = (ViewportControl)base.FindName("viewportControl");
            this.stackPanelRoot = (MyVirtualizingStackPanel)base.FindName("stackPanelRoot");
            this.canvasBackground = (Canvas)base.FindName("canvasBackground");
            this.gridHeader = (Grid)base.FindName("gridHeader");
            this.ucUserProfileInfoHeader = (VKClient.Common.Profiles.Users.Views.ProfileInfoHeaderUC)base.FindName("ucUserProfileInfoHeader");
            this.ucGroupProfileInfoHeader = (VKClient.Common.Profiles.Groups.Views.ProfileInfoHeaderUC)base.FindName("ucGroupProfileInfoHeader");
            this.PhotoMenu = (ContextMenu)base.FindName("PhotoMenu");
            this.borderHeaderPlaceholder = (Border)base.FindName("borderHeaderPlaceholder");
            this.ucMedia = (MediaItemsHorizontalUC)base.FindName("ucMedia");
            this.stackPanelNotLoaded = (StackPanel)base.FindName("stackPanelNotLoaded");
            this.progressRing = (ProgressRing)base.FindName("progressRing");
            this.textBlockLoadingStatus = (TextBlock)base.FindName("textBlockLoadingStatus");
            this.buttonTryAgain = (Button)base.FindName("buttonTryAgain");
            this.wallPanel = (MyVirtualizingPanel2)base.FindName("wallPanel");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
        }
    }
}
