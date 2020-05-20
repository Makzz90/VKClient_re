using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class GroupCatalogInfoExtended
  {
    public int enabled { get; set; }

    public List<GroupCatalogCategoryPreview> categories { get; set; }
  }
}
