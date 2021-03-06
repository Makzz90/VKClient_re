using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.ImageViewer;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
    public class HideHeaderHelper
    {
        //private const double SYSTEM_TRAY_HEIGHT = 32.0;
        //private const double FRESH_NEWS_RESET_SCROLL = 100.0;
        //private const int FRESH_NEWS_EXTRA_Y_OFFSET = 8;
        //private const int EXTRA_DELTA_WHEN_SCROLLING_UPWARDS = 530;
        private readonly NewsfeedHeaderUC _ucHeader;
        private readonly ViewportControl _viewportControl;
        private readonly PhoneApplicationPage _page;
        private readonly TranslateTransform _translateHeader;
        private readonly TranslateTransform _translateFreshNews;
        private readonly double _minOffsetHeader;
        private readonly double _maxOffsetHeader;
        private readonly double _minOffsetFreshNews;
        private double? _previousScrollPosition;
        private bool _isAnimating;
        private double _initialScrollPositionAfterDirectionChange;
        private bool _directionDownwards;
        private FreshNewsState _freshNewsState;
        private bool _isFreshNewsShowed;

        public HideHeaderHelper(NewsfeedHeaderUC ucHeader, ViewportControl viewportControl, PhoneApplicationPage page)
        {
            this._ucHeader = ucHeader;
            this._viewportControl = viewportControl;
            this._page = page;
            TranslateTransform renderTransform = this._ucHeader.RenderTransform as TranslateTransform;
            if (renderTransform == null)
            {
                TranslateTransform translateTransform = new TranslateTransform();
                this._ucHeader.RenderTransform = translateTransform;
                this._translateHeader = translateTransform;
            }
            else
                this._translateHeader = renderTransform;
            //this._minOffsetHeader = (-this._ucHeader.Height) + 32.0;
            //
            this._minOffsetHeader = (-this._ucHeader.Height);
            if (!VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.HideSystemTray)
                this._minOffsetHeader += 32;
            //
            this._maxOffsetHeader = 0.0;
            this._minOffsetFreshNews = 0.0;
            this._ucHeader.borderFreshNews.Visibility = Visibility.Visible;
            this._translateFreshNews = this._ucHeader.translateFreshNews;
            this._translateFreshNews.Y = this._minOffsetFreshNews;
            this._viewportControl.ViewportChanged += (new EventHandler<ViewportChangedEventArgs>(this.ViewportControl_OnViewportControlChanged));
            this._viewportControl.ManipulationStateChanged += (new EventHandler<ManipulationStateChangedEventArgs>(this.ViewportControl_OnManipulationStateChanged));
        }

        private double GetMaxFreshNewsTranslateY()
        {
            return this._ucHeader.Height + 8.0;
        }

        private void ViewportControl_OnManipulationStateChanged(object sender, ManipulationStateChangedEventArgs e)
        {
            if (this._isAnimating || this._viewportControl.ManipulationState != ManipulationState.Idle)
                return;
            this.UpdateSystemTrayAndAppBarIfNeeded();
            this._isAnimating = true;
            if (this.ShouldHide())
            {
                Rect viewport = this._viewportControl.Viewport;
                // ISSUE: explicit reference operation
                this.Hide(viewport.Y < this._ucHeader.ActualHeight);
            }
            else
                this.Show(true);
        }

        public void Show(bool showFreshNews = false)
        {
            this.UpdateSystemTrayAndAppBarIfNeeded();
            this._isAnimating = true;
            IEasingFunction ieasingFunction = (IEasingFunction)new QuadraticEase();
            object yproperty = TranslateTransform.YProperty;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>() { new AnimationInfo() { target = (DependencyObject)this._translateHeader, propertyPath = yproperty, from = this._translateHeader.Y, to = this._maxOffsetHeader, easing = ieasingFunction, duration = 150 } };
            if (showFreshNews && this._isFreshNewsShowed)
                animInfoList.Add(new AnimationInfo()
                {
                    target = (DependencyObject)this._translateFreshNews,
                    propertyPath = yproperty,
                    from = this._translateFreshNews.Y,
                    to = ((FrameworkElement)this._ucHeader).Height + 8.0,
                    easing = ieasingFunction,
                    duration = 150
                });
            AnimationUtil.AnimateSeveral(animInfoList, new int?(0), (Action)(() =>
            {
                this.UpdateExtraCrop();
                this.UpdateSystemTrayAndAppBarIfNeeded();
                this._isAnimating = false;
                if (this._isFreshNewsShowed)
                    return;
                this.ShowFreshNews();
            }));
        }

        private void Hide(bool hideFreshNews = false)
        {
            this.UpdateSystemTrayAndAppBarIfNeeded();
            this._isAnimating = true;
            IEasingFunction ieasingFunction = new QuadraticEase();
            object yproperty = TranslateTransform.YProperty;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>() { new AnimationInfo() { target = (DependencyObject)this._translateHeader, propertyPath = yproperty, from = this._translateHeader.Y, to = this._minOffsetHeader, easing = ieasingFunction, duration = 150 } };
            if (hideFreshNews && this._isFreshNewsShowed)
                animInfoList.Add(new AnimationInfo()
                {
                    target = this._translateFreshNews,
                    propertyPath = yproperty,
                    from = this._translateFreshNews.Y,
                    to = this._minOffsetFreshNews,
                    easing = ieasingFunction,
                    duration = 150
                });
            AnimationUtil.AnimateSeveral(animInfoList, new int?(0), (Action)(() =>
            {
                this.UpdateExtraCrop();
                this.UpdateSystemTrayAndAppBarIfNeeded();
                this._isAnimating = false;
            }));
        }

        public void UpdateFreshNewsState(FreshNewsState state)
        {
            this._freshNewsState = state;
            if (this._freshNewsState == FreshNewsState.NoNews)
                this.HideFreshNews();
            else
                this.ShowFreshNews();
        }

        public void ShowFreshNews()
        {
            if (this._freshNewsState == FreshNewsState.NoNews || this._isAnimating || this._isFreshNewsShowed)
                return;
            this._isAnimating = true;
            TranslateTransform translateFreshNews = this._translateFreshNews;
            double y = this._translateFreshNews.Y;
            double freshNewsTranslateY = this.GetMaxFreshNewsTranslateY();
            
            int? startTime = new int?(0);
            BackEase backEase = new BackEase();
            backEase.EasingMode = ((EasingMode)0);
            backEase.Amplitude = 0.5;
            Action completed = (Action)(() =>
            {
                this._isAnimating = false;
                this._isFreshNewsShowed = true;
            });
            translateFreshNews.Animate(y, freshNewsTranslateY, TranslateTransform.YProperty, 350, startTime, backEase, completed, false);
        }

        private void HideFreshNews()
        {
            if (this._isAnimating || !this._isFreshNewsShowed || this._ucHeader.IsLoadingFreshNews)
                return;
            this._isAnimating = true;
            TranslateTransform translateFreshNews = this._translateFreshNews;
            double y = this._translateFreshNews.Y;
            double minOffsetFreshNews = this._minOffsetFreshNews;
            int? startTime = new int?(0);
            QuadraticEase quadraticEase = new QuadraticEase();
            ((EasingFunctionBase)quadraticEase).EasingMode = ((EasingMode)0);
            Action completed = (Action)(() =>
            {
                this._isAnimating = false;
                this._isFreshNewsShowed = false;
            });
            ((DependencyObject)translateFreshNews).Animate(y, minOffsetFreshNews, TranslateTransform.YProperty, 250, startTime, quadraticEase, completed, false);
        }

        private void UpdateSystemTrayAndAppBarIfNeeded()
        {
            if (VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.HideSystemTray == false)
                return;
            if (this._previousScrollPosition>=0)
                Microsoft.Phone.Shell.SystemTray.IsVisible = !this.ShouldHide();
        }

        private bool ShouldHide()
        {
            Rect viewport = this._viewportControl.Viewport;
            // ISSUE: explicit reference operation
            if (viewport.Y < this._ucHeader.ActualHeight/* - 32.0*/)
                return false;
            return this._translateHeader.Y < this._maxOffsetHeader;
        }

        private void ViewportControl_OnViewportControlChanged(object sender, ViewportChangedEventArgs e)
        {
            Rect viewport = this._viewportControl.Viewport;
            // ISSUE: explicit reference operation
            double y = viewport.Y;
            if (this._viewportControl.ManipulationState != ManipulationState.Idle && !this._isAnimating)
            {
                if (!this._previousScrollPosition.HasValue)
                {
                    this._previousScrollPosition = new double?(y);
                    this._initialScrollPositionAfterDirectionChange = y;
                    this._directionDownwards = true;
                }
                double num1 = y;
                double? previousScrollPosition = this._previousScrollPosition;
                double valueOrDefault1 = previousScrollPosition.GetValueOrDefault();
                if ((num1 < valueOrDefault1 ? (previousScrollPosition.HasValue ? 1 : 0) : 0) != 0)
                {
                    if (this._directionDownwards)
                        this._initialScrollPositionAfterDirectionChange = y;
                    this._directionDownwards = false;
                }
                
                previousScrollPosition = this._previousScrollPosition;
                double valueOrDefault2 = previousScrollPosition.GetValueOrDefault();
                if ((y > valueOrDefault2 ? (previousScrollPosition.HasValue ? 1 : 0) : 0) != 0)
                {
                    if (!this._directionDownwards)
                        this._initialScrollPositionAfterDirectionChange = y;
                    this._directionDownwards = true;
                }
                if (!this._directionDownwards && y < 100.0 && this._freshNewsState != FreshNewsState.Reload)
                    this.UpdateFreshNewsState(FreshNewsState.NoNews);
                if (this._directionDownwards && y >= this._initialScrollPositionAfterDirectionChange && y > 0.0)
                {
                    double num3 = this._translateHeader.Y - (y - this._previousScrollPosition.Value);
                    if (num3 < this._minOffsetHeader)
                        num3 = this._minOffsetHeader;
                    int num4 = this._translateHeader.Y != num3 ? 1 : 0;
                    this._translateHeader.Y = num3;
                    if (num4 != 0 && num3 == this._minOffsetHeader)
                        this.HideFreshNews();
                    this.UpdateExtraCrop();
                }
                else if (!this._directionDownwards && y <= Math.Max(((FrameworkElement)this._ucHeader).Height, this._initialScrollPositionAfterDirectionChange - 530.0))
                {
                    double num3 = this._translateHeader.Y - (y - this._previousScrollPosition.Value);
                    bool flag = false;
                    if (num3 > this._maxOffsetHeader)
                    {
                        num3 = this._maxOffsetHeader;
                        flag = true;
                    }
                    this._translateHeader.Y = num3;
                    if (flag)
                        this.ShowFreshNews();
                    this.UpdateExtraCrop();
                }
            }
            if (y == 0.0)
                this.Show(false);
            this._previousScrollPosition = new double?(y);
            this.UpdateSystemTrayAndAppBarIfNeeded();
        }

        private void UpdateExtraCrop()
        {
            AttachedProperties.SetExtraDeltaYCropWhenHidingImage(this._viewportControl, Math.Max(this._ucHeader.Height + this._translateHeader.Y, 32.0));
        }
    }
}
