using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventSubscriptionFromPost : AppEventBase
  {
    public override string e
    {
      get
      {
        return "subscription_from_post";
      }
    }

    public List<string> post_ids { get; set; }
  }
}
