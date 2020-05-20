using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventOpenGroup : AppEventBase
  {
    public override string e
    {
      get
      {
        return "open_group";
      }
    }

    public List<long> group_ids { get; set; }

    public string source { get; set; }
  }
}
