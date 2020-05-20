using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventFriendRecommendationShowed : AppEventBase
  {
    public override string e
    {
      get
      {
        return "show_user_rec";
      }
    }

    public List<long> user_ids { get; set; }
  }
}
