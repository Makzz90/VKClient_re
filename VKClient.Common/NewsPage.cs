using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
    public class NewsPage : PageBase
    {
        private bool _isInitialized;
        private ApplicationBar _appBarNews;
        private ApplicationBarMenuItem _gcMenuItem;
        private ApplicationBarIconButton _appBarButtonRefreshNews;
        private ApplicationBarIconButton _appBarButtonAddNews;
        private ApplicationBarIconButton _appBarButtonAttachImage;
        private ApplicationBarIconButton _appBarButtonLists;
        private static double _scrollPosition;
        private bool _needToScrollToOffset;
        private bool _photoFeedMoveTutorial;
        private readonly HideHeaderHelper _hideHelper;
        private ObservableCollection<PickableNewsfeedSourceItemViewModel> _newsSources;
        private ListPickerUC2 _picker;
        private DialogService _photoFeedMoveTutorialDialog;
        private DialogService _newsfeedTopPromoDialog;
        internal Grid LayoutRoot;
        internal Grid ContentPanel;
        internal ViewportControl scrollNews;
        internal MyVirtualizingStackPanel stackPanel;
        //internal NewsfeedNewPostUC ucNewPost;
        internal MyVirtualizingPanel2 panelNews;
        internal NewsfeedHeaderUC Header;
        internal Rectangle rectSystemTrayPlaceholder;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;

        private bool CanShowNewsfeedTopPromo
        {
            get
            {
                if ((FramePageUtils.CurrentPage).GetType() == (this).GetType() && ((UIElement)MenuUC.Instance).Opacity <= 0.0 && !this.ImageViewerDecorator.IsShown)
                    return this._photoFeedMoveTutorialDialog == null;
                return false;
            }
        }

        public NewsPage()
        {
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = appBarBgColor;
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = appBarFgColor;
            applicationBar.Opacity = 0.9;
            this._appBarNews = applicationBar;
            ApplicationBarMenuItem applicationBarMenuItem = new ApplicationBarMenuItem();
            applicationBarMenuItem.Text = "garbage collect";
            this._gcMenuItem = applicationBarMenuItem;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            string mainPageNewsRefresh = CommonResources.MainPage_News_Refresh;
            applicationBarIconButton1.Text = mainPageNewsRefresh;
            Uri uri1 = new Uri("/Resources/appbar.refresh.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            this._appBarButtonRefreshNews = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            string mainPageNewsAddNews = CommonResources.MainPage_News_AddNews;
            applicationBarIconButton2.Text = mainPageNewsAddNews;
            Uri uri2 = new Uri("/Resources/AppBarNewPost-WXGA.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            this._appBarButtonAddNews = applicationBarIconButton2;
            ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
            Uri uri3 = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative);
            applicationBarIconButton3.IconUri = uri3;
            string postAppBarAddPhoto = CommonResources.NewPost_AppBar_AddPhoto;
            applicationBarIconButton3.Text = postAppBarAddPhoto;
            this._appBarButtonAttachImage = applicationBarIconButton3;
            ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
            Uri uri4 = new Uri("/Resources/lists.png", UriKind.Relative);
            applicationBarIconButton4.IconUri = uri4;
            string appBarLists = CommonResources.AppBar_Lists;
            applicationBarIconButton4.Text = appBarLists;
            this._appBarButtonLists = applicationBarIconButton4;
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            this.InitializeComponent();
            //this.panelNews.ExtraOffsetY = ((FrameworkElement) this.rectSystemTrayPlaceholder).Height;//in 4.7 this is commented
            if (VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.HideSystemTray==true)
                rectSystemTrayPlaceholder.Height = 0;//mod
            this.panelNews.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.scrollNews), false);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.panelNews);
            this.scrollNews.BindViewportBoundsTo(this.stackPanel);
            this.BuildAppBar();
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.NewsPage_Loaded));
            this.Header.OnFreshNewsTap = new Action(this.OnFreshNewsTap);
            this.Header.ucHeader.OnHeaderTap = (Action)(() => this.OnHeaderTap(true));
            this.Header.ucHeader.OnTitleTap = new Action(this.OpenNewsSourcePicker);
            this.Header.ucHeader.borderMenuOpenIcon.Visibility = Visibility.Visible;
            this._hideHelper = new HideHeaderHelper(this.Header, this.scrollNews, this);
            //((FrameworkElement)this.ucNewPost).DataContext = MenuViewModel.Instance;
        }

        private void BuildAppBar()
        {
            this._appBarButtonAttachImage.Click += (new EventHandler(this._appBarButtonAttachImage_Click));
            this._appBarButtonAddNews.Click += (new EventHandler(this._appBarMenuItemAddNews_Click));
            this._appBarButtonRefreshNews.Click += (new EventHandler(this._appBarMenuItemRefreshNews_Click));
            this._appBarButtonLists.Click += (new EventHandler(this._appBarButtonLists_Click));
        }

        private void _appBarButtonAttachImage_Click(object sender, EventArgs e)
        {
            ParametersRepository.SetParameterForId("GoPickImage", true);
            Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
        }

        private void _appBarMenuItemAddNews_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
        }

        private void _appBarMenuItemRefreshNews_Click(object sender, EventArgs e)
        {
            NewsViewModel.Instance.NewsFeedVM.LoadData(true, false, null, false);
        }

        private void _appBarButtonLists_Click(object sender, EventArgs e)
        {
        }

        private void _gcMenuItem_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        public static void Reset()
        {
            NewsPage._scrollPosition = 0.0;
        }

        private void NewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._needToScrollToOffset)
            {
                this.panelNews.ScrollTo(NewsPage._scrollPosition);
                this.panelNews.Opacity = 1.0;
                this._needToScrollToOffset = false;
            }
            if (!this._photoFeedMoveTutorial)
                return;
            this.ShowPhotoFeedMoveTutorial();
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            base.OnRemovedFromJournal(e);
            this.Header.ucHeader.CleanupBinding();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                bool flag = false;
                long newsSourceId = 0;
                if (((Page)this).NavigationContext.QueryString.ContainsKey("NewsSourceId"))
                    newsSourceId = (long)int.Parse(((Page)this).NavigationContext.QueryString["NewsSourceId"]);
                if (((Page)this).NavigationContext.QueryString.ContainsKey("PhotoFeedMoveTutorial"))
                    this._photoFeedMoveTutorial = ((Page)this).NavigationContext.QueryString["PhotoFeedMoveTutorial"] == bool.TrueString;
                if (newsSourceId == 0L && NewsViewModel.Instance.ForceNewsFeedUpdate)
                    newsSourceId = NewsSources.NewsFeed.PickableItem.ID;
                if (newsSourceId != 0L)
                {
                    NewsViewModel instance = NewsViewModel.Instance;
                    PickableNewsfeedSourceItemViewModel m0 = Enumerable.FirstOrDefault<PickableNewsfeedSourceItemViewModel>(NewsSources.GetAllPredefinedNewsSources(), (Func<PickableNewsfeedSourceItemViewModel, bool>)(item => item.PickableItem.ID == newsSourceId));
                    PickableItem pickableItem = m0 != null ? m0.PickableItem : null;
                    instance.NewsSource = pickableItem;
                    flag = true;
                }
                base.DataContext = NewsViewModel.Instance;
                //NewsViewModel.Instance.EnsureUpToDate();
                if (NewsViewModel.Instance.NewsFeedVM.Collection.Count == 0)
                    NewsViewModel.Instance.ReloadNews(true, true, false);
                //
                NewsViewModel.Instance.FreshNewsStateChangedCallback = new Action<FreshNewsState>(this.FreshNewsStateChangedCallback);
                if (e.NavigationMode == NavigationMode.New && NewsPage._scrollPosition != 0.0 && !flag)
                {
                    this._needToScrollToOffset = true;
                    this.panelNews.Opacity = 0.0;
                }
                //this.AskToast();//todo:delete?
                this.ucPullToRefresh.TrackListBox(this.panelNews);
                this.panelNews.OnRefresh = (Action)(() => NewsViewModel.Instance.ReloadNews(false, true, false));
                this._isInitialized = true;
            }
            this.UpdateKeepScrollPosition();
            NewsViewModel.Instance.KeepScrollPositionChanged = new Action(this.UpdateKeepScrollPosition);
            NewsViewModel.Instance.ShowNewsfeedTopPromoAction = new Action<UserNotification>(this.ShowNewsfeedTopPromo);
            CurrentMediaSource.AudioSource = StatisticsActionSource.news;
            CurrentMediaSource.VideoSource = StatisticsActionSource.news;
            CurrentMediaSource.GifPlaySource = StatisticsActionSource.news;
            CurrentMarketItemSource.Source = MarketItemSource.feed;
            CurrentNewsFeedSource.Source = ViewPostSource.NewsFeed;
            CurrentCommunitySource.Source = CommunityOpenSource.Newsfeed;
            NewsViewModel.Instance.UpdateCurrentNewsFeedSource();
            this.ProcessInputParameters();
            this.HandleProtocolLaunchIfNeeded();
            this.CheckFreshNews();
            PickableItem newsSource = NewsViewModel.Instance.NewsSource;
            long? nullable = newsSource != null ? new long?(newsSource.ID) : new long?();
            long id = NewsSources.NewsFeed.PickableItem.ID;
            if ((nullable.GetValueOrDefault() == id ? (nullable.HasValue ? 1 : 0) : 0) == 0 || !NewsViewModel.Instance.ForceNewsFeedUpdate)
                return;
            NewsViewModel.Instance.ReloadNews(true, true, false);
        }

        private void UpdateKeepScrollPosition()
        {
            this.panelNews.KeepScrollPositionWhenAddingItems = NewsViewModel.Instance.KeepScrollPosition;
        }

        private void CheckFreshNews()
        {
            NewsViewModel.Instance.CheckForFreshNewsIfNeeded(this.scrollNews.Viewport.Y - 176.0 + 32.0);
        }

        private void FreshNewsStateChangedCallback(FreshNewsState state)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (state == FreshNewsState.ForcedReload)
                {
                    NewsViewModel.Instance.ReplaceAllWithPendingFreshNews();
                    this.OnHeaderTap(false);
                    state = NewsViewModel.Instance.FreshNewsState;
                }
                this.Header.IsLoadingFreshNews = false;
                this._hideHelper.UpdateFreshNewsState(state);
                if (state == FreshNewsState.NoNews)
                    return;
                this._hideHelper.ShowFreshNews();
            }));
        }

        private void HandleProtocolLaunchIfNeeded()
        {
            if (AppGlobalStateManager.Current.IsUserLoginRequired() || !(PageBase.ProtocolLaunchAfterLogin != null))
                return;
            ((Page)this).NavigationService.Navigate(PageBase.ProtocolLaunchAfterLogin);
            PageBase.ProtocolLaunchAfterLogin = null;
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.HandleOnNavigatingFrom(e);
            NewsPage._scrollPosition = this.scrollNews.Viewport.Y;
            NewsViewModel.Instance.NavigatedFromNewsfeedTime = DateTime.Now;
        }

        private void AskToast()
        {
            if (!AppGlobalStateManager.Current.GlobalState.AllowToastNotificationsQuestionAsked)
            {
                if (MessageBox.Show(CommonResources.Toast_AllowQuestion, CommonResources.Settings_AllowToast, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    AppGlobalStateManager.Current.GlobalState.PushSettings.EnableAll();
                    AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled = true;
                }
                else
                {
                    AppGlobalStateManager.Current.GlobalState.PushSettings.EnableAll();
                    AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled = false;
                }
                AppGlobalStateManager.Current.GlobalState.AllowToastNotificationsQuestionAsked = true;
            }
            PushNotificationsManager.Instance.Initialize();
        }

        private void ProcessInputParameters()
        {
            Group parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (parameterForIdAndReset == null)
                return;
            IEnumerator<IVirtualizable> enumerator = ((Collection<IVirtualizable>)NewsViewModel.Instance.NewsFeedVM.Collection).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    IVirtualizable current = enumerator.Current;
                    WallPostItem wallPostItem = current as WallPostItem;
                    if (wallPostItem == null && current is NewsFeedAdsItem)
                        wallPostItem = (current as NewsFeedAdsItem).WallPostItem;
                    if ((wallPostItem != null ? wallPostItem.LikesAndCommentsItem : null) != null && wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(parameterForIdAndReset.id, parameterForIdAndReset.name))
                        break;
                    VideosNewsItem videosNewsItem = current as VideosNewsItem;
                    if (videosNewsItem != null)
                        videosNewsItem.LikesAndCommentsItem.ShareInGroupIfApplicable(parameterForIdAndReset.id, parameterForIdAndReset.name);
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
        }

        private void OnHeaderTap(bool scrollAnimated = true)
        {
            if (scrollAnimated)
                this.panelNews.ScrollToBottom(false);
            else
                this.panelNews.ScrollTo(0.0);
            if (this._hideHelper == null)
                return;
            NewsViewModel instance = NewsViewModel.Instance;
            if (instance.FreshNewsState == FreshNewsState.Insert)
                instance.FreshNewsState = FreshNewsState.NoNews;
            this._hideHelper.Show(false);
        }

        private void OnFreshNewsTap()
        {
            if (this._hideHelper == null)
                return;
            NewsViewModel instance = NewsViewModel.Instance;
            switch (instance.FreshNewsState)
            {
                case FreshNewsState.Insert:
                    this.OnHeaderTap(false);
                    break;
                case FreshNewsState.Reload:
                    if (instance.AreFreshNewsUpToDate && instance.HasFreshNewsToInsert)
                    {
                        NewsViewModel.Instance.ReplaceAllWithPendingFreshNews();
                        this.OnHeaderTap(false);
                        break;
                    }
                    this.Header.IsLoadingFreshNews = true;
                    NewsViewModel.Instance.ReloadNews(false, false, true);
                    break;
            }
        }

        private void OnHeaderFixedTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.OnHeaderTap(true);
        }

        private void OpenNewsSourcePicker()
        {
            this._newsSources = NewsViewModel.Instance.GetSectionsAndLists();
            if (this._newsSources == null)
                return;
            this.SelectNewsSourceItem((PickableNewsfeedSourceItemViewModel)Enumerable.FirstOrDefault<PickableNewsfeedSourceItemViewModel>(this._newsSources, (Func<PickableNewsfeedSourceItemViewModel, bool>)(item => item.PickableItem.ID == NewsViewModel.Instance.NewsSource.ID)));
            this._picker = new ListPickerUC2()
            {
                ItemsSource = (IList)this._newsSources,
                PickerMaxWidth = 408.0,
                PickerMaxHeight = 768.0,
                BackgroundColor = (Brush)Application.Current.Resources["PhoneCardOverlayBrush"],
                PickerMargin = new Thickness(0.0, 0.0, 0.0, 64.0),
                ItemTemplate = (DataTemplate)base.Resources["NewsSourceItemTemplate"]
            };
            this._picker.ItemTapped += (EventHandler<object>)((sender, item) =>
            {
                PickableNewsfeedSourceItemViewModel newsSource = item as PickableNewsfeedSourceItemViewModel;
                if (newsSource == null)
                    return;
                this.SelectNewsSourceItem(newsSource);
                NewsViewModel.Instance.NewsSource = newsSource.PickableItem;
            });
            bool flag1 = false;
            DialogService newsfeedTopPromoDialog = this._newsfeedTopPromoDialog;
            if ((newsfeedTopPromoDialog != null ? (newsfeedTopPromoDialog.IsOpen ? 1 : 0) : 0) != 0)
            {
                this._newsfeedTopPromoDialog.Hide();
                flag1 = true;
            }
            bool flag2 = false;
            DialogService moveTutorialDialog = this._photoFeedMoveTutorialDialog;
            if ((moveTutorialDialog != null ? (moveTutorialDialog.IsOpen ? 1 : 0) : 0) != 0)
            {
                this._photoFeedMoveTutorialDialog.Hide();
                flag2 = true;
            }
            PickableNewsfeedSourceItemViewModel feedNewsSource = (PickableNewsfeedSourceItemViewModel)Enumerable.FirstOrDefault<PickableNewsfeedSourceItemViewModel>(this._newsSources, (Func<PickableNewsfeedSourceItemViewModel, bool>)(source => source == NewsSources.NewsFeed));
            if (flag1 && feedNewsSource != null)
            {
                feedNewsSource.FadeOutToggleEnabled = true;
                this._picker.Closed += (EventHandler)((sender, args) => feedNewsSource.FadeOutToggleEnabled = false);
            }
            PickableNewsfeedSourceItemViewModel photosNewsSource = (PickableNewsfeedSourceItemViewModel)Enumerable.FirstOrDefault<PickableNewsfeedSourceItemViewModel>(this._newsSources, (Func<PickableNewsfeedSourceItemViewModel, bool>)(source => source == NewsSources.Photos));
            if (flag2 && photosNewsSource != null)
            {
                photosNewsSource.FadeOutEnabled = true;
                this._picker.Closed += (EventHandler)((sender, args) => photosNewsSource.FadeOutEnabled = false);
            }
            this._picker.Show(new Point(8.0, 32.0), (FrameworkElement)FramePageUtils.CurrentPage);
        }

        private void SelectNewsSourceItem(PickableNewsfeedSourceItemViewModel newsSource)
        {
            if (this._newsSources == null || newsSource == null)
                return;
            IEnumerator<PickableNewsfeedSourceItemViewModel> enumerator = ((Collection<PickableNewsfeedSourceItemViewModel>)this._newsSources).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    PickableNewsfeedSourceItemViewModel current = enumerator.Current;
                    int num = current.PickableItem.ID == newsSource.PickableItem.ID ? 1 : 0;
                    current.IsSelected = num != 0;
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
        }

        private async void ToggleControl_OnTap(object sender, EventArgs e)
        {
            this._picker.DisableContent();
            this.SelectNewsSourceItem(NewsSources.NewsFeed);
            await Task.Delay(200);
            ListPickerUC2 picker = this._picker;
            if (picker == null)
                return;
            picker.Hide();
        }

        private async void ShowPhotoFeedMoveTutorial()
        {
            if (AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown)
                return;
            AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown = true;
            PhotoFeedMoveTutorialUC childUC = new PhotoFeedMoveTutorialUC();
            GenericHeaderUC header = this.Header.ucHeader;
            childUC.SetCutArea(header.GetTitleMarginLeft(), header.GetTitleWidth());
            DialogService dialogService = new DialogService();
            dialogService.Child = (FrameworkElement)childUC;
            dialogService.BackgroundBrush = null;
            dialogService.IsOverlayApplied = false;
            dialogService.AnimationType = (DialogService.AnimationTypes)5;
            dialogService.AnimationTypeChild = (DialogService.AnimationTypes)6;
            dialogService.IsBackKeyOverride = false;
            this._photoFeedMoveTutorialDialog = dialogService;
            this._photoFeedMoveTutorialDialog.Closing += (EventHandler)((sender, args) => header.HideNewsfeedPromoOverlay());
            this._photoFeedMoveTutorialDialog.Show(null);
            header.ShowNewsfeedPromoOverlay();
            await Task.Delay(600);
            childUC.BackgroundTapCallback = (Action)(() =>
            {
                if (this._photoFeedMoveTutorialDialog == null || !this._photoFeedMoveTutorialDialog.IsOpen)
                    return;
                this._photoFeedMoveTutorialDialog.Hide();
            });
        }

        private async void ShowNewsfeedTopPromo(UserNotification notification)
        {
            if (!this.CanShowNewsfeedTopPromo || notification == null || (notification.Type != UserNotificationType.bubble_newsfeed || notification.bubble_newsfeed == null))
                return;
            NewsfeedTopPromoUC newsfeedTopPromoUc = new NewsfeedTopPromoUC();
            NewsfeedTopPromoViewModel topPromoViewModel = new NewsfeedTopPromoViewModel(notification.bubble_newsfeed);
            newsfeedTopPromoUc.DataContext = topPromoViewModel;
            NewsfeedTopPromoUC childUC = newsfeedTopPromoUc;
            GenericHeaderUC header = this.Header.ucHeader;
            childUC.SetCutArea(header.GetTitleMarginLeft(), header.GetTitleWidth());
            DialogService dialogService = new DialogService();
            dialogService.Child = childUC;
            dialogService.BackgroundBrush = null;
            dialogService.IsOverlayApplied = false;
            dialogService.AnimationType = (DialogService.AnimationTypes)5;
            dialogService.AnimationTypeChild = (DialogService.AnimationTypes)6;
            dialogService.IsBackKeyOverride = false;
            this._newsfeedTopPromoDialog = dialogService;
            childUC.ButtonPrimaryTapCallback = (Action)(() =>
            {
                NewsViewModel.Instance.TopFeedPromoAnswer = new bool?(true);
                this.OpenNewsSourcePicker();
            });
            childUC.ButtonSecondaryTapCallback = (Action)(() => this._newsfeedTopPromoDialog.Hide());
            this._newsfeedTopPromoDialog.Closing += (EventHandler)((sender, args) => header.HideNewsfeedPromoOverlay());
            this.OnHeaderTap(false);
            this._newsfeedTopPromoDialog.Show(null);
            header.ShowNewsfeedPromoOverlay();
            NewsViewModel.Instance.TopFeedPromoAnswer = new bool?(false);
            NewsViewModel.Instance.TopFeedPromoId = notification.id;
            await Task.Delay(600);
            childUC.BackgroundTapCallback = (Action)(() =>
            {
                if (this._newsfeedTopPromoDialog == null || !this._newsfeedTopPromoDialog.IsOpen)
                    return;
                this._newsfeedTopPromoDialog.Hide();
            });
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/NewsPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
            this.scrollNews = (ViewportControl)base.FindName("scrollNews");
            this.stackPanel = (MyVirtualizingStackPanel)base.FindName("stackPanel");
            //this.ucNewPost = (NewsfeedNewPostUC)base.FindName("ucNewPost");
            this.panelNews = (MyVirtualizingPanel2)base.FindName("panelNews");
            this.Header = (NewsfeedHeaderUC)base.FindName("Header");
            this.rectSystemTrayPlaceholder = (Rectangle)base.FindName("rectSystemTrayPlaceholder");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
        }

        private void StackPanel_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Dictionary<string, string> s = new Dictionary<string, string>();
            VKRequestsDispatcher.DispatchRequestToVK2("stories.get", s);
        }
    }
}
