using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class CareerData
  {
    public List<Career> Items { get; set; }

    public List<City> Cities { get; set; }

    public List<Group> Groups { get; set; }
  }
}
