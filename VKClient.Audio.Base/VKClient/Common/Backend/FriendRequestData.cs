using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class FriendRequestData
  {
    public FriendRequest Model { get; set; }

    public User[] Profiles { get; set; }

    public bool IsSuggestedFriend { get; set; }

    public object ParentViewModel { get; set; }
  }
}
