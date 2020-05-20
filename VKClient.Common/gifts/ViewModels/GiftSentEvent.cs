using System.Collections.Generic;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftSentEvent
  {
      public long GiftId { get; private set; }

      public List<long> UserIds { get; private set; }

    public GiftSentEvent(long giftId, List<long> userIds)
    {
      this.GiftId = giftId;
      this.UserIds = userIds;
    }
  }
}
