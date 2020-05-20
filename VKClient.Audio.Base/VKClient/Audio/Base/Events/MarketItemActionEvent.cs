using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VKClient.Audio.Base.Events
{
  public class MarketItemActionEvent : StatEventBase
  {
    public string itemId { get; set; }

    [JsonConverter(typeof (StringEnumConverter))]
    public MarketItemSource source { get; set; }
  }
}
