using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public class VisualTreeAdapter : ILinqTree<DependencyObject>
  {
    private DependencyObject _item;

    public DependencyObject Parent
    {
      get
      {
        return VisualTreeHelper.GetParent(this._item);
      }
    }

    public VisualTreeAdapter(DependencyObject item)
    {
      this._item = item;
    }

    public IEnumerable<DependencyObject> Children()
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(this._item);
        int num;
        for (int i = 0; i < childrenCount; i = num + 1)
        {
            yield return VisualTreeHelper.GetChild(this._item, i);
            num = i;
        }
        yield break;
    }
  }
}
