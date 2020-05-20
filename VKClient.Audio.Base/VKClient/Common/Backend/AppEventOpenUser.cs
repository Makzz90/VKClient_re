using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventOpenUser : AppEventBase
  {
    public override string e
    {
      get
      {
        return "open_user";
      }
    }

    public List<long> user_ids { get; set; }

    public string source { get; set; }
  }
}
