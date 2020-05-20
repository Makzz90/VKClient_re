using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public class ScrollViewerOffsetMediator : FrameworkElement
  {
      public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ScrollViewerOffsetMediator), new PropertyMetadata(new PropertyChangedCallback(ScrollViewerOffsetMediator.OnScrollViewerChanged)));
      public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(ScrollViewerOffsetMediator), new PropertyMetadata(new PropertyChangedCallback(ScrollViewerOffsetMediator.OnVerticalOffsetChanged)));
      public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(ScrollViewerOffsetMediator), new PropertyMetadata(new PropertyChangedCallback(ScrollViewerOffsetMediator.OnHorizontalOffsetChanged)));

    public ScrollViewer ScrollViewer
    {
      get
      {
        return (ScrollViewer) base.GetValue(ScrollViewerOffsetMediator.ScrollViewerProperty);
      }
      set
      {
        base.SetValue(ScrollViewerOffsetMediator.ScrollViewerProperty, value);
      }
    }

    public double VerticalOffset
    {
      get
      {
        return (double) base.GetValue(ScrollViewerOffsetMediator.VerticalOffsetProperty);
      }
      set
      {
        base.SetValue(ScrollViewerOffsetMediator.VerticalOffsetProperty, value);
      }
    }

    public double HorizontalOffset
    {
      get
      {
        return (double) base.GetValue(ScrollViewerOffsetMediator.HorizontalOffsetProperty);
      }
      set
      {
        base.SetValue(ScrollViewerOffsetMediator.HorizontalOffsetProperty, value);
      }
    }

    public ScrollViewerOffsetMediator()
    {
      //base.\u002Ector();
    }

    private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewerOffsetMediator viewerOffsetMediator = (ScrollViewerOffsetMediator) o;
      // ISSUE: explicit reference operation
      ScrollViewer newValue = (ScrollViewer) e.NewValue;
      if (newValue == null)
        return;
      newValue.ScrollToVerticalOffset(viewerOffsetMediator.VerticalOffset);
    }

    public static void OnVerticalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewerOffsetMediator viewerOffsetMediator = (ScrollViewerOffsetMediator) o;
      if (viewerOffsetMediator.ScrollViewer == null)
        return;
      // ISSUE: explicit reference operation
      viewerOffsetMediator.ScrollViewer.ScrollToVerticalOffset((double) e.NewValue);
    }

    public static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ScrollViewerOffsetMediator viewerOffsetMediator = (ScrollViewerOffsetMediator) o;
      if (viewerOffsetMediator.ScrollViewer == null)
        return;
      // ISSUE: explicit reference operation
      viewerOffsetMediator.ScrollViewer.ScrollToHorizontalOffset((double) e.NewValue);
    }
  }
}
