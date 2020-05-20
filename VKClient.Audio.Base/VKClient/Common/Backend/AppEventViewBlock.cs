using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventViewBlock : AppEventBase
  {
    public override string e
    {
      get
      {
        return "view_block";
      }
    }

    public List<string> blocks { get; set; }
  }
}
