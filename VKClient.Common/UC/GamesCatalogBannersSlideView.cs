using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VKClient.Common.Emoji;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library.Games;
using Windows.Foundation;

namespace VKClient.Common.UC
{
    public class GamesCatalogBannersSlideView : UserControl, INotifyPropertyChanged
    {
        private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
        private const double HIDE_VELOCITY_THRESHOLD = 100.0;
        private const int DURATION_BOUNCING = 175;
        private const int DURATION_MOVE_TO_NEXT = 200;
        private static readonly IEasingFunction ANIMATION_EASING;
        private DispatcherTimer _nextElementSwipeTimer;
        public static readonly DependencyProperty BackgroundColorProperty;
        private bool _moveNext;
        public static readonly DependencyProperty IsCycledProperty;
        private ObservableCollection<object> _items;
        private int _selectedIndex;
        private List<Control> _elements;
        private bool _isReadyToHideFired;
        private bool _isAnimating;
        private bool _isInVerticalSwipe;
        internal Grid gridRoot;
        internal Grid LayoutRoot;
        private bool _contentLoaded;

        public Func<Control> CreateSingleElement { get; set; }

        public bool ChangeIndexBeforeAnimation { get; set; }

        public double NextElementMargin { get; set; }

        public bool IsScrollListeningEnabled { get; set; }

        public bool AllowVerticalSwipe { get; set; }

        public System.TimeSpan NextElementSwipeDelay { get; set; }

        public Brush BackgroundColor
        {
            get
            {
                return (Brush)base.GetValue(GamesCatalogBannersSlideView.BackgroundColorProperty);
            }
            set
            {
                base.SetValue(GamesCatalogBannersSlideView.BackgroundColorProperty, value);
            }
        }

        public bool IsCycled
        {
            get
            {
                return (bool)base.GetValue(GamesCatalogBannersSlideView.IsCycledProperty);
            }
            set
            {
                base.SetValue(GamesCatalogBannersSlideView.IsCycledProperty, value);
            }
        }

        public ObservableCollection<object> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                if (this._items != null)
                    this._items.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.ItemsOnCollectionChanged);
                this._items = value;
                if (this._items != null)
                    this._items.CollectionChanged += new NotifyCollectionChangedEventHandler(this.ItemsOnCollectionChanged);
                this.OnPropertyChanged("Items");
                this.EnsureElements();
                this.ArrangeElements();
                this.SelectedIndex = 0;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }
            set
            {
                int selectedIndex = this._selectedIndex;
                this._selectedIndex = value;
                this.OnPropertyChanged("SelectedIndex");
                if (this._selectedIndex - selectedIndex == 2)
                {
                    GamesCatalogBannersSlideView.Swap(this._elements, 0, 2);
                    this.UpdateSources(false, true, true);
                    this.ArrangeElements();
                }
                else if (selectedIndex - this._selectedIndex == 2)
                {
                    GamesCatalogBannersSlideView.Swap(this._elements, 0, 2);
                    this.UpdateSources(true, true, false);
                    this.ArrangeElements();
                }
                else if (this._selectedIndex - selectedIndex == 1)
                {
                    this.MoveToNextOrPrevious(true);
                    this.ArrangeElements();
                }
                else if (selectedIndex - this._selectedIndex == 1)
                {
                    this.MoveToNextOrPrevious(false);
                    this.ArrangeElements();
                }
                else
                    this.UpdateSources(false, new bool?());
            }
        }

        private Control CurrentElement
        {
            get
            {
                return this._elements[1];
            }
        }

        private double ArrangeWidth
        {
            get
            {
                return base.ActualWidth - this.NextElementMargin;
            }
        }

        public event TypedEventHandler<GamesCatalogBannersSlideView, int> SelectionChanged;

        public event EventHandler ItemsCleared;

        //public event EventHandler SwipedToHide;

        public event PropertyChangedEventHandler PropertyChanged;

        static GamesCatalogBannersSlideView()
        {
            CubicEase cubicEase = new CubicEase();
            int num = 0;
            ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num);
            GamesCatalogBannersSlideView.ANIMATION_EASING = (IEasingFunction)cubicEase;
            // ISSUE: method pointer
            GamesCatalogBannersSlideView.BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(GamesCatalogBannersSlideView), new PropertyMetadata(new PropertyChangedCallback(GamesCatalogBannersSlideView.OnBackgroundBrushChanged)));
            GamesCatalogBannersSlideView.IsCycledProperty = DependencyProperty.Register("IsCycled", typeof(bool), typeof(GamesCatalogBannersSlideView), new PropertyMetadata(false));
        }

        public GamesCatalogBannersSlideView()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            // ISSUE: method pointer
            base.SizeChanged += (new SizeChangedEventHandler(this.OnSizeChanged));
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.OnLoaded));
            // ISSUE: method pointer
            base.Unloaded += (new RoutedEventHandler(this.OnUnloaded));
            base.DataContext = this;
        }

        public GamesCatalogBannersSlideView(double nextElementMargin)
            : this()
        {
            this.NextElementMargin = nextElementMargin;
        }

        private static void OnBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GamesCatalogBannersSlideView bannersSlideView = d as GamesCatalogBannersSlideView;
            if (bannersSlideView == null)
                return;
            // ISSUE: explicit reference operation
            Brush newValue = e.NewValue as Brush;
            if (newValue == null)
                return;
            ((Panel)bannersSlideView.gridRoot).Background = newValue;
        }

        private void NextElementSwipeTimer_OnTick(object sender, EventArgs eventArgs)
        {
            if (this.Items.Count < 2)
                return;
            this._isAnimating = true;
            if (this.IsCycled)
            {
                if (this.SelectedIndex == 0)
                    this._moveNext = true;
                else if (this.SelectedIndex == this.Items.Count - 1)
                    this._moveNext = false;
            }
            UIElement element1;
            UIElement element2;
            if (this._moveNext)
            {
                element1 = (UIElement)this._elements[1];
                element2 = (UIElement)this._elements[2];
            }
            else
            {
                element1 = (UIElement)this._elements[0];
                element2 = (UIElement)this._elements[1];
            }
            this.Move(element1, element2, this._moveNext, (Action)(() =>
            {
                this.MoveToNextOrPrevious(this._moveNext);
                this.ArrangeElements();
                this._isAnimating = false;
            }));
            this.ChangeCurrentInd(this._moveNext);
        }

        private void Move(UIElement element1, UIElement element2, bool next, Action completedCallback)
        {
            double num1 = -this.ArrangeWidth;
            if (!next)
                num1 *= -1.0;
            CubicEase cubicEase = new CubicEase();
            int num2 = 2;
            ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num2);
            IEasingFunction ieasingFunction = (IEasingFunction)cubicEase;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            TranslateTransform renderTransform1 = element1.RenderTransform as TranslateTransform;
            TranslateTransform renderTransform2 = element2.RenderTransform as TranslateTransform;
            animInfoList.Add(new AnimationInfo()
            {
                from = renderTransform1.X,
                to = renderTransform1.X + num1,
                propertyPath = TranslateTransform.XProperty,
                duration = 500,
                target = (DependencyObject)renderTransform1,
                easing = ieasingFunction
            });
            animInfoList.Add(new AnimationInfo()
            {
                from = renderTransform2.X,
                to = renderTransform2.X + num1,
                propertyPath = TranslateTransform.XProperty,
                duration = 500,
                target = (DependencyObject)renderTransform2,
                easing = ieasingFunction
            });
            int? startTime = new int?(0);
            Action completed = completedCallback;
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.NextElementSwipeDelay.Ticks <= 0L)
                return;
            this._nextElementSwipeTimer.Stop();
            this._nextElementSwipeTimer.Tick -= (new EventHandler(this.NextElementSwipeTimer_OnTick));
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            System.TimeSpan elementSwipeDelay = this.NextElementSwipeDelay;
            dispatcherTimer.Interval = elementSwipeDelay;
            this._nextElementSwipeTimer = dispatcherTimer;
            this._nextElementSwipeTimer.Tick += (new EventHandler(this.NextElementSwipeTimer_OnTick));
            this._nextElementSwipeTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this._nextElementSwipeTimer == null)
                return;
            this._nextElementSwipeTimer.Stop();
            this._nextElementSwipeTimer.Tick -= (new EventHandler(this.NextElementSwipeTimer_OnTick));
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.UpdateSources(true, true, true);
            if (this.SelectedIndex >= this.Items.Count)
                this.SelectedIndex = this.Items.Count - 1;
            // ISSUE: reference to a compiler-generated field
            if (this.Items.Count != 0 || this.ItemsCleared == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.ItemsCleared(this, EventArgs.Empty);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeElements();
        }

        private void ArrangeElements()
        {
            double num = this.SelectedIndex != 0 ? (this.SelectedIndex != this.Items.Count - 1 ? 0.0 : this.NextElementMargin) : 0.0;
            (((UIElement)this._elements[0]).RenderTransform as TranslateTransform).X = (-this.ArrangeWidth + num);
            (((UIElement)this._elements[1]).RenderTransform as TranslateTransform).X = num;
            (((UIElement)this._elements[2]).RenderTransform as TranslateTransform).X = (this.ArrangeWidth + num);
        }

        private void UpdateSources(bool update0, bool update1, bool update2)
        {
            if (update1)
                this.SetDataContext((FrameworkElement)this._elements[1], this.GetItem(this._selectedIndex));
            if (update0)
                this.SetDataContext((FrameworkElement)this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (update2)
                this.SetDataContext((FrameworkElement)this._elements[2], this.GetItem(this._selectedIndex + 1));
            this.SetActiveState((FrameworkElement)this._elements[0], false);
            this.SetActiveState((FrameworkElement)this._elements[1], true);
            this.SetActiveState((FrameworkElement)this._elements[2], false);
        }

        private void UpdateSources(bool keepCurrentAsIs = false, bool? movedForvard = null)
        {
            if (!keepCurrentAsIs && !movedForvard.HasValue)
                this.SetDataContext((FrameworkElement)this._elements[1], this.GetItem(this._selectedIndex));
            int num = !movedForvard.HasValue ? 1 : (movedForvard.Value ? 1 : 0);
            if ((!movedForvard.HasValue ? 1 : (!movedForvard.Value ? 1 : 0)) != 0)
                this.SetDataContext((FrameworkElement)this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (num != 0)
                this.SetDataContext((FrameworkElement)this._elements[2], this.GetItem(this._selectedIndex + 1));
            this.SetActiveState((FrameworkElement)this._elements[0], false);
            this.SetActiveState((FrameworkElement)this._elements[1], true);
            this.SetActiveState((FrameworkElement)this._elements[2], false);
        }

        private void SetDataContext(FrameworkElement frameworkElement, object dc)
        {
            ISupportDataContext supportDataContext = frameworkElement as ISupportDataContext;
            if (supportDataContext != null)
                supportDataContext.SetDataContext(dc);
            else
                frameworkElement.DataContext = dc;
        }

        public GameHeader GetCurrentGame()
        {
            CatalogBannerUC element = this._elements[1] as CatalogBannerUC;
            if (element != null)
                return element.CatalogBanner;
            return null;
        }

        private void SetActiveState(FrameworkElement frameworkElement, bool isActive)
        {
            ISupportState supportState = frameworkElement as ISupportState;
            if (supportState == null)
                return;
            supportState.SetState(isActive);
        }

        private object GetItem(int ind)
        {
            if (ind < 0 || ind >= this.Items.Count)
                return null;
            return this.Items[ind];
        }

        private void EnsureElements()
        {
            if (this._elements != null)
                return;
            List<Control> controlList = new List<Control>(3);
            Control control1 = this.CreateSingleElement();
            controlList.Add(control1);
            Control control2 = this.CreateSingleElement();
            controlList.Add(control2);
            Control control3 = this.CreateSingleElement();
            controlList.Add(control3);
            this._elements = controlList;
            using (List<Control>.Enumerator enumerator = this._elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Control current = enumerator.Current;
                    ((UIElement)current).RenderTransform = ((Transform)new TranslateTransform());
                    ((UIElement)current).CacheMode = ((CacheMode)new BitmapCache());
                    ((UIElement)current).ManipulationStarted += (new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted));
                    ((UIElement)current).ManipulationDelta += (new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta));
                    ((UIElement)current).ManipulationCompleted += (new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted));
                    ((PresentationFrameworkCollection<UIElement>)((Panel)this.LayoutRoot).Children).Add((UIElement)current);
                }
            }
        }

        private void Element_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
            this._nextElementSwipeTimer.Stop();
        }

        private void Element_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            if (e.PinchManipulation != null)
                return;
            System.Windows.Point translation = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            this.HandleDragDelta(translation.X, translation.Y);
        }

        private void Element_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            if (this.NextElementSwipeDelay.Ticks > 0L)
                this._nextElementSwipeTimer.Start();
            System.Windows.Point linearVelocity = e.FinalVelocities.LinearVelocity;
            // ISSUE: explicit reference operation
            this.HandleDragCompleted(linearVelocity.X);
        }

        private void HandleDragDelta(double hDelta, double vDelta)
        {
            if (this._isAnimating)
                return;
            TranslateTransform renderTransform1 = ((UIElement)this.CurrentElement).RenderTransform as TranslateTransform;
            if ((renderTransform1.X == 0.0 || Math.Abs(renderTransform1.X) == this.NextElementMargin) && this.AllowVerticalSwipe && (hDelta == 0.0 && vDelta != 0.0 || Math.Abs(vDelta) / Math.Abs(hDelta) > 1.2))
            {
                if (vDelta < 0.0)
                    return;
                this._isInVerticalSwipe = true;
                using (List<Control>.Enumerator enumerator = this._elements.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        TranslateTransform renderTransform2 = ((UIElement)enumerator.Current).RenderTransform as TranslateTransform;
                        double num = renderTransform2.Y + vDelta;
                        renderTransform2.Y = num;
                    }
                }
                if (renderTransform1.Y <= 100.0 && vDelta <= 100.0 || this._isReadyToHideFired)
                    return;
                this._isReadyToHideFired = true;

                //if (this.SwipedToHide != null)
                //  this.SwipedToHide(this, EventArgs.Empty);
            }
            else
            {
                if (this._isInVerticalSwipe)
                    return;
                if (this._selectedIndex == 0 && hDelta > 0.0 && renderTransform1.X > 0.0 || this._selectedIndex == this._items.Count - 1 && hDelta < 0.0 && renderTransform1.X < 0.0)
                    hDelta /= 3.0;
                using (List<Control>.Enumerator enumerator = this._elements.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        TranslateTransform renderTransform2 = ((UIElement)enumerator.Current).RenderTransform as TranslateTransform;
                        double num = renderTransform2.X + hDelta;
                        renderTransform2.X = num;
                    }
                }
            }
        }

        private void HandleDragCompleted(double hVelocity)
        {
            if (this._isAnimating)
                return;
            double num1 = hVelocity;
            bool? moveNext = new bool?();
            double x = (((UIElement)this.CurrentElement).RenderTransform as TranslateTransform).X;
            if ((((UIElement)this.CurrentElement).RenderTransform as TranslateTransform).Y < 100.0)
            {
                using (List<Control>.Enumerator enumerator = this._elements.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        this.AnimateElementVerticalOnDragComplete((FrameworkElement)enumerator.Current, 0.0);
                }
            }
            this._isInVerticalSwipe = false;
            double num2 = num1;
            if ((num2 < -100.0 && x < 0.0 || x <= -base.Width / 2.0) && this._selectedIndex < this._items.Count - 1)
                moveNext = new bool?(true);
            else if ((num2 > 100.0 && x > 0.0 || x >= base.Width / 2.0) && this._selectedIndex > 0)
                moveNext = new bool?(false);
            bool flag1 = this.SelectedIndex <= 1;
            bool flag2 = this.SelectedIndex >= this.Items.Count - 2;
            bool? nullable1 = moveNext;
            bool flag3 = true;
            double num3;
            if ((nullable1.GetValueOrDefault() == flag3 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
            {
                num3 = !flag2 ? -this.ArrangeWidth : -this.ArrangeWidth + this.NextElementMargin;
            }
            else
            {
                bool? nullable2 = moveNext;
                bool flag4 = false;
                num3 = (nullable2.GetValueOrDefault() == flag4 ? (nullable2.HasValue ? 1 : 0) : 0) == 0 ? (this.SelectedIndex <= this.Items.Count - 2 ? (this.SelectedIndex >= 1 ? 0.0 : 0.0) : (this.Items.Count <= 1 ? 0.0 : this.NextElementMargin)) : (!flag1 ? this.ArrangeWidth : this.ArrangeWidth);
            }
            double delta = num3 - x;
            if (moveNext.HasValue && moveNext.Value)
            {
                this._isAnimating = true;
                this.AnimateTwoElementsOnDragComplete((FrameworkElement)this._elements[1], (FrameworkElement)this._elements[2], delta, (Action)(() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                    this._isAnimating = false;
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else if (moveNext.HasValue && !moveNext.Value)
            {
                this._isAnimating = true;
                this.AnimateTwoElementsOnDragComplete((FrameworkElement)this._elements[0], (FrameworkElement)this._elements[1], delta, (Action)(() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                    this._isAnimating = false;
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else
            {
                if (delta == 0.0)
                    return;
                this.AnimateElementOnDragComplete((FrameworkElement)this._elements[0], delta, null, moveNext.HasValue);
                this.AnimateElementOnDragComplete((FrameworkElement)this._elements[1], delta, null, moveNext.HasValue);
                this.AnimateElementOnDragComplete((FrameworkElement)this._elements[2], delta, new Action(this.ArrangeElements), moveNext.HasValue);
            }
        }

        private void AnimateTwoElementsOnDragComplete(FrameworkElement element1, FrameworkElement element2, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int num = movingToNextOrPrevious ? 200 : 175;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            TranslateTransform renderTransform1 = ((UIElement)element1).RenderTransform as TranslateTransform;
            TranslateTransform renderTransform2 = ((UIElement)element2).RenderTransform as TranslateTransform;
            animInfoList.Add(new AnimationInfo()
            {
                from = renderTransform1.X,
                to = renderTransform1.X + delta,
                propertyPath = TranslateTransform.XProperty,
                duration = num,
                target = (DependencyObject)renderTransform1,
                easing = GamesCatalogBannersSlideView.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationInfo()
            {
                from = renderTransform2.X,
                to = renderTransform2.X + delta,
                propertyPath = TranslateTransform.XProperty,
                duration = num,
                target = (DependencyObject)renderTransform2,
                easing = GamesCatalogBannersSlideView.ANIMATION_EASING
            });
            int? startTime = new int?(0);
            Action completed = completedCallback;
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void AnimateElementOnDragComplete(FrameworkElement element, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int duration = movingToNextOrPrevious ? 200 : 175;
            TranslateTransform renderTransform = ((UIElement)element).RenderTransform as TranslateTransform;
            ((DependencyObject)renderTransform).Animate(renderTransform.X, renderTransform.X + delta, TranslateTransform.XProperty, duration, new int?(0), GamesCatalogBannersSlideView.ANIMATION_EASING, completedCallback);
        }

        private void AnimateElementVerticalOnDragComplete(FrameworkElement element, double to)
        {
            int duration = 200;
            TranslateTransform renderTransform = ((UIElement)element).RenderTransform as TranslateTransform;
            ((DependencyObject)renderTransform).Animate(renderTransform.Y, to, TranslateTransform.YProperty, duration, new int?(0), GamesCatalogBannersSlideView.ANIMATION_EASING, null);
        }

        private void MoveToNextOrPrevious(bool next)
        {
            if (next)
            {
                GamesCatalogBannersSlideView.Swap(this._elements, 0, 1);
                GamesCatalogBannersSlideView.Swap(this._elements, 1, 2);
            }
            else
            {
                GamesCatalogBannersSlideView.Swap(this._elements, 1, 2);
                GamesCatalogBannersSlideView.Swap(this._elements, 0, 1);
            }
            this.UpdateSources(false, new bool?(next));
        }

        private void ChangeCurrentInd(bool next)
        {
            this._selectedIndex = !next ? this._selectedIndex - 1 : this._selectedIndex + 1;
            // ISSUE: reference to a compiler-generated field
            if (this.SelectionChanged != null)
            {
                // ISSUE: reference to a compiler-generated field
                this.SelectionChanged(this, this._selectedIndex);
            }
            this.OnPropertyChanged("SelectedIndex");
        }

        private static void Swap(List<Control> elements, int ind1, int ind2)
        {
            Control element = elements[ind1];
            elements[ind1] = elements[ind2];
            elements[ind2] = element;
        }

        public void EnableSwipe()
        {
            this.DisableSwipe();
            using (List<Control>.Enumerator enumerator = this._elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Control current = enumerator.Current;
                    EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                    ((UIElement)current).ManipulationStarted += (eventHandler1);
                    EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                    ((UIElement)current).ManipulationDelta += (eventHandler2);
                    EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                    ((UIElement)current).ManipulationCompleted += (eventHandler3);
                }
            }
        }

        public void DisableSwipe()
        {
            using (List<Control>.Enumerator enumerator = this._elements.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Control current = enumerator.Current;
                    EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                    ((UIElement)current).ManipulationStarted -= (eventHandler1);
                    EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                    ((UIElement)current).ManipulationDelta -= (eventHandler2);
                    EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                    ((UIElement)current).ManipulationCompleted -= (eventHandler3);
                }
            }
        }

        public void PushScrollPosition(double sp)
        {
            if (!this.IsScrollListeningEnabled)
                return;
            if (sp > 0.0)
                this.DisableSwipe();
            else
                this.EnableSwipe();
        }

        private double CalculateOpacityFadeAwayBasedOnScroll(double sp)
        {
            return sp >= 10.0 ? (sp <= 60.0 ? 1.0 - (0.025 * sp - 0.25) : 0.0) : 1.0;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // ISSUE: reference to a compiler-generated field
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new System.Uri("/VKClient.Common;component/UC/GamesCatalogBannersSlideView.xaml", UriKind.Relative));
            this.gridRoot = (Grid)base.FindName("gridRoot");
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
        }
    }
}
