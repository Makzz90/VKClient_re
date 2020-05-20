using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class CountriesResponse
  {
    public List<Country> countriesNearby { get; set; }

    public List<Country> countries { get; set; }
  }
}
