using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventPostLinkClick : AppEventBase
  {
    public override string e
    {
      get
      {
        return "post_link_click";
      }
    }

    public List<string> post_ids { get; set; }
  }
}
