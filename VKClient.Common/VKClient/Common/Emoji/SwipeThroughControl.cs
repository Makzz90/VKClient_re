using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Utils;
using Windows.Foundation;

namespace VKClient.Common.Emoji
{
    public class SwipeThroughControl : UserControl, INotifyPropertyChanged
    {
        private readonly ScrollViewerOffsetMediator _scrollMediator = new ScrollViewerOffsetMediator();
        private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
        private const int DURATION_BOUNCING = 175;
        private const int DURATION_MOVE_TO_NEXT = 200;
        private static readonly IEasingFunction ANIMATION_EASING;
        public static readonly DependencyProperty BackgroundColorProperty;
        private ObservableCollection<object> _items;
        private List<object> _headerItems;
        private List<object> _footerItems;
        private int _selectedIndex;
        private List<Control> _elements;
        //private bool _isInVerticalSwipe;
        private bool _isAnimating;
        internal DataTemplate SystemItemTemplate;
        internal Grid gridRoot;
        internal Grid LayoutRoot;
        internal ScrollViewer filtersScrollViewer;
        internal ItemsControl itemsControlHeader;
        internal ItemsControl itemsControlFooter;
        internal Rectangle rectBackspacePlaceholder;
        internal Grid gridBackspace;
        internal TranslateTransform translateBackspace;
        private bool _contentLoaded;

        public Func<Control> CreateSingleElement { get; set; }

        public bool ChangeIndexBeforeAnimation { get; set; }

        public double NextElementMargin { get; set; }

        public bool IsScrollListeningEnabled { get; set; }

        public bool IsFilteringEnabled
        {
            set
            {
                this.filtersScrollViewer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Action BackspaceTapCallback { get; set; }

        public Brush BackgroundColor
        {
            get
            {
                return (Brush)this.GetValue(SwipeThroughControl.BackgroundColorProperty);
            }
            set
            {
                this.SetValue(SwipeThroughControl.BackgroundColorProperty, (object)value);
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

        public Visibility HeaderItemsVisibility { get; private set; }

        public Visibility FooterItemsVisibility { get; private set; }

        public List<object> HeaderItems
        {
            get
            {
                return this._headerItems;
            }
            set
            {
                this._headerItems = value;
                this.OnPropertyChanged("HeaderItems");
                this.OnPropertyChanged("HeaderItemsVisibility");
            }
        }

        public List<object> FooterItems
        {
            get
            {
                return this._footerItems;
            }
            set
            {
                this._footerItems = value;
                this.OnPropertyChanged("FooterItems");
                this.OnPropertyChanged("FooterItemsVisibility");
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
                int num = this._selectedIndex;
                this._selectedIndex = value;
                this.OnPropertyChanged("SelectedIndex");
                this.UpdateIsSelected();
                if (this._selectedIndex - num == 2)
                {
                    SwipeThroughControl.Swap(this._elements, 0, 2);
                    this.UpdateSources(false, true, true);
                    this.ArrangeElements();
                }
                else if (num - this._selectedIndex == 2)
                {
                    SwipeThroughControl.Swap(this._elements, 0, 2);
                    this.UpdateSources(true, true, false);
                    this.ArrangeElements();
                }
                else if (this._selectedIndex - num == 1)
                {
                    this.MoveToNextOrPrevious(true);
                    this.ArrangeElements();
                }
                else if (num - this._selectedIndex == 1)
                {
                    this.MoveToNextOrPrevious(false);
                    this.ArrangeElements();
                }
                else
                    this.UpdateSources(false, new bool?());
                this.UpdateBackspaceVisibility();
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
                return this.ActualWidth - this.NextElementMargin;
            }
        }

        public bool AllowVerticalSwipe { get; set; }

        public event TypedEventHandler<SwipeThroughControl, int> SelectionChanged;

        public event EventHandler ItemsCleared;

        public event PropertyChangedEventHandler PropertyChanged;

        static SwipeThroughControl()
        {
            CubicEase cubicEase = new CubicEase();
            int num = 0;
            cubicEase.EasingMode = (EasingMode)num;
            SwipeThroughControl.ANIMATION_EASING = (IEasingFunction)cubicEase;
            SwipeThroughControl.BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(SwipeThroughControl), new PropertyMetadata(new PropertyChangedCallback(SwipeThroughControl.OnBackgroundBrushChanged)));
        }

        public SwipeThroughControl()
        {
            this.InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
            this.IsFilteringEnabled = true;
            this._scrollMediator.ScrollViewer = this.filtersScrollViewer;
            this.DataContext = (object)this;
        }

        public SwipeThroughControl(double nextElementMargin)
            : this()
        {
            this.NextElementMargin = nextElementMargin;
        }

        private static void OnBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SwipeThroughControl swipeThroughControl = d as SwipeThroughControl;
            if (swipeThroughControl == null)
                return;
            Brush brush = e.NewValue as Brush;
            if (brush == null)
                return;
            swipeThroughControl.gridRoot.Background = brush;
        }

        private void UpdateIsSelected()
        {
            if (this.Items == null)
                return;
            for (int p = 0; p < this.Items.Count; ++p)
            {
                object obj = this.Items[p];
                if (obj is SpriteListItemData)
                {
                    (obj as SpriteListItemData).IsSelected = p == this.SelectedIndex;
                    if (p == this.SelectedIndex)
                        this.ScrollToIndex(p, (Action)(() => { }));
                }
            }
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.UpdateSources(true, true, true);
            if (this.SelectedIndex >= this.Items.Count)
                this.SelectedIndex = this.Items.Count - 1;
            if (this.Items.Count != 0 || this.ItemsCleared == null)
                return;
            this.ItemsCleared((object)this, EventArgs.Empty);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeElements();
            this.ScrollToIndex(this.SelectedIndex, (Action)(() => { }));
        }

        private void ArrangeElements()
        {
            double num = this.SelectedIndex != 0 ? (this.SelectedIndex != this.Items.Count - 1 ? 0.0 : this.NextElementMargin) : 0.0;
            (this._elements[0].RenderTransform as TranslateTransform).X = -this.ArrangeWidth + num;
            (this._elements[1].RenderTransform as TranslateTransform).X = num;
            (this._elements[2].RenderTransform as TranslateTransform).X = this.ArrangeWidth + num;
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

        private void SetActiveState(FrameworkElement frameworkElement, bool isActive)
        {
            ISupportState supportState = frameworkElement as ISupportState;
            if (supportState == null)
                return;
            supportState.SetState(isActive);
        }

        private void UpdateBackspaceVisibility()
        {
            if (this.Items == null)
                return;
            ISupportBackspaceListItem backspaceListItem = this.Items[this.SelectedIndex] as ISupportBackspaceListItem;
            double num1;
            double num2;
            if (backspaceListItem != null && backspaceListItem.IsBackspaceVisible)
            {
                num1 = 0.0;
                num2 = this.gridBackspace.Width;
            }
            else
            {
                num1 = this.gridBackspace.Width;
                num2 = 0.0;
            }
            if (this.translateBackspace.X == num1 && this.rectBackspacePlaceholder.Width == num2)
                return;
            CubicEase cubicEase = new CubicEase();
            int num3 = 0;
            cubicEase.EasingMode = (EasingMode)num3;
            IEasingFunction easingFunction = (IEasingFunction)cubicEase;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.translateBackspace,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = 256,
                from = this.translateBackspace.X,
                to = num1,
                easing = easingFunction
            });
            animInfoList.Add(new AnimationInfo()
            {
                target = (DependencyObject)this.rectBackspacePlaceholder,
                propertyPath = (object)FrameworkElement.WidthProperty,
                duration = 256,
                from = this.rectBackspacePlaceholder.Width,
                to = num2,
                easing = easingFunction
            });
            int? startTime = new int?();
            AnimationUtil.AnimateSeveral(animInfoList, startTime, null);
        }

        private object GetItem(int ind)
        {
            if (ind < 0 || ind >= this.Items.Count)
                return (object)null;
            return this.Items[ind];
        }

        private void EnsureElements()
        {
            if (this._elements != null)
                return;
            this._elements = new List<Control>(3)
      {
        this.CreateSingleElement(),
        this.CreateSingleElement(),
        this.CreateSingleElement()
      };
            foreach (Control element in this._elements)
            {
                element.RenderTransform = (Transform)new TranslateTransform();
                element.CacheMode = (CacheMode)new BitmapCache();
                element.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                element.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                element.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                this.LayoutRoot.Children.Add((UIElement)element);
            }
        }

        private void Element_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
        }

        private void Element_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            if (e.PinchManipulation != null)
                return;
            this.HandleDragDelta(e.DeltaManipulation.Translation.X);
        }

        private void Element_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            this.HandleDragCompleted(e.FinalVelocities.LinearVelocity.X);
        }

        private void HandleDragDelta(double hDelta)
        {
            if (this._isAnimating)
                return;
            TranslateTransform translateTransform = this.CurrentElement.RenderTransform as TranslateTransform;
            if (this._selectedIndex == 0 && hDelta > 0.0 && translateTransform.X > 0.0 || this._selectedIndex == this._items.Count - 1 && hDelta < 0.0 && translateTransform.X < 0.0)
                hDelta /= 3.0;
            foreach (UIElement element in this._elements)
                (element.RenderTransform as TranslateTransform).X += hDelta;
        }

        private void HandleDragCompleted(double hVelocity)
        {
            if (this._isAnimating)
                return;
            //this._isInVerticalSwipe = false;
            double num1 = hVelocity;
            bool? moveNext = new bool?();
            double x = (this._elements[1].RenderTransform as TranslateTransform).X;
            double num2 = num1;
            if ((num2 < -100.0 && x < 0.0 || x <= -this.Width / 2.0) && this._selectedIndex < this._items.Count - 1)
                moveNext = new bool?(true);
            else if ((num2 > 100.0 && x > 0.0 || x >= this.Width / 2.0) && this._selectedIndex > 0)
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
                this.UpdateBackspaceVisibility();
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
                this.UpdateBackspaceVisibility();
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
            TranslateTransform translateTransform1 = element1.RenderTransform as TranslateTransform;
            TranslateTransform translateTransform2 = element2.RenderTransform as TranslateTransform;
            animInfoList.Add(new AnimationInfo()
            {
                from = translateTransform1.X,
                to = translateTransform1.X + delta,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = num,
                target = (DependencyObject)translateTransform1,
                easing = SwipeThroughControl.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationInfo()
            {
                from = translateTransform2.X,
                to = translateTransform2.X + delta,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = num,
                target = (DependencyObject)translateTransform2,
                easing = SwipeThroughControl.ANIMATION_EASING
            });
            int? startTime = new int?(0);
            Action completed = completedCallback;
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void AnimateElementOnDragComplete(FrameworkElement element, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int duration = movingToNextOrPrevious ? 200 : 175;
            TranslateTransform target = element.RenderTransform as TranslateTransform;
            target.Animate(target.X, target.X + delta, (object)TranslateTransform.XProperty, duration, new int?(0), SwipeThroughControl.ANIMATION_EASING, completedCallback);
        }

        private void MoveToNextOrPrevious(bool next)
        {
            if (next)
            {
                SwipeThroughControl.Swap(this._elements, 0, 1);
                SwipeThroughControl.Swap(this._elements, 1, 2);
            }
            else
            {
                SwipeThroughControl.Swap(this._elements, 1, 2);
                SwipeThroughControl.Swap(this._elements, 0, 1);
            }
            this.UpdateSources(false, new bool?(next));
        }

        private void ChangeCurrentInd(bool next)
        {
            this._selectedIndex = !next ? this._selectedIndex - 1 : this._selectedIndex + 1;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged.Invoke(this, this._selectedIndex);
            }
            this.OnPropertyChanged("SelectedIndex");
            this.UpdateIsSelected();
        }

        private static void Swap(List<Control> elements, int ind1, int ind2)
        {
            Control control = elements[ind1];
            elements[ind1] = elements[ind2];
            elements[ind2] = control;
        }

        private void SystemItem_OnTap(object sender, GestureEventArgs e)
        {
            SpriteListItemData spriteListItemData = (sender as FrameworkElement).DataContext as SpriteListItemData;
            if (spriteListItemData == null)
                return;
            spriteListItemData.ProcessSystemTab();
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            SpriteListItemData spriteListItemData = (sender as FrameworkElement).DataContext as SpriteListItemData;
            if (spriteListItemData == null)
                return;
            int num = this.Items.IndexOf((object)spriteListItemData);
            if (num < 0 || this.SelectedIndex == num)
                return;
            this.SelectedIndex = num;
        }

        private void ScrollToIndex(int p, Action callback)
        {
            if (this.filtersScrollViewer.ActualWidth == 0.0)
                return;
            double actualWidth = this.filtersScrollViewer.ActualWidth;
            ItemsControl itemsControl = this.itemsControlHeader;
            double num = itemsControl != null ? itemsControl.ActualWidth : 0.0;
            this.ScrollToOffset(Math.Min(Math.Max((double)(p * 72) + 36.0 + num - actualWidth / 2.0, 0.0), this.filtersScrollViewer.ScrollableWidth), callback);
        }

        private void ScrollToOffset(double to, Action callback)
        {
            ScrollViewerOffsetMediator target = this._scrollMediator;
            double horizontalOffset = this.filtersScrollViewer.HorizontalOffset;
            double to1 = to;
            DependencyProperty dependencyProperty = ScrollViewerOffsetMediator.HorizontalOffsetProperty;
            int duration = 300;
            int? startTime = new int?(0);
            CubicEase cubicEase = new CubicEase();
            int num1 = 2;
            cubicEase.EasingMode = (EasingMode)num1;
            Action completed = callback;
            int num2 = 0;
            target.Animate(horizontalOffset, to1, (object)dependencyProperty, duration, startTime, (IEasingFunction)cubicEase, completed, num2 != 0);
        }

        public void EnableSwipe()
        {
            this.DisableSwipe();
            foreach (Control element in this._elements)
            {
                EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                element.ManipulationStarted += eventHandler1;
                EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                element.ManipulationDelta += eventHandler2;
                EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                element.ManipulationCompleted += eventHandler3;
            }
        }

        public void DisableSwipe()
        {
            foreach (Control element in this._elements)
            {
                EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                element.ManipulationStarted -= eventHandler1;
                EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                element.ManipulationDelta -= eventHandler2;
                EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                element.ManipulationCompleted -= eventHandler3;
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

        private void BorderBackspace_OnTap(object sender, GestureEventArgs e)
        {
            Action backspaceTapCallback = this.BackspaceTapCallback;
            if (backspaceTapCallback == null)
                return;
            backspaceTapCallback();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/Emoji/SwipeThroughControl.xaml", UriKind.Relative));
            this.SystemItemTemplate = (DataTemplate)this.FindName("SystemItemTemplate");
            this.gridRoot = (Grid)this.FindName("gridRoot");
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.filtersScrollViewer = (ScrollViewer)this.FindName("filtersScrollViewer");
            this.itemsControlHeader = (ItemsControl)this.FindName("itemsControlHeader");
            this.itemsControlFooter = (ItemsControl)this.FindName("itemsControlFooter");
            this.rectBackspacePlaceholder = (Rectangle)this.FindName("rectBackspacePlaceholder");
            this.gridBackspace = (Grid)this.FindName("gridBackspace");
            this.translateBackspace = (TranslateTransform)this.FindName("translateBackspace");
        }
    }
}
