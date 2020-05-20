using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.AutoSuggest;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.System;

namespace VKClient.Common.Framework
{
    public class PageBase : PhoneApplicationPage, IPageDataRequesteeInfo
    {
        // NEW: 4.8.0
        public static readonly DependencyProperty IsTransitionEnabledProperty = DependencyProperty.Register("IsTransitionEnabled", typeof(bool), typeof(PageBase), new PropertyMetadata((object)true, new PropertyChangedCallback(PageBase.IsTransitionEnabled_OnChanged)));

        private static IPageDataRequesteeInfo _currentPageDataRequestee;
        private PageBase.CommonPageParameters _commonPageParameters = new PageBase.CommonPageParameters();
        private CaptchaRequestControl _captchaRequestControl = new CaptchaRequestControl();
        protected ImageViewerDecoratorUC _imageViewerDecorator = new ImageViewerDecoratorUC();
        private bool _isInitialized;
        private bool _isLoadedAtLeastOnce;
        protected ProgressBar _progressBar;
        private PageOrientation _lastOrientation;
        private ITransition _transition;
        private DateTime _onNavToTime;
        private bool _shouldResetStack;
        private ApplicationBarIconButton _appBarButtonSettings;
        private bool _suppressMenu;
        private Guid _guid;
        private static int TotalCount;
        private const int HEADER_HEIGHT = 96;
        private const int MENU_WIDTH = 433;
        private const int CONTENT_MARGIN = 16;
        private bool _isMenuOpen;
        private static IEasingFunction _menuEasing;
        private ApplicationBar _menuAppBar;
        private IApplicationBar _savedAppBar;
        private bool _isAnimatingMenu;
        private Rectangle _openMenuTapArea;
        protected Rectangle _menuCallout;
        protected Rectangle _transparentOverlay;
        private bool _manipulationStarted;
        private bool _show;
        protected bool _ignoreNextLostFocus;
        protected bool _isCurrentPage;
        private bool _hookedUpManipulationEvents;
        private bool _suppressOpenMenuTapArea;
        private bool _isRemovedFromJournal;
        private DateTime _lastTimeNavigatedTo;
        private bool _lastNavigationIsReset;
        private bool _isPopupPage = false;//
        protected bool _manualHandleValidationParams;
        private Action<ValidationUserResponse> _validationResponseCallback;
        private Action<ValidationUserResponse> _validation2FAResponseCallback;
        protected List<IMyVirtualizingPanel> _panels;

        public static IPageDataRequesteeInfo CurrentPageDataRequestee
        {
            get
            {
                return PageBase._currentPageDataRequestee;
            }
            private set
            {
                PageBase._currentPageDataRequestee = value;
            }
        }

        public static Uri ProtocolLaunchAfterLogin { get; set; }

        public ImageViewerDecoratorUC ImageViewerDecorator
        {
            get
            {
                return this._imageViewerDecorator;
            }
        }

        public PageBase.CommonPageParameters CommonParameters
        {
            get
            {
                return this._commonPageParameters;
            }
        }

        public bool SuppressMenu
        {
            get
            {
                return this._suppressMenu;
            }
            set
            {
                this._suppressMenu = value;
                this._menuCallout.Visibility = this._suppressMenu ? Visibility.Collapsed : Visibility.Visible;
                this.UpdateOpenMenuTapAreaVisibility();
            }
        }

        public ObservableCollection<IFlyout> Flyouts { get; set; }

        public ObservableCollection<FullscreenLoader> FullscreenLoaders { get; set; }

        public bool IsTransitionEnabled
        {
            get
            {
                return (bool)this.GetValue(PageBase.IsTransitionEnabledProperty);
            }
            set
            {
                this.SetValue(PageBase.IsTransitionEnabledProperty, value);
            }
        }

        public bool IsMenuOpen
        {
            get
            {
                return this._isMenuOpen;
            }
            private set
            {
                this._isMenuOpen = value;
                this._menuCallout.Width = this._isMenuOpen ? 47.0 : 16.0;
            }
        }

        public bool SuppressOpenMenuTapArea
        {
            get
            {
                return this._suppressOpenMenuTapArea;
            }
            set
            {
                this._suppressOpenMenuTapArea = value;
                this.UpdateOpenMenuTapAreaVisibility();
            }
        }

        private bool CanOpenMenu
        {
            get
            {
                if (!this.SuppressMenu && this.Flyouts.Count == 0)
                    return this.FullscreenLoaders.Count == 0;
                return false;
            }
        }

        public Guid Guid
        {
            get
            {
                return this._guid;
            }
        }

        public RequestExecutionRule ExecutionRule
        {
            get
            {
                if ((Application.Current as IAppStateInfo).StartState != StartState.TombstonedThenRessurected || !this._lastNavigationIsReset)
                    return RequestExecutionRule.ExecuteNow;
                if (this._isRemovedFromJournal)
                    return RequestExecutionRule.Cancel;
                return (DateTime.Now - this._lastTimeNavigatedTo).TotalMilliseconds > 800.0 ? RequestExecutionRule.ExecuteNow : RequestExecutionRule.Delay;
            }
        }

        public bool SkipNextNavigationParametersRepositoryClearing { get; set; }

        public bool CanNavigateBack
        {
            get
            {
                if (!this.ImageViewerDecorator.IsShown)
                    return this.Flyouts.Count == 0;
                return false;
            }
        }

        static PageBase()
        {
            QuadraticEase quadraticEase = new QuadraticEase();
            int num = 0;
            quadraticEase.EasingMode = (EasingMode)num;
            PageBase._menuEasing = (IEasingFunction)quadraticEase;
        }

        public PageBase()
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.IsIndeterminate = true;
            int num1 = 0;
            progressBar.VerticalAlignment = (VerticalAlignment)num1;
            SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.White);
            progressBar.Foreground = (Brush)solidColorBrush1;
            int num2 = 1;
            progressBar.Visibility = (Visibility)num2;
            this._progressBar = progressBar;
            this._appBarButtonSettings = new ApplicationBarIconButton()
            {
                IconUri = new Uri("Resources/appbar.feature.settings.rest.png", UriKind.Relative),
                Text = CommonResources.MainPage_Menu_AppBar_Settings
            };
            this.Flyouts = new ObservableCollection<IFlyout>();
            this.FullscreenLoaders = new ObservableCollection<FullscreenLoader>();
            this._guid = Guid.NewGuid();
            this._menuAppBar = new ApplicationBar();
            Rectangle rectangle1 = new Rectangle();
            int num3 = 0;
            rectangle1.HorizontalAlignment = (HorizontalAlignment)num3;
            int num4 = 0;
            rectangle1.VerticalAlignment = (VerticalAlignment)num4;
            double num5 = 65.0;
            rectangle1.Width = num5;
            double num6 = 96.0;
            rectangle1.Height = num6;
            SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Transparent);
            rectangle1.Fill = (Brush)solidColorBrush2;
            int num7 = 0;
            rectangle1.UseOptimizedManipulationRouting = num7 != 0;
            this._openMenuTapArea = rectangle1;
            Rectangle rectangle2 = new Rectangle();
            int num8 = 0;
            rectangle2.HorizontalAlignment = (HorizontalAlignment)num8;
            int num9 = 3;
            rectangle2.VerticalAlignment = (VerticalAlignment)num9;
            double num10 = 16.0;
            rectangle2.Width = num10;
            SolidColorBrush solidColorBrush3 = new SolidColorBrush(Colors.Transparent);
            rectangle2.Fill = (Brush)solidColorBrush3;
            int num11 = 0;
            rectangle2.UseOptimizedManipulationRouting = num11 != 0;
            Thickness thickness = new Thickness(0.0, 96.0, 0.0, 0.0);
            rectangle2.Margin = thickness;
            this._menuCallout = rectangle2;
            Rectangle rectangle3 = new Rectangle();
            int num12 = 3;
            rectangle3.HorizontalAlignment = (HorizontalAlignment)num12;
            int num13 = 3;
            rectangle3.VerticalAlignment = (VerticalAlignment)num13;
            SolidColorBrush solidColorBrush4 = new SolidColorBrush(Colors.Transparent);
            rectangle3.Fill = (Brush)solidColorBrush4;
            int num14 = 0;
            rectangle3.UseOptimizedManipulationRouting = num14 != 0;
            int num15 = 1;
            rectangle3.Visibility = (Visibility)num15;
            this._transparentOverlay = rectangle3;
            this._lastTimeNavigatedTo = DateTime.MinValue;
            this._panels = new List<IMyVirtualizingPanel>();
            ++PageBase.TotalCount;
            this.SetNavigationEffects();
            this._imageViewerDecorator.SetPage((PhoneApplicationPage)this);
            if (ParametersRepository.GetParameterForIdAndReset("SwitchNavigationEffects") != null)
                this.SwitchNavigationEffects();
            if (ParametersRepository.GetParameterForIdAndReset("IsMenuNavigation") != null)
            {
                TransitionService.SetNavigationInTransition((UIElement)this, (NavigationInTransition)null);
                if (ParametersRepository.GetParameterForIdAndReset("NeedClearNavigationStack") != null)
                    this._shouldResetStack = true;
            }
            this.Loaded += new RoutedEventHandler(this.PageBase_Loaded);
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(this.PageBase_OrientationChanged);
            this.Flyouts.CollectionChanged += (NotifyCollectionChangedEventHandler)((sender, args) =>
            {
                this.UpdateOpenMenuTapAreaVisibility();
                this.FlyoutsCollectionChanged();
            });
            this.FullscreenLoaders.CollectionChanged += (NotifyCollectionChangedEventHandler)((sender, args) =>
            {
                this.UpdateOpenMenuTapAreaVisibility();
                this.FlyoutsCollectionChanged();
            });
        }

        ~PageBase()
        {
            --PageBase.TotalCount;
        }

        public static void SetInProgress(bool isInProgress)
        {
            PageBase currentPage = FramePageUtils.CurrentPage;
            if (currentPage == null || !(currentPage.DataContext is ViewModelBase))
                return;
            (currentPage.DataContext as ViewModelBase).SetInProgress(isInProgress, "");
        }

        // NEW: 4.8.0
        private static void IsTransitionEnabled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PageBase)d).ResetNavigationEffects();
        }

        protected virtual void FlyoutsCollectionChanged()
        {
        }

        protected virtual void FullscreenLoadersCollectionChanged()
        {
        }

        private void _appBarButtonSettings_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToSettings();
        }

        private void _menuCallout_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (!this.CheckShouldRespondToManipulation())
                return;
            if (e.OriginalSource == this._menuCallout)
                e.Handled = true;
            this._manipulationStarted = true;
        }

        private bool CheckShouldRespondToManipulation()
        {
            if (this._isAnimatingMenu || !this.CanOpenMenu)
                return false;
            TranslateTransform translateTransform = (TranslateTransform)((TransformGroup)Application.Current.RootVisual.RenderTransform).Children[0];
            bool flag = true;
            if (translateTransform.Y != 0.0)
                flag = false;
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown)
                flag = false;
            if (this.Orientation == PageOrientation.Landscape || this.Orientation == PageOrientation.LandscapeLeft || this.Orientation == PageOrientation.LandscapeRight)
                flag = false;
            return flag;
        }

        private void _menuCallout_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!this.CheckShouldRespondToManipulation())
                return;
            if (e.OriginalSource == this._menuCallout)
                e.Handled = true;
            TranslateTransform translateTransform = (this.Content as Grid).RenderTransform as TranslateTransform;
            double num = translateTransform.X + e.DeltaManipulation.Translation.X;
            if (num <= 0.0)
            {
                this.ShowHideMenu(false, false);
                translateTransform.X = 0.0;
            }
            else if (num > 433.0)
            {
                this.ShowHideMenu(true, false);
                translateTransform.X = 433.0;
            }
            else
            {
                this.ShowHideMenu(true, false);
                translateTransform.X = num;
            }
        }

        private void _menuCallout_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!this.CheckShouldRespondToManipulation())
                return;
            if (e.OriginalSource == this._menuCallout)
                e.Handled = true;
            this._manipulationStarted = false;
            Point point;
            double num1;
            if (e.FinalVelocities != null)
            {
                point = e.FinalVelocities.LinearVelocity;
                num1 = point.X;
            }
            else
                num1 = 0.0;
            double num2 = num1;
            point = e.TotalManipulation.Translation;
            double x = point.X;
            this.OpenCloseMenu(!this.IsMenuOpen ? x >= 216.0 || num2 >= 10.0 : x > -216.0 && num2 > -10.0, null, false);
        }

        public void OpenCloseMenu(bool open, Action callback = null, bool withoutAnimation = false)
        {
            if (open && !this.CheckShouldRespondToManipulation())
            {
                if (callback == null)
                    return;
                callback();
            }
            else
            {
                bool shouldOpenMenu = open;
                if (shouldOpenMenu)
                    this.ShowHideMenu(true, true);
                this.IsMenuOpen = shouldOpenMenu;
                this.AnimateMenu(shouldOpenMenu, (Action)(() =>
                {
                    if (!shouldOpenMenu)
                        this.ShowHideMenu(false, false);
                    else
                        this.Focus();
                    if (callback == null)
                        return;
                    callback();
                }), withoutAnimation ? 0 : 300);
            }
        }

        protected virtual void AnimateMenu(bool open, Action callback, int duration = 300)
        {
            int num = open ? 433 : 0;
            double from = 0.0;
            if (this.Content.RenderTransform is TranslateTransform)
                from = (this.Content.RenderTransform as TranslateTransform).X;
            if ((double)num == from)
            {
                callback();
            }
            else
            {
                this._isAnimatingMenu = true;
                this.Content.RenderTransform.Animate(from, (double)num, (object)TranslateTransform.XProperty, duration, new int?(0), PageBase._menuEasing, (Action)(() =>
                {
                    this._isAnimatingMenu = false;
                    callback();
                }), false);
            }
        }

        private void ShowHideMenu(bool show, bool force = false)
        {
            Grid grid = this.Content as Grid;
            if (grid == null)
                return;
            TranslateTransform translateTransform = grid.RenderTransform as TranslateTransform;
            if (((translateTransform == null || translateTransform.X != 0.0 ? 0 : (!this._manipulationStarted ? 1 : 0)) & (show ? 1 : 0)) != 0 && !force)
                return;
            if (MenuUC.Instance != null)
            {
                if (!show && MenuUC.Instance.scrollViewer != null)
                    MenuUC.Instance.scrollViewer.ScrollToVerticalOffset(0.0);
                MenuUC.Instance.Visibility = show ? Visibility.Visible : Visibility.Collapsed; // UPDATE: 4.8.0
                MenuUC.Instance.IsHitTestVisible = show;
            }
            if (this._show == show)
                return;
            if (show)
            {
                this._savedAppBar = this.ApplicationBar;
                this.ApplicationBar = (IApplicationBar)null;
            }
            else
                this.ApplicationBar = this._savedAppBar;
            if (show && MenuUC.Instance != null)
                MenuUC.Instance.UpdateState();
            grid.Background = show ? (Brush)Application.Current.Resources["PhoneBackgroundBrush"] : (Brush)new SolidColorBrush(Colors.Transparent);
            this._transparentOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            this._show = show;
        }

        protected virtual void TextBoxPanelIsOpenedChanged(object sender, bool e)
        {
        }

        private void PageBase_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.UpdateImageViewerDecoratorPosition();
            this.OpenCloseMenu(false, null, false);
        }

        private void UpdateImageViewerDecoratorPosition()
        {
            if (!(this.Content is Grid) || this._imageViewerDecorator == null)
                return;
            Grid grid = this.Content as Grid;
            if (grid.Margin.Top > 0.0)
            {
                this._imageViewerDecorator.RenderTransform = (Transform)new CompositeTransform()
                {
                    TranslateY = -grid.Margin.Top
                };
            }
            else
            {
                Thickness margin = grid.Margin;
                if (margin.Left <= 0.0)
                    return;
                ImageViewerDecoratorUC viewerDecoratorUc = this._imageViewerDecorator;
                CompositeTransform compositeTransform = new CompositeTransform();
                margin = grid.Margin;
                double num = -margin.Left;
                compositeTransform.TranslateX = num;
                viewerDecoratorUc.RenderTransform = (Transform)compositeTransform;
            }
        }

        private void PageBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this._isLoadedAtLeastOnce)
            {
                this.InitializeAdornerControls();
                BirthdaysNotificationManager.Instance.ShowNotificationsIfNeeded();
                this._isLoadedAtLeastOnce = true;
            }
            Logger.Instance.Info("{0} OnNavTo-Loaded took {1} ms.", (object)this.GetType(), (object)(DateTime.Now - this._onNavToTime).TotalMilliseconds);
        }

        public virtual void InitializeAdornerControls()
        {
            if (!(this.Content is Grid))
                return;
            Grid grid = this.Content as Grid;
            int count1 = grid.RowDefinitions.Count;
            int count2 = grid.ColumnDefinitions.Count;
            this.UpdateImageViewerDecoratorPosition();
            if (count1 > 0)
            {
                Grid.SetRowSpan((FrameworkElement)this._captchaRequestControl, count1);
                Grid.SetRowSpan((FrameworkElement)this._imageViewerDecorator, count1);
                Grid.SetRowSpan((FrameworkElement)this._transparentOverlay, count1);
                Grid.SetRowSpan((FrameworkElement)this._menuCallout, count1);
                Grid.SetRowSpan((FrameworkElement)this._openMenuTapArea, count1);
            }
            if (count2 > 0)
            {
                Grid.SetColumnSpan((FrameworkElement)this._captchaRequestControl, count2);
                Grid.SetColumnSpan((FrameworkElement)this._imageViewerDecorator, count2);
                Grid.SetColumnSpan((FrameworkElement)this._transparentOverlay, count2);
            }
            grid.Children.Remove((UIElement)this._imageViewerDecorator);
            grid.Children.Remove((UIElement)this._captchaRequestControl);
            grid.Children.Remove((UIElement)this._transparentOverlay);
            grid.Children.Remove((UIElement)this._menuCallout);
            grid.Children.Remove((UIElement)this._openMenuTapArea);
            grid.Children.Remove((UIElement)this._progressBar);
            grid.Children.Add((UIElement)this._imageViewerDecorator);
            grid.Children.Add((UIElement)this._captchaRequestControl);
            grid.Children.Add((UIElement)this._transparentOverlay);
            grid.Children.Add((UIElement)this._menuCallout);
            grid.Children.Add((UIElement)this._openMenuTapArea);
            grid.Children.Add((UIElement)this._progressBar);
            if (!this._hookedUpManipulationEvents)
            {
                this._menuCallout.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this._menuCallout_ManipulationStarted);
                this._menuCallout.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this._menuCallout_ManipulationDelta);
                this._menuCallout.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this._menuCallout_ManipulationCompleted);
                this._openMenuTapArea.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._openMenuTapArea_Tap);
                this._transparentOverlay.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._transparentOverlay_Tap);
                //if (this.FindPivot() == null)
                //{
                //  grid.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this._menuCallout_ManipulationStarted);
                //  grid.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this._menuCallout_ManipulationDelta);
                //  grid.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this._menuCallout_ManipulationCompleted);
                //}
                this._hookedUpManipulationEvents = true;
            }
            grid.RenderTransform = (Transform)new TranslateTransform();
        }

        private void _transparentOverlay_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void UpdateOpenMenuTapAreaVisibility()
        {
            this._openMenuTapArea.Visibility = this.SuppressOpenMenuTapArea || !this.CanOpenMenu ? Visibility.Collapsed : Visibility.Visible;
        }

        private void _openMenuTapArea_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.SuppressOpenMenuTapArea)
                return;
            this.OpenCloseMenu(!this.IsMenuOpen, null, false);
            e.Handled = true;
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            base.OnRemovedFromJournal(e);
            Logger.Instance.Info("PageBase.OnRemovedFromJournal {0}, {1}, hashcode= {2}", (object)this.GetType(), (object)e.Entry.Source, (object)this.GetHashCode());
            foreach (IMyVirtualizingPanel panel in this._panels)
                panel.Cleanup();
            this._imageViewerDecorator.Hide(true);
            EventAggregator.Current.Unsubscribe((object)this);
            this.PerformCleanup();
            this._isRemovedFromJournal = true;
        }

        private void PerformCleanup()
        {
            this._progressBar.ClearValue(UIElement.VisibilityProperty);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PageBase.CurrentPageDataRequestee = (IPageDataRequesteeInfo)this;
            this._lastNavigationIsReset = e.NavigationMode == NavigationMode.Reset;
            this._lastTimeNavigatedTo = DateTime.Now;
            this.HandleOnNavigatedTo(e);
            if (!this.SkipNextNavigationParametersRepositoryClearing)
                ParametersRepository.Clear();
            else
                this.SkipNextNavigationParametersRepositoryClearing = false;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            this.HandleOnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Logger.Instance.Info("PageBase.OnNavigatedFrom " + this.GetType().Name + " NavigationMode = " + (object)e.NavigationMode + " HashCode " + (object)this.GetHashCode());
            this.HandleOnNavigatedFrom(e);
            this._isCurrentPage = false;
        }

        protected virtual void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            this._isCurrentPage = true;
            if (!this._manualHandleValidationParams)
                this.HandleValidationInputParams();
            this._onNavToTime = DateTime.Now;
            Logger.Instance.Info("OnNavigatedTo " + this.GetType().Name + " Mode=" + (object)e.NavigationMode);
            this.NavigationService.PauseOnBack = true;
            if (MemoryInfo.IsLowMemDevice)
                GC.Collect();
            this.SetNavigationEffects();
            if (!this._isInitialized)
            {
                if (this._shouldResetStack)
                    this.NavigationService.ClearBackStack();
                this.InitializeProgressIndicator();
                this.InitializeCommonParameters();
                CaptchaUserRequestHandler.CaptchaRequest = new Action<CaptchaUserRequest, Action<CaptchaUserResponse>>(this.CaptchaRequest);
                CaptchaUserRequestHandler.ValidationRequest = new Action<ValidationUserRequest, Action<ValidationUserResponse>>(this.ValidationRequest);
                CaptchaUserRequestHandler.Validation2FARequest = new Action<Validation2FAUserRequest, Action<ValidationUserResponse>>(this.Validation2FARequest);
                LogoutRequestHandler.LogoutRequest = new Action(this.LogoutRequest);
                PushNotificationsManager.Instance.Initialize();
                if (this.NavigationContext.QueryString.ContainsKey("ClearBackStack"))
                    this.NavigationService.ClearBackStack();
                this._isInitialized = true;
            }
            this.ShowHideMenu(this.IsMenuOpen, false);
            this.HandleCallbackParameters();
            BaseDataManager.Instance.RefreshBaseDataIfNeeded();
            StickersAutoSuggestDictionary.Instance.EnsureDictIsLoadedAndUpToDate(false);
            this.SetupSystemTray();
        }

        private async void HandleCallbackParameters()
        {
            string uriString = ParametersRepository.GetParameterForIdAndReset("CallbackUriToLaunch") as string;
            if (uriString == null)
                return;
            await Launcher.LaunchUriAsync(new Uri(uriString, UriKind.Absolute));
        }

        private void SetupSystemTray()
        {
            SystemTray.Opacity = 0.0;
            SystemTray.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneSystemTrayForegroundBrush"]).Color;
            SystemTray.IsVisible = true;
        }

        protected virtual void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.New || !e.Uri.OriginalString.Contains("HideLayout=True") && !e.Uri.OriginalString.Contains("PeopleExtension"))
                return;
            this.ShowHideLayoutRoot(false);
            TransitionService.SetNavigationOutTransition((UIElement)this, (NavigationOutTransition)null);
        }

        protected virtual void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (this.FullscreenLoaders.Count > 0)
            {
                FullscreenLoader fullscreenLoader = this.FullscreenLoaders.LastOrDefault<FullscreenLoader>((Func<FullscreenLoader, bool>)(l => l.HideOnBackKeyPress));
                if (fullscreenLoader != null)
                {
                    int num = 1;
                    fullscreenLoader.Hide(num != 0);
                }
                e.Cancel = true;
            }
            else if (this.Flyouts.Count > 0)
            {
                e.Cancel = true;
                this.Flyouts.Last<IFlyout>().Hide();
            }
            else
            {
                if (!this.IsMenuOpen || this.Flyouts.Count != 0)
                    return;
                this.OpenCloseMenu(false, null, false);
                e.Cancel = true;
            }
        }

        protected void HandleValidationInputParams()
        {
            ValidationUserResponse validationUserResponse = ParametersRepository.GetParameterForIdAndReset("ValidationResponse") as ValidationUserResponse;
            if (validationUserResponse == null)
                return;
            if (this._validationResponseCallback != null)
            {
                this._validationResponseCallback(validationUserResponse);
            }
            else
            {
                if (this._validation2FAResponseCallback == null)
                    return;
                this._validation2FAResponseCallback(validationUserResponse);
            }
        }

        private void ValidationRequest(ValidationUserRequest validationRequest, Action<ValidationUserResponse> validationResponseCallback)
        {
            this._validationResponseCallback = validationResponseCallback;
            Navigator.Current.NavigationToValidationPage(validationRequest.ValidationUri);
        }

        private void Validation2FARequest(Validation2FAUserRequest validationRequest, Action<ValidationUserResponse> validationResponseCallback)
        {
            this._validation2FAResponseCallback = validationResponseCallback;
            Navigator.Current.NavigateTo2FASecurityCheck(validationRequest.username, validationRequest.password, validationRequest.phoneMask, validationRequest.validationType, validationRequest.validationSid);
        }

        public void RegisterForCleanup(IMyVirtualizingPanel panel)
        {
            this._panels.Add(panel);
        }

        private void ShowHideLayoutRoot(bool show)
        {
            if (!(this.Content is Grid))
                return;
            (this.Content as Grid).Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LogoutRequest()
        {
            Navigator.Current.NavigateToWelcomePage();
        }

        private void CaptchaRequest(CaptchaUserRequest captchaUserRequest, Action<CaptchaUserResponse> action)
        {
            this.Focus();
            if (this._captchaRequestControl == null)
                return;
            this._captchaRequestControl.ShowCaptchaRequest((PhoneApplicationPage)this, captchaUserRequest, action);
        }

        // UPDATE: 4.8.0
        protected virtual void SetNavigationEffects()
        {
            NavigationInTransition navigationInTransition1;
            NavigationOutTransition navigationOutTransition1;
            if (!this._isPopupPage)
            {
                NavigationInTransition navigationInTransition2 = new NavigationInTransition();
                navigationInTransition2.Backward = (TransitionElement)new TurnstileTransition()
                {
                    Mode = TurnstileTransitionMode.BackwardIn
                };
                navigationInTransition2.Forward = (TransitionElement)new TurnstileTransition()
                {
                    Mode = TurnstileTransitionMode.ForwardIn
                };
                navigationInTransition1 = navigationInTransition2;
                NavigationOutTransition navigationOutTransition2 = new NavigationOutTransition();
                navigationOutTransition2.Backward = (TransitionElement)new TurnstileTransition()
                {
                    Mode = TurnstileTransitionMode.BackwardOut
                };
                navigationOutTransition2.Forward = (TransitionElement)new TurnstileTransition()
                {
                    Mode = TurnstileTransitionMode.ForwardOut
                };
                navigationOutTransition1 = navigationOutTransition2;
            }
            else
            {
                NavigationInTransition navigationInTransition2 = new NavigationInTransition();
                navigationInTransition2.Backward = (TransitionElement)new TurnstileTransition()
                {
                    Mode = TurnstileTransitionMode.BackwardIn
                };
                navigationInTransition2.Forward = (TransitionElement)new SlideTransition()
                {
                    Mode = SlideTransitionMode.SlideUpFadeIn
                };
                navigationInTransition1 = navigationInTransition2;
                NavigationOutTransition navigationOutTransition2 = new NavigationOutTransition();
                navigationOutTransition2.Backward = (TransitionElement)new SlideTransition()
                {
                    Mode = SlideTransitionMode.SlideDownFadeOut
                };
                navigationOutTransition2.Forward = (TransitionElement)new TurnstileTransition()
                {
                    Mode = TurnstileTransitionMode.ForwardOut
                };
                navigationOutTransition1 = navigationOutTransition2;
            }
            navigationInTransition1.EndTransition += (RoutedEventHandler)((sender, args) => this.Projection = null);
            TransitionService.SetNavigationInTransition((UIElement)this, navigationInTransition1);
            TransitionService.SetNavigationOutTransition((UIElement)this, navigationOutTransition1);
        }

        // NEW: 4.8.0
        private void ResetNavigationEffects()
        {
            TransitionService.SetNavigationInTransition((UIElement)this, null);
            TransitionService.SetNavigationOutTransition((UIElement)this, null);
        }

        public void SwitchNavigationEffects()
        {
            NavigationInTransition navigationInTransition = new NavigationInTransition();
            navigationInTransition.Backward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.ForwardIn
            };
            navigationInTransition.Forward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.BackwardIn
            };
            TransitionService.SetNavigationInTransition((UIElement)this, navigationInTransition);
            NavigationOutTransition navigationOutTransition = new NavigationOutTransition();
            navigationOutTransition.Backward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.ForwardOut
            };
            navigationOutTransition.Forward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.BackwardOut
            };
            TransitionService.SetNavigationOutTransition((UIElement)this, navigationOutTransition);
        }

        private void InitializeCommonParameters()
        {
            if (this.NavigationContext.QueryString.ContainsKey("IsGroup"))
                this._commonPageParameters.IsGroup = this.NavigationContext.QueryString["IsGroup"] == bool.TrueString;
            if (this.NavigationContext.QueryString.ContainsKey("UserOrGroupId"))
                this._commonPageParameters.UserOrGroupId = long.Parse(this.NavigationContext.QueryString["UserOrGroupId"]);
            if (this.NavigationContext.QueryString.ContainsKey("PickMode"))
                this._commonPageParameters.PickMode = this.NavigationContext.QueryString["PickMode"] == bool.TrueString;
            if (this.NavigationContext.QueryString.ContainsKey("PostId"))
                this._commonPageParameters.PostId = long.Parse(this.NavigationContext.QueryString["PostId"]);
            if (this.NavigationContext.QueryString.ContainsKey("OwnerId"))
                this._commonPageParameters.OwnerId = long.Parse(this.NavigationContext.QueryString["OwnerId"]);
            if (!this.NavigationContext.QueryString.ContainsKey("UserId"))
                return;
            this._commonPageParameters.UserId = long.Parse(this.NavigationContext.QueryString["UserId"]);
        }

        protected virtual void InitializeProgressIndicator()
        {
            BindingOperations.SetBinding((DependencyObject)this._progressBar, UIElement.VisibilityProperty, (BindingBase)new Binding()
            {
                Path = new PropertyPath("IsInProgressVisibility", new object[0])
            });
        }

        private void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation orientation = e.Orientation;
            RotateTransition rotateTransition = new RotateTransition();
            switch (orientation)
            {
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    rotateTransition.Mode = this._lastOrientation != PageOrientation.LandscapeLeft ? RotateTransitionMode.In90Clockwise : RotateTransitionMode.In90Counterclockwise;
                    break;
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeRight:
                    rotateTransition.Mode = this._lastOrientation != PageOrientation.PortraitUp ? RotateTransitionMode.In180Clockwise : RotateTransitionMode.In90Counterclockwise;
                    break;
                case PageOrientation.LandscapeLeft:
                    rotateTransition.Mode = this._lastOrientation != PageOrientation.LandscapeRight ? RotateTransitionMode.In90Clockwise : RotateTransitionMode.In180Counterclockwise;
                    break;
            }
            this._transition = rotateTransition.GetTransition((UIElement)this);
            this._transition.Completed += (EventHandler)((param0, param1) => this._transition.Stop());
            this._transition.Begin();
            this._lastOrientation = orientation;
        }

        private Pivot FindPivot()
        {
            return this.Content.Elements().FirstOrDefault<DependencyObject>((Func<DependencyObject, bool>)(el => el is Pivot)) as Pivot;
        }

        internal void PrepareForMenuNavigation(Action callback, bool needClearStack)
        {
            TransitionService.SetNavigationOutTransition((UIElement)this, (NavigationOutTransition)null);
            ParametersRepository.SetParameterForId("IsMenuNavigation", (object)true);
            if (needClearStack)
                ParametersRepository.SetParameterForId("NeedClearNavigationStack", (object)true);
            callback();
        }

        public void PreparePopupForwardOutNavigation()
        {
            NavigationOutTransition navigationOutTransition1 = new NavigationOutTransition();
            navigationOutTransition1.Forward = null;
            navigationOutTransition1.Backward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.BackwardOut
            };
            NavigationOutTransition navigationOutTransition2 = navigationOutTransition1;
            NavigationInTransition navigationInTransition1 = new NavigationInTransition();
            navigationInTransition1.Forward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.ForwardIn
            };
            navigationInTransition1.Backward = null;
            NavigationInTransition navigationInTransition2 = navigationInTransition1;
            TransitionService.SetNavigationOutTransition((UIElement)this, navigationOutTransition2);
            TransitionService.SetNavigationInTransition((UIElement)this, navigationInTransition2);
        }

        public class CommonPageParameters
        {
            public long UserOrGroupId { get; set; }

            public bool IsGroup { get; set; }

            public bool PickMode { get; set; }

            public long PostId { get; set; }

            public long OwnerId { get; set; }

            public long UserId { get; set; }
        }
    }
}
