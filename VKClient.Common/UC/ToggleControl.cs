using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.Extensions;

namespace VKClient.Common.UC
{
  public class ToggleControl : UserControl
  {
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof (bool), typeof (ToggleControl), new PropertyMetadata(new PropertyChangedCallback(ToggleControl.IsChecked_OnChanged)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ToggleControl), new PropertyMetadata(new PropertyChangedCallback(ToggleControl.Title_OnChanged)));
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register("TitleFontSize", typeof (double), typeof (ToggleControl), new PropertyMetadata(new PropertyChangedCallback((d,e)=>((ToggleControl)d).UpdateTitleFontSize())));
    internal QuadraticEase EasingFunc;
    internal Storyboard AnimateChecked;
    internal Storyboard AnimateUnchecked;
    internal TextBlock textBlockTitle;
    //internal ToggleSwitchControl controlToggleSwitch;
    internal VKClient.Common.UC.w10m.ToggleSwitch controlToggleSwitch;
    private bool _contentLoaded;

    public bool IsChecked
    {
      get
      {
        return (bool) base.GetValue(ToggleControl.IsCheckedProperty);
      }
      set
      {
        base.SetValue(ToggleControl.IsCheckedProperty, value);
      }
    }

    public string Title
    {
      get
      {
        return (string) base.GetValue(ToggleControl.TitleProperty);
      }
      set
      {
        base.SetValue(ToggleControl.TitleProperty, value);
      }
    }

    public double TitleFontSize
    {
      get
      {
        return (double) base.GetValue(ToggleControl.TitleFontSizeProperty);
      }
      set
      {
        base.SetValue(ToggleControl.TitleFontSizeProperty, value);
      }
    }

    public event EventHandler<bool> CheckedUnchecked;

    public ToggleControl()
    {
        this.InitializeComponent();
        this.textBlockTitle.Text = "";
        this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args)
    {
      Size newSize = args.NewSize;
      // ISSUE: explicit reference operation
      double width = ((Size) @newSize).Width;
      if (double.IsNaN(width) || double.IsInfinity(width))
        return;
      this.textBlockTitle.Text = this.Title;
      this.textBlockTitle.CorrectText(Math.Max(0.0, width - ((FrameworkElement) this.controlToggleSwitch).Width));
    }

    private static void IsChecked_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ToggleControl toggleControl = (ToggleControl) d;
      toggleControl.UpdateToggle();
      toggleControl.FireCheckedEvent();
    }

    private void UpdateToggle()
    {
       // this.controlToggleSwitch.Checked -= this.ControlToggleSwitch_OnCheckedUnchecked;
      //  this.controlToggleSwitch.Unchecked -= new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
        this.controlToggleSwitch.IsChecked = /*new bool?*/(this.IsChecked);
       // this.controlToggleSwitch.Checked += new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
       // this.controlToggleSwitch.Unchecked += new RoutedEventHandler(this.ControlToggleSwitch_OnCheckedUnchecked);
    }

    private void FireCheckedEvent()
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<bool> checkedUnchecked = this.CheckedUnchecked;
      if (checkedUnchecked == null)
        return;
      int num = this.IsChecked ? 1 : 0;
      checkedUnchecked(this, num != 0);
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((ToggleControl) d).UpdateTitle();
    }

    private void UpdateTitle()
    {
      this.textBlockTitle.Text = this.Title;
      if (double.IsNaN(base.ActualWidth) || double.IsInfinity(base.ActualWidth))
        return;
      this.textBlockTitle.CorrectText(Math.Max(0.0, base.ActualWidth - ((FrameworkElement) this.controlToggleSwitch).Width));
    }

    private void UpdateTitleFontSize()
    {
      this.textBlockTitle.FontSize = this.TitleFontSize;
    }

    private void BorderToggleTitle_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.IsChecked = !this.IsChecked;
    }

    private void ControlToggleSwitch_OnCheckedUnchecked(object sender, RoutedEventArgs e)
    {
      bool? isChecked = this.controlToggleSwitch.IsChecked;
      this.IsChecked = isChecked.HasValue && isChecked.GetValueOrDefault();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ToggleControl.xaml", UriKind.Relative));
      this.EasingFunc = (QuadraticEase) base.FindName("EasingFunc");
      this.AnimateChecked = (Storyboard) base.FindName("AnimateChecked");
      this.AnimateUnchecked = (Storyboard) base.FindName("AnimateUnchecked");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.controlToggleSwitch = (VKClient.Common.UC.w10m.ToggleSwitch)/*(ToggleSwitchControl)*/ base.FindName("controlToggleSwitch");
    }
  }
}
