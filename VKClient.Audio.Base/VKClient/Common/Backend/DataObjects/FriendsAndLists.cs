using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class FriendsAndLists
  {
    public List<User> friends { get; set; }

    public List<FriendsList> friendLists { get; set; }
  }
}
