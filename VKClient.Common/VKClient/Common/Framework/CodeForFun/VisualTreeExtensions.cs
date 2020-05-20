using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework.CodeForFun
{
  public static class VisualTreeExtensions
  {
    public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
    {
      int childCount = VisualTreeHelper.GetChildrenCount(parent);
      for (int counter = 0; counter < childCount; ++counter)
        yield return VisualTreeHelper.GetChild(parent, counter);
    }

    public static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(this FrameworkElement parent)
    {
      Queue<FrameworkElement> queue = new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());
      while (queue.Count > 0)
      {
        FrameworkElement element = queue.Dequeue();
        yield return element;
        foreach (FrameworkElement frameworkElement in element.GetVisualChildren().OfType<FrameworkElement>())
          queue.Enqueue(frameworkElement);
        element = null;
      }
    }
  }
}
