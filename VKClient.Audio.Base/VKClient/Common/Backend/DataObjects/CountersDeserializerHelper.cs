using Newtonsoft.Json;

namespace VKClient.Common.Backend.DataObjects
{
  public static class CountersDeserializerHelper
  {
    public static OwnCounters Deserialize(string jsonStr)
    {
      jsonStr = jsonStr.Replace("\"response\":[]", "\"response\":{}");
      return JsonConvert.DeserializeObject<GenericRoot<OwnCounters>>(jsonStr).response;
    }
  }
}
