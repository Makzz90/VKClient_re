using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class FriendsAndMutualFriends
  {
    public List<User> friends { get; set; }

    public List<long> mutualFriends { get; set; }
  }
}
