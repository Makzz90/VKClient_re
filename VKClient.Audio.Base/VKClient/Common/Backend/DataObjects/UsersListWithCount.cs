using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class UsersListWithCount
  {
    public List<User> users { get; set; }

    public int count { get; set; }
  }
}
