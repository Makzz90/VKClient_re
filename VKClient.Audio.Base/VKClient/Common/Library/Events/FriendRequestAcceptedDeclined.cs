namespace VKClient.Common.Library.Events
{
  public class FriendRequestAcceptedDeclined
  {
    public long UserId { get; private set; }

    public bool Accepted { get; private set; }

    public FriendRequestAcceptedDeclined(bool accepted, long uid)
    {
      this.UserId = uid;
      this.Accepted = accepted;
    }
  }
}
