using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class GetCatalogSectionResponse
  {
    public List<VideoCatalogItem> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public string next { get; set; }

    public GetCatalogSectionResponse()
    {
      this.items = new List<VideoCatalogItem>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
