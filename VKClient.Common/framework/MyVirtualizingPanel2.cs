using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Common.Library;
using VKClient.Common.Utils;

using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.Framework
{
    public class MyVirtualizingPanel2 : Canvas, IMyVirtualizingPanel, ISupportPullToRefresh
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(MyVirtualizingPanel2), new PropertyMetadata(new PropertyChangedCallback(MyVirtualizingPanel2.OnItemsSourcePropertyChanged)));
        public static readonly double PULL_EXTRA_MARGIN = 150.0;
        public double LoadUnloadThreshold = 500.0;
        public double LoadedHeightUpwards = 500.0;
        public double LoadedHeightUpwardsNotScrolling = 500.0;
        public double LoadedHeightDownwards = 1200.0;
        public double LoadedHeightDownwardsNotScrolling = 1200.0;
        private bool _enableLog = false;
        private bool _changingVerticalOffset;
        private IScrollableArea _listScrollViewer;
        private bool _isScrolling;
        private bool _notReactToScroll;
        private double _savedDelta;
        private DelayedExecutor _de = new DelayedExecutor(450);
        private Canvas _itemsPanel = new Canvas();
        private double _previousScrollOffset;
        private DateTime _previousScrollOffsetChangedTime = DateTime.MinValue;
        private readonly double _pixelsPerSecondThreshold = 250.0;
        private List<IVirtualizable> _virtualizableItems = new List<IVirtualizable>();
        private bool _upsideDown;
        private Segment _loadedSegment = new Segment();
        private Dictionary<int, int> _thresholdPointIndexes = new Dictionary<int, int>();
        private int _countOfItemsBeforeLoadMore = 20;
        public bool KeepScrollPositionWhenAddingItems;
        // private bool _subscribedOnEdit;
        private ISupportCollectionEdit _editable;
        private List<IVirtualizable> _addedWhileEdited = new List<IVirtualizable>();
        private bool _isEditing;
        private DependencyProperty ListVerticalOffsetProperty = DependencyProperty.Register("ListVerticalOffset", typeof(double), typeof(MyVirtualizingPanel2), new PropertyMetadata(new PropertyChangedCallback(MyVirtualizingPanel2.OnListVerticalOffsetChanged)));
        private bool _isScrollingToUpOrBottom;
        private int _scrollOffsetChangedCount;
        private double? _lastReportedVerticalOffset;
        private readonly double MIN_DELTA_OFFSET_FOR_LOAD_UNLOAD = 150.0;
        private DateTime _lastTimeCalledGCCollect = DateTime.MinValue;
        //private bool _logTrackPosition;
        private Panel _outerPanel;
        private TextBlock _textBlock1 = new TextBlock();
        private TextBlock _textBlock2 = new TextBlock();
        private CheckBox _boundToScrollCheckBox = new CheckBox();
        private StackPanel _statContainer = new StackPanel();
        private int _numberOfScrollChangedCalls;
        private bool _showStatistics = false;
        private bool _statisticsBlocksAdded;
        private string _loadedSegmentsStr = "";
        private bool _preventPullUntilNextManipulationStateChange;
        private bool _lockBounds;

        public bool OnlyPartialLoad { get; set; }

        public bool ManuallyLoadMore { get; set; }

        public int CountOfItemsBeforeLoadMore
        {
            get
            {
                return this._countOfItemsBeforeLoadMore;
            }
            set
            {
                this._countOfItemsBeforeLoadMore = value;
            }
        }

        public double OffsetY { get; set; }

        public double ExtraOffsetY { get; set; }

        public IList ItemsSource
        {
            get
            {
                return (IList)base.GetValue(MyVirtualizingPanel2.ItemsSourceProperty);
            }
            set
            {
                base.SetValue(MyVirtualizingPanel2.ItemsSourceProperty, value);
            }
        }

        public Func<object, IVirtualizable> CreateVirtItemFunc { get; set; }

        public IScrollableArea ScrollViewer
        {
            get
            {
                return this._listScrollViewer;
            }
        }

        public double ListVerticalOffset
        {
            get
            {
                return (double)base.GetValue(this.ListVerticalOffsetProperty);
            }
            set
            {
                base.SetValue(this.ListVerticalOffsetProperty, value);
            }
        }

        private PhoneApplicationPage Page
        {
            get
            {
                return ((ContentControl)(Application.Current.RootVisual as PhoneApplicationFrame)).Content as PhoneApplicationPage;
            }
        }

        public List<IVirtualizable> VirtualizableItems
        {
            get
            {
                return this._virtualizableItems;
            }
        }

        public double DeltaOffset { get; set; }

        internal Panel OuterPanel
        {
            get
            {
                if (this._outerPanel == null)
                {
                    IEnumerable<ContentPresenter> logicalChildrenByType1 = ((FrameworkElement)(Application.Current.RootVisual as Frame)).GetLogicalChildrenByType<ContentPresenter>(false);
                    for (int index = 0; index < Enumerable.Count<ContentPresenter>(logicalChildrenByType1); ++index)
                    {
                        IEnumerable<Panel> logicalChildrenByType2 = ((FrameworkElement)Enumerable.ElementAt<ContentPresenter>(logicalChildrenByType1, index)).GetLogicalChildrenByType<Panel>(false);
                        if (Enumerable.Any<Panel>(logicalChildrenByType2))
                        {
                            this._outerPanel = Enumerable.First<Panel>(logicalChildrenByType2);
                            break;
                        }
                    }
                }
                return this._outerPanel;
            }
        }

        public double PullPercentage
        {
            get
            {
                if (this._preventPullUntilNextManipulationStateChange || !this._lockBounds || this.ScrollViewer.VerticalOffset >= 0.0)
                    return 0.0;
                return -this.ScrollViewer.VerticalOffset * 100.0 / MyVirtualizingPanel2.PULL_EXTRA_MARGIN;
            }
        }

        public Action OnPullPercentageChanged { get; set; }

        public Action OnRefresh { get; set; }

        public event MyVirtualizingPanel2.OnCompression Compression;

        public event EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs> ScrollPositionChanged;

        public MyVirtualizingPanel2()
        {
            //base.\u002Ector();
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.MyVirtualizingPanel_Loaded));
            this.Children.Add(this._itemsPanel);
        }

        public void Cleanup()
        {
            this.ClearItems();
            base.ClearValue(MyVirtualizingPanel2.ItemsSourceProperty);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MyVirtualizingPanel2 virtualizingPanel2 = d as MyVirtualizingPanel2;
            if (virtualizingPanel2 == null)
                return;
            virtualizingPanel2.ClearItems();
            // ISSUE: explicit reference operation
            INotifyCollectionChanged oldValue = e.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                virtualizingPanel2.UnhookCollectionChanged(oldValue);
            // ISSUE: explicit reference operation
            INotifyCollectionChanged newValue = e.NewValue as INotifyCollectionChanged;
            if (newValue != null)
            {
                List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
                IEnumerator enumerator = (newValue as ICollection).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            virtualizableList.Add(current as IVirtualizable);
                        else if (virtualizingPanel2.CreateVirtItemFunc != null)
                            virtualizableList.Add(virtualizingPanel2.CreateVirtItemFunc(current));
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                virtualizingPanel2.AddItems((IEnumerable<IVirtualizable>)virtualizableList);
                virtualizingPanel2.HookUpCollectionChanged(newValue);
                virtualizingPanel2.SubscribeOnEdit();
            }
            else
                virtualizingPanel2.UnsubscribeOnEdit();
        }

        private void UnsubscribeOnEdit()
        {
            if (this._editable == null)
                return;
            this._editable.StartedEdit -= new EventHandler(this.editable_StartedEdit);
            this._editable.EndedEdit -= new EventHandler(this.editable_EndedEdit);
            this._editable = null;
        }

        private void SubscribeOnEdit()
        {
            ISupportCollectionEdit dataContext = base.DataContext as ISupportCollectionEdit;
            if (dataContext == null || this._editable == dataContext)
                return;
            this.UnsubscribeOnEdit();
            dataContext.StartedEdit += new EventHandler(this.editable_StartedEdit);
            dataContext.EndedEdit += new EventHandler(this.editable_EndedEdit);
            this._editable = dataContext;
        }

        private void editable_StartedEdit(object sender, EventArgs e)
        {
            this._isEditing = true;
        }

        private void editable_EndedEdit(object sender, EventArgs e)
        {
            this._isEditing = false;
            this.AddItems((IEnumerable<IVirtualizable>)this._addedWhileEdited);
            this._addedWhileEdited.Clear();
        }

        public void HookUpCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.collection_CollectionChanged);
        }

        public void UnhookCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.collection_CollectionChanged);
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MyVirtualizingPanel2 virtualizingPanel2 = this;
            List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
            if (e.NewItems != null)
            {
                IEnumerator enumerator = ((IEnumerable)e.NewItems).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            itemsToInsert.Add(current as IVirtualizable);
                        else if (this.CreateVirtItemFunc != null)
                            itemsToInsert.Add(this.CreateVirtItemFunc(current));
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            List<IVirtualizable> virtualizableList1 = new List<IVirtualizable>();
            if (e.OldItems != null)
            {
                IEnumerator enumerator = ((IEnumerable)e.OldItems).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is IVirtualizable)
                            virtualizableList1.Add(current as IVirtualizable);
                        else if (e.OldStartingIndex >= 0 && e.OldStartingIndex < virtualizingPanel2.VirtualizableItems.Count)
                        {
                            IVirtualizable virtualizableItem = virtualizingPanel2.VirtualizableItems[e.OldStartingIndex];
                            virtualizingPanel2.RemoveItem(virtualizableItem);
                        }
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            if (this._isEditing)
                this._addedWhileEdited.AddRange((IEnumerable<IVirtualizable>)itemsToInsert);
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex >= virtualizingPanel2.VirtualizableItems.Count)
                    virtualizingPanel2.AddItems((IEnumerable<IVirtualizable>)itemsToInsert);
                else
                    virtualizingPanel2.InsertRemoveItems(e.NewStartingIndex, itemsToInsert, virtualizingPanel2.KeepScrollPositionWhenAddingItems, null);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                virtualizingPanel2.ClearItems();
                List<IVirtualizable> virtualizableList2 = new List<IVirtualizable>();
                IEnumerator enumerator = virtualizingPanel2.ItemsSource.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        virtualizableList2.Add(current is IVirtualizable ? current as IVirtualizable : this.CreateVirtItemFunc(current));
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                virtualizingPanel2.AddItems((IEnumerable<IVirtualizable>)virtualizableList2);
            }
            else
            {
                if (e.Action != NotifyCollectionChangedAction.Remove || virtualizableList1.Count <= 0)
                    return;
                virtualizingPanel2.RemoveItem(virtualizableList1[0]);
            }
        }

        private void HookupScrollEvents()
        {
            if (this.ScrollViewer == null)
                return;
            this.ScrollViewer.OnCompressionTop = (Action)(() =>
            {
                if (this.Compression == null)
                    return;
                this.Compression(this, new CompressionEventArgs(CompressionType.Top));
            });
            this.ScrollViewer.OnCompressionBottom = (Action)(() =>
            {
                if (this.Compression == null)
                    return;
                this.Compression(this, new CompressionEventArgs(CompressionType.Bottom));
            });
        }

        private static void OnListVerticalOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as MyVirtualizingPanel2).OnListVerticalOffsetChanged();
        }

        public void InitializeWithScrollViewer(IScrollableArea scrollViewer, bool upsideDown = false)
        {
            this._listScrollViewer = scrollViewer;
            this._upsideDown = upsideDown;
            if (!upsideDown)
                return;
            IScrollableArea listScrollViewer = this._listScrollViewer;
            PlaneProjection planeProjection = new PlaneProjection();
            planeProjection.RotationZ = 180.0;
            listScrollViewer.Projection = planeProjection;
        }

        protected void EnsureBoundToScrollViewer()
        {
            this.ScrollViewer.OnVerticalOffsetChanged = (Action<double>)(vo =>
            {
                this.ListVerticalOffset = vo;
                if (this.ScrollViewer.VerticalOffset == 0.0 && this.ScrollViewer.IsManipulating)
                    this.LockBoundsForPull();
                if (this.OnPullPercentageChanged != null)
                    this.OnPullPercentageChanged();
                if (this.PullPercentage < 99.9 || this.OnRefresh == null)
                    return;
                this.OnRefresh();
                this._preventPullUntilNextManipulationStateChange = true;
                this.UnlockBounds();
                if (this.OnPullPercentageChanged == null)
                    return;
                this.OnPullPercentageChanged();
            });
            this.SubscribeToStateChanging(true);
        }

        protected void UnbindFromScrollViewer()
        {
            this.ScrollViewer.OnVerticalOffsetChanged = null;
            this.SubscribeToStateChanging(false);
        }

        private void SubscribeToStateChanging(bool subscribe = true)
        {
            if (subscribe)
                this.ScrollViewer.OnScrollStateChanged = (Action<bool, bool>)((isScrolling, isManipulating) =>
                {
                    this.group_CurrentStateChanging(isScrolling);
                    this._preventPullUntilNextManipulationStateChange = false;
                    if (isManipulating && this.ScrollViewer.VerticalOffset == 0.0)
                        this.LockBoundsForPull();
                    else
                        this.UnlockBounds();
                    if (isScrolling)
                        return;
                    foreach (UIElement uiElement in VisualTreeHelper.FindElementsInHostCoordinates(new Point(FramePageUtils.Frame.ActualWidth / 2.0, FramePageUtils.Frame.ActualHeight / 2.0), Application.Current.RootVisual).Where<UIElement>((Func<UIElement, bool>)(e => e is INotifiableWhenOnScreenCenter)))
                        (uiElement as INotifiableWhenOnScreenCenter).NotifyInTheCenterOfScreen();
                });
            else
                this.ScrollViewer.OnScrollStateChanged = (Action<bool, bool>)null;
        }

        private void LockBoundsForPull()
        {
            if (this._lockBounds)
                return;
            this._lockBounds = true;
            this.ScrollViewer.Bounds = new Rect(this.ScrollViewer.Bounds.Left, -MyVirtualizingPanel2.PULL_EXTRA_MARGIN, this.ScrollViewer.Bounds.Width, this.ScrollViewer.Bounds.Height + MyVirtualizingPanel2.PULL_EXTRA_MARGIN);
        }

        private void UnlockBounds()
        {
            if (!this._lockBounds)
                return;
            this._lockBounds = false;
            this.ScrollViewer.Bounds = new Rect(this.ScrollViewer.Bounds.Left, 0.0, this.ScrollViewer.Bounds.Width, this.ScrollViewer.Bounds.Height - MyVirtualizingPanel2.PULL_EXTRA_MARGIN);
        }

        public void ScrollToBottom(bool toBottom = true)
        {
            if (this._isScrollingToUpOrBottom)
                return;
            this._isScrollingToUpOrBottom = true;
            this.EnsureFocusIsOnPage();
            this._notReactToScroll = true;
            this._savedDelta = this.DeltaOffset;
            this.DeltaOffset = !toBottom ? -this._listScrollViewer.VerticalOffset : this._listScrollViewer.ExtentHeight - this._listScrollViewer.ViewportHeight - this._listScrollViewer.VerticalOffset;
            this.Log("PrepareForScrollToBottom");
            this.PerformLoadUnload2(VirtualizableState.LoadedPartially, true);
            this.UnbindFromScrollViewer();
            this.ScrollViewer.ScrollToTopOrBottom(toBottom, (Action)(() =>
            {
                this._isScrollingToUpOrBottom = false;
                this.ScrollToBottomCompleted(toBottom);
            }));
        }

        private void EnsureFocusIsOnPage()
        {
            if (FocusManager.GetFocusedElement() is TextBox || this.Page == null)
                return;
            ((Control)this.Page).Focus();
        }

        internal void ScrollToBottomCompleted(bool toBottom = true)
        {
            this.EnsureFocusIsOnPage();
            this.EnsureBoundToScrollViewer();
            this._notReactToScroll = false;
            this.DeltaOffset = this._savedDelta;
            this.OnlyPartialLoad = false;
            this.PerformLoadUnload(VirtualizableState.LoadedFully);
            this.Log("ScrolltoBottomCompleted");
        }

        private static VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;
            IEnumerator enumerator = ((IEnumerable)VisualStateManager.GetVisualStateGroups(element)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    VisualStateGroup current = (VisualStateGroup)enumerator.Current;
                    if (current.Name == name)
                        return current;
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            return null;
        }

        private void OnListVerticalOffsetChanged()
        {
            if (this._notReactToScroll || this._changingVerticalOffset)
                return;
            if (!this._lastReportedVerticalOffset.HasValue)
                this._lastReportedVerticalOffset = new double?(this._listScrollViewer.VerticalOffset);
            if (Math.Abs(this._listScrollViewer.VerticalOffset - this._lastReportedVerticalOffset.Value) >= this.MIN_DELTA_OFFSET_FOR_LOAD_UNLOAD)
            {
                this.PerformLoadUnload(VirtualizableState.LoadedFully);
                this._lastReportedVerticalOffset = new double?(this._listScrollViewer.VerticalOffset);
            }
            // ISSUE: reference to a compiler-generated field
            if (this.ScrollPositionChanged != null)
            {
                // ISSUE: reference to a compiler-generated field
                this.ScrollPositionChanged(this, new MyVirtualizingPanel2.ScrollPositionChangedEventAgrs(this._listScrollViewer.VerticalOffset, base.Height));
            }
            this.LoadMoreIfNeeded(this._listScrollViewer.VerticalOffset, base.Height);
            this.Log(string.Concat("Reported Offset: ", this._listScrollViewer.VerticalOffset));
            int offsetChangedCount = this._scrollOffsetChangedCount;
            this._scrollOffsetChangedCount = offsetChangedCount + 1;
            if (offsetChangedCount % 20 != 0 || !MemoryInfo.IsLowMemDevice || (DateTime.Now - this._lastTimeCalledGCCollect).TotalSeconds <= 30.0)
                return;
            GC.Collect();
            this._lastTimeCalledGCCollect = DateTime.Now;
        }

        private void LoadMoreIfNeeded(double currentPosition, double scrollHeight)
        {
            if (this.ManuallyLoadMore || scrollHeight == 0.0 || this.VirtualizableItems.Count <= 0 || (scrollHeight - currentPosition >= (double)this.CountOfItemsBeforeLoadMore * Enumerable.Sum<IVirtualizable>(this.VirtualizableItems, (Func<IVirtualizable, double>)(v => v.FixedHeight)) / (double)this.VirtualizableItems.Count || currentPosition <= 150.0 || !(base.DataContext is ISupportLoadMore)))
                return;
            (base.DataContext as ISupportLoadMore).LoadMore();
        }

        private void group_CurrentStateChanging(bool isScrolling)
        {
            if (isScrolling)
            {
                this.Log("STarted scroll!");
                this._isScrolling = true;
                VeryLowProfileImageLoader.AllowBoostLoading = false;
                this.UpdateScrollObservation();
            }
            else
            {
                this._isScrolling = false;
                VeryLowProfileImageLoader.AllowBoostLoading = true;
                this.PerformLoadUnload(VirtualizableState.LoadedFully);
            }
        }

        private void UpdateScrollObservation()
        {
            this._previousScrollOffset = this._listScrollViewer.VerticalOffset;
            this._previousScrollOffsetChangedTime = DateTime.Now;
        }

        private bool DetermineIfScrollingIsFast()
        {
            DateTime now = DateTime.Now;
            bool flag = false;
            if (this._previousScrollOffsetChangedTime != now)
            {
                double num = Math.Abs(this._listScrollViewer.VerticalOffset - this._previousScrollOffset);
                double totalSeconds = (now - this._previousScrollOffsetChangedTime).TotalSeconds;
                if (num != 0.0 && num / totalSeconds > this._pixelsPerSecondThreshold)
                    flag = true;
            }
            this.UpdateScrollObservation();
            return flag;
        }

        public void ChangeHeight(double height)
        {
            base.Height = height;
        }

        private void MyVirtualizingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode((DependencyObject)this))
                this.EnsureBoundToScrollViewer();
            this.HookupScrollEvents();
        }

        public void RespondToOrientationChange(bool isHorizontal)
        {
            this._changingVerticalOffset = true;
            int indexOfItemOnScreen = this.GetIndexOfItemOnScreen();
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>((IEnumerable<IVirtualizable>)this.VirtualizableItems);
            this.ClearItems();
            IEnumerator<IVirtualizable> enumerator = ((IEnumerable<IVirtualizable>)Enumerable.Where<IVirtualizable>(virtualizableList, (Func<IVirtualizable, bool>)(i => i is ISupportOrientationChange))).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    (enumerator.Current as ISupportOrientationChange).SetIsHorizontal(isHorizontal);
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
            this.AddItems((IEnumerable<IVirtualizable>)virtualizableList);
            this.ScrollViewer.ScrollToVerticalOffset(this.GetScrollOffsetForItem(indexOfItemOnScreen));
            this._changingVerticalOffset = false;
            this.PerformLoadUnload(VirtualizableState.LoadedFully);
        }

        public void ScrollTo(double offset)
        {
            if (offset < 0.0)
                offset = 0.0;
            this._changingVerticalOffset = true;
            this.ScrollViewer.ScrollToVerticalOffset(offset);
            this._changingVerticalOffset = false;
            this.PerformLoadUnload(VirtualizableState.LoadedFully);
        }

        public int GetIndexOfItemOnScreen()
        {
            double verticalOffset = this.ScrollViewer.VerticalOffset;
            double num1 = 0.0;
            int num2 = 0;
            List<IVirtualizable>.Enumerator enumerator = this.VirtualizableItems.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    IVirtualizable current = enumerator.Current;
                    double num3 = num1;
                    double fixedHeight = current.FixedHeight;
                    Thickness margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double top = margin.Top;
                    double num4 = fixedHeight + top;
                    margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double bottom = margin.Bottom;
                    double num5 = num4 + bottom;
                    num1 = num3 + num5;
                    if (num1 <= verticalOffset)
                        ++num2;
                    else
                        break;
                }
            }
            finally
            {
                enumerator.Dispose();
            }
            return num2;
        }

        public double GetScrollOffsetForItem(int index)
        {
            if (index < 0)
                return 0.0;
            if (index >= this.VirtualizableItems.Count)
                return Enumerable.Sum<IVirtualizable>(this.VirtualizableItems, (Func<IVirtualizable, double>)(v =>
                {
                    double fixedHeight = v.FixedHeight;
                    Thickness margin1 = v.Margin;
                    // ISSUE: explicit reference operation
                    double top = margin1.Top;
                    double num = fixedHeight + top;
                    Thickness margin2 = v.Margin;
                    // ISSUE: explicit reference operation
                    double bottom = ((Thickness)@margin2).Bottom;
                    return num + bottom;
                }));
            double num1 = 0.0;
            for (int index1 = 0; index1 < index; ++index1)
            {
                double num2 = num1;
                double fixedHeight = this.VirtualizableItems[index1].FixedHeight;
                Thickness margin1 = this.VirtualizableItems[index1].Margin;
                // ISSUE: explicit reference operation
                double top = margin1.Top;
                double num3 = fixedHeight + top;
                Thickness margin2 = this.VirtualizableItems[index1].Margin;
                // ISSUE: explicit reference operation
                double bottom = ((Thickness)@margin2).Bottom;
                double num4 = num3 + bottom;
                num1 = num2 + num4;
            }
            return num1;
        }

        public void AddItems(IEnumerable<IVirtualizable> _itemsToBeAdded)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double num1 = 0.0;
            if (this._virtualizableItems.Count > 0)
                num1 = Enumerable.Sum<IVirtualizable>(this._virtualizableItems, (Func<IVirtualizable, double>)(vi =>
                {
                    double fixedHeight = vi.FixedHeight;
                    Thickness margin1 = vi.Margin;
                    // ISSUE: explicit reference operation
                    double top = margin1.Top;
                    double num2 = fixedHeight + top;
                    Thickness margin2 = vi.Margin;
                    // ISSUE: explicit reference operation
                    double bottom = ((Thickness)@margin2).Bottom;
                    return num2 + bottom;
                }));
            IEnumerator<IVirtualizable> enumerator1 = _itemsToBeAdded.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    IVirtualizable current = enumerator1.Current;
                    if (current == null)
                        throw new Exception("Can only add virtualizable items.");
                    IVirtualizable virtualizable = current;
                    Thickness margin1 = current.Margin;
                    // ISSUE: explicit reference operation
                    double left = margin1.Left;
                    Thickness margin2 = current.Margin;
                    // ISSUE: explicit reference operation
                    double num2 = ((Thickness)@margin2).Top + num1;
                    Thickness margin3 = current.Margin;
                    // ISSUE: explicit reference operation
                    double right = ((Thickness)@margin3).Right;
                    margin3 = current.Margin;
                    // ISSUE: explicit reference operation
                    double bottom1 = ((Thickness)@margin3).Bottom;
                    Thickness thickness = new Thickness(left, num2, right, bottom1);
                    virtualizable.ViewMargin = thickness;
                    this.SetResetParent(true, current);
                    this._virtualizableItems.Add(current);
                    double fixedHeight = current.FixedHeight;
                    Thickness margin4 = current.Margin;
                    // ISSUE: explicit reference operation
                    double top = ((Thickness)@margin4).Top;
                    double num3 = fixedHeight + top;
                    Thickness margin5 = current.Margin;
                    // ISSUE: explicit reference operation
                    double bottom2 = ((Thickness)@margin5).Bottom;
                    double num4 = num3 + bottom2;
                    List<int>.Enumerator enumerator2 = this.GetCoveredPoints(num1, num1 + num4).GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                            this._thresholdPointIndexes[enumerator2.Current] = this._virtualizableItems.Count - 1;
                    }
                    finally
                    {
                        enumerator2.Dispose();
                    }
                    num1 += num4;
                }
            }
            finally
            {
                if (enumerator1 != null)
                    enumerator1.Dispose();
            }
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            this.ChangeHeight(num1);
            stopwatch.Stop();
            this.Log(string.Format("MyVirtualizingPanel.AddItems {0}", stopwatch.ElapsedMilliseconds));
        }

        public void InsertRemoveItems(int index, List<IVirtualizable> itemsToInsert, bool keepItemsBelowIndexFixed = false, IVirtualizable itemToRemove = null)
        {
            try
            {
                bool flag = false;
                if (keepItemsBelowIndexFixed)
                {
                    double num1 = 0.0;
                    for (int index1 = 0; index1 < index; ++index1)
                    {
                        double num2 = num1;
                        double fixedHeight = this.VirtualizableItems[index1].FixedHeight;
                        Thickness margin1 = this.VirtualizableItems[index1].Margin;
                        // ISSUE: explicit reference operation
                        double top = margin1.Top;
                        double num3 = fixedHeight + top;
                        Thickness margin2 = this.VirtualizableItems[index1].Margin;
                        // ISSUE: explicit reference operation
                        double bottom = margin2.Bottom;
                        double num4 = num3 + bottom;
                        num1 = num2 + num4;
                    }
                    if (num1 < this._listScrollViewer.VerticalOffset + this._listScrollViewer.ViewportHeight)
                        flag = true;
                }
                this._loadedSegment = new Segment();
                double num5 = Enumerable.Sum<IVirtualizable>(itemsToInsert, (Func<IVirtualizable, double>)(i =>
                {
                    double fixedHeight = i.FixedHeight;
                    Thickness margin1 = i.Margin;
                    // ISSUE: explicit reference operation
                    double top = margin1.Top;
                    double num = fixedHeight + top;
                    Thickness margin2 = i.Margin;
                    // ISSUE: explicit reference operation
                    double bottom = margin2.Bottom;
                    return num + bottom;
                }));
                this.SetResetParent(true, (IEnumerable<IVirtualizable>)itemsToInsert);
                this._virtualizableItems.InsertRange(index, (IEnumerable<IVirtualizable>)itemsToInsert);
                if (itemToRemove != null)
                {
                    itemToRemove.ChangeState(VirtualizableState.Unloaded);
                    double num1 = num5;
                    double fixedHeight = itemToRemove.FixedHeight;
                    Thickness margin = itemToRemove.Margin;
                    // ISSUE: explicit reference operation
                    double top = margin.Top;
                    double num2 = fixedHeight + top;
                    margin = itemToRemove.Margin;
                    // ISSUE: explicit reference operation
                    double bottom = margin.Bottom;
                    double num3 = num2 + bottom;
                    num5 = num1 - num3;
                    this.SetResetParent(false, itemToRemove);
                    this._virtualizableItems.Remove(itemToRemove);
                }
                this.RearrangeAllItems();
                if (flag)
                {
                    this._changingVerticalOffset = true;
                    this.Log(string.Concat(new object[5]
          {
            "SCROLLING TO ",
            this._listScrollViewer.VerticalOffset,
            num5,
            " scroll height : ",
            this._listScrollViewer.ExtentHeight
          }));
                    this._listScrollViewer.ScrollToVerticalOffset(this._listScrollViewer.VerticalOffset + num5);
                    this._changingVerticalOffset = false;
                }
                this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            }
            catch (Exception)
            {
            }
        }

        public void RemoveItem(IVirtualizable itemToBeRemoved)
        {
            itemToBeRemoved.ChangeState(VirtualizableState.Unloaded);
            this.SetResetParent(false, itemToBeRemoved);
            this._virtualizableItems.Remove(itemToBeRemoved);
            this._loadedSegment = new Segment();
            this.RearrangeAllItems();
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
        }

        public void RearrangeAllItems()
        {
            double num1 = 0.0;
            this._thresholdPointIndexes.Clear();
            int num2 = 0;
            List<IVirtualizable>.Enumerator enumerator1 = this._virtualizableItems.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    IVirtualizable current = enumerator1.Current;
                    if (current == null)
                        throw new Exception("Can only add virtualizable items.");
                    IVirtualizable virtualizable = current;
                    Thickness margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double left = margin.Left;
                    margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double num3 = margin.Top + num1;
                    margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double right = margin.Right;
                    margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double bottom1 = margin.Bottom;
                    Thickness thickness = new Thickness(left, num3, right, bottom1);
                    virtualizable.ViewMargin = thickness;
                    double fixedHeight = current.FixedHeight;
                    margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double top = margin.Top;
                    double num4 = fixedHeight + top;
                    margin = current.Margin;
                    // ISSUE: explicit reference operation
                    double bottom2 = margin.Bottom;
                    double num5 = num4 + bottom2;
                    List<int>.Enumerator enumerator2 = this.GetCoveredPoints(num1, num1 + num5).GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                            this._thresholdPointIndexes[enumerator2.Current] = num2;
                    }
                    finally
                    {
                        enumerator2.Dispose();
                    }
                    num1 += num5;
                    ++num2;
                }
            }
            finally
            {
                enumerator1.Dispose();
            }
            this.ChangeHeight(num1);
            this._listScrollViewer.UpdateLayout();
        }

        private void PerformLoadUnload2(VirtualizableState desiredState, bool bypassUnload = false)
        {
            if (this._virtualizableItems.Count == 0)
                return;
            double realOffset = this.GetRealOffset();
            bool flag1 = false;
            Thickness viewMargin1;
            if (desiredState == VirtualizableState.LoadedFully || this._loadedSegment.IsEmpty)
            {
                flag1 = true;
            }
            else
            {
                int lowerBound = this._loadedSegment.LowerBound;
                int upperBound = this._loadedSegment.UpperBound;
                Thickness viewMargin2 = this._virtualizableItems[lowerBound].ViewMargin;
                // ISSUE: explicit reference operation
                double top = ((Thickness)@viewMargin2).Top;
                viewMargin1 = this._virtualizableItems[upperBound].ViewMargin;
                // ISSUE: explicit reference operation
                double num = ((Thickness)@viewMargin1).Top + this._virtualizableItems[upperBound].FixedHeight;
                if (realOffset - top < 500.0 || num - realOffset < 1500.0)
                    flag1 = true;
            }
            if (flag1)
            {
                int key = (int)Math.Floor(realOffset - realOffset % this.LoadUnloadThreshold);
                int num1 = this._thresholdPointIndexes.ContainsKey(key) ? this._thresholdPointIndexes[key] : -1;
                int upperBoundInd;
                int lowerBoundInd = upperBoundInd = num1 < 0 ? 0 : num1;
                int index1 = lowerBoundInd;
                double num2 = this._isScrolling ? this.LoadedHeightUpwards : this.LoadedHeightDownwardsNotScrolling;
                double num3 = this._isScrolling ? this.LoadedHeightDownwards : this.LoadedHeightDownwardsNotScrolling;
                for (; lowerBoundInd > 0; --lowerBoundInd)
                {
                    double num4 = realOffset;
                    viewMargin1 = this._virtualizableItems[lowerBoundInd].ViewMargin;
                    // ISSUE: explicit reference operation
                    double top = ((Thickness)@viewMargin1).Top;
                    if (num4 - top >= num2)
                        break;
                }
                bool flag2 = false;
                bool flag3 = false;
                for (; upperBoundInd < this._virtualizableItems.Count - 1; ++upperBoundInd)
                {
                    viewMargin1 = this._virtualizableItems[upperBoundInd].ViewMargin;
                    // ISSUE: explicit reference operation
                    if (((Thickness)@viewMargin1).Top - realOffset < num3)
                    {
                        if (!flag2)
                        {
                            viewMargin1 = this._virtualizableItems[upperBoundInd].ViewMargin;
                            // ISSUE: explicit reference operation
                            if (((Thickness)@viewMargin1).Top >= realOffset)
                            {
                                viewMargin1 = this._virtualizableItems[upperBoundInd].ViewMargin;
                                // ISSUE: explicit reference operation
                                if (((Thickness)@viewMargin1).Top - realOffset > 300.0 && upperBoundInd > 0)
                                {
                                    index1 = upperBoundInd - 1;
                                    flag3 = true;
                                }
                                else
                                    index1 = upperBoundInd;
                                flag2 = true;
                            }
                        }
                    }
                    else
                        break;
                }
                this.SetLoadedBounds(lowerBoundInd, upperBoundInd, desiredState, bypassUnload);
                if (flag2)
                {
                    if (flag3)
                        this._virtualizableItems[index1 + 1].IsOnScreen();
                    this._virtualizableItems[index1].IsOnScreen();
                }
                if (this._enableLog)
                {
                    string str = "Loaded indexes : ";
                    for (int index2 = 0; index2 < this._virtualizableItems.Count; ++index2)
                    {
                        if (this._virtualizableItems[index2].CurrentState != VirtualizableState.Unloaded)
                            str = string.Concat(str, index2, ",");
                    }
                    this.Log(str);
                }
            }
            this.TrackImpressions();
            this.TrackItemsPosition();
        }

        private void TrackImpressions()
        {
            double verticalOffset = this.ScrollViewer.VerticalOffset;
            double num1 = verticalOffset + this.ScrollViewer.ViewportHeight;
            for (int lowerBound = this._loadedSegment.LowerBound; lowerBound <= this._loadedSegment.UpperBound; ++lowerBound)
            {
                ISupportImpressionTracking virtualizableItem = this._virtualizableItems[lowerBound] as ISupportImpressionTracking;
                if (virtualizableItem != null)
                {
                    Thickness viewMargin = this._virtualizableItems[lowerBound].ViewMargin;
                    // ISSUE: explicit reference operation
                    double num2 = ((Thickness)@viewMargin).Top + this.OffsetY - this.ExtraOffsetY;
                    double num3 = num2 + this._virtualizableItems[lowerBound].FixedHeight;
                    if (num2 >= verticalOffset && num2 <= num1)
                        virtualizableItem.TopIsOnScreen();
                    if (num3 >= verticalOffset && num3 <= num1)
                        virtualizableItem.BottomIsOnScreen();
                }
            }
        }

        private void TrackItemsPosition()
        {
            double num = this.OffsetY - this.ExtraOffsetY;
            Rect bounds1 = new Rect(0.0, this.ScrollViewer.VerticalOffset, 0.0, Math.Max(0.0, this.ScrollViewer.ViewportHeight - this.ExtraOffsetY));
            for (int lowerBound = this._loadedSegment.LowerBound; lowerBound <= this._loadedSegment.UpperBound; ++lowerBound)
            {
                VirtualizableItemBase virtualizableItem1 = this._virtualizableItems[lowerBound] as VirtualizableItemBase;
                if (virtualizableItem1 != null)
                {
                    Thickness viewMargin = this._virtualizableItems[lowerBound].ViewMargin;
                    // ISSUE: explicit reference operation
                    double offset1 = ((Thickness)@viewMargin).Top + num;
                    ISupportPositionTracking virtualizableItem2 = this._virtualizableItems[lowerBound] as ISupportPositionTracking;
                    if (virtualizableItem2 != null)
                    {
                        Rect bounds2 = bounds1;
                        double offset2 = offset1;
                        virtualizableItem2.TrackPositionChanged(bounds2, offset2);
                    }
                    this.TrackItemChildrenPosition(virtualizableItem1, bounds1, offset1, "-");
                }
            }
        }

        private void TrackItemChildrenPosition(VirtualizableItemBase parent, Rect bounds, double offset, string tag)
        {
            ObservableCollection<IVirtualizable> observableCollection = parent != null ? parent.VirtualizableChildren : null;
            if (observableCollection == null || ((Collection<IVirtualizable>)observableCollection).Count == 0)
                return;
            IEnumerator<VirtualizableItemBase> enumerator = ((IEnumerable<VirtualizableItemBase>)Enumerable.OfType<VirtualizableItemBase>((IEnumerable)observableCollection)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    VirtualizableItemBase current = enumerator.Current;
                    double num = offset;
                    Thickness viewMargin = current.ViewMargin;
                    // ISSUE: explicit reference operation
                    double top = ((Thickness)@viewMargin).Top;
                    double offset1 = num + top;
                    ISupportPositionTracking positionTracking = current as ISupportPositionTracking;
                    if (positionTracking != null)
                    {
                        Rect bounds1 = bounds;
                        double offset2 = offset1;
                        positionTracking.TrackPositionChanged(bounds1, offset2);
                    }
                    this.TrackItemChildrenPosition(current, bounds, offset1, string.Concat(tag, "-"));
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
        }

        private double GetRealOffset()
        {
            return this._listScrollViewer.VerticalOffset + this.DeltaOffset;
        }

        private double HeightOfItemIncludingMargin(IVirtualizable virtualizableItem)
        {
            double fixedHeight = virtualizableItem.FixedHeight;
            Thickness margin1 = virtualizableItem.Margin;
            // ISSUE: explicit reference operation
            double top = margin1.Top;
            double num = fixedHeight + top;
            Thickness margin2 = virtualizableItem.Margin;
            // ISSUE: explicit reference operation
            double bottom = ((Thickness)@margin2).Bottom;
            return num + bottom;
        }

        private void PerformLoadUnload(VirtualizableState desiredState)
        {
            this.PerformLoadUnload2(desiredState, false);
        }

        private void SetLoadedBounds(int lowerBoundInd, int upperBoundInd, VirtualizableState desiredState, bool bypassUnload = false)
        {
            Segment segment = new Segment(lowerBoundInd, upperBoundInd);
            Segment thisMinusOther1;
            Segment thisMinusOther2;
            Segment intersection;
            Segment otherMinusThis1;
            Segment otherMinusThis2;
            segment.CompareToSegment(this._loadedSegment, out thisMinusOther1, out thisMinusOther2, out intersection, out otherMinusThis1, out otherMinusThis2);
            this.Log(string.Format("LoadedSegment:{0}, NewSegment:{1}, NewMinusLoaded1:{2}, NewMinusLoaded2:{3}, loadedMinusNew1:{4}, loadedMinusNew2:{5}", new object[6]
      {
        this._loadedSegment,
        segment,
        thisMinusOther1,
        thisMinusOther2,
        otherMinusThis1,
        otherMinusThis2
      }));
            if (desiredState == VirtualizableState.LoadedPartially || this.OnlyPartialLoad)
            {
                this.LoadItemsInSegment(thisMinusOther1, VirtualizableState.LoadedPartially);
                this.LoadItemsInSegment(thisMinusOther2, VirtualizableState.LoadedPartially);
            }
            else if (desiredState == VirtualizableState.LoadedFully)
                this.LoadItemsInSegment(segment, VirtualizableState.LoadedFully);
            if (!bypassUnload)
            {
                this.UnloadItemsInSegment(otherMinusThis1);
                this.UnloadItemsInSegment(otherMinusThis2);
            }
            this._loadedSegment = segment;
            this._numberOfScrollChangedCalls = this._numberOfScrollChangedCalls + 1;
            this.ShowStatistics(desiredState);
        }

        private void UnloadItemsInSegment(Segment segment)
        {
            for (int lowerBound = segment.LowerBound; lowerBound <= segment.UpperBound; ++lowerBound)
            {
                IVirtualizable virtualizableItem = this._virtualizableItems[lowerBound];
                this.RemoveFromChildren((UIElement)virtualizableItem.View);
                virtualizableItem.ChangeState(VirtualizableState.Unloaded);
            }
        }

        private void LoadItemsInSegment(Segment segment, VirtualizableState desiredState)
        {
            for (int lowerBound = segment.LowerBound; lowerBound <= segment.UpperBound; ++lowerBound)
            {
                IVirtualizable virtualizableItem = this._virtualizableItems[lowerBound];
                virtualizableItem.ChangeState(desiredState);
                this.AddToChildren((UIElement)virtualizableItem.View);
            }
        }

        private void AddToChildren(UIElement element)
        {
            if (element.CacheMode == null)
                element.CacheMode = ((CacheMode)new BitmapCache());
            if (((PresentationFrameworkCollection<UIElement>)((Panel)this._itemsPanel).Children).Contains(element))
                return;
            if (element.Projection == null && this._upsideDown)
            {
                UIElement uiElement = element;
                PlaneProjection planeProjection = new PlaneProjection();
                double num = 180.0;
                planeProjection.RotationZ = num;
                uiElement.Projection = ((Projection)planeProjection);
            }
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._itemsPanel).Children).Add(element);
        }

        private void RemoveFromChildren(UIElement element)
        {
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._itemsPanel).Children).Remove(element);
        }

        private List<int> GetCoveredPoints(double from, double to)
        {
            List<int> intList = new List<int>();
            double d = from - from % this.LoadUnloadThreshold;
            while (d <= to)
            {
                if (d >= from)
                    intList.Add((int)Math.Floor(d));
                d += this.LoadUnloadThreshold;
            }
            return intList;
        }

        private void Log(string str)
        {
            int num = this._enableLog ? 1 : 0;
        }

        public void ClearItems()
        {
            List<IVirtualizable>.Enumerator enumerator = this._virtualizableItems.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    enumerator.Current.ChangeState(VirtualizableState.Unloaded);
            }
            finally
            {
                enumerator.Dispose();
            }
            this.SetResetParent(false, (IEnumerable<IVirtualizable>)this._virtualizableItems);
            this._virtualizableItems.Clear();
            this.ClearChildren();
            this._loadedSegment = new Segment();
            this._thresholdPointIndexes.Clear();
            this._listScrollViewer.ScrollToVerticalOffset(0.0);
            this.ChangeHeight(0.0);
        }

        private void ClearChildren()
        {
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._itemsPanel).Children).Clear();
        }

        private void SetResetParent(bool set, IVirtualizable item)
        {
            if (set)
                item.Parent = (IMyVirtualizingPanel)this;
            else
                item.Parent = null;
        }

        private void SetResetParent(bool set, IEnumerable<IVirtualizable> items)
        {
            ListExtensions.ForEach<IVirtualizable>(items, (Action<IVirtualizable>)(item => this.SetResetParent(set, item)));
        }

        public void ShowStatistics(VirtualizableState desiredState)
        {
            if (!this._showStatistics)
                return;
            string str1 = string.Concat(new object[4] { "NumberOfLoadUnload: ", this._numberOfScrollChangedCalls, ", state= ", desiredState.ToString() });
            this._loadedSegmentsStr = string.Concat(this._loadedSegment.ToString(), ", ", this._loadedSegmentsStr);
            if (((string)this._loadedSegmentsStr).Length >= 40)
                this._loadedSegmentsStr = ((string)this._loadedSegmentsStr).Substring(0, 40);
            string str2 = string.Concat("LoadedSegment: ", this._loadedSegmentsStr);
            this._textBlock1.Text = str1;
            this._textBlock1.Foreground = ((Brush)new SolidColorBrush(Colors.Green));
            ((UIElement)this._textBlock1).IsHitTestVisible = false;
            this._textBlock2.Text = str2;
            ((UIElement)this._textBlock2).IsHitTestVisible = false;
            this._textBlock2.Foreground = ((Brush)new SolidColorBrush(Colors.Green));
            if (this._statisticsBlocksAdded)
                return;
            ((ToggleButton)this._boundToScrollCheckBox).IsChecked = (new bool?(true));
            // ISSUE: method pointer
            ((ToggleButton)this._boundToScrollCheckBox).Checked += (new RoutedEventHandler(this._boundToScrollCheckBox_Checked));
            // ISSUE: method pointer
            ((ToggleButton)this._boundToScrollCheckBox).Unchecked += (new RoutedEventHandler(this._boundToScrollCheckBox_Unchecked));
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._statContainer).Children).Add((UIElement)this._textBlock1);
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._statContainer).Children).Add((UIElement)this._textBlock2);
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._statContainer).Children).Add((UIElement)this._boundToScrollCheckBox);
            this._statContainer.Orientation = ((Orientation)0);
            ((FrameworkElement)this._statContainer).VerticalAlignment = ((VerticalAlignment)0);
            Grid.SetRowSpan((FrameworkElement)this._statContainer, 10);
            Grid.SetColumn((FrameworkElement)this._statContainer, 10);
            ((PresentationFrameworkCollection<UIElement>)this.OuterPanel.Children).Add((UIElement)this._statContainer);
            this._statisticsBlocksAdded = true;
        }

        private void _boundToScrollCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.UnbindFromScrollViewer();
        }

        private void _boundToScrollCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.EnsureBoundToScrollViewer();
        }

        public void Substitute(IVirtualizable item, IVirtualizable updatedItem)
        {
            int num1 = this.VirtualizableItems.IndexOf(item);
            if (num1 < 0)
                return;
            this.RemoveItem(item);
            int index = num1;
            List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
            itemsToInsert.Add(updatedItem);
            int num2 = 0;
            // ISSUE: variable of the null type

            this.InsertRemoveItems(index, itemsToInsert, num2 != 0, null);
        }
        /*
      [SpecialName]
      object IMyVirtualizingPanel.DataContext
      {
        return base.DataContext;
      }
        */
        public class ScrollPositionChangedEventAgrs : EventArgs
        {
            public double CurrentPosition { get; private set; }

            public double ScrollHeight { get; private set; }

            public ScrollPositionChangedEventAgrs(double currentPosition, double scrollHeight)
            {
                this.CurrentPosition = currentPosition;
                this.ScrollHeight = scrollHeight;
            }
        }

        public delegate void OnCompression(object sender, CompressionEventArgs e);
    }
}
