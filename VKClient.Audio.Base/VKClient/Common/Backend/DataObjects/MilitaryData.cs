using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class MilitaryData
  {
    public List<VKClient.Common.Backend.DataObjects.Military> Military { get; set; }

    public List<Country> Countries { get; set; }
  }
}
