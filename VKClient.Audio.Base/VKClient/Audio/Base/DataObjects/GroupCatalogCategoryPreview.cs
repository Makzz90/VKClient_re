using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GroupCatalogCategoryPreview
  {
    public string name { get; set; }

    public int page_count { get; set; }

    public List<Group> page_previews { get; set; }

    public int id { get; set; }
  }
}
