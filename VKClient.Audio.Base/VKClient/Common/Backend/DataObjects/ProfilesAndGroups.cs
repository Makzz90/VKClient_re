using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class ProfilesAndGroups
  {
    public List<Group> groups { get; set; }

    public List<User> profiles { get; set; }

    public ProfilesAndGroups()
    {
      this.groups = new List<Group>();
      this.profiles = new List<User>();
    }
  }
}
