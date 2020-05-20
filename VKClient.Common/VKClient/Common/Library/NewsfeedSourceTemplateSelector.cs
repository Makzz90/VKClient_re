using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public class NewsfeedSourceTemplateSelector : DataTemplateSelector
  {
    public DataTemplate FeedTemplate { get; set; }

    public DataTemplate GenericTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      PickableNewsfeedSourceItemViewModel sourceItemViewModel = item as PickableNewsfeedSourceItemViewModel;
      if ((sourceItemViewModel != null ? sourceItemViewModel.PickableItem : (PickableItem) null) == null)
        return (DataTemplate) null;
      if (sourceItemViewModel.PickableItem.ID == -10L)
        return this.FeedTemplate;
      return this.GenericTemplate;
    }
  }
}
