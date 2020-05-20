using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VKClient.Audio.Base.Events
{
  public class GamesActionEvent : StatEventBase
  {
    public long game_id { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string request_name { get; set; }

    [JsonConverter(typeof (StringEnumConverter))]
    public GamesActionType action_type { get; set; }

    [JsonConverter(typeof (StringEnumConverter))]
    public GamesVisitSource visit_source { get; set; }

    [JsonConverter(typeof (StringEnumConverter))]
    public GamesClickSource click_source { get; set; }

    public GamesActionEvent()
    {
      this.ShouldSendImmediately = true;
    }
  }
}
