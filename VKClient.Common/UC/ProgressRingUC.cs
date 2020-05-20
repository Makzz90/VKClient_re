using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VKClient.Common.UC
{
  public class ProgressRingUC : UserControl
  {
      public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(ProgressRingUC), new PropertyMetadata(0.0, new PropertyChangedCallback(ProgressRingUC.Progress_OnChanged)));
    private const double RADIUS = 26.0;
    internal ProgressRing progressRing;
    internal ArcSegment arcProgress;
    private bool _contentLoaded;

    public double Progress
    {
      get
      {
        return (double) base.GetValue(ProgressRingUC.ProgressProperty);
      }
      set
      {
        base.SetValue(ProgressRingUC.ProgressProperty, value);
      }
    }

    public ProgressRingUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.progressRing.IsActive = true;
    }

    private static void Progress_OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      double num1 = (double) e.NewValue;
      if (num1 < 0.0)
        num1 = 0.0;
      else if (num1 > 100.0)
        num1 = 100.0;
      ProgressRingUC progressRingUc = (ProgressRingUC) obj;
      double num2 = num1 / 100.0;
      progressRingUc.progressRing.IsActive = num2 == 0.0;
      progressRingUc.arcProgress.Point=(new Point(26.0 * Math.Sin(2.0 * Math.PI * num2), 26.0 * (1.0 - Math.Cos(2.0 * Math.PI * num2))));
      progressRingUc.arcProgress.IsLargeArc=(num1 >= 50.0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ProgressRingUC.xaml", UriKind.Relative));
      this.progressRing = (ProgressRing) base.FindName("progressRing");
      this.arcProgress = (ArcSegment) base.FindName("arcProgress");
    }
  }
}
