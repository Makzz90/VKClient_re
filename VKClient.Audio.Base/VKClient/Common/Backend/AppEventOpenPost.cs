using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventOpenPost : AppEventBase
  {
    public override string e
    {
      get
      {
        return "open_post";
      }
    }

    public List<string> post_ids { get; set; }

    public List<string> repost_ids { get; set; }
  }
}
