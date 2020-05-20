using System.Windows;

namespace VKClient.Common.Framework
{
  public class ExtendedReorderListBox : ReorderListBox
  {
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      base.PrepareContainerForItemOverride(element, item);
      ReorderListBoxItem reorderListBoxItem = item as ReorderListBoxItem;
      if (reorderListBoxItem == null)
        return;
      IReorderable reorderable = item as IReorderable;
      if (reorderable == null || !reorderable.CanReorder)
        return;
      reorderListBoxItem.IsReorderEnabled = true;
    }
  }
}
