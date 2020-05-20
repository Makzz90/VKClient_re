namespace VKClient.Audio.Base.Events
{
  public class MessagesFromGroupAllowedDeniedEvent
  {
    public long UserOrGroupId { get; set; }

    public bool IsAllowed { get; set; }
  }
}
