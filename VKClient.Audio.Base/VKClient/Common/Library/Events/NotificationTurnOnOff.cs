namespace VKClient.Common.Library.Events
{
  public class NotificationTurnOnOff
  {
    public long UserOrChatId { get; set; }

    public bool IsChat { get; set; }
  }
}
