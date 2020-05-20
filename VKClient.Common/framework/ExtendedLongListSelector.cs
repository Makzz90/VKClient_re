using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace VKClient.Common.Framework
{
    public class ExtendedLongListSelector : LongListSelector, ISupportPullToRefresh
    {
        public static readonly DependencyProperty GroupItemsPanelProperty = DependencyProperty.Register("GroupItemsPanel", typeof(ItemsPanelTemplate), typeof(LongListSelector), new PropertyMetadata(null));
        public static readonly double PULL_EXTRA_MARGIN = 150.0;
        public static readonly DependencyProperty ScrollPositionProperty = DependencyProperty.Register("ScrollPosition", typeof(double), typeof(ExtendedLongListSelector), new PropertyMetadata(0.0, new PropertyChangedCallback(ExtendedLongListSelector.OnScrollPositionChanged)));
        public static readonly DependencyProperty BoundsProperty = DependencyProperty.Register("Bounds", typeof(Rect), typeof(ExtendedLongListSelector), new PropertyMetadata(new Rect(), new PropertyChangedCallback(ExtendedLongListSelector.OnBoundsChanged)));
        private ViewportControl _viewport;
        private bool _lockBounds;
        private bool _preventPullUntilNextManipulationStateChange;
        private Rect _lockedBoundsRect;
        private Rect _savedBoundsRect;

        public bool IsFlatList
        {
            get
            {
                return !this.IsGroupingEnabled;
            }
            set
            {
                this.IsGroupingEnabled = (!value);
            }
        }

        public ItemsPanelTemplate GroupItemsPanel
        {
            get
            {
                return (ItemsPanelTemplate)base.GetValue(ExtendedLongListSelector.GroupItemsPanelProperty);
            }
            set
            {
                base.SetValue(ExtendedLongListSelector.GroupItemsPanelProperty, value);
            }
        }

        public ViewportControl Viewport
        {
            get
            {
                return this._viewport;
            }
        }

        public bool LockedBounds
        {
            get
            {
                return this._lockBounds;
            }
        }

        public double ScrollPosition
        {
            get
            {
                double num = (double)base.GetValue(ExtendedLongListSelector.ScrollPositionProperty);
                if (num == 8388608.0)
                    num = 0.0;
                return num;
            }
            set
            {
                base.SetValue(ExtendedLongListSelector.ScrollPositionProperty, value);
            }
        }

        public double PullPercentage
        {
            get
            {
                if (this._preventPullUntilNextManipulationStateChange || !this._lockBounds)
                    return 0.0;
                double scrollPosition = this.ScrollPosition;
                if (scrollPosition >= ExtendedLongListSelector.PULL_EXTRA_MARGIN)
                    return 0.0;
                return (ExtendedLongListSelector.PULL_EXTRA_MARGIN - scrollPosition) * 100.0 / ExtendedLongListSelector.PULL_EXTRA_MARGIN;
            }
        }

        public Action OnPullPercentageChanged { get; set; }

        public Action OnRefresh { get; set; }

        public event EventHandler<LinkUnlinkEventArgs> Link;

        public event EventHandler<LinkUnlinkEventArgs> Unlink;

        //public event ExtendedLongListSelector.OnCompression Compression;

        public event EventHandler ScrollPositionChanged;

        public ExtendedLongListSelector()
        {
            this.ItemRealized += this.ExtendedLongListSelector_ItemRealized;
            this.ItemUnrealized += this.ExtendedLongListSelector_ItemUnrealized;
        }

        private void ExtendedLongListSelector_ItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.Unlink == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.Unlink(this, new LinkUnlinkEventArgs(e.Container));
        }

        private void ExtendedLongListSelector_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.Link == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.Link(this, new LinkUnlinkEventArgs(e.Container));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._viewport = (ViewportControl)this.GetTemplateChild("ViewportControl");
            this._viewport.ViewportChanged += (new EventHandler<ViewportChangedEventArgs>(this.OnViewportChanged));
            this._viewport.ManipulationStateChanged += (new EventHandler<ManipulationStateChangedEventArgs>(this._viewport_ManipulationStateChanged));
            Binding binding = new Binding("Bounds");
            binding.Source = this._viewport;
            BindingOperations.SetBinding((DependencyObject)this, ExtendedLongListSelector.BoundsProperty, (BindingBase)binding);
        }

        private void _viewport_ManipulationStateChanged(object sender, ManipulationStateChangedEventArgs e)
        {
            this._preventPullUntilNextManipulationStateChange = false;
            ViewportControl viewport = this._viewport;
            if (viewport.ManipulationState == ManipulationState.Manipulating && this.ScrollPosition == 0.0)
                this.LockBoundsForPull(viewport);
            else
                this.UnlockBounds();
        }

        private void LockBoundsForPull(ViewportControl viewport)
        {
            if (!this._lockBounds)
            {
                this._lockBounds = true;
                this._savedBoundsRect = viewport.Bounds;
                this._lockedBoundsRect = new Rect(viewport.Bounds.Left, viewport.Bounds.Top - ExtendedLongListSelector.PULL_EXTRA_MARGIN, viewport.Bounds.Width, viewport.Bounds.Height + ExtendedLongListSelector.PULL_EXTRA_MARGIN);
                viewport.Bounds = (this._lockedBoundsRect);
            }
        }

        private void UnlockBounds()
        {
            if (!this._lockBounds)
                return;
            this._lockBounds = false;
            this._viewport.Bounds = this._savedBoundsRect;
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs args)
        {
            if (this._preventPullUntilNextManipulationStateChange)
                return;
            this.UpdateScrollPosition();
            if (this.ScrollPosition == 0.0 && this.Viewport.ManipulationState == ManipulationState.Manipulating)
                this.LockBoundsForPull(this._viewport);
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
        }

        private void UpdateScrollPosition()
        {
            Rect rect = this._viewport.Viewport;
            // ISSUE: explicit reference operation
            double top1 = rect.Top;
            rect = this._viewport.Bounds;
            // ISSUE: explicit reference operation
            double top2 = rect.Top;
            this.ScrollPosition = top1 - top2;
        }

        public void ScrollToTop()
        {
            Rect rect = this.Viewport.Viewport;
            // ISSUE: explicit reference operation
            double y = rect.Y;
            rect = this._viewport.Bounds;
            // ISSUE: explicit reference operation
            double top1 = rect.Top;
            if (y == top1)
                return;
            if (this.ItemsSource != null && this.ItemsSource.Count > 0)
                this.ScrollTo(this.ItemsSource[0]);
            ViewportControl viewport = this.Viewport;
            double num = 0.0;
            Rect bounds = this._viewport.Bounds;
            // ISSUE: explicit reference operation
            double top2 = ((Rect)@bounds).Top;
            Point point = new Point(num, top2);
            viewport.SetViewportOrigin(point);
            this.UpdateScrollPosition();
        }

        public void ScrollToPosition(double position)
        {
            Rect viewport = this.Viewport.Viewport;
            // ISSUE: explicit reference operation
            if (viewport.Y == position)
                return;
            this.Viewport.SetViewportOrigin(new Point(0.0, position));
            this.UpdateScrollPosition();
        }

        private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedLongListSelector longListSelector = (ExtendedLongListSelector)d;
            ViewportControl viewport = ((ExtendedLongListSelector)d)._viewport;
            longListSelector.UpdateScrollPosition();
            if (!longListSelector._lockBounds)
                return;
            viewport.Bounds = longListSelector._lockedBoundsRect;
        }

        private static void OnScrollPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExtendedLongListSelector)d).FireScrollPositionChanged();
        }

        private void FireScrollPositionChanged()
        {
            // ISSUE: reference to a compiler-generated field
            if (this.ScrollPositionChanged == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.ScrollPositionChanged(this, EventArgs.Empty);
        }

        public delegate void OnCompression(object sender, CompressionEventArgs e);
    }
}
