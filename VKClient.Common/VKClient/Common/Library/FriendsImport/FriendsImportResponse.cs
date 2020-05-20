using System.Collections.Generic;

namespace VKClient.Common.Library.FriendsImport
{
  public class FriendsImportResponse
  {
    public List<ISubscriptionItemHeader> FoundUsers { get; set; }

    public List<ISubscriptionItemHeader> OtherUsers { get; set; }

    public FriendsImportResponse()
    {
      this.FoundUsers = new List<ISubscriptionItemHeader>();
      this.OtherUsers = new List<ISubscriptionItemHeader>();
    }
  }
}
