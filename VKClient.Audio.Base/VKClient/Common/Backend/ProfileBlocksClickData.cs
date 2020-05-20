using Newtonsoft.Json;

namespace VKClient.Common.Backend
{
  public class ProfileBlocksClickData
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? friends { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? followers { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? photos { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? videos { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? audios { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? gifts { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? docs { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? subscriptions { get; set; }
  }
}
