using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class GetCatalogResponse
  {
    public List<VideoCatalogCategory> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public string next { get; set; }

    public GetCatalogResponse()
    {
      this.items = new List<VideoCatalogCategory>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
