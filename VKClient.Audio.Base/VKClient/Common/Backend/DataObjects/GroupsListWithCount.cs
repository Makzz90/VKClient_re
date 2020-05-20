using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class GroupsListWithCount
  {
    public List<Group> groups { get; set; }

    public List<User> users { get; set; }

    public int Count { get; set; }
  }
}
