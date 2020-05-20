using System.Windows;
using System.Windows.Controls.Primitives;

namespace VKClient.Common.Framework
{
  public class ViewportMediator : FrameworkElement
  {
    public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register("ViewportControl", typeof (ViewportControl), typeof (ViewportMediator), new PropertyMetadata(new PropertyChangedCallback(ViewportMediator.OnScrollViewerChanged)));
    public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof (double), typeof (ViewportMediator), new PropertyMetadata(new PropertyChangedCallback(ViewportMediator.OnVerticalOffsetChanged)));

    public ViewportControl ViewportControl
    {
      get
      {
        return (ViewportControl) this.GetValue(ViewportMediator.ViewportProperty);
      }
      set
      {
        this.SetValue(ViewportMediator.ViewportProperty, (object) value);
      }
    }

    public double VerticalOffset
    {
      get
      {
        return (double) this.GetValue(ViewportMediator.VerticalOffsetProperty);
      }
      set
      {
        this.SetValue(ViewportMediator.VerticalOffsetProperty, (object) value);
      }
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
