namespace VKClient.Common.Library.Events
{
  public class FriendRemoved
  {
    public long UserId { get; private set; }

    public FriendRemoved(long uid)
    {
      this.UserId = uid;
    }
  }
}
