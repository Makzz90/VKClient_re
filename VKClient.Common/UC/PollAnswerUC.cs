using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VKClient.Common.UC
{
  public class PollAnswerUC : UserControl
  {
      public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(PollAnswerUC), new PropertyMetadata(new PropertyChangedCallback(PollAnswerUC.Value_OnChanged)));
    internal Storyboard StoryboardAnimationUpdateValue;
    internal DoubleAnimation AnimationUpdateValue;
    internal RectangleGeometry clipRectangleFill;
    internal TranslateTransform transformRectangleFill;
    private bool _contentLoaded;

    public double Value
    {
      get
      {
        return (double) base.GetValue(PollAnswerUC.ValueProperty);
      }
      set
      {
        base.SetValue(PollAnswerUC.ValueProperty, value);
      }
    }

    public PollAnswerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.SizeChanged+=(delegate(object sender, SizeChangedEventArgs args)
      {
          this.clipRectangleFill.Rect=(new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight));
          this.UpdateValue(false);
      });
    }

    private static void Value_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((PollAnswerUC) d).UpdateValue(true);
    }

    private void UpdateValue(bool animated = true)
    {
      double num1 = base.ActualWidth * this.Value / 100.0;
      double num2 = -base.ActualWidth + num1;
      if (num1 == 0.0 || !animated)
      {
        this.transformRectangleFill.X = num2;
      }
      else
      {
        this.transformRectangleFill.X=(-base.ActualWidth);
        this.AnimationUpdateValue.To=(new double?(num2));
        this.StoryboardAnimationUpdateValue.Begin();
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/PollAnswerUC.xaml", UriKind.Relative));
      this.StoryboardAnimationUpdateValue = (Storyboard) base.FindName("StoryboardAnimationUpdateValue");
      this.AnimationUpdateValue = (DoubleAnimation) base.FindName("AnimationUpdateValue");
      this.clipRectangleFill = (RectangleGeometry) base.FindName("clipRectangleFill");
      this.transformRectangleFill = (TranslateTransform) base.FindName("transformRectangleFill");
    }
  }
}
