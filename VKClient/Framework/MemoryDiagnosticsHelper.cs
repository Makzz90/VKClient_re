using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.System;

namespace VKClient.Framework
{
  public static class MemoryDiagnosticsHelper
  {
    private static int lastSafetyBand = -1;
    private static bool alreadyFailedPeak = false;
    private static Popup popup;
    private static TextBlock currentMemoryBlock;
    private static TextBlock peakMemoryBlock;
    private static DispatcherTimer timer;
    private static bool forceGc;
    private const long MAX_MEMORY = 209715200;
    private const long MAX_CHECKPOINTS = 10;
    private static Queue<MemoryCheckpoint> recentCheckpoints;

    public static IEnumerable<MemoryCheckpoint> RecentCheckpoints
    {
      get
      {
        if (MemoryDiagnosticsHelper.recentCheckpoints != null)
        {
          foreach (MemoryCheckpoint recentCheckpoint in MemoryDiagnosticsHelper.recentCheckpoints)
            yield return recentCheckpoint;
      //    Queue<MemoryCheckpoint>.Enumerator enumerator = new Queue<MemoryCheckpoint>.Enumerator();
        }
      }
    }

    public static void Start(TimeSpan timespan, bool forceGc)
    {
      if (MemoryDiagnosticsHelper.timer != null)
        throw new InvalidOperationException("Diagnostics already running");
      MemoryDiagnosticsHelper.forceGc = forceGc;
      MemoryDiagnosticsHelper.recentCheckpoints = new Queue<MemoryCheckpoint>();
      MemoryDiagnosticsHelper.StartTimer(timespan);
      MemoryDiagnosticsHelper.ShowPopup();
    }

    public static void Stop()
    {
      MemoryDiagnosticsHelper.HidePopup();
      MemoryDiagnosticsHelper.StopTimer();
      MemoryDiagnosticsHelper.recentCheckpoints =  null;
    }

    public static void Checkpoint(string text)
    {
      if (MemoryDiagnosticsHelper.recentCheckpoints == null)
        return;
      if ((long) MemoryDiagnosticsHelper.recentCheckpoints.Count >= 9L)
        MemoryDiagnosticsHelper.recentCheckpoints.Dequeue();
      MemoryDiagnosticsHelper.recentCheckpoints.Enqueue(new MemoryCheckpoint(text, MemoryDiagnosticsHelper.GetCurrentMemoryUsage()));
    }

    public static long GetCurrentMemoryUsage()
    {
      return (long) MemoryManager.AppMemoryUsage;
    }

    public static long GetPeakMemoryUsage()
    {
      return 0;
    }

    private static void ShowPopup()
    {
      MemoryDiagnosticsHelper.popup = new Popup();
      double num1 = (double) Application.Current.Resources["PhoneFontSizeSmall"] - 2.0;
      Brush brush1 = (Brush) Application.Current.Resources["PhoneForegroundBrush"];
      StackPanel stackPanel1 = new StackPanel();
      int num2 = 1;
      stackPanel1.Orientation=((Orientation) num2);
      Brush brush2 = (Brush) Application.Current.Resources["PhoneSemitransparentBrush"];
      ((Panel) stackPanel1).Background = brush2;
      int num3 = 0;
      ((UIElement) stackPanel1).IsHitTestVisible=(num3 != 0);
      StackPanel stackPanel2 = stackPanel1;
      TextBlock textBlock1 = new TextBlock();
      string str1 = "---";
      textBlock1.Text = str1;
      double num4 = num1;
      textBlock1.FontSize = num4;
      Brush brush3 = brush1;
      textBlock1.Foreground = brush3;
      MemoryDiagnosticsHelper.currentMemoryBlock = textBlock1;
      TextBlock textBlock2 = new TextBlock();
      string str2 = "";
      textBlock2.Text = str2;
      double num5 = num1;
      textBlock2.FontSize = num5;
      Brush brush4 = brush1;
      textBlock2.Foreground = brush4;
      Thickness thickness = new Thickness(5.0, 0.0, 0.0, 0.0);
      ((FrameworkElement) textBlock2).Margin = thickness;
      MemoryDiagnosticsHelper.peakMemoryBlock = textBlock2;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Add((UIElement) MemoryDiagnosticsHelper.currentMemoryBlock);
      UIElementCollection children = ((Panel) stackPanel2).Children;
      TextBlock textBlock3 = new TextBlock();
      string str3 = " kb";
      textBlock3.Text = str3;
      double num6 = num1;
      textBlock3.FontSize = num6;
      Brush brush5 = brush1;
      textBlock3.Foreground = brush5;
      ((PresentationFrameworkCollection<UIElement>) children).Add((UIElement) textBlock3);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Add((UIElement) MemoryDiagnosticsHelper.peakMemoryBlock);
      StackPanel stackPanel3 = stackPanel2;
      CompositeTransform compositeTransform = new CompositeTransform();
      double num7 = 90.0;
      compositeTransform.Rotation = num7;
      double num8 = 480.0;
      compositeTransform.TranslateX = num8;
      double num9 = 425.0;
      compositeTransform.TranslateY = num9;
      double num10 = 0.0;
      compositeTransform.CenterX = num10;
      double num11 = 0.0;
      compositeTransform.CenterY = num11;
      ((UIElement) stackPanel3).RenderTransform = ((Transform) compositeTransform);
      MemoryDiagnosticsHelper.popup.Child = ((UIElement) stackPanel2);
      ((UIElement) MemoryDiagnosticsHelper.popup).IsHitTestVisible = false;
      MemoryDiagnosticsHelper.popup.IsOpen = true;
    }

    private static void StartTimer(TimeSpan timespan)
    {
      MemoryDiagnosticsHelper.timer = new DispatcherTimer();
      MemoryDiagnosticsHelper.timer.Interval = timespan;
      MemoryDiagnosticsHelper.timer.Tick+=(new EventHandler(MemoryDiagnosticsHelper.timer_Tick));
      MemoryDiagnosticsHelper.timer.Start();
    }

    private static void timer_Tick(object sender, EventArgs e)
    {
      if (MemoryDiagnosticsHelper.forceGc)
        GC.Collect();
      MemoryDiagnosticsHelper.UpdateCurrentMemoryUsage();
      MemoryDiagnosticsHelper.UpdatePeakMemoryUsage();
    }

    private static void UpdatePeakMemoryUsage()
    {
      if (MemoryDiagnosticsHelper.alreadyFailedPeak || MemoryDiagnosticsHelper.GetPeakMemoryUsage() < 209715200L)
        return;
      MemoryDiagnosticsHelper.alreadyFailedPeak = true;
      MemoryDiagnosticsHelper.Checkpoint("*MEMORY USAGE FAIL*");
      MemoryDiagnosticsHelper.peakMemoryBlock.Text = ("FAIL!");
      MemoryDiagnosticsHelper.peakMemoryBlock.Foreground = ((Brush) new SolidColorBrush(Colors.Red));
      int num = Debugger.IsAttached ? 1 : 0;
    }

    private static void UpdateCurrentMemoryUsage()
    {
      long currentMemoryUsage = MemoryDiagnosticsHelper.GetCurrentMemoryUsage();
      MemoryDiagnosticsHelper.currentMemoryBlock.Text = (string.Format("{0:N}", (currentMemoryUsage / 1024L)));
      int safetyBand = MemoryDiagnosticsHelper.GetSafetyBand(currentMemoryUsage);
      if (safetyBand == MemoryDiagnosticsHelper.lastSafetyBand)
        return;
      MemoryDiagnosticsHelper.currentMemoryBlock.Foreground = (MemoryDiagnosticsHelper.GetBrushForSafetyBand(safetyBand));
      MemoryDiagnosticsHelper.lastSafetyBand = safetyBand;
    }

    private static Brush GetBrushForSafetyBand(int safetyBand)
    {
      if (safetyBand == 0)
        return (Brush) new SolidColorBrush(Colors.Green);
      if (safetyBand == 1)
        return (Brush) new SolidColorBrush(Colors.Orange);
      return (Brush) new SolidColorBrush(Colors.Red);
    }

    private static int GetSafetyBand(long mem)
    {
      double num = (double) mem / 209715200.0;
      if (num <= 0.75)
        return 0;
      return num <= 0.9 ? 1 : 2;
    }

    private static void StopTimer()
    {
      MemoryDiagnosticsHelper.timer.Stop();
      MemoryDiagnosticsHelper.timer =  null;
    }

    private static void HidePopup()
    {
      MemoryDiagnosticsHelper.popup.IsOpen = false;
      MemoryDiagnosticsHelper.popup =  null;
    }
  }
}
