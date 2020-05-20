using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public static class TreeHelpers
  {
    public static IEnumerable<FrameworkElement> GetVisualAncestors(this FrameworkElement node)
    {
      for (FrameworkElement parent = node.GetVisualParent(); parent != null; parent = parent.GetVisualParent())
        yield return parent;
    }

    public static FrameworkElement GetVisualParent(this FrameworkElement node)
    {
      return VisualTreeHelper.GetParent((DependencyObject) node) as FrameworkElement;
    }
  }
}
