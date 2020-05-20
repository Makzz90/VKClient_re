using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Shared;
using VKClient.Video.VideoCatalog;

namespace VKClient.Common.VideoCatalog
{
  public class VideoCatalogTemplateSelector : DataTemplateSelector
  {
    public DataTemplate CategoryHeaderTemplate { get; set; }

    public DataTemplate CategoryMoreFooterTemplate { get; set; }

    public DataTemplate CatalogItemTempate { get; set; }

    public DataTemplate CatalogItemsHorizontalTemplate { get; set; }

    public DataTemplate CatalogItemsHorizontalExtTemplate { get; set; }

    public DataTemplate DividerUpTemplate { get; set; }

    public DataTemplate DividerDownTemplate { get; set; }

    public DataTemplate CatalogItemTwoInARowTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is ListHeaderViewModel)
        return this.CategoryHeaderTemplate;
      if (item is CategoryMoreFooter)
        return this.CategoryMoreFooterTemplate;
      if (item is CatalogItemViewModel)
        return this.CatalogItemTempate;
      if (item is CatalogItemsHorizontalViewModel)
        return this.CatalogItemsHorizontalExtTemplate;
      if (item is DividerSpaceUpViewModel)
        return this.DividerUpTemplate;
      if (item is DividerSpaceDownViewModel)
        return this.DividerDownTemplate;
      if (item is CatalogItemTwoInARowViewModel)
        return this.CatalogItemTwoInARowTemplate;
      return  null;
    }
  }
}
