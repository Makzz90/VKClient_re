using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftInfoFromStoreResponse
  {
    public List<long> userIds { get; set; }

    public GiftsSectionItem giftItem { get; set; }
  }
}
