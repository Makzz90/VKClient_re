using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class LikesList
  {
    public List<User> All { get; set; }

    public List<Group> AllGroups { get; set; }

    public int AllCount { get; set; }

    public List<User> Shared { get; set; }

    public int SharedCount { get; set; }

    public List<User> Friends { get; set; }

    public int FriendsCount { get; set; }

    public List<long> AllIds { get; set; }
  }
}
