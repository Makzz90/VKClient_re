namespace VKClient.Common.Backend.DataObjects
{
  public class FriendStatus
  {
    public long uid { get; set; }

    public FriendshipStatus friend_status { get; set; }

    public int read_state { get; set; }
  }
}
