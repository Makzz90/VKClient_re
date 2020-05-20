using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Games
{
  public class GamesSectionItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate InvitesTemplate { get; set; }

    public DataTemplate MyTemplate { get; set; }

    public DataTemplate FriendsActivityTemplate { get; set; }

    public DataTemplate CatalogTemplate { get; set; }

    public DataTemplate CatalogHeaderTemplate { get; set; }

    public DataTemplate CatalogHeaderEmptyTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      GamesSectionItem gamesSectionItem = item as GamesSectionItem;
      if (gamesSectionItem == null)
        return (DataTemplate) null;
      switch (gamesSectionItem.Type)
      {
        case GamesSectionType.MyGames:
          return this.MyTemplate;
        case GamesSectionType.Invites:
          return this.InvitesTemplate;
        case GamesSectionType.FriendsActivity:
          return this.FriendsActivityTemplate;
        case GamesSectionType.Catalog:
          return this.CatalogTemplate;
        case GamesSectionType.CatalogHeader:
          return this.CatalogHeaderTemplate;
        case GamesSectionType.CatalogHeaderEmpty:
          return this.CatalogHeaderEmptyTemplate;
        default:
          return (DataTemplate) null;
      }
    }
  }
}
