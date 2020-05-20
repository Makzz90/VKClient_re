using System.Windows;
using System.Windows.Controls.Primitives;

namespace VKClient.Common.Framework
{
  public class ViewportMediator : FrameworkElement
  {
      public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register("ViewportControl", typeof(ViewportControl), typeof(ViewportMediator), new PropertyMetadata(new PropertyChangedCallback(ViewportMediator.OnScrollViewerChanged)));
      public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(ViewportMediator), new PropertyMetadata(new PropertyChangedCallback(ViewportMediator.OnVerticalOffsetChanged)));

    public ViewportControl ViewportControl
    {
      get
      {
        return (ViewportControl) base.GetValue(ViewportMediator.ViewportProperty);
      }
      set
      {
        base.SetValue(ViewportMediator.ViewportProperty, value);
      }
    }

    public double VerticalOffset
    {
      get
      {
        return (double) base.GetValue(ViewportMediator.VerticalOffsetProperty);
      }
      set
      {
        base.SetValue(ViewportMediator.VerticalOffsetProperty, value);
      }
    }

    public ViewportMediator()
    {
      //base.\u002Ector();
    }

    private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ViewportMediator viewportMediator = d as ViewportMediator;
      viewportMediator.ViewportControl.SetViewportOrigin(new Point(0.0, viewportMediator.VerticalOffset));
    }
  }
}
