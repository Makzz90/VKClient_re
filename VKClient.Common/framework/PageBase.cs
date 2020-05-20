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
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Library;
using VKClient.Common.Stickers.AutoSuggest;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.System;

namespace VKClient.Common.Framework
{
    public class PageBase : PhoneApplicationPage, IPageDataRequesteeInfo
    {
        public static readonly DependencyProperty IsTransitionEnabledProperty = DependencyProperty.Register("IsTransitionEnabled", typeof(bool), typeof(PageBase), new PropertyMetadata(true, new PropertyChangedCallback(PageBase.IsTransitionEnabled_OnChanged)));
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
        private bool _suppressMenu;
        private Guid _guid;
        private static int TotalCount;
        private const int HEADER_HEIGHT = 96;
        public const int MENU_WIDTH = 416;
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
        private bool _isPopupPage;
        private bool _isSystemTraySetUp;
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
                ((UIElement)this._menuCallout).Visibility = (this._suppressMenu ? Visibility.Collapsed : Visibility.Visible);
                this.UpdateOpenMenuTapAreaVisibility();
            }
        }

        public ObservableCollection<IFlyout> Flyouts { get; set; }

        public ObservableCollection<FullscreenLoader> FullscreenLoaders { get; set; }

        public bool IsTransitionEnabled
        {
            get
            {
                return (bool)((DependencyObject)this).GetValue(PageBase.IsTransitionEnabledProperty);
            }
            set
            {
                ((DependencyObject)this).SetValue(PageBase.IsTransitionEnabledProperty, value);
            }
        }

        public EventHandler IsMenuOpenChanged { get; set; }

        public bool IsMenuOpen
        {
            get
            {
                return this._isMenuOpen;
            }
            private set
            {
                this._isMenuOpen = value;
                ((FrameworkElement)this._menuCallout).Width = (this._isMenuOpen ? 64.0 : 16.0);
                EventHandler isMenuOpenChanged = this.IsMenuOpenChanged;
                if (isMenuOpenChanged == null)
                    return;
                EventArgs empty = EventArgs.Empty;
                isMenuOpenChanged(this, empty);
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
            ((EasingFunctionBase)quadraticEase).EasingMode = ((EasingMode)num);
            PageBase._menuEasing = (IEasingFunction)quadraticEase;
        }

        public PageBase()
        {
            ProgressBar progressBar = new ProgressBar();
            int num1 = 1;
            progressBar.IsIndeterminate = (num1 != 0);
            int num2 = 0;
            ((FrameworkElement)progressBar).VerticalAlignment = ((VerticalAlignment)num2);
            SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.White);
            ((Control)progressBar).Foreground = ((Brush)solidColorBrush1);
            int num3 = 1;
            ((UIElement)progressBar).Visibility = ((Visibility)num3);
            this._progressBar = progressBar;
            this.Flyouts = new ObservableCollection<IFlyout>();
            this.FullscreenLoaders = new ObservableCollection<FullscreenLoader>();
            this._guid = Guid.NewGuid();
            this._menuAppBar = new ApplicationBar();
            Rectangle rectangle1 = new Rectangle();
            int num4 = 0;
            ((FrameworkElement)rectangle1).HorizontalAlignment = ((HorizontalAlignment)num4);
            int num5 = 0;
            ((FrameworkElement)rectangle1).VerticalAlignment = ((VerticalAlignment)num5);
            double num6 = 65.0;
            ((FrameworkElement)rectangle1).Width = num6;
            double num7 = 96.0;
            ((FrameworkElement)rectangle1).Height = num7;
            SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Transparent);
            ((Shape)rectangle1).Fill = ((Brush)solidColorBrush2);
            int num8 = 0;
            ((FrameworkElement)rectangle1).UseOptimizedManipulationRouting = (num8 != 0);
            this._openMenuTapArea = rectangle1;
            Rectangle rectangle2 = new Rectangle();
            int num9 = 0;
            ((FrameworkElement)rectangle2).HorizontalAlignment = ((HorizontalAlignment)num9);
            int num10 = 3;
            ((FrameworkElement)rectangle2).VerticalAlignment = ((VerticalAlignment)num10);
            double num11 = 16.0;
            ((FrameworkElement)rectangle2).Width = num11;
            SolidColorBrush solidColorBrush3 = new SolidColorBrush(Colors.Transparent);
            ((Shape)rectangle2).Fill = ((Brush)solidColorBrush3);
            int num12 = 0;
            ((FrameworkElement)rectangle2).UseOptimizedManipulationRouting = (num12 != 0);
            Thickness thickness = new Thickness(0.0, 96.0, 0.0, 0.0);
            ((FrameworkElement)rectangle2).Margin = thickness;
            this._menuCallout = rectangle2;
            Rectangle rectangle3 = new Rectangle();
            int num13 = 3;
            ((FrameworkElement)rectangle3).HorizontalAlignment = ((HorizontalAlignment)num13);
            int num14 = 3;
            ((FrameworkElement)rectangle3).VerticalAlignment = ((VerticalAlignment)num14);
            SolidColorBrush solidColorBrush4 = new SolidColorBrush(Colors.Transparent);
            ((Shape)rectangle3).Fill = ((Brush)solidColorBrush4);
            int num15 = 0;
            ((FrameworkElement)rectangle3).UseOptimizedManipulationRouting = (num15 != 0);
            int num16 = 1;
            ((UIElement)rectangle3).Visibility = ((Visibility)num16);
            this._transparentOverlay = rectangle3;
            this._lastTimeNavigatedTo = DateTime.MinValue;
            this._panels = new List<IMyVirtualizingPanel>();
            //base.\u002Ector();
            ++PageBase.TotalCount;
            this.SetNavigationEffects();
            this._imageViewerDecorator.SetPage((PhoneApplicationPage)this);
            if (ParametersRepository.GetParameterForIdAndReset("SwitchNavigationEffects") != null)
                this.SwitchNavigationEffects();
            if (ParametersRepository.GetParameterForIdAndReset("IsMenuNavigation") != null)
            {
                TransitionService.SetNavigationInTransition((UIElement)this, null);
                if (ParametersRepository.GetParameterForIdAndReset("NeedClearNavigationStack") != null)
                    this._shouldResetStack = true;
            }
            // ISSUE: method pointer
            ((FrameworkElement)this).Loaded += (new RoutedEventHandler(this.PageBase_Loaded));
            this.OrientationChanged += (new EventHandler<OrientationChangedEventArgs>(this.PageBase_OrientationChanged));
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
            //try
            //{
            --PageBase.TotalCount;
            //}
            //finally
            //{
            //  base.Finalize();
            //}
        }

        public static void SetInProgress(bool isInProgress)
        {
            PageBase currentPage = FramePageUtils.CurrentPage;
            if (currentPage == null || !(((FrameworkElement)currentPage).DataContext is ViewModelBase))
                return;
            (((FrameworkElement)currentPage).DataContext as ViewModelBase).SetInProgress(isInProgress, "");
        }

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
            if (((RoutedEventArgs)e).OriginalSource == this._menuCallout)
                e.Handled = true;
            this._manipulationStarted = true;
        }

        private bool CheckShouldRespondToManipulation()
        {
            if (this._isAnimatingMenu || !this.CanOpenMenu)
                return false;
            TranslateTransform translateTransform = (TranslateTransform)((PresentationFrameworkCollection<Transform>)((TransformGroup)Application.Current.RootVisual.RenderTransform).Children)[0];
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
            if (((RoutedEventArgs)e).OriginalSource == this._menuCallout)
                e.Handled = true;
            TranslateTransform renderTransform = ((UIElement)(((UserControl)this).Content as Grid)).RenderTransform as TranslateTransform;
            double x1 = renderTransform.X;
            Point translation = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            double x2 = ((Point)@translation).X;
            double num = x1 + x2;
            if (num <= 0.0)
            {
                this.ShowHideMenu(false, false);
                renderTransform.X = 0.0;
            }
            else if (num > 416.0)
            {
                this.ShowHideMenu(true, false);
                renderTransform.X = 416.0;
            }
            else
            {
                this.ShowHideMenu(true, false);
                renderTransform.X = num;
            }
        }

        private void _menuCallout_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!this.CheckShouldRespondToManipulation())
                return;
            if (((RoutedEventArgs)e).OriginalSource == this._menuCallout)
                e.Handled = true;
            this._manipulationStarted = false;
            Point point;
            double num1;
            if (e.FinalVelocities != null)
            {
                point = e.FinalVelocities.LinearVelocity;
                // ISSUE: explicit reference operation
                num1 = point.X;
            }
            else
                num1 = 0.0;
            double num2 = num1;
            point = e.TotalManipulation.Translation;
            // ISSUE: explicit reference operation
            double x = point.X;
            this.OpenCloseMenu(!this.IsMenuOpen ? x >= 208.0 || num2 >= 10.0 : x > -208.0 && num2 > -10.0, null, false);
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
                        ((Control)this).Focus();
                    if (callback == null)
                        return;
                    callback();
                }), withoutAnimation ? 0 : 300);
            }
        }

        protected virtual void AnimateMenu(bool open, Action callback, int duration = 300)
        {
            int num = open ? 416 : 0;
            double from = 0.0;
            if (((UserControl)this).Content.RenderTransform is TranslateTransform)
                from = (((UserControl)this).Content.RenderTransform as TranslateTransform).X;
            if ((double)num == from)
            {
                callback();
            }
            else
            {
                this._isAnimatingMenu = true;
                ((DependencyObject)((UserControl)this).Content.RenderTransform).Animate(from, (double)num, TranslateTransform.XProperty, duration, new int?(0), PageBase._menuEasing, (Action)(() =>
                {
                    this._isAnimatingMenu = false;
                    callback();
                }), false);
            }
        }

        private void ShowHideMenu(bool show, bool force = false)
        {
            Grid content = ((UserControl)this).Content as Grid;
            if (content == null)
                return;
            TranslateTransform renderTransform = ((UIElement)content).RenderTransform as TranslateTransform;
            if (((renderTransform == null || renderTransform.X != 0.0 ? 0 : (!this._manipulationStarted ? 1 : 0)) & (show ? 1 : 0)) != 0 && !force)
                return;
            if (MenuUC.Instance != null)
            {
                if (!show && MenuUC.Instance.scrollViewer != null)
                    MenuUC.Instance.scrollViewer.ScrollToVerticalOffset(0.0);
                ((UIElement)MenuUC.Instance).Visibility = (show ? Visibility.Visible : Visibility.Collapsed);
                ((UIElement)MenuUC.Instance).IsHitTestVisible = show;
            }
            if (this._show == show)
                return;
            if (show)
            {
                this._savedAppBar = this.ApplicationBar;
                this.ApplicationBar = (null);
            }
            else
                this.ApplicationBar = this._savedAppBar;
            if (show && MenuUC.Instance != null)
                MenuUC.Instance.UpdateState();
            ((Panel)content).Background = (show ? (Brush)Application.Current.Resources["PhoneBackgroundBrush"] : (Brush)new SolidColorBrush(Colors.Transparent));
            ((UIElement)this._transparentOverlay).Visibility = (show ? Visibility.Visible : Visibility.Collapsed);
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
            if (!(((UserControl)this).Content is Grid) || this._imageViewerDecorator == null)
                return;
            Grid content = ((UserControl)this).Content as Grid;
            Thickness margin1 = ((FrameworkElement)content).Margin;
            // ISSUE: explicit reference operation
            if (((Thickness)@margin1).Top > 0.0)
            {
                ImageViewerDecoratorUC imageViewerDecorator = this._imageViewerDecorator;
                CompositeTransform compositeTransform = new CompositeTransform();
                Thickness margin2 = ((FrameworkElement)content).Margin;
                // ISSUE: explicit reference operation
                double num = -((Thickness)@margin2).Top;
                compositeTransform.TranslateY = num;
                ((UIElement)imageViewerDecorator).RenderTransform = ((Transform)compositeTransform);
            }
            else
            {
                Thickness margin2 = ((FrameworkElement)content).Margin;
                // ISSUE: explicit reference operation
                if (((Thickness)@margin2).Left <= 0.0)
                    return;
                ImageViewerDecoratorUC imageViewerDecorator = this._imageViewerDecorator;
                CompositeTransform compositeTransform = new CompositeTransform();
                margin2 = ((FrameworkElement)content).Margin;
                // ISSUE: explicit reference operation
                double num = -((Thickness)@margin2).Left;
                compositeTransform.TranslateX = num;
                ((UIElement)imageViewerDecorator).RenderTransform = ((Transform)compositeTransform);
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
            Logger.Instance.Info("{0} OnNavTo-Loaded took {1} ms.", (this).GetType(), (DateTime.Now - this._onNavToTime).TotalMilliseconds);
        }

        public virtual void InitializeAdornerControls()
        {
            if (!(((UserControl)this).Content is Grid))
                return;
            Grid content = ((UserControl)this).Content as Grid;
            int count1 = ((PresentationFrameworkCollection<RowDefinition>)content.RowDefinitions).Count;
            int count2 = ((PresentationFrameworkCollection<ColumnDefinition>)content.ColumnDefinitions).Count;
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
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Remove((UIElement)this._imageViewerDecorator);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Remove((UIElement)this._captchaRequestControl);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Remove((UIElement)this._transparentOverlay);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Remove((UIElement)this._menuCallout);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Remove((UIElement)this._openMenuTapArea);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Remove((UIElement)this._progressBar);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Add((UIElement)this._imageViewerDecorator);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Add((UIElement)this._captchaRequestControl);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Add((UIElement)this._transparentOverlay);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Add((UIElement)this._menuCallout);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Add((UIElement)this._openMenuTapArea);
            ((PresentationFrameworkCollection<UIElement>)((Panel)content).Children).Add((UIElement)this._progressBar);
            if (!this._hookedUpManipulationEvents)
            {
                this._menuCallout.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this._menuCallout_ManipulationStarted);
                this._menuCallout.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this._menuCallout_ManipulationDelta);
                this._menuCallout.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this._menuCallout_ManipulationCompleted);
                this._openMenuTapArea.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._openMenuTapArea_Tap);
                this._transparentOverlay.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._transparentOverlay_Tap);
                if (this.FindPivot() == null)
                {
                    content.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this._menuCallout_ManipulationStarted);
                    content.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this._menuCallout_ManipulationDelta);
                    content.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this._menuCallout_ManipulationCompleted);
                }
                this._hookedUpManipulationEvents = true;
            }
            ((UIElement)content).RenderTransform = ((Transform)new TranslateTransform());
        }

        private void _transparentOverlay_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void UpdateOpenMenuTapAreaVisibility()
        {
            ((UIElement)this._openMenuTapArea).Visibility = (this.SuppressOpenMenuTapArea || !this.CanOpenMenu ? Visibility.Collapsed : Visibility.Visible);
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
            Logger.Instance.Info("PageBase.OnRemovedFromJournal {0}, {1}, hashcode= {2}", (this).GetType(), e.Entry.Source, (this).GetHashCode());
            foreach (IMyVirtualizingPanel panel in this._panels)
                panel.Cleanup();
            this._imageViewerDecorator.Hide(true);
            EventAggregator.Current.Unsubscribe(this);
            this.PerformCleanup();
            this._isRemovedFromJournal = true;
        }

        private void PerformCleanup()
        {
            ((DependencyObject)this._progressBar).ClearValue((DependencyProperty)UIElement.VisibilityProperty);
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
            Logger.Instance.Info("PageBase.OnNavigatedFrom " + (this).GetType().Name + " NavigationMode = " + e.NavigationMode + " HashCode " + (this).GetHashCode());
            this.HandleOnNavigatedFrom(e);
            this._isCurrentPage = false;
        }

        protected virtual void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            this._isCurrentPage = true;
            if (!this._manualHandleValidationParams)
                this.HandleValidationInputParams();
            this.HandleMoneyTransferValidationInputParams();
            this._onNavToTime = DateTime.Now;
            Logger.Instance.Info("OnNavigatedTo " + (this).GetType().Name + " Mode=" + e.NavigationMode);
            ((Page)this).NavigationService.PauseOnBack = true;
            if (MemoryInfo.IsLowMemDevice)
                GC.Collect();
            this.SetNavigationEffects();
            if (!this._isInitialized)
            {
                if (this._shouldResetStack)
                    ((Page)this).NavigationService.ClearBackStack();
                this.InitializeProgressIndicator();
                this.InitializeCommonParameters();
                CaptchaUserRequestHandler.ValidationRequest = new Action<ValidationUserRequest, Action<ValidationUserResponse>>(this.ValidationRequest);
                CaptchaUserRequestHandler.Validation2FARequest = new Action<Validation2FAUserRequest, Action<ValidationUserResponse>>(this.Validation2FARequest);
                LogoutRequestHandler.LogoutRequest = new Action(this.LogoutRequest);
                PushNotificationsManager.Instance.Initialize();
                if (((Page)this).NavigationContext.QueryString.ContainsKey("ClearBackStack"))
                    ((Page)this).NavigationService.ClearBackStack();
                this._isInitialized = true;
            }
            CaptchaUserRequestHandler.CaptchaRequest = new Action<CaptchaUserRequest, Action<CaptchaUserResponse>>(this.CaptchaRequest);
            this.ShowHideMenu(this.IsMenuOpen, false);
            this.HandleCallbackParameters();
            BaseDataManager.Instance.RefreshBaseDataIfNeeded();
            StickersAutoSuggestDictionary.Instance.EnsureDictIsLoadedAndUpToDate(false);
            if (this._isSystemTraySetUp)
                return;
            this.SetupSystemTray();
        }

        private async void HandleCallbackParameters()
        {
            string parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("CallbackUriToLaunch") as string;
            if (parameterForIdAndReset == null)
                return;
            await Launcher.LaunchUriAsync(new Uri(parameterForIdAndReset, UriKind.Absolute));
        }

        private void SetupSystemTray()
        {
            SystemTray.Opacity = 0.0;
            SystemTray.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneSystemTrayForegroundBrush"]).Color;
            SystemTray.IsVisible = true;
            this._isSystemTraySetUp = true;
        }

        protected virtual void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.New || !e.Uri.OriginalString.Contains("HideLayout=True") && !e.Uri.OriginalString.Contains("PeopleExtension"))
                return;
            this.ShowHideLayoutRoot(false);
            TransitionService.SetNavigationOutTransition(this, null);
        }

        protected virtual void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (this.FullscreenLoaders.Count > 0)
            {
                FullscreenLoader m0 = Enumerable.LastOrDefault<FullscreenLoader>(this.FullscreenLoaders, (Func<FullscreenLoader, bool>)(l => l.HideOnBackKeyPress));
                if (m0 != null)
                {
                    m0.Hide(true);
                }
                e.Cancel = true;
            }
            else if (this.Flyouts.Count > 0)
            {
                e.Cancel = true;
                ((IFlyout)Enumerable.Last<IFlyout>(this.Flyouts)).Hide();
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
            ValidationUserResponse parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("ValidationResponse") as ValidationUserResponse;
            if (parameterForIdAndReset == null)
                return;
            if (this._validationResponseCallback != null)
            {
                this._validationResponseCallback(parameterForIdAndReset);
            }
            else
            {
                if (this._validation2FAResponseCallback == null)
                    return;
                this._validation2FAResponseCallback(parameterForIdAndReset);
            }
        }

        private void HandleMoneyTransferValidationInputParams()
        {
            MoneyTransferAcceptedResponse parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("MoneyTransferAcceptedResponse") as MoneyTransferAcceptedResponse;
            if (parameterForIdAndReset1 != null)
            {
                if (!parameterForIdAndReset1.IsSucceeded)
                    return;
                EventAggregator.Current.Publish(new MoneyTransferAcceptedEvent(parameterForIdAndReset1.TransferId, parameterForIdAndReset1.FromId, parameterForIdAndReset1.ToId));
            }
            else
            {
                MoneyTransferSentResponse parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("MoneyTransferSentResponse") as MoneyTransferSentResponse;
                if (parameterForIdAndReset2 == null || !parameterForIdAndReset2.IsSucceeded)
                    return;
                EventAggregator.Current.Publish(new MoneyTransferSentEvent());
                this.HandleMoneyTransferSentResponse(parameterForIdAndReset2);
            }
        }

        protected virtual void HandleMoneyTransferSentResponse(MoneyTransferSentResponse response)
        {
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
            if (!(((UserControl)this).Content is Grid))
                return;
            ((UIElement)(((UserControl)this).Content as Grid)).Visibility = (show ? Visibility.Visible : Visibility.Collapsed);
        }

        private void LogoutRequest()
        {
            Navigator.Current.NavigateToWelcomePage();
        }

        private void CaptchaRequest(CaptchaUserRequest captchaUserRequest, Action<CaptchaUserResponse> action)
        {
            ((Control)this).Focus();
            if (this._captchaRequestControl == null)
                return;
            this._captchaRequestControl.ShowCaptchaRequest((PhoneApplicationPage)this, captchaUserRequest, action);
        }

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
            // ISSUE: method pointer
            navigationInTransition1.EndTransition += (RoutedEventHandler)((sender, args) => this.Projection = null);
            TransitionService.SetNavigationInTransition(this, navigationInTransition1);
            TransitionService.SetNavigationOutTransition(this, navigationOutTransition1);
        }

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
            if (((Page)this).NavigationContext.QueryString.ContainsKey("IsGroup"))
                this._commonPageParameters.IsGroup = ((Page)this).NavigationContext.QueryString["IsGroup"] == bool.TrueString;
            if (((Page)this).NavigationContext.QueryString.ContainsKey("UserOrGroupId"))
                this._commonPageParameters.UserOrGroupId = long.Parse(((Page)this).NavigationContext.QueryString["UserOrGroupId"]);
            if (((Page)this).NavigationContext.QueryString.ContainsKey("PickMode"))
                this._commonPageParameters.PickMode = ((Page)this).NavigationContext.QueryString["PickMode"] == bool.TrueString;
            if (((Page)this).NavigationContext.QueryString.ContainsKey("PostId"))
                this._commonPageParameters.PostId = long.Parse(((Page)this).NavigationContext.QueryString["PostId"]);
            if (((Page)this).NavigationContext.QueryString.ContainsKey("OwnerId"))
                this._commonPageParameters.OwnerId = long.Parse(((Page)this).NavigationContext.QueryString["OwnerId"]);
            if (!((Page)this).NavigationContext.QueryString.ContainsKey("UserId"))
                return;
            this._commonPageParameters.UserId = long.Parse(((Page)this).NavigationContext.QueryString["UserId"]);
        }

        protected virtual void InitializeProgressIndicator()
        {
            Binding binding = new Binding();
            binding.Path = (new PropertyPath("IsInProgressVisibility", new object[0]));
            BindingOperations.SetBinding(this._progressBar, (DependencyProperty)UIElement.VisibilityProperty, (BindingBase)binding);
        }

        private void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation orientation = e.Orientation;
            RotateTransition rotateTransition = new RotateTransition();
            switch ((int)orientation - 1)
            {
                case 0:
                case 4:
                    rotateTransition.Mode = this._lastOrientation != PageOrientation.LandscapeLeft ? RotateTransitionMode.In90Clockwise : RotateTransitionMode.In90Counterclockwise;
                    goto case 2;
                case 1:
                    rotateTransition.Mode = this._lastOrientation != PageOrientation.PortraitUp ? RotateTransitionMode.In180Clockwise : RotateTransitionMode.In90Counterclockwise;
                    goto case 2;
                case 2:
                case 3:
                    this._transition = rotateTransition.GetTransition((UIElement)this);
                    this._transition.Completed += ((EventHandler)((param0, param1) => this._transition.Stop()));
                    this._transition.Begin();
                    this._lastOrientation = orientation;
                    break;
                default:
                    if (orientation != PageOrientation.LandscapeLeft)
                    {
                        if (orientation != PageOrientation.LandscapeRight)
                            goto case 2;
                        else
                            goto case 1;
                    }
                    else
                    {
                        rotateTransition.Mode = this._lastOrientation != PageOrientation.LandscapeRight ? RotateTransitionMode.In90Clockwise : RotateTransitionMode.In180Counterclockwise;
                        goto case 2;
                    }
            }
        }

        private Pivot FindPivot()
        {
            IEnumerable<DependencyObject> arg_2A_0 = this.Content.Elements();
            Func<DependencyObject, bool> arg_2A_1 = new Func<DependencyObject, bool>((el) => { return el is Pivot; });

            return Enumerable.FirstOrDefault<DependencyObject>(arg_2A_0, arg_2A_1) as Pivot;
        }

        internal void PrepareForMenuNavigation(Action callback, bool needClearStack)
        {
            TransitionService.SetNavigationOutTransition((UIElement)this, null);
            ParametersRepository.SetParameterForId("IsMenuNavigation", true);
            if (needClearStack)
                ParametersRepository.SetParameterForId("NeedClearNavigationStack", true);
            callback();
        }

        public void PreparePopupForwardOutNavigation()
        {
            NavigationOutTransition navigationOutTransition1 = new NavigationOutTransition();
            // ISSUE: variable of the null type
            navigationOutTransition1.Forward = null;
            navigationOutTransition1.Backward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.BackwardOut
            };
            NavigationInTransition navigationInTransition1 = new NavigationInTransition();
            navigationInTransition1.Forward = (TransitionElement)new TurnstileTransition()
            {
                Mode = TurnstileTransitionMode.ForwardIn
            };
            // ISSUE: variable of the null type
            navigationInTransition1.Backward = null;
            TransitionService.SetNavigationOutTransition(this, navigationOutTransition1);
            TransitionService.SetNavigationInTransition(this, navigationInTransition1);
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
