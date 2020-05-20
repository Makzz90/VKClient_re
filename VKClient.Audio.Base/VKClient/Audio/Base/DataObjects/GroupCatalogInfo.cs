using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class GroupCatalogInfo
  {
    public int enabled { get; set; }

    public Dictionary<int, string> categories { get; set; }
  }
}
