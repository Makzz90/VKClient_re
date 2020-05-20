using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects.Maps
{
  public class GoogleGeocodeResult
  {
    public string formatted_address { get; set; }

    public List<GoogleAddressComponent> address_components { get; set; }
  }
}
