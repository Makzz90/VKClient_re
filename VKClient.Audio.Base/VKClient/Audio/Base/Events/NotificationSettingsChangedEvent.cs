namespace VKClient.Audio.Base.Events
{
  public class NotificationSettingsChangedEvent
  {
    public long ChatId { get; set; }

    public bool AreNotificationsDisabled { get; set; }
  }
}
