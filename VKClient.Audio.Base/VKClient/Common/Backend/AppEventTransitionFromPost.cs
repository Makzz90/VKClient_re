using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventTransitionFromPost : AppEventBase
  {
    public override string e
    {
      get
      {
        return "transition_from_post";
      }
    }

    public List<string> post_ids { get; set; }

    public Dictionary<string, string> parent_ids { get; set; }
  }
}
