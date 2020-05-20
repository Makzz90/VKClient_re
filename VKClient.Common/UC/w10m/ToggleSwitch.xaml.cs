using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Input;

namespace VKClient.Common.UC.w10m
{
    public partial class ToggleSwitch : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.IsChecked_OnChanged)));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.Title_OnChanged)));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.Description_OnChanged)));
        public static readonly DependencyProperty StateTextOnProperty = DependencyProperty.Register("StateTextOn", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.StateTextOn_OnChanged)));
        public static readonly DependencyProperty StateTextOffProperty = DependencyProperty.Register("StateTextOff", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.StateTextOff_OnChanged)));
        //public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.BackgroundBrush_OnChanged)));
        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.BorderColor_OnChanged)));
        //public static readonly DependencyProperty KnobColorProperty = DependencyProperty.Register("KnobColor", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.KnobColor_OnChanged)));
        //public static readonly DependencyProperty IsStateTextVisibleProperty = DependencyProperty.Register("IsStateTextVisible", typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(new PropertyChangedCallback(ToggleSwitch.IsStateTextVisible_OnChanged)));

        private Storyboard _storyboard;
        double total_width;
        public event EventHandler<RoutedEventArgs> Checked;

        public ToggleSwitch()
        {
            InitializeComponent();

            total_width = SwitchBackground.Width - SwitchKnobBounds.Width;

            _storyboard = new Storyboard();

            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            _storyboard.Children.Add(myDoubleAnimation1);
            Storyboard.SetTarget(myDoubleAnimation1, this.SwitchKnobBounds.RenderTransform);

            PropertyPath path1 = new PropertyPath(CompositeTransform.TranslateXProperty);
            Storyboard.SetTargetProperty(myDoubleAnimation1, path1);
            //myDoubleAnimation1.BeginTime = TimeSpan.FromSeconds(0);
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            AnimateChecked.Completed += myDoubleAnimation1_Completed;
        }

        public bool IsChecked
        {
            get
            {
                return (bool)base.GetValue(ToggleSwitch.IsCheckedProperty);
            }
            set
            {
                base.SetValue(ToggleSwitch.IsCheckedProperty, value);
            }
        }

        public bool IsStateTextVisible
        {
            get
            {
                //return (bool)base.GetValue(ToggleSwitch.IsStateTextVisibleProperty);
                return this.TextState.Visibility == Visibility.Visible;
            }
            set
            {
                this.TextState.Visibility = (bool)value ? Visibility.Visible : Visibility.Collapsed;
                //base.SetValue(ToggleSwitch.IsStateTextVisibleProperty, value);
            }
        }

        public string Title
        {
            get
            {
                return (string)base.GetValue(ToggleSwitch.TitleProperty);
            }
            set
            {
                base.SetValue(ToggleSwitch.TitleProperty, value);
            }
        }

        public string Description
        {
            get
            {
                return (string)base.GetValue(ToggleSwitch.DescriptionProperty);
            }
            set
            {
                base.SetValue(ToggleSwitch.DescriptionProperty, value);
            }
        }

        public string StateTextOn
        {
            get
            {
                return (string)base.GetValue(ToggleSwitch.StateTextOnProperty);
            }
            set
            {
                base.SetValue(ToggleSwitch.StateTextOnProperty, value);
            }
        }

        public string StateTextOff
        {
            get
            {
                return (string)base.GetValue(ToggleSwitch.StateTextOffProperty);
            }
            set
            {
                base.SetValue(ToggleSwitch.StateTextOffProperty, value);
            }
        }

        public Brush BorderColor
        {
            get
            {
                return (Brush)base.GetValue(ToggleSwitch.BorderColorProperty);
            }
            set
            {
                base.SetValue(ToggleSwitch.BorderColorProperty, value);
            }
        }

        private static void IsChecked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleControl = (ToggleSwitch)d;
            toggleControl.FireCheckedEvent();
            if ((bool)e.NewValue == true)
            {
                toggleControl.MoveLeft(toggleControl.total_width);
                toggleControl.UpdateOpacity(1);
                toggleControl.TextStateOn.Visibility = Visibility.Visible;
                toggleControl.TextStateOff.Visibility = Visibility.Collapsed;
            }
            else
            {
                toggleControl.MoveLeft(0);
                toggleControl.UpdateOpacity(0);
                toggleControl.TextStateOn.Visibility = Visibility.Collapsed;
                toggleControl.TextStateOff.Visibility = Visibility.Visible;
            }
        }

        private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleControl = (ToggleSwitch)d;
            toggleControl.textBlockTitle.Text = toggleControl.Title;
            toggleControl.textBlockTitle.Visibility = string.IsNullOrEmpty(toggleControl.Title) ? Visibility.Collapsed : Visibility.Visible;
        }

        private static void Description_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleControl = (ToggleSwitch)d;
            toggleControl.textBlockDescription.Text = toggleControl.Description;
            if (string.IsNullOrEmpty(toggleControl.Description))//нет описания
            {
                toggleControl.textBlockDescription.Visibility = Visibility.Collapsed;
                toggleControl.textBlockTitle.FontWeight = FontWeights.Normal;
            }
            else
            {
                toggleControl.textBlockDescription.Visibility = Visibility.Visible;
                toggleControl.textBlockTitle.FontWeight = FontWeights.Medium;
            }

        }

        private static void StateTextOn_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleControl = (ToggleSwitch)d;
            toggleControl.TextStateOn.Text = toggleControl.StateTextOn;
        }

        private static void StateTextOff_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleControl = (ToggleSwitch)d;
            toggleControl.TextStateOff.Text = toggleControl.StateTextOff;
        }

        private static void BorderColor_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleControl = d as ToggleSwitch;
            Brush newValue = e.NewValue as Brush;
            toggleControl.SwitchBackground.Fill = toggleControl.OuterBorder.Stroke = toggleControl.SwitchKnobOff.Fill = newValue;
        }


        private void FireCheckedEvent()
        {
            if (this.Checked == null)
                return;

            this.Checked(this, new RoutedEventArgs());
        }

        void myDoubleAnimation1_Completed(object sender, EventArgs e)
        {
            this.IsChecked = this.SwitchBackground.Opacity == 1.0;
            //System.Diagnostics.Debug.WriteLine("Completed {0}", this.IsChecked ? "Checked" : "UnChecked");
            //this.FireCheckedEvent();
        }

        private void ForeGroundCellGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            double main_left = Canvas.GetLeft(this.Content);
            double finalLeft = main_left + e.DeltaManipulation.Translation.X;
            //double total_width = SwitchBackground.Width - SwitchKnobBounds.Width;
            if (finalLeft > total_width || finalLeft <= 0)
                return;

            double newop = finalLeft / this.total_width;
            UpdateOpacity(newop);


            MoveLeft(finalLeft);

            if (e.IsInertial)
            {
                e.Complete();
            }
        }

        private void UpdateOpacity(double newop)
        {
            this.SwitchKnobOn.Opacity = this.SwitchBackground.Opacity = Math.Abs(newop);
        }

        public void MoveLeft(double left, bool fast = false)
        {
            double curLeft = Canvas.GetLeft(this.Content);

            Storyboard anim = _storyboard;
            DoubleAnimation direction = (DoubleAnimation)anim.Children[0];

            direction.From = curLeft;

            anim.SkipToFill();

            direction.To = left;

            _storyboard.Begin();


            Canvas.SetLeft(this.Content, left);
        }

        private void ForeGroundCellGrid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            AnimateOp.From = AnimateCheckedAnim.From = this.SwitchBackground.Opacity;

            if (e.TotalManipulation.Translation.X > this.total_width / 2.0)
            {
                MoveLeft(this.total_width);
                AnimateOp.To = AnimateCheckedAnim.To = 1;
            }
            else
            {
                MoveLeft(0);
                AnimateOp.To = AnimateCheckedAnim.To = 0;
            }

            AnimateChecked.Begin();
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            this.IsChecked = !this.IsChecked;
        }
    }
}
