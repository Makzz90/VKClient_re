namespace VKClient.Common.Library.Events
{
  public class SubscriptionCancelled
  {
    public long UserId { get; set; }

    public SubscriptionCancelled(long uid)
    {
      this.UserId = uid;
    }
  }
}
