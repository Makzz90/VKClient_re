using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class UsersAndGroups
  {
    public List<User> users { get; set; }

    public List<Group> pages { get; set; }

    public List<Group> groups { get; set; }

    public UsersAndGroups()
    {
      this.users = new List<User>();
      this.pages = new List<Group>();
      this.groups = new List<Group>();
    }
  }
}
