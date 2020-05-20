using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventViewPost : AppEventBase
  {
    public override string e
    {
      get
      {
        return "view_post";
      }
    }

    public List<string> post_ids { get; set; }

    public List<string> repost_ids { get; set; }
  }
}
