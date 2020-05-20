using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public class Clip
  {
    public static readonly DependencyProperty ToBoundsProperty = DependencyProperty.RegisterAttached("ToBounds", typeof (bool), typeof (Clip), new PropertyMetadata((object) false, new PropertyChangedCallback(Clip.OnToBoundsPropertyChanged)));

    public static bool GetToBounds(DependencyObject depObj)
    {
      return (bool) depObj.GetValue(Clip.ToBoundsProperty);
    }

    public static void SetToBounds(DependencyObject depObj, bool clipToBounds)
    {
      depObj.SetValue(Clip.ToBoundsProperty, (object) clipToBounds);
    }

    private static void OnToBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement fe = d as FrameworkElement;
      if (fe == null)
        return;
      Clip.ClipToBounds(fe);
      fe.Loaded += new RoutedEventHandler(Clip.fe_Loaded);
      fe.SizeChanged += new SizeChangedEventHandler(Clip.fe_SizeChanged);
    }

    private static void ClipToBounds(FrameworkElement fe)
    {
      if (Clip.GetToBounds((DependencyObject) fe))
        fe.Clip = (Geometry) new RectangleGeometry()
        {
          Rect = new Rect(0.0, 0.0, fe.ActualWidth, fe.ActualHeight)
        };
      else
        fe.Clip = (Geometry) null;
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
