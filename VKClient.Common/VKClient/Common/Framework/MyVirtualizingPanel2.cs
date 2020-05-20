using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private DelayedExecutor _de = new DelayedExecutor(450);
        private Canvas _itemsPanel = new Canvas();
        private DateTime _previousScrollOffsetChangedTime = DateTime.MinValue;
        private readonly double _pixelsPerSecondThreshold = 250.0;
        private List<IVirtualizable> _virtualizableItems = new List<IVirtualizable>();
        private Segment _loadedSegment = new Segment();
        private Dictionary<int, int> _thresholdPointIndexes = new Dictionary<int, int>();
        private int _countOfItemsBeforeLoadMore = 20;
        private List<IVirtualizable> _addedWhileEdited = new List<IVirtualizable>();
        private DependencyProperty ListVerticalOffsetProperty = DependencyProperty.Register("ListVerticalOffset", typeof(double), typeof(MyVirtualizingPanel2), new PropertyMetadata(new PropertyChangedCallback(MyVirtualizingPanel2.OnListVerticalOffsetChanged)));
        private readonly double MIN_DELTA_OFFSET_FOR_LOAD_UNLOAD = 150.0;
        private DateTime _lastTimeCalledGCCollect = DateTime.MinValue;
        private TextBlock _textBlock1 = new TextBlock();
        private TextBlock _textBlock2 = new TextBlock();
        private CheckBox _boundToScrollCheckBox = new CheckBox();
        private StackPanel _statContainer = new StackPanel();
        private string _loadedSegmentsStr = "";
        private bool _enableLog = false;//
        private bool _changingVerticalOffset;
        private IScrollableArea _listScrollViewer;
        private bool _isScrolling;
        private bool _notReactToScroll;
        private double _savedDelta;
        private double _previousScrollOffset;
        private bool _upsideDown;
        public bool KeepScrollPositionWhenAddingItems;
        //private bool _subscribedOnEdit = false;//
        private ISupportCollectionEdit _editable;
        private bool _isEditing;
        private bool _isScrollingToUpOrBottom;
        private int _scrollOffsetChangedCount;
        private double? _lastReportedVerticalOffset;
        private Panel _outerPanel;
        private int _numberOfScrollChangedCalls;
        private bool _showStatistics = false;//
        private bool _statisticsBlocksAdded;
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


        public IList ItemsSource
        {
            get
            {
                return (IList)this.GetValue(MyVirtualizingPanel2.ItemsSourceProperty);
            }
            set
            {
                this.SetValue(MyVirtualizingPanel2.ItemsSourceProperty, (object)value);
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
                return (double)this.GetValue(this.ListVerticalOffsetProperty);
            }
            set
            {
                this.SetValue(this.ListVerticalOffsetProperty, (object)value);
            }
        }

        private PhoneApplicationPage Page
        {
            get
            {
                return (Application.Current.RootVisual as PhoneApplicationFrame).Content as PhoneApplicationPage;
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
                    IEnumerable<ContentPresenter> logicalChildrenByType1 = (Application.Current.RootVisual as Frame).GetLogicalChildrenByType<ContentPresenter>(false);
                    for (int index = 0; index < logicalChildrenByType1.Count<ContentPresenter>(); ++index)
                    {
                        IEnumerable<Panel> logicalChildrenByType2 = logicalChildrenByType1.ElementAt<ContentPresenter>(index).GetLogicalChildrenByType<Panel>(false);
                        if (logicalChildrenByType2.Any<Panel>())
                        {
                            this._outerPanel = logicalChildrenByType2.First<Panel>();
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
            this.Loaded += this.MyVirtualizingPanel_Loaded;
            this.Children.Add((UIElement)this._itemsPanel);
        }

        public void Cleanup()
        {
            this.ClearItems();
            this.ClearValue(MyVirtualizingPanel2.ItemsSourceProperty);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MyVirtualizingPanel2 virtualizingPanel2 = d as MyVirtualizingPanel2;
            if (virtualizingPanel2 == null)
                return;
            virtualizingPanel2.ClearItems();
            INotifyCollectionChanged collection1 = e.OldValue as INotifyCollectionChanged;
            if (collection1 != null)
                virtualizingPanel2.UnhookCollectionChanged(collection1);
            INotifyCollectionChanged collection2 = e.NewValue as INotifyCollectionChanged;
            if (collection2 != null)
            {
                List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
                foreach (object obj in (IEnumerable)(collection2 as ICollection))
                {
                    if (obj is IVirtualizable)
                        virtualizableList.Add(obj as IVirtualizable);
                    else if (virtualizingPanel2.CreateVirtItemFunc != null)
                        virtualizableList.Add(virtualizingPanel2.CreateVirtItemFunc(obj));
                }
                virtualizingPanel2.AddItems((IEnumerable<IVirtualizable>)virtualizableList);
                virtualizingPanel2.HookUpCollectionChanged(collection2);
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
            this._editable = (ISupportCollectionEdit)null;
        }

        private void SubscribeOnEdit()
        {
            ISupportCollectionEdit supportCollectionEdit = this.DataContext as ISupportCollectionEdit;
            if (supportCollectionEdit == null || this._editable == supportCollectionEdit)
                return;
            this.UnsubscribeOnEdit();
            supportCollectionEdit.StartedEdit += new EventHandler(this.editable_StartedEdit);
            supportCollectionEdit.EndedEdit += new EventHandler(this.editable_EndedEdit);
            this._editable = supportCollectionEdit;
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
                foreach (object newItem in (IEnumerable)e.NewItems)
                {
                    if (newItem is IVirtualizable)
                        itemsToInsert.Add(newItem as IVirtualizable);
                    else if (this.CreateVirtItemFunc != null)
                        itemsToInsert.Add(this.CreateVirtItemFunc(newItem));
                }
            }
            List<IVirtualizable> virtualizableList1 = new List<IVirtualizable>();
            if (e.OldItems != null)
            {
                foreach (object oldItem in (IEnumerable)e.OldItems)
                {
                    if (oldItem is IVirtualizable)
                        virtualizableList1.Add(oldItem as IVirtualizable);
                    else if (e.OldStartingIndex >= 0 && e.OldStartingIndex < virtualizingPanel2.VirtualizableItems.Count)
                    {
                        IVirtualizable itemToBeRemoved = virtualizingPanel2.VirtualizableItems[e.OldStartingIndex];
                        virtualizingPanel2.RemoveItem(itemToBeRemoved);
                    }
                }
            }
            if (this._isEditing)
                this._addedWhileEdited.AddRange((IEnumerable<IVirtualizable>)itemsToInsert);
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewStartingIndex >= virtualizingPanel2.VirtualizableItems.Count)
                    virtualizingPanel2.AddItems((IEnumerable<IVirtualizable>)itemsToInsert);
                else
                    virtualizingPanel2.InsertRemoveItems(e.NewStartingIndex, itemsToInsert, virtualizingPanel2.KeepScrollPositionWhenAddingItems, (IVirtualizable)null);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                virtualizingPanel2.ClearItems();
                List<IVirtualizable> virtualizableList2 = new List<IVirtualizable>();
                foreach (object obj in (IEnumerable)virtualizingPanel2.ItemsSource)
                    virtualizableList2.Add(obj is IVirtualizable ? obj as IVirtualizable : this.CreateVirtItemFunc(obj));
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
                this.Compression((object)this, new CompressionEventArgs(CompressionType.Top));
            });
            this.ScrollViewer.OnCompressionBottom = (Action)(() =>
            {
                if (this.Compression == null)
                    return;
                this.Compression((object)this, new CompressionEventArgs(CompressionType.Bottom));
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
            this._listScrollViewer.Projection = (Projection)new PlaneProjection()
            {
                RotationZ = 180.0
            };
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
            this.ScrollViewer.OnVerticalOffsetChanged = (Action<double>)null;
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
            this.Page.Focus();
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
                return (VisualStateGroup)null;
            foreach (VisualStateGroup visualStateGroup in (IEnumerable)VisualStateManager.GetVisualStateGroups(element))
            {
                if (visualStateGroup.Name == name)
                    return visualStateGroup;
            }
            return (VisualStateGroup)null;
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
            if (this.ScrollPositionChanged != null)
            {
                this.ScrollPositionChanged((object)this, new MyVirtualizingPanel2.ScrollPositionChangedEventAgrs(this._listScrollViewer.VerticalOffset, this.Height));
            }
            this.LoadMoreIfNeeded(this._listScrollViewer.VerticalOffset, this.Height);
            this.Log("Reported Offset: " + (object)this._listScrollViewer.VerticalOffset);
            int num = this._scrollOffsetChangedCount;
            this._scrollOffsetChangedCount = num + 1;
            if (num % 20 != 0 || !MemoryInfo.IsLowMemDevice || (DateTime.Now - this._lastTimeCalledGCCollect).TotalSeconds <= 30.0)
                return;
            GC.Collect();
            this._lastTimeCalledGCCollect = DateTime.Now;
        }

        private void LoadMoreIfNeeded(double currentPosition, double scrollHeight)
        {
            if (this.ManuallyLoadMore || scrollHeight == 0.0 || this.VirtualizableItems.Count <= 0 || (scrollHeight - currentPosition >= (double)this.CountOfItemsBeforeLoadMore * this.VirtualizableItems.Sum<IVirtualizable>((Func<IVirtualizable, double>)(v => v.FixedHeight)) / (double)this.VirtualizableItems.Count || currentPosition <= 150.0 || !(this.DataContext is ISupportLoadMore)))
                return;
            (this.DataContext as ISupportLoadMore).LoadMore();
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
            this.Height = height;
        }

        private void MyVirtualizingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //
            //
            //this.DataContext = NewsViewModel.Instance.NewsFeedVM;
            //
            //
            if (!DesignerProperties.GetIsInDesignMode((DependencyObject)this))
                this.EnsureBoundToScrollViewer();
            this.HookupScrollEvents();
        }

        public void RespondToOrientationChange(bool isHorizontal)
        {
            this._changingVerticalOffset = true;
            int indexOfItemOnScreen = this.GetIndexOfItemOnScreen();
            List<IVirtualizable> source = new List<IVirtualizable>((IEnumerable<IVirtualizable>)this.VirtualizableItems);
            this.ClearItems();
            foreach (IVirtualizable virtualizable in source.Where<IVirtualizable>((Func<IVirtualizable, bool>)(i => i is ISupportOrientationChange)))
                (virtualizable as ISupportOrientationChange).SetIsHorizontal(isHorizontal);
            this.AddItems((IEnumerable<IVirtualizable>)source);
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
            foreach (IVirtualizable virtualizableItem in this.VirtualizableItems)
            {
                double num3 = num1;
                double fixedHeight = virtualizableItem.FixedHeight;
                Thickness margin = virtualizableItem.Margin;
                double top = margin.Top;
                double num4 = fixedHeight + top;
                margin = virtualizableItem.Margin;
                double bottom = margin.Bottom;
                double num5 = num4 + bottom;
                num1 = num3 + num5;
                if (num1 <= verticalOffset)
                    ++num2;
                else
                    break;
            }
            return num2;
        }

        public double GetScrollOffsetForItem(int index)
        {
            if (index < 0)
                return 0.0;
            if (index >= this.VirtualizableItems.Count)
                return this.VirtualizableItems.Sum<IVirtualizable>((Func<IVirtualizable, double>)(v => v.FixedHeight + v.Margin.Top + v.Margin.Bottom));
            double num = 0.0;
            for (int index1 = 0; index1 < index; ++index1)
                num += this.VirtualizableItems[index1].FixedHeight + this.VirtualizableItems[index1].Margin.Top + this.VirtualizableItems[index1].Margin.Bottom;
            return num;
        }

        public void AddItems(IEnumerable<IVirtualizable> _itemsToBeAdded)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double num1 = 0.0;
            if (this._virtualizableItems.Count > 0)
                num1 = this._virtualizableItems.Sum<IVirtualizable>((Func<IVirtualizable, double>)(vi => vi.FixedHeight + vi.Margin.Top + vi.Margin.Bottom));
            foreach (IVirtualizable virtualizable1 in _itemsToBeAdded)
            {
                if (virtualizable1 == null)
                    throw new Exception("Can only add virtualizable items.");
                IVirtualizable virtualizable2 = virtualizable1;
                double left = virtualizable1.Margin.Left;
                double top = virtualizable1.Margin.Top + num1;
                Thickness margin = virtualizable1.Margin;
                double right = margin.Right;
                margin = virtualizable1.Margin;
                double bottom = margin.Bottom;
                Thickness thickness = new Thickness(left, top, right, bottom);
                virtualizable2.ViewMargin = thickness;
                this.SetResetParent(true, virtualizable1);
                this._virtualizableItems.Add(virtualizable1);
                double num2 = virtualizable1.FixedHeight + virtualizable1.Margin.Top + virtualizable1.Margin.Bottom;
                foreach (int coveredPoint in this.GetCoveredPoints(num1, num1 + num2))
                    this._thresholdPointIndexes[coveredPoint] = this._virtualizableItems.Count - 1;
                num1 += num2;
            }
            this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            this.ChangeHeight(num1);
            stopwatch.Stop();
            this.Log(string.Format("MyVirtualizingPanel.AddItems {0}", (object)stopwatch.ElapsedMilliseconds));
        }

        public void InsertRemoveItems(int index, List<IVirtualizable> itemsToInsert, bool keepItemsBelowIndexFixed = false, IVirtualizable itemToRemove = null)
        {
            try
            {
                bool flag = false;
                if (keepItemsBelowIndexFixed)
                {
                    double num = 0.0;
                    for (int index1 = 0; index1 < index; ++index1)
                        num += this.VirtualizableItems[index1].FixedHeight + this.VirtualizableItems[index1].Margin.Top + this.VirtualizableItems[index1].Margin.Bottom;
                    if (num < this._listScrollViewer.VerticalOffset + this._listScrollViewer.ViewportHeight)
                        flag = true;
                }
                this._loadedSegment = new Segment();
                double num1 = itemsToInsert.Sum<IVirtualizable>((Func<IVirtualizable, double>)(i => i.FixedHeight + i.Margin.Top + i.Margin.Bottom));
                this.SetResetParent(true, (IEnumerable<IVirtualizable>)itemsToInsert);
                this._virtualizableItems.InsertRange(index, (IEnumerable<IVirtualizable>)itemsToInsert);
                if (itemToRemove != null)
                {
                    itemToRemove.ChangeState(VirtualizableState.Unloaded);
                    double num2 = num1;
                    double fixedHeight = itemToRemove.FixedHeight;
                    Thickness margin = itemToRemove.Margin;
                    double top = margin.Top;
                    double num3 = fixedHeight + top;
                    margin = itemToRemove.Margin;
                    double bottom = margin.Bottom;
                    double num4 = num3 + bottom;
                    num1 = num2 - num4;
                    this.SetResetParent(false, itemToRemove);
                    this._virtualizableItems.Remove(itemToRemove);
                }
                this.RearrangeAllItems();
                if (flag)
                {
                    this._changingVerticalOffset = true;
                    this.Log("SCROLLING TO " + (object)this._listScrollViewer.VerticalOffset + (object)num1 + " scroll height : " + (object)this._listScrollViewer.ExtentHeight);
                    this._listScrollViewer.ScrollToVerticalOffset(this._listScrollViewer.VerticalOffset + num1);
                    this._changingVerticalOffset = false;
                }
                this.PerformLoadUnload(this._isScrolling ? VirtualizableState.LoadedPartially : VirtualizableState.LoadedFully);
            }
            catch
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
            foreach (IVirtualizable virtualizableItem in this._virtualizableItems)
            {
                if (virtualizableItem == null)
                    throw new Exception("Can only add virtualizable items.");
                IVirtualizable virtualizable = virtualizableItem;
                Thickness margin = virtualizableItem.Margin;
                double left = margin.Left;
                margin = virtualizableItem.Margin;
                double top1 = margin.Top + num1;
                margin = virtualizableItem.Margin;
                double right = margin.Right;
                margin = virtualizableItem.Margin;
                double bottom1 = margin.Bottom;
                Thickness thickness = new Thickness(left, top1, right, bottom1);
                virtualizable.ViewMargin = thickness;
                double fixedHeight = virtualizableItem.FixedHeight;
                margin = virtualizableItem.Margin;
                double top2 = margin.Top;
                double num3 = fixedHeight + top2;
                margin = virtualizableItem.Margin;
                double bottom2 = margin.Bottom;
                double num4 = num3 + bottom2;
                foreach (int coveredPoint in this.GetCoveredPoints(num1, num1 + num4))
                    this._thresholdPointIndexes[coveredPoint] = num2;
                num1 += num4;
                ++num2;
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
            Thickness viewMargin;
            if (desiredState == VirtualizableState.LoadedFully || this._loadedSegment.IsEmpty)
            {
                flag1 = true;
            }
            else
            {
                int lowerBound = this._loadedSegment.LowerBound;
                int upperBound = this._loadedSegment.UpperBound;
                double top = this._virtualizableItems[lowerBound].ViewMargin.Top;
                viewMargin = this._virtualizableItems[upperBound].ViewMargin;
                double num = viewMargin.Top + this._virtualizableItems[upperBound].FixedHeight;
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
                    viewMargin = this._virtualizableItems[lowerBoundInd].ViewMargin;
                    double top = viewMargin.Top;
                    if (num4 - top >= num2)
                        break;
                }
                bool flag2 = false;
                bool flag3 = false;
                for (; upperBoundInd < this._virtualizableItems.Count - 1; ++upperBoundInd)
                {
                    viewMargin = this._virtualizableItems[upperBoundInd].ViewMargin;
                    if (viewMargin.Top - realOffset < num3)
                    {
                        if (!flag2)
                        {
                            viewMargin = this._virtualizableItems[upperBoundInd].ViewMargin;
                            if (viewMargin.Top >= realOffset)
                            {
                                viewMargin = this._virtualizableItems[upperBoundInd].ViewMargin;
                                if (viewMargin.Top - realOffset > 300.0 && upperBoundInd > 0)
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
                            str = str + (object)index2 + ",";
                    }
                    this.Log(str);
                }
            }
            this.TrackImpressions();
        }

        private void TrackImpressions()
        {
            double verticalOffset = this.ScrollViewer.VerticalOffset;
            double num1 = this.ScrollViewer.VerticalOffset + this.ScrollViewer.ViewportHeight;
            for (int lowerBound = this._loadedSegment.LowerBound; lowerBound <= this._loadedSegment.UpperBound; ++lowerBound)
            {
                ISupportImpressionTracking impressionTracking = this._virtualizableItems[lowerBound] as ISupportImpressionTracking;
                if (impressionTracking != null)
                {
                    double top = this._virtualizableItems[lowerBound].ViewMargin.Top;
                    double num2 = top + this._virtualizableItems[lowerBound].FixedHeight;
                    if (top >= verticalOffset && top <= num1)
                        impressionTracking.TopIsOnScreen();
                    if (num2 >= verticalOffset && num2 <= num1)
                        impressionTracking.BottomIsOnScreen();
                }
            }
        }

        private double GetRealOffset()
        {
            return this._listScrollViewer.VerticalOffset + this.DeltaOffset;
        }

        private double HeightOfItemIncludingMargin(IVirtualizable virtualizableItem)
        {
            return virtualizableItem.FixedHeight + virtualizableItem.Margin.Top + virtualizableItem.Margin.Bottom;
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
            this.Log(string.Format("LoadedSegment:{0}, NewSegment:{1}, NewMinusLoaded1:{2}, NewMinusLoaded2:{3}, loadedMinusNew1:{4}, loadedMinusNew2:{5}", (object)this._loadedSegment, (object)segment, (object)thisMinusOther1, (object)thisMinusOther2, (object)otherMinusThis1, (object)otherMinusThis2));
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
                IVirtualizable virtualizable = this._virtualizableItems[lowerBound];
                this.RemoveFromChildren((UIElement)virtualizable.View);
                virtualizable.ChangeState(VirtualizableState.Unloaded);
            }
        }

        private void LoadItemsInSegment(Segment segment, VirtualizableState desiredState)
        {
            for (int lowerBound = segment.LowerBound; lowerBound <= segment.UpperBound; ++lowerBound)
            {
                IVirtualizable virtualizable = this._virtualizableItems[lowerBound];
                virtualizable.ChangeState(desiredState);
                this.AddToChildren((UIElement)virtualizable.View);
            }
        }

        private void AddToChildren(UIElement element)
        {
            if (element.CacheMode == null)
                element.CacheMode = (CacheMode)new BitmapCache();
            if (this._itemsPanel.Children.Contains(element))
                return;
            if (element.Projection == null && this._upsideDown)
                element.Projection = (Projection)new PlaneProjection()
                {
                    RotationZ = 180.0
                };
            this._itemsPanel.Children.Add(element);
        }

        private void RemoveFromChildren(UIElement element)
        {
            this._itemsPanel.Children.Remove(element);
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
            if (this._enableLog)
                System.Diagnostics.Debug.WriteLine(str);
        }

        public void ClearItems()
        {
            foreach (IVirtualizable virtualizableItem in this._virtualizableItems)
                virtualizableItem.ChangeState(VirtualizableState.Unloaded);
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
            this._itemsPanel.Children.Clear();
        }

        private void SetResetParent(bool set, IVirtualizable item)
        {
            if (set)
                item.Parent = (IMyVirtualizingPanel)this;
            else
                item.Parent = (IMyVirtualizingPanel)null;
        }

        private void SetResetParent(bool set, IEnumerable<IVirtualizable> items)
        {
            items.ForEach<IVirtualizable>((Action<IVirtualizable>)(item => this.SetResetParent(set, item)));
        }

        public void ShowStatistics(VirtualizableState desiredState)
        {
            if (!this._showStatistics)
                return;
            string str1 = "NumberOfLoadUnload: " + (object)this._numberOfScrollChangedCalls + ", state= " + desiredState.ToString();
            this._loadedSegmentsStr = this._loadedSegment.ToString() + ", " + this._loadedSegmentsStr;
            if (this._loadedSegmentsStr.Length >= 40)
                this._loadedSegmentsStr = this._loadedSegmentsStr.Substring(0, 40);
            string str2 = "LoadedSegment: " + this._loadedSegmentsStr;
            this._textBlock1.Text = str1;
            this._textBlock1.Foreground = (Brush)new SolidColorBrush(Colors.Green);
            this._textBlock1.IsHitTestVisible = false;
            this._textBlock2.Text = str2;
            this._textBlock2.IsHitTestVisible = false;
            this._textBlock2.Foreground = (Brush)new SolidColorBrush(Colors.Green);
            if (this._statisticsBlocksAdded)
                return;
            this._boundToScrollCheckBox.IsChecked = new bool?(true);
            this._boundToScrollCheckBox.Checked += new RoutedEventHandler(this._boundToScrollCheckBox_Checked);
            this._boundToScrollCheckBox.Unchecked += new RoutedEventHandler(this._boundToScrollCheckBox_Unchecked);
            this._statContainer.Children.Add((UIElement)this._textBlock1);
            this._statContainer.Children.Add((UIElement)this._textBlock2);
            this._statContainer.Children.Add((UIElement)this._boundToScrollCheckBox);
            this._statContainer.Orientation = Orientation.Vertical;
            this._statContainer.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRowSpan((FrameworkElement)this._statContainer, 10);
            Grid.SetColumn((FrameworkElement)this._statContainer, 10);
            this.OuterPanel.Children.Add((UIElement)this._statContainer);
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
            this.InsertRemoveItems(index, itemsToInsert, false, null);
        }

        object IMyVirtualizingPanel.DataContext//TODO: DELETE?
        {
            get
            {
                return DataContext;
            }
        }
        

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
