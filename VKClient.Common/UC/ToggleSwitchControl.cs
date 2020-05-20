using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace VKClient.Common.UC
{
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "CheckStates", Name = "Checked")]
    [TemplateVisualState(GroupName = "CheckStates", Name = "Unchecked")]
    [TemplatePart(Name = "SwitchRoot", Type = typeof(Grid))]
    [TemplatePart(Name = "BorderSwitchForeground", Type = typeof(Border))]
    [TemplatePart(Name = "SwitchTrack", Type = typeof(Grid))]
    [TemplatePart(Name = "SwitchThumb", Type = typeof(FrameworkElement))]
    public class ToggleSwitchControl : ToggleButton
    {
        public static readonly DependencyProperty SwitchBackgroundProperty = DependencyProperty.Register("SwitchBackground", typeof(Brush), typeof(ToggleSwitchControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SwitchForegroundProperty = DependencyProperty.Register("SwitchForeground", typeof(Brush), typeof(ToggleSwitchControl), new PropertyMetadata(null));
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(ToggleSwitchControl), new PropertyMetadata(null));
        private const string CommonStates = "CommonStates";
        private const string NormalState = "Normal";
        private const string DisabledState = "Disabled";
        private const string CheckStates = "CheckStates";
        private const string CheckedState = "Checked";
        private const string UncheckedState = "Unchecked";
        private const string SwitchRootPart = "SwitchRoot";
        private const string SwitchBorderForeground = "BorderSwitchForeground";
        private const string SwitchTrackPart = "SwitchTrack";
        private const string SwitchThumbPart = "SwitchThumb";
        private TranslateTransform _thumbTranslation;
        private Grid _root;
        private Border _borderForeground;
        private Grid _track;
        private FrameworkElement _thumb;
        private const double _uncheckedTranslation = 0.0;
        private double _checkedTranslation;
        private double _dragTranslation;
        private bool _wasDragged;

        public Brush SwitchBackground
        {
            get
            {
                return (Brush)base.GetValue(ToggleSwitchControl.SwitchBackgroundProperty);
            }
            set
            {
                base.SetValue(ToggleSwitchControl.SwitchBackgroundProperty, value);
            }
        }

        public Brush SwitchForeground
        {
            get
            {
                return (Brush)base.GetValue(ToggleSwitchControl.SwitchForegroundProperty);
            }
            set
            {
                base.SetValue(ToggleSwitchControl.SwitchForegroundProperty, value);
            }
        }

        public Brush Fill
        {
            get
            {
                return (Brush)base.GetValue(ToggleSwitchControl.FillProperty);
            }
            set
            {
                base.SetValue(ToggleSwitchControl.FillProperty, value);
            }
        }

        private double Translation
        {
            get
            {
                return this._thumbTranslation.X;
            }
            set
            {
                if (this._thumbTranslation != null)
                    this._thumbTranslation.X = value;
                if (this._borderForeground == null || this._checkedTranslation == 0.0)
                    return;
                ((UIElement)this._borderForeground).Opacity = (value / this._checkedTranslation);
            }
        }

        public ToggleSwitchControl()
        {
            //base.\u002Ector();
            base.DefaultStyleKey = (typeof(ToggleSwitchControl));
        }

        private void ChangeVisualState(bool useTransitions)
        {
            bool? isChecked = this.IsChecked;
            if ((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                VisualStateManager.GoToState((Control)this, "Checked", useTransitions);
            else
                VisualStateManager.GoToState((Control)this, "Unchecked", useTransitions);
        }

        protected override void OnToggle()
        {
            bool? isChecked = this.IsChecked;
            this.IsChecked = (new bool?((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 0) == 0));
            this.ChangeVisualState(true);
        }

        public override void OnApplyTemplate()
        {
            if (this._track != null)
            {
                // ISSUE: method pointer
                ((FrameworkElement)this._track).SizeChanged -= (new SizeChangedEventHandler(this.OnSizeChanged));
            }
            if (this._thumb != null)
            {
                // ISSUE: method pointer
                this._thumb.SizeChanged -= (new SizeChangedEventHandler(this.OnSizeChanged));
            }
            if (this._root != null)
            {
                ((UIElement)this._root).ManipulationStarted -= (new EventHandler<ManipulationStartedEventArgs>(this.OnManipulationStarted));
                ((UIElement)this._root).ManipulationDelta -= (new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta));
                ((UIElement)this._root).ManipulationCompleted -= (new EventHandler<ManipulationCompletedEventArgs>(this.OnManipulationCompleted));
            }
            base.OnApplyTemplate();
            this._root = base.GetTemplateChild("SwitchRoot") as Grid;
            this._borderForeground = base.GetTemplateChild("BorderSwitchForeground") as Border;
            this._track = base.GetTemplateChild("SwitchTrack") as Grid;
            this._thumb = base.GetTemplateChild("SwitchThumb") as FrameworkElement;
            FrameworkElement thumb = this._thumb;
            this._thumbTranslation = (thumb != null ? ((UIElement)thumb).RenderTransform : null) as TranslateTransform;
            if (this._root != null && this._track != null && (this._thumb != null && this._thumbTranslation != null))
            {
                ((UIElement)this._root).ManipulationStarted += (new EventHandler<ManipulationStartedEventArgs>(this.OnManipulationStarted));
                ((UIElement)this._root).ManipulationDelta += (new EventHandler<ManipulationDeltaEventArgs>(this.OnManipulationDelta));
                ((UIElement)this._root).ManipulationCompleted += (new EventHandler<ManipulationCompletedEventArgs>(this.OnManipulationCompleted));
                // ISSUE: method pointer
                ((FrameworkElement)this._track).SizeChanged += (new SizeChangedEventHandler(this.OnSizeChanged));
                // ISSUE: method pointer
                this._thumb.SizeChanged += (new SizeChangedEventHandler(this.OnSizeChanged));
            }
            this.ChangeVisualState(false);
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
            this._dragTranslation = this.Translation;
            this.Translation = this._dragTranslation;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            Point translation1 = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            double x = translation1.X;
            double num1 = Math.Abs(x);
            Point translation2 = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            double num2 = Math.Abs(translation2.Y);
            if ((num1 >= num2 ? 1 : 0) != 1 || x == 0.0)
                return;
            this._wasDragged = true;
            this._dragTranslation = this._dragTranslation + x;
            this.Translation = Math.Max(0.0, Math.Min(this._checkedTranslation, this._dragTranslation));
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            bool flag = false;
            if (this._wasDragged)
            {
                bool? isChecked = this.IsChecked;
                if (this.Translation != ((isChecked.HasValue ? (isChecked.GetValueOrDefault() ? 1 : 0) : 0) != 0 ? this._checkedTranslation : 0.0))
                    flag = true;
            }
            else
                flag = true;
            if (flag)
                base.OnClick();
            this._wasDragged = false;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid track = this._track;
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            Rect rect = new Rect(0.0, 0.0, ((FrameworkElement)this._track).ActualWidth, ((FrameworkElement)this._track).ActualHeight);
            rectangleGeometry.Rect = rect;
            ((UIElement)track).Clip = ((Geometry)rectangleGeometry);
            double num1 = ((FrameworkElement)this._track).ActualWidth - this._thumb.ActualWidth;
            Thickness margin1 = this._thumb.Margin;
            // ISSUE: explicit reference operation
            double left = margin1.Left;
            double num2 = num1 - left;
            Thickness margin2 = this._thumb.Margin;
            // ISSUE: explicit reference operation
            double right = margin2.Right;
            this._checkedTranslation = num2 - right;
        }
    }
}
