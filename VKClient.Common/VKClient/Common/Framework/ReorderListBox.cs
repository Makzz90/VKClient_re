using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.Framework
{
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "DragIndicator", Type = typeof(Image))]
    [TemplatePart(Name = "DragInterceptor", Type = typeof(Canvas))]
    [TemplatePart(Name = "RearrangeCanvas", Type = typeof(Canvas))]
    public class ReorderListBox : ListBox
    {
        internal static void b__60_0(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ReorderListBox)d).OnIsReorderEnabledChanged(e);
        }

        public static readonly DependencyProperty IsReorderEnabledProperty = DependencyProperty.Register("IsReorderEnabled", typeof(bool), typeof(ReorderListBox), new PropertyMetadata((object)false, new PropertyChangedCallback(ReorderListBox.b__60_0)));
        public static readonly DependencyProperty AutoScrollMarginProperty = DependencyProperty.Register("AutoScrollMargin", typeof(int), typeof(ReorderListBox), new PropertyMetadata(32));
        public const string ScrollViewerPart = "ScrollViewer";
        public const string DragIndicatorPart = "DragIndicator";
        public const string DragInterceptorPart = "DragInterceptor";
        public const string RearrangeCanvasPart = "RearrangeCanvas";
        private const string ScrollViewerScrollingVisualState = "Scrolling";
        private const string ScrollViewerNotScrollingVisualState = "NotScrolling";
        private const string IsReorderEnabledPropertyName = "IsReorderEnabled";
        private double dragScrollDelta;
        private Panel itemsPanel;
        private ScrollViewer scrollViewer;
        private Canvas dragInterceptor;
        private Image dragIndicator;
        private object dragItem;
        private ReorderListBoxItem dragItemContainer;
        private bool isDragItemSelected;
        private Rect dragInterceptorRect;
        private int dropTargetIndex;
        private Canvas rearrangeCanvas;
        private Queue<KeyValuePair<Action, Duration>> rearrangeQueue;
        private bool _isManipulating;

        public bool IsReorderEnabled
        {
            get
            {
                return (bool)this.GetValue(ReorderListBox.IsReorderEnabledProperty);
            }
            set
            {
                this.SetValue(ReorderListBox.IsReorderEnabledProperty, (object)value);
            }
        }

        public double AutoScrollMargin
        {
            get
            {
                return (double)(int)this.GetValue(ReorderListBox.AutoScrollMarginProperty);
            }
            set
            {
                this.SetValue(ReorderListBox.AutoScrollMarginProperty, (object)value);
            }
        }

        public event EventHandler<MyLinkUnlinkEventArgs> Link;

        public event EventHandler<MyLinkUnlinkEventArgs> Unlink;

        public ReorderListBox()
        {
            this.DefaultStyleKey = (object)typeof(ReorderListBox);
        }

        protected void OnIsReorderEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.dragInterceptor != null)
                this.dragInterceptor.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            if (this.IsReorderEnabled)
                this.VerticalAlignment = VerticalAlignment.Top;
            else
                this.VerticalAlignment = VerticalAlignment.Stretch;
            this.InvalidateArrange();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.scrollViewer = (ScrollViewer)this.GetTemplateChild("ScrollViewer");
            this.dragInterceptor = this.GetTemplateChild("DragInterceptor") as Canvas;
            this.dragIndicator = this.GetTemplateChild("DragIndicator") as Image;
            this.rearrangeCanvas = this.GetTemplateChild("RearrangeCanvas") as Canvas;
            if (this.scrollViewer == null || this.dragInterceptor == null || this.dragIndicator == null)
                return;
            this.dragInterceptor.Visibility = this.IsReorderEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.dragInterceptor.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this.dragInterceptor_ManipulationStarted);
            this.dragInterceptor.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.dragInterceptor_ManipulationDelta);
            this.dragInterceptor.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.dragInterceptor_ManipulationCompleted);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return (DependencyObject)new ReorderListBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ReorderListBoxItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ReorderListBoxItem itemContainer = (ReorderListBoxItem)element;
            itemContainer.ApplyTemplate();
            string stateName = this.IsReorderEnabled ? "ReorderEnabled" : "ReorderDisabled";
            if (item == this.DataContext)
                stateName = "ReorderDisabled";
            VisualStateManager.GoToState((Control)itemContainer, stateName, false);
            itemContainer.SetBinding(ReorderListBoxItem.IsReorderEnabledProperty, new Binding("IsReorderEnabled")
            {
                Source = (object)this
            });
            if (item == this.dragItem)
            {
                itemContainer.IsSelected = this.isDragItemSelected;
                VisualStateManager.GoToState((Control)itemContainer, "Dragging", false);
                if (this.dropTargetIndex >= 0)
                {
                    itemContainer.Visibility = Visibility.Collapsed;
                    this.dragItemContainer = itemContainer;
                }
                else
                {
                    itemContainer.Opacity = 0.0;
                    this.Dispatcher.BeginInvoke((Action)(() => this.AnimateDrop(itemContainer)));
                }
            }
            else
                VisualStateManager.GoToState((Control)itemContainer, "NotDragging", false);
            ContentPresenter logicalChildByType = itemContainer.GetFirstLogicalChildByType<ContentPresenter>(true);
            EventHandler<MyLinkUnlinkEventArgs> eventHandler = this.Link;
            if (logicalChildByType == null || eventHandler == null || this._isManipulating)
                return;
            eventHandler((object)this, new MyLinkUnlinkEventArgs(logicalChildByType));
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            if ((ReorderListBoxItem)element == this.dragItemContainer)
            {
                this.dragItemContainer.Visibility = Visibility.Visible;
                this.dragItemContainer = (ReorderListBoxItem)null;
            }
            ContentPresenter logicalChildByType = ((FrameworkElement)element).GetFirstLogicalChildByType<ContentPresenter>(true);
            EventHandler<MyLinkUnlinkEventArgs> eventHandler = this.Unlink;
            if (logicalChildByType == null || eventHandler == null)
                return;
            eventHandler((object)this, new MyLinkUnlinkEventArgs(logicalChildByType));
        }

        private void dragInterceptor_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            this.scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            if (this.dragItem != null)
                return;
            e.Handled = true;
            if (this.itemsPanel == null)
                this.itemsPanel = (Panel)VisualTreeHelper.GetChild((DependencyObject)this.scrollViewer.Content, 0);
            this._isManipulating = true;
            GeneralTransform visual = this.dragInterceptor.TransformToVisual(Application.Current.RootVisual);
            List<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(ReorderListBox.GetHostCoordinates(visual.Transform(e.ManipulationOrigin)), (UIElement)this.itemsPanel).ToList<UIElement>();
            ReorderListBoxItem reorderListBoxItem = list.OfType<ReorderListBoxItem>().FirstOrDefault<ReorderListBoxItem>();
            if (reorderListBoxItem == null || !list.Contains((UIElement)reorderListBoxItem.DragHandle))
                return;
            VisualStateManager.GoToState((Control)reorderListBoxItem, "Dragging", true);
            Point point = reorderListBoxItem.TransformToVisual((UIElement)this.dragInterceptor).Transform(new Point(0.0, 0.0));
            Canvas.SetLeft((UIElement)this.dragIndicator, point.X);
            Canvas.SetTop((UIElement)this.dragIndicator, point.Y);
            this.dragIndicator.Width = reorderListBoxItem.RenderSize.Width;
            this.dragIndicator.Height = reorderListBoxItem.RenderSize.Height;
            this.dragItemContainer = reorderListBoxItem;
            this.dragItem = this.dragItemContainer.Content;
            this.isDragItemSelected = this.dragItemContainer.IsSelected;
            this.dragInterceptorRect = visual.TransformBounds(new Rect(new Point(0.0, 0.0), this.dragInterceptor.RenderSize));
            this.dropTargetIndex = -1;
        }

        private void dragInterceptor_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (this.Items.Count <= 1 || this.dragItem == null)
                return;
            e.Handled = true;
            if (this.dropTargetIndex == -1)
            {
                if (this.dragItemContainer == null)
                    return;
                Size renderSize = this.dragItemContainer.RenderSize;
                WriteableBitmap writeableBitmap = new WriteableBitmap((int)renderSize.Width, (int)renderSize.Height);
                VisualStateManager.GoToState((Control)this.dragItemContainer, "NotDragging", false);
                VisualStateManager.GoToState((Control)this.dragItemContainer, "Dragging", false);
                writeableBitmap.Render((UIElement)this.dragItemContainer, (Transform)null);
                writeableBitmap.Invalidate();
                this.dragIndicator.Source = (ImageSource)writeableBitmap;
                this.dragIndicator.Visibility = Visibility.Visible;
                this.dragItemContainer.Visibility = Visibility.Collapsed;
                if (this.itemsPanel.Children.IndexOf((UIElement)this.dragItemContainer) < this.itemsPanel.Children.Count - 2)
                    this.UpdateDropTarget(Canvas.GetTop((UIElement)this.dragIndicator) + this.dragIndicator.Height + 1.0, false);
                else
                    this.UpdateDropTarget(Canvas.GetTop((UIElement)this.dragIndicator) - 1.0, false);
            }
            double height = this.dragIndicator.Height;
            TranslateTransform translateTransform = (TranslateTransform)this.dragIndicator.RenderTransform;
            double top = Canvas.GetTop((UIElement)this.dragIndicator);
            double num1 = top + e.CumulativeManipulation.Translation.Y;
            if (num1 < 0.0)
            {
                num1 = 0.0;
                this.UpdateDropTarget(0.0, true);
            }
            else if (num1 >= this.dragInterceptorRect.Height - height)
            {
                num1 = this.dragInterceptorRect.Height - height;
                this.UpdateDropTarget(this.dragInterceptorRect.Height - 1.0, true);
            }
            else
                this.UpdateDropTarget(num1 + height / 2.0, true);
            double num2 = num1 - top;
            translateTransform.Y = num2;
            bool flag = this.dragScrollDelta != 0.0;
            double autoScrollMargin = this.AutoScrollMargin;
            if (autoScrollMargin > 0.0 && num1 < autoScrollMargin)
            {
                this.dragScrollDelta = num1 - autoScrollMargin;
                if (flag)
                    return;
                VisualStateManager.GoToState((Control)this.scrollViewer, "Scrolling", true);
                this.Dispatcher.BeginInvoke((Action)(() => this.DragScroll()));
            }
            else if (autoScrollMargin > 0.0 && num1 + height > this.dragInterceptorRect.Height - autoScrollMargin)
            {
                this.dragScrollDelta = num1 + height - (this.dragInterceptorRect.Height - autoScrollMargin);
                if (flag)
                    return;
                VisualStateManager.GoToState((Control)this.scrollViewer, "Scrolling", true);
                this.Dispatcher.BeginInvoke((Action)(() => this.DragScroll()));
            }
            else
                this.dragScrollDelta = 0.0;
        }

        private void dragInterceptor_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (this.dragItem == null)
            {
                this.scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                e.Handled = true;
                this._isManipulating = false;
                if (this.dropTargetIndex >= 0)
                    this.MoveItem(this.dragItem, this.dropTargetIndex);
                if (this.dragItemContainer != null)
                {
                    this.dragItemContainer.Visibility = Visibility.Visible;
                    this.dragItemContainer.Opacity = 0.0;
                    this.AnimateDrop(this.dragItemContainer);
                    this.dragItemContainer = (ReorderListBoxItem)null;
                }
                this.dragScrollDelta = 0.0;
                this.dropTargetIndex = -1;
                this.ClearDropTarget();
                this.scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
        }

        private void AnimateDrop(ReorderListBoxItem itemContainer)
        {
            Rect rect = itemContainer.TransformToVisual((UIElement)this.dragInterceptor).TransformBounds(new Rect(new Point(0.0, 0.0), itemContainer.RenderSize));
            double num = Math.Abs(rect.Y - Canvas.GetTop((UIElement)this.dragIndicator) - ((TranslateTransform)this.dragIndicator.RenderTransform).Y);
            if (num > 0.0)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(0.25 * num / rect.Height);
                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                Storyboard.SetTarget((Timeline)doubleAnimation, (DependencyObject)this.dragIndicator.RenderTransform);
                Storyboard.SetTargetProperty((Timeline)doubleAnimation, new PropertyPath((object)TranslateTransform.YProperty));
                doubleAnimation.To = new double?(rect.Y - Canvas.GetTop((UIElement)this.dragIndicator));
                doubleAnimation.Duration = (Duration)timeSpan;
                storyboard.Children.Add((Timeline)doubleAnimation);
                EventHandler eventHandler = (EventHandler)((param0, param1) =>
                {
                    this.dragItem = null;
                    itemContainer.Opacity = 1.0;
                    this.dragIndicator.Visibility = Visibility.Collapsed;
                    this.dragIndicator.Source = null;
                    ((TranslateTransform)this.dragIndicator.RenderTransform).Y = 0.0;
                    VisualStateManager.GoToState((Control)itemContainer, "NotDragging", true);
                });
                storyboard.Completed += eventHandler;
                storyboard.Begin();
            }
            else
            {
                this.dragItem = null;
                itemContainer.Opacity = 1.0;
                this.dragIndicator.Visibility = Visibility.Collapsed;
                this.dragIndicator.Source = null;
                VisualStateManager.GoToState((Control)itemContainer, "NotDragging", true);
            }
        }

        private void DragScroll()
        {
            if (this.dragScrollDelta != 0.0)
            {
                this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.VerticalOffset + this.dragScrollDelta * (this.scrollViewer.ViewportHeight / this.scrollViewer.RenderSize.Height));
                this.Dispatcher.BeginInvoke((Action)(() => this.DragScroll()));
                this.UpdateDropTarget(Canvas.GetTop((UIElement)this.dragIndicator) + ((TranslateTransform)this.dragIndicator.RenderTransform).Y + this.dragIndicator.Height / 2.0, true);
            }
            else
                VisualStateManager.GoToState((Control)this.scrollViewer, "NotScrolling", true);
        }

        private void UpdateDropTarget(double dragItemOffset, bool showTransition)
        {
            ReorderListBoxItem targetItemContainer = VisualTreeHelper.FindElementsInHostCoordinates(ReorderListBox.GetHostCoordinates(new Point(this.dragInterceptorRect.Left, this.dragInterceptorRect.Top + dragItemOffset)), (UIElement)this.itemsPanel).OfType<ReorderListBoxItem>().FirstOrDefault<ReorderListBoxItem>();
            if (targetItemContainer == null)
                return;
            Rect rect = targetItemContainer.DragHandle.TransformToVisual((UIElement)this.dragInterceptor).TransformBounds(new Rect(new Point(0.0, 0.0), targetItemContainer.DragHandle.RenderSize));
            double num1 = (rect.Top + rect.Bottom) / 2.0;
            int num2 = this.itemsPanel.Children.IndexOf((UIElement)targetItemContainer);
            int count = this.itemsPanel.Children.Count;
            bool after = dragItemOffset > num1;
            ReorderListBoxItem reorderListBoxItem1 = (ReorderListBoxItem)null;
            if (!after && num2 > 0)
            {
                ReorderListBoxItem reorderListBoxItem2 = (ReorderListBoxItem)this.itemsPanel.Children[num2 - 1];
                if (reorderListBoxItem2.Tag as string == "DropAfterIndicator")
                    reorderListBoxItem1 = reorderListBoxItem2;
            }
            else if (after && num2 < count - 1)
            {
                ReorderListBoxItem reorderListBoxItem2 = (ReorderListBoxItem)this.itemsPanel.Children[num2 + 1];
                if (reorderListBoxItem2.Tag as string == "DropBeforeIndicator")
                    reorderListBoxItem1 = reorderListBoxItem2;
            }
            if (reorderListBoxItem1 == null)
            {
                targetItemContainer.DropIndicatorHeight = this.dragIndicator.Height;
                string stateName = after ? "DropAfterIndicator" : "DropBeforeIndicator";
                VisualStateManager.GoToState((Control)targetItemContainer, stateName, showTransition);
                targetItemContainer.Tag = (object)stateName;
                reorderListBoxItem1 = targetItemContainer;
            }
            for (int index = num2 - 5; index <= num2 + 5; ++index)
            {
                if (index >= 0 && index < count)
                {
                    ReorderListBoxItem reorderListBoxItem2 = (ReorderListBoxItem)this.itemsPanel.Children[index];
                    if (reorderListBoxItem2 != reorderListBoxItem1)
                    {
                        VisualStateManager.GoToState((Control)reorderListBoxItem2, "NoDropIndicator", showTransition);
                        reorderListBoxItem2.Tag = (object)"NoDropIndicator";
                    }
                }
            }
            this.UpdateDropTargetIndex(targetItemContainer, after);
        }

        private void UpdateDropTargetIndex(ReorderListBoxItem targetItemContainer, bool after)
        {
            int num1 = this.Items.IndexOf(this.dragItem);
            int num2 = this.Items.IndexOf(targetItemContainer.Content);
            int val1 = num2 != num1 ? num2 + (after ? 1 : 0) - (num2 >= num1 ? 1 : 0) : num1;
            if (val1 == this.dropTargetIndex)
                return;
            this.dropTargetIndex = Math.Min(val1, this.Items.Count - 2);
        }

        private void ClearDropTarget()
        {
            foreach (ReorderListBoxItem child in (PresentationFrameworkCollection<UIElement>)this.itemsPanel.Children)
            {
                string stateName = "NoDropIndicator";
                int num = 0;
                VisualStateManager.GoToState((Control)child, stateName, num != 0);
                object local = null;
                child.Tag = (object)local;
            }
        }

        private bool MoveItem(object item, int toIndex)
        {
            IList list = this.ItemsSource as IList;
            if (!(list is INotifyCollectionChanged))
                list = (IList)this.Items;
            int index = list.IndexOf(item);
            if (index == toIndex)
                return false;
            double verticalOffset = this.scrollViewer.VerticalOffset;
            list.RemoveAt(index);
            list.Insert(toIndex, item);
            if ((double)index <= verticalOffset && (double)toIndex > verticalOffset)
                this.scrollViewer.ScrollToVerticalOffset(verticalOffset - 1.0);
            return true;
        }

        public void GetViewIndexRange(bool includePartial, out int firstIndex, out int lastIndex)
        {
            if (this.Items.Count > 0)
            {
                firstIndex = 0;
                lastIndex = this.Items.Count - 1;
                if (this.scrollViewer == null || this.Items.Count <= 1)
                    return;
                Thickness thickness = new Thickness(this.scrollViewer.BorderThickness.Left + this.scrollViewer.Padding.Left, this.scrollViewer.BorderThickness.Top + this.scrollViewer.Padding.Top, this.scrollViewer.BorderThickness.Right + this.scrollViewer.Padding.Right, this.scrollViewer.BorderThickness.Bottom + this.scrollViewer.Padding.Bottom);
                Rect rect1 = this.scrollViewer.TransformToVisual(Application.Current.RootVisual).TransformBounds(new Rect(new Point(0.0, 0.0), this.scrollViewer.RenderSize));
                ReorderListBoxItem reorderListBoxItem1 = VisualTreeHelper.FindElementsInHostCoordinates(ReorderListBox.GetHostCoordinates(new Point(rect1.Left + thickness.Left, rect1.Top + thickness.Top)), (UIElement)this.scrollViewer).OfType<ReorderListBoxItem>().FirstOrDefault<ReorderListBoxItem>();
                if (reorderListBoxItem1 != null)
                {
                    Rect rect2 = reorderListBoxItem1.TransformToVisual(Application.Current.RootVisual).TransformBounds(new Rect(new Point(0.0, 0.0), reorderListBoxItem1.RenderSize));
                    firstIndex = this.ItemContainerGenerator.IndexFromContainer((DependencyObject)reorderListBoxItem1);
                    if (!includePartial && firstIndex < this.Items.Count - 1 && (rect2.Top < rect1.Top && rect2.Bottom < rect1.Bottom))
                        firstIndex = firstIndex + 1;
                }
                ReorderListBoxItem reorderListBoxItem2 = VisualTreeHelper.FindElementsInHostCoordinates(ReorderListBox.GetHostCoordinates(new Point(rect1.Left + thickness.Left, rect1.Bottom - thickness.Bottom - 1.0)), (UIElement)this.scrollViewer).OfType<ReorderListBoxItem>().FirstOrDefault<ReorderListBoxItem>();
                if (reorderListBoxItem2 == null)
                    return;
                Rect rect3 = reorderListBoxItem2.TransformToVisual(Application.Current.RootVisual).TransformBounds(new Rect(new Point(0.0, 0.0), reorderListBoxItem2.RenderSize));
                lastIndex = this.ItemContainerGenerator.IndexFromContainer((DependencyObject)reorderListBoxItem2);
                if (includePartial || lastIndex <= firstIndex || (rect3.Bottom <= rect1.Bottom || rect3.Top <= rect1.Top))
                    return;
                lastIndex = lastIndex - 1;
            }
            else
            {
                firstIndex = -1;
                lastIndex = -1;
            }
        }

        public void AnimateRearrange(Duration animationDuration, Action rearrangeAction)
        {
            if (rearrangeAction == null)
                throw new ArgumentNullException("rearrangeAction");
            if (this.rearrangeCanvas == null)
                throw new InvalidOperationException("ReorderListBox control template is missing a part required for rearrange: RearrangeCanvas");
            if (this.rearrangeQueue == null)
            {
                this.rearrangeQueue = new Queue<KeyValuePair<Action, Duration>>();
                this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.VerticalOffset);
                this.Dispatcher.BeginInvoke((Action)(() => this.AnimateRearrangeInternal(rearrangeAction, animationDuration)));
            }
            else
                this.rearrangeQueue.Enqueue(new KeyValuePair<Action, Duration>(rearrangeAction, animationDuration));
        }

        private void AnimateRearrangeInternal(Action rearrangeAction, Duration animationDuration)
        {
            int firstIndex;
            int lastIndex;
            this.GetViewIndexRange(true, out firstIndex, out lastIndex);
            ReorderListBox.RearrangeItemInfo[] map = this.BuildRearrangeMap(firstIndex, lastIndex);
            rearrangeAction();
            this.rearrangeCanvas.Visibility = Visibility.Visible;
            this.UpdateLayout();
            int viewLastIndex = this.FindViewLastIndex(firstIndex);
            ReorderListBox.RearrangeItemInfo[] rearrangeItemInfoArray = this.BuildRearrangeMap2(map, firstIndex, viewLastIndex);
            IEnumerable<ReorderListBox.RearrangeItemInfo> visibleMoves = ((IEnumerable<ReorderListBox.RearrangeItemInfo>)map).Where<ReorderListBox.RearrangeItemInfo>((Func<ReorderListBox.RearrangeItemInfo, bool>)(rii =>
            {
                if (!double.IsNaN(rii.FromY))
                    return !double.IsNaN(rii.ToY);
                return false;
            })).Concat<ReorderListBox.RearrangeItemInfo>(((IEnumerable<ReorderListBox.RearrangeItemInfo>)map).Where<ReorderListBox.RearrangeItemInfo>((Func<ReorderListBox.RearrangeItemInfo, bool>)(rii =>
            {
                if (!double.IsNaN(rii.FromY))
                    return double.IsNaN(rii.ToY);
                return false;
            }))).Concat<ReorderListBox.RearrangeItemInfo>(((IEnumerable<ReorderListBox.RearrangeItemInfo>)rearrangeItemInfoArray).Where<ReorderListBox.RearrangeItemInfo>((Func<ReorderListBox.RearrangeItemInfo, bool>)(rii =>
            {
                if (double.IsNaN(rii.FromY))
                    return !double.IsNaN(rii.ToY);
                return false;
            })));
            this.rearrangeCanvas.Clip = (Geometry)new RectangleGeometry()
            {
                Rect = new Rect(new Point(0.0, 0.0), this.rearrangeCanvas.RenderSize)
            };
            Storyboard rearrangeStoryboard = this.CreateRearrangeStoryboard(visibleMoves, animationDuration);
            if (rearrangeStoryboard.Children.Count > 0)
            {
                this.scrollViewer.Visibility = Visibility.Collapsed;
                rearrangeStoryboard.Completed += (EventHandler)((param0, param1) =>
                {
                    rearrangeStoryboard.Stop();
                    this.rearrangeCanvas.Children.Clear();
                    this.rearrangeCanvas.Visibility = Visibility.Collapsed;
                    this.scrollViewer.Visibility = Visibility.Visible;
                    this.AnimateNextRearrange();
                });
                this.Dispatcher.BeginInvoke((Action)(() => rearrangeStoryboard.Begin()));
            }
            else
            {
                this.rearrangeCanvas.Visibility = Visibility.Collapsed;
                this.AnimateNextRearrange();
            }
        }

        private void AnimateNextRearrange()
        {
            if (this.rearrangeQueue.Count > 0)
            {
                KeyValuePair<Action, Duration> nextRearrange = this.rearrangeQueue.Dequeue();
                this.Dispatcher.BeginInvoke((Action)(() => this.AnimateRearrangeInternal(nextRearrange.Key, nextRearrange.Value)));
            }
            else
                this.rearrangeQueue = (Queue<KeyValuePair<Action, Duration>>)null;
        }

        private ReorderListBox.RearrangeItemInfo[] BuildRearrangeMap(int viewFirstIndex, int viewLastIndex)
        {
            ReorderListBox.RearrangeItemInfo[] rearrangeItemInfoArray = new ReorderListBox.RearrangeItemInfo[this.Items.Count];
            for (int index = 0; index < rearrangeItemInfoArray.Length; ++index)
            {
                object obj = this.Items[index];
                ReorderListBox.RearrangeItemInfo rearrangeItemInfo = new ReorderListBox.RearrangeItemInfo()
                {
                    Item = obj,
                    FromIndex = index
                };
                if (viewFirstIndex <= index && index <= viewLastIndex)
                {
                    ReorderListBoxItem reorderListBoxItem = (ReorderListBoxItem)this.ItemContainerGenerator.ContainerFromIndex(index);
                    if (reorderListBoxItem != null)
                    {
                        Point point = reorderListBoxItem.TransformToVisual((UIElement)this.rearrangeCanvas).Transform(new Point(0.0, 0.0));
                        rearrangeItemInfo.FromY = point.Y;
                        rearrangeItemInfo.Height = reorderListBoxItem.RenderSize.Height;
                    }
                }
                rearrangeItemInfoArray[index] = rearrangeItemInfo;
            }
            return rearrangeItemInfoArray;
        }

        private ReorderListBox.RearrangeItemInfo[] BuildRearrangeMap2(ReorderListBox.RearrangeItemInfo[] map, int viewFirstIndex, int viewLastIndex)
        {
            ReorderListBox.RearrangeItemInfo[] rearrangeItemInfoArray = new ReorderListBox.RearrangeItemInfo[this.Items.Count];
            for (int index = 0; index < rearrangeItemInfoArray.Length; ++index)
            {
                object item = this.Items[index];
                ReorderListBox.RearrangeItemInfo rearrangeItemInfo = ((IEnumerable<ReorderListBox.RearrangeItemInfo>)map).FirstOrDefault<ReorderListBox.RearrangeItemInfo>((Func<ReorderListBox.RearrangeItemInfo, bool>)(rii =>
                {
                    if (rii.ToIndex < 0)
                        return rii.Item == item;
                    return false;
                }));
                if (rearrangeItemInfo == null)
                    rearrangeItemInfo = new ReorderListBox.RearrangeItemInfo()
                    {
                        Item = item
                    };
                rearrangeItemInfo.ToIndex = index;
                if (viewFirstIndex <= index && index <= viewLastIndex)
                {
                    ReorderListBoxItem reorderListBoxItem = (ReorderListBoxItem)this.ItemContainerGenerator.ContainerFromIndex(index);
                    if (reorderListBoxItem != null)
                    {
                        Point point = reorderListBoxItem.TransformToVisual((UIElement)this.rearrangeCanvas).Transform(new Point(0.0, 0.0));
                        rearrangeItemInfo.ToY = point.Y;
                        rearrangeItemInfo.Height = reorderListBoxItem.RenderSize.Height;
                    }
                }
                rearrangeItemInfoArray[index] = rearrangeItemInfo;
            }
            return rearrangeItemInfoArray;
        }

        private int FindViewLastIndex(int firstIndex)
        {
            int num = firstIndex;
            Rect rect1 = this.scrollViewer.TransformToVisual(Application.Current.RootVisual).TransformBounds(new Rect(new Point(0.0, 0.0), this.scrollViewer.RenderSize));
            for (; num < this.Items.Count - 1; ++num)
            {
                ReorderListBoxItem reorderListBoxItem = (ReorderListBoxItem)this.ItemContainerGenerator.ContainerFromIndex(num + 1);
                if (reorderListBoxItem != null)
                {
                    Rect rect2 = reorderListBoxItem.TransformToVisual(Application.Current.RootVisual).TransformBounds(new Rect(new Point(0.0, 0.0), reorderListBoxItem.RenderSize));
                    rect2.Intersect(rect1);
                    if (rect2 == Rect.Empty)
                        break;
                }
                else
                    break;
            }
            return num;
        }

        private Storyboard CreateRearrangeStoryboard(IEnumerable<ReorderListBox.RearrangeItemInfo> visibleMoves, Duration animationDuration)
        {
            Storyboard storyboard = new Storyboard();
            ReorderListBoxItem reorderListBoxItem1 = (ReorderListBoxItem)null;
            foreach (ReorderListBox.RearrangeItemInfo visibleMove in visibleMoves)
            {
                Size size = new Size(this.rearrangeCanvas.RenderSize.Width, visibleMove.Height);
                ReorderListBoxItem reorderListBoxItem2 = (ReorderListBoxItem)null;
                if (visibleMove.ToIndex >= 0)
                    reorderListBoxItem2 = (ReorderListBoxItem)this.ItemContainerGenerator.ContainerFromIndex(visibleMove.ToIndex);
                if (reorderListBoxItem2 == null)
                {
                    if (reorderListBoxItem1 == null)
                        reorderListBoxItem1 = new ReorderListBoxItem();
                    reorderListBoxItem2 = reorderListBoxItem1;
                    reorderListBoxItem2.Width = size.Width;
                    reorderListBoxItem2.Height = size.Height;
                    this.rearrangeCanvas.Children.Add((UIElement)reorderListBoxItem2);
                    this.PrepareContainerForItemOverride((DependencyObject)reorderListBoxItem2, visibleMove.Item);
                    reorderListBoxItem2.UpdateLayout();
                }
                WriteableBitmap writeableBitmap = new WriteableBitmap((int)size.Width, (int)size.Height);
                writeableBitmap.Render((UIElement)reorderListBoxItem2, (Transform)null);
                writeableBitmap.Invalidate();
                Image image = new Image();
                image.Width = size.Width;
                image.Height = size.Height;
                image.Source = (ImageSource)writeableBitmap;
                image.RenderTransform = (Transform)new TranslateTransform();
                this.rearrangeCanvas.Children.Add((UIElement)image);
                if (reorderListBoxItem2 == reorderListBoxItem1)
                    this.rearrangeCanvas.Children.Remove((UIElement)reorderListBoxItem2);
                if (!double.IsNaN(visibleMove.FromY) && !double.IsNaN(visibleMove.ToY))
                {
                    Canvas.SetTop((UIElement)image, visibleMove.FromY);
                    if (visibleMove.FromY != visibleMove.ToY)
                    {
                        DoubleAnimation doubleAnimation = new DoubleAnimation();
                        doubleAnimation.Duration = animationDuration;
                        Storyboard.SetTarget((Timeline)doubleAnimation, (DependencyObject)image.RenderTransform);
                        Storyboard.SetTargetProperty((Timeline)doubleAnimation, new PropertyPath((object)TranslateTransform.YProperty));
                        doubleAnimation.To = new double?(visibleMove.ToY - visibleMove.FromY);
                        storyboard.Children.Add((Timeline)doubleAnimation);
                    }
                }
                else if (double.IsNaN(visibleMove.FromY) != double.IsNaN(visibleMove.ToY))
                {
                    if (visibleMove.FromIndex >= 0 && visibleMove.ToIndex >= 0)
                    {
                        DoubleAnimation doubleAnimation = new DoubleAnimation();
                        doubleAnimation.Duration = animationDuration;
                        Storyboard.SetTarget((Timeline)doubleAnimation, (DependencyObject)image.RenderTransform);
                        Storyboard.SetTargetProperty((Timeline)doubleAnimation, new PropertyPath((object)TranslateTransform.YProperty));
                        if (!double.IsNaN(visibleMove.FromY))
                        {
                            Canvas.SetTop((UIElement)image, visibleMove.FromY);
                            if (visibleMove.FromIndex < visibleMove.ToIndex)
                                doubleAnimation.To = new double?(200.0);
                            else if (visibleMove.FromIndex > visibleMove.ToIndex)
                                doubleAnimation.To = new double?(-200.0);
                        }
                        else
                        {
                            Canvas.SetTop((UIElement)image, visibleMove.ToY);
                            if (visibleMove.FromIndex < visibleMove.ToIndex)
                                doubleAnimation.From = new double?(-200.0);
                            else if (visibleMove.FromIndex > visibleMove.ToIndex)
                                doubleAnimation.From = new double?(200.0);
                        }
                        storyboard.Children.Add((Timeline)doubleAnimation);
                    }
                    DoubleAnimation doubleAnimation1 = new DoubleAnimation();
                    doubleAnimation1.Duration = animationDuration;
                    Storyboard.SetTarget((Timeline)doubleAnimation1, (DependencyObject)image);
                    Storyboard.SetTargetProperty((Timeline)doubleAnimation1, new PropertyPath((object)UIElement.OpacityProperty));
                    if (double.IsNaN(visibleMove.FromY))
                    {
                        image.Opacity = 0.0;
                        doubleAnimation1.To = new double?(1.0);
                        Canvas.SetTop((UIElement)image, visibleMove.ToY);
                    }
                    else
                    {
                        image.Opacity = 1.0;
                        doubleAnimation1.To = new double?(0.0);
                        Canvas.SetTop((UIElement)image, visibleMove.FromY);
                    }
                    storyboard.Children.Add((Timeline)doubleAnimation1);
                }
            }
            return storyboard;
        }

        private static Point GetHostCoordinates(Point point)
        {
            PhoneApplicationFrame applicationFrame = (PhoneApplicationFrame)Application.Current.RootVisual;
            switch (applicationFrame.Orientation)
            {
                case PageOrientation.LandscapeLeft:
                    return new Point(applicationFrame.RenderSize.Width - point.Y, point.X);
                case PageOrientation.LandscapeRight:
                    return new Point(point.Y, applicationFrame.RenderSize.Height - point.X);
                default:
                    return point;
            }
        }

        private class RearrangeItemInfo
        {
            public int FromIndex = -1;
            public int ToIndex = -1;
            public double FromY = double.NaN;
            public double ToY = double.NaN;
            public double Height = double.NaN;
            public object Item;
        }
    }
}
