using System.Windows;
using System.Windows.Controls.Primitives;

namespace VKClient.Common.Framework
{
  public static class ViewportExtensions
  {
    public static void BindViewportBoundsTo(this ViewportControl viewportControl, FrameworkElement element)
    {
        viewportControl.Bounds = new Rect();
        element.SizeChanged+=(delegate(object s, SizeChangedEventArgs e)
        {
            Rect bounds = new Rect(0.0, viewportControl.Bounds.Y, element.ActualWidth, element.ActualHeight - viewportControl.Bounds.Y);
            viewportControl.Bounds=bounds;
        });
    }
  }
}
