using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class FriendRecommendationItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate DefaultTemplate { get; set; }

    public DataTemplate ContactsSyncPromoTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      FriendRecommendationItem recommendationItem = item as FriendRecommendationItem;
      if (recommendationItem == null)
        return  null;
      switch (recommendationItem.Type)
      {
        case FriendRecommendationItemType.Default:
          return this.DefaultTemplate;
        case FriendRecommendationItemType.ContactsSyncPromo:
          return this.ContactsSyncPromoTemplate;
        default:
          return  null;
      }
    }
  }
}
