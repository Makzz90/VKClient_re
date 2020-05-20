using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public class ScrollViewerOffsetMediator : FrameworkElement
  {
    public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register("ScrollViewer", typeof (ScrollViewer), typeof (ScrollViewerOffsetMediator), new PropertyMetadata(new PropertyChangedCallback(ScrollViewerOffsetMediator.OnScrollViewerChanged)));
    public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof (double), typeof (ScrollViewerOffsetMediator), new PropertyMetadata(new PropertyChangedCallback(ScrollViewerOffsetMediator.OnVerticalOffsetChanged)));
    public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof (double), typeof (ScrollViewerOffsetMediator), new PropertyMetadata(new PropertyChangedCallback(ScrollViewerOffsetMediator.OnHorizontalOffsetChanged)));

    public ScrollViewer ScrollViewer
    {
      get
      {
        return (ScrollViewer) this.GetValue(ScrollViewerOffsetMediator.ScrollViewerProperty);
      }
      set
      {
        this.SetValue(ScrollViewerOffsetMediator.ScrollViewerProperty, (object) value);
      }
    }

    public double VerticalOffset
    {
      get
      {
        return (double) this.GetValue(ScrollViewerOffsetMediator.VerticalOffsetProperty);
      }
      set
      {
        this.SetValue(ScrollViewerOffsetMediator.VerticalOffsetProperty, (object) value);
      }
    }

    public double HorizontalOffset
    {
      get
      {
        return (double) this.GetValue(ScrollViewerOffsetMediator.HorizontalOffsetProperty);
      }
      set
      {
        this.SetValue(ScrollViewerOffsetMediator.HorizontalOffsetProperty, (object) value);
      }
    }

    private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewerOffsetMediator viewerOffsetMediator = (ScrollViewerOffsetMediator) o;
      ScrollViewer scrollViewer = (ScrollViewer) e.NewValue;
      if (scrollViewer == null)
        return;
      scrollViewer.ScrollToVerticalOffset(viewerOffsetMediator.VerticalOffset);
    }

    public static void OnVerticalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewerOffsetMediator viewerOffsetMediator = (ScrollViewerOffsetMediator) o;
      if (viewerOffsetMediator.ScrollViewer == null)
        return;
      viewerOffsetMediator.ScrollViewer.ScrollToVerticalOffset((double) e.NewValue);
    }

    public static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewerOffsetMediator viewerOffsetMediator = (ScrollViewerOffsetMediator) o;
      if (viewerOffsetMediator.ScrollViewer == null)
        return;
      viewerOffsetMediator.ScrollViewer.ScrollToHorizontalOffset((double) e.NewValue);
    }
  }
}
