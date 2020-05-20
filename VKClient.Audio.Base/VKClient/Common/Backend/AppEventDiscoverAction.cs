using Newtonsoft.Json;

namespace VKClient.Common.Backend
{
  public class AppEventDiscoverAction : AppEventBase
  {
    public override string e
    {
      get
      {
        return "discover_action";
      }
    }

    public string action_type { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string action_param { get; set; }
  }
}
