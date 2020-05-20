using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VKClient.Audio.Base.Events
{
  public class OpenGamesEvent : StatEventBase
  {
    [JsonConverter(typeof (StringEnumConverter))]
    public GamesVisitSource visit_source { get; set; }
  }
}
