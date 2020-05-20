namespace VKClient.Common.Library.Events
{
  public class ProfileIsSubscribedUnsubscribedToEvent
  {
    public long Id { get; set; }

    public bool IsSubscribed { get; set; }
  }
}
