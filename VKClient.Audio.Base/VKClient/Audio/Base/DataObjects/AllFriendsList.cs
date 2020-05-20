using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class AllFriendsList
  {
    public List<User> friends { get; set; }

    public FriendRequests requests { get; set; }
  }
}
