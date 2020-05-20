using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class SubscriptionItemHeaderTemplateSelector : DataTemplateSelector
  {
    public DataTemplate SubscriptionItemTemplate { get; set; }

    public DataTemplate InvitationItemTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      ISubscriptionItemHeader subscriptionItemHeader = item as ISubscriptionItemHeader;
      if (subscriptionItemHeader == null)
        return (DataTemplate) null;
      switch (subscriptionItemHeader.SubscriptionItemType)
      {
        case SubscriptionItemType.Subscription:
          return this.SubscriptionItemTemplate;
        case SubscriptionItemType.Invitation:
          return this.InvitationItemTemplate;
        default:
          return (DataTemplate) null;
      }
    }
  }
}
