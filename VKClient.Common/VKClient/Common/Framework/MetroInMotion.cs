using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VKClient.Common.Framework
{
  public static class MetroInMotion
  {
    public static readonly DependencyProperty AnimationLevelProperty = DependencyProperty.RegisterAttached("AnimationLevel", typeof (int), typeof (MetroInMotion), new PropertyMetadata((object) -1));
    public static readonly DependencyProperty TiltProperty = DependencyProperty.RegisterAttached("Tilt", typeof (double), typeof (MetroInMotion), new PropertyMetadata((object) 2.0, new PropertyChangedCallback(MetroInMotion.OnTiltChanged)));
    private static double TiltAngleFactor = 4.0;
    private static double ScaleFactor = 100.0;
    public static readonly DependencyProperty IsPivotAnimatedProperty = DependencyProperty.RegisterAttached("IsPivotAnimated", typeof (bool), typeof (MetroInMotion), new PropertyMetadata((object) false, new PropertyChangedCallback(MetroInMotion.OnIsPivotAnimatedChanged)));

    public static int GetAnimationLevel(DependencyObject obj)
    {
      return (int) obj.GetValue(MetroInMotion.AnimationLevelProperty);
    }

    public static void SetAnimationLevel(DependencyObject obj, int value)
    {
      obj.SetValue(MetroInMotion.AnimationLevelProperty, (object) value);
    }

    public static double GetTilt(DependencyObject obj)
    {
      return (double) obj.GetValue(MetroInMotion.TiltProperty);
    }

    public static void SetTilt(DependencyObject obj, double value)
    {
      obj.SetValue(MetroInMotion.TiltProperty, (object) value);
    }

    private static void OnTiltChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      FrameworkElement targetElement = d as FrameworkElement;
      double tiltFactor = MetroInMotion.GetTilt(d);
      PlaneProjection projection = new PlaneProjection();
      ScaleTransform scale = new ScaleTransform();
      TranslateTransform translate = new TranslateTransform();
      TransformGroup transformGroup = new TransformGroup();
      transformGroup.Children.Add((Transform) scale);
      transformGroup.Children.Add((Transform) translate);
      targetElement.Projection = (Projection) projection;
      targetElement.RenderTransform = (Transform) transformGroup;
      targetElement.RenderTransformOrigin = new Point(0.5, 0.5);
      MouseButtonEventHandler buttonEventHandler = (MouseButtonEventHandler) ((s, e) =>
      {
        Point position = e.GetPosition((UIElement) targetElement);
        double num1 = Math.Max(targetElement.ActualWidth, targetElement.ActualHeight);
        if (num1 == 0.0)
          return;
        double num2 = 2.0 * (targetElement.ActualWidth / 2.0 - position.X) / num1;
        projection.RotationY = num2 * MetroInMotion.TiltAngleFactor * tiltFactor;
        double num3 = 2.0 * (targetElement.ActualHeight / 2.0 - position.Y) / num1;
        projection.RotationX = -num3 * MetroInMotion.TiltAngleFactor * tiltFactor;
        double num4 = tiltFactor * (1.0 - Math.Sqrt(num2 * num2 + num3 * num3)) / MetroInMotion.ScaleFactor;
        scale.ScaleX = 1.0 - num4;
        scale.ScaleY = 1.0 - num4;
        FrameworkElement frameworkElement = Application.Current.RootVisual as FrameworkElement;
        double num5 = (targetElement.GetRelativePosition((UIElement) frameworkElement).Y - frameworkElement.ActualHeight / 2.0) / 2.0;
        translate.Y = -num5;
        projection.LocalOffsetY = num5;
      });
      EventHandler<ManipulationCompletedEventArgs> eventHandler = (EventHandler<ManipulationCompletedEventArgs>) ((s, e) =>
      {
        new Storyboard()
        {
          Children = {
            (Timeline) MetroInMotion.CreateAnimation(new double?(), new double?(0.0), 0.1, "RotationY", (DependencyObject) projection),
            (Timeline) MetroInMotion.CreateAnimation(new double?(), new double?(0.0), 0.1, "RotationX", (DependencyObject) projection),
            (Timeline) MetroInMotion.CreateAnimation(new double?(), new double?(1.0), 0.1, "ScaleX", (DependencyObject) scale),
            (Timeline) MetroInMotion.CreateAnimation(new double?(), new double?(1.0), 0.1, "ScaleY", (DependencyObject) scale)
          }
        }.Begin();
        translate.Y = 0.0;
        projection.LocalOffsetY = 0.0;
      });
      targetElement.MouseLeftButtonDown -= buttonEventHandler;
      targetElement.ManipulationCompleted -= eventHandler;
      if (tiltFactor <= 0.0)
        return;
      targetElement.MouseLeftButtonDown += buttonEventHandler;
      targetElement.ManipulationCompleted += eventHandler;
    }

    public static bool GetIsPivotAnimated(DependencyObject obj)
    {
      return (bool) obj.GetValue(MetroInMotion.IsPivotAnimatedProperty);
    }

    public static void SetIsPivotAnimated(DependencyObject obj, bool value)
    {
      obj.SetValue(MetroInMotion.IsPivotAnimatedProperty, (object) value);
    }

    private static void OnIsPivotAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      ItemsControl list = d as ItemsControl;
      list.Loaded += (RoutedEventHandler) ((s2, e2) =>
      {
        Pivot pivot = list.Ancestors<Pivot>().Single<DependencyObject>() as Pivot;
        int pivotIndex = pivot.Items.IndexOf((object) list.Ancestors<PivotItem>().Single<DependencyObject>());
        bool selectionChanged = false;
        pivot.SelectionChanged += (SelectionChangedEventHandler) ((s3, e3) => selectionChanged = true);
        pivot.ManipulationCompleted += (EventHandler<ManipulationCompletedEventArgs>) ((s, e) =>
        {
          if (!selectionChanged)
            return;
          selectionChanged = false;
          if (pivotIndex != pivot.SelectedIndex)
            return;
          bool fromRight = e.TotalManipulation.Translation.X <= 0.0;
          List<FrameworkElement> list1 = list.GetItemsInView().ToList<FrameworkElement>();
          for (int index = 0; index < list1.Count; ++index)
          {
            FrameworkElement lbi = list1[index];
            list.Dispatcher.BeginInvoke((Action) (() =>
            {
              foreach (FrameworkElement element in lbi.Descendants().Where<DependencyObject>((Func<DependencyObject, bool>) (p => MetroInMotion.GetAnimationLevel(p) > -1)))
                MetroInMotion.GetSlideAnimation(element, fromRight).Begin();
            }));
          }
        });
      });
    }

    public static void Peel(this IEnumerable<FrameworkElement> elements, Action endAction)
    {
      List<FrameworkElement> list = elements.ToList<FrameworkElement>();
      FrameworkElement frameworkElement1 = list.Last<FrameworkElement>();
      double num = 0.0;
      foreach (FrameworkElement element in list)
      {
        double delay = num;
        Storyboard peelAnimation = MetroInMotion.GetPeelAnimation(element, delay);
        FrameworkElement frameworkElement2 = frameworkElement1;
        if (element.Equals((object) frameworkElement2))
          peelAnimation.Completed += (EventHandler) ((s, e) => endAction());
        peelAnimation.Begin();
        num += 50.0;
      }
    }

    public static IEnumerable<FrameworkElement> GetItemsInView(this ItemsControl itemsControl)
    {
      VirtualizingStackPanel virtualizingStackPanel = itemsControl.Descendants<VirtualizingStackPanel>().First<DependencyObject>() as VirtualizingStackPanel;
      int firstVisibleItem = (int) virtualizingStackPanel.VerticalOffset;
      int visibleItemCount = (int) virtualizingStackPanel.ViewportHeight;
      for (int index = firstVisibleItem; index <= firstVisibleItem + visibleItemCount + 1; ++index)
      {
        FrameworkElement frameworkElement = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
        if (frameworkElement != null)
          yield return frameworkElement;
      }
    }

    private static Storyboard GetPeelAnimation(FrameworkElement element, double delay)
    {
      PlaneProjection planeProjection = new PlaneProjection()
      {
        CenterOfRotationX = -0.1
      };
      element.Projection = (Projection) planeProjection;
      double num = Math.Atan(1000.0 / (element.ActualWidth / 2.0)) * 180.0 / Math.PI;
      Storyboard storyboard = new Storyboard();
      TimeSpan? nullable = new TimeSpan?(TimeSpan.FromMilliseconds(delay));
      storyboard.BeginTime = nullable;
      storyboard.Children.Add((Timeline) MetroInMotion.CreateAnimation(new double?(0.0), new double?(-(180.0 - num)), 0.3, "RotationY", (DependencyObject) planeProjection));
      storyboard.Children.Add((Timeline) MetroInMotion.CreateAnimation(new double?(0.0), new double?(23.0), 0.3, "RotationZ", (DependencyObject) planeProjection));
      storyboard.Children.Add((Timeline) MetroInMotion.CreateAnimation(new double?(0.0), new double?(-23.0), 0.3, "GlobalOffsetZ", (DependencyObject) planeProjection));
      return storyboard;
    }

    private static DoubleAnimation CreateAnimation(double? from, double? to, double duration, string targetProperty, DependencyObject target)
    {
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.To = to;
      doubleAnimation.From = from;
      doubleAnimation.EasingFunction = (IEasingFunction) new SineEase();
      doubleAnimation.Duration = (Duration) TimeSpan.FromSeconds(duration);
      Storyboard.SetTarget((Timeline) doubleAnimation, target);
      Storyboard.SetTargetProperty((Timeline) doubleAnimation, new PropertyPath(targetProperty, new object[0]));
      return doubleAnimation;
    }

    private static Storyboard GetSlideAnimation(FrameworkElement element, bool fromRight)
    {
      double num1 = fromRight ? 80.0 : -80.0;
      double num2 = (double) MetroInMotion.GetAnimationLevel((DependencyObject) element) * 0.1 + 0.1;
      TranslateTransform translateTransform = new TranslateTransform()
      {
        X = num1
      };
      element.RenderTransform = (Transform) translateTransform;
      Storyboard storyboard = new Storyboard();
      TimeSpan? nullable = new TimeSpan?(TimeSpan.FromSeconds(num2));
      storyboard.BeginTime = nullable;
      storyboard.Children.Add((Timeline) MetroInMotion.CreateAnimation(new double?(num1), new double?(0.0), 0.8, "X", (DependencyObject) translateTransform));
      return storyboard;
    }
  }
}
