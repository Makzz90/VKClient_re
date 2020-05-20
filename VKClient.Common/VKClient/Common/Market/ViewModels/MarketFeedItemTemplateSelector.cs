using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Market.ViewModels
{
  public class MarketFeedItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate AlbumsTemplate { get; set; }

    public DataTemplate ProductTemplate { get; set; }

    public DataTemplate ProductsHeaderTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      MarketFeedItem marketFeedItem = item as MarketFeedItem;
      if (marketFeedItem == null)
        return (DataTemplate) null;
      switch (marketFeedItem.Type)
      {
        case MarketFeedItemType.Albums:
          return this.AlbumsTemplate;
        case MarketFeedItemType.Product:
          return this.ProductTemplate;
        case MarketFeedItemType.ProductsHeader:
          return this.ProductsHeaderTemplate;
        default:
          return (DataTemplate) null;
      }
    }
  }
}
