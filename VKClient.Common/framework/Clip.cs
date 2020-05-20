using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public class Clip
  {
      public static readonly DependencyProperty ToBoundsProperty = DependencyProperty.RegisterAttached("ToBounds", typeof(bool), typeof(Clip), new PropertyMetadata(false, new PropertyChangedCallback(Clip.OnToBoundsPropertyChanged)));

    public static bool GetToBounds(DependencyObject depObj)
    {
      return (bool) depObj.GetValue(Clip.ToBoundsProperty);
    }

    public static void SetToBounds(DependencyObject depObj, bool clipToBounds)
    {
      depObj.SetValue(Clip.ToBoundsProperty, clipToBounds);
    }

    private static void OnToBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement fe = d as FrameworkElement;
      if (fe == null)
        return;
      Clip.ClipToBounds(fe);
      // ISSUE: method pointer
      fe.Loaded += (new RoutedEventHandler(Clip.fe_Loaded));
      // ISSUE: method pointer
      fe.SizeChanged += (new SizeChangedEventHandler(Clip.fe_SizeChanged));
    }

    private static void ClipToBounds(FrameworkElement fe)
    {
      if (Clip.GetToBounds((DependencyObject) fe))
      {
        FrameworkElement frameworkElement = fe;
        RectangleGeometry rectangleGeometry = new RectangleGeometry();
        Rect rect = new Rect(0.0, 0.0, fe.ActualWidth, fe.ActualHeight);
        rectangleGeometry.Rect = rect;
        ((UIElement) frameworkElement).Clip=((Geometry) rectangleGeometry);
      }
      else
        ((UIElement) fe).Clip=( null);
    }

    private static void fe_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      Clip.ClipToBounds(sender as FrameworkElement);
    }

    private static void fe_Loaded(object sender, RoutedEventArgs e)
    {
      Clip.ClipToBounds(sender as FrameworkElement);
    }
  }
}
