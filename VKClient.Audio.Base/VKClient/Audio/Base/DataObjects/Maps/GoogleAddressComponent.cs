using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects.Maps
{
  public class GoogleAddressComponent
  {
    public string long_name { get; set; }

    public string short_name { get; set; }

    public List<string> types { get; set; }
  }
}
