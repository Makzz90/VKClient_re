namespace VKClient.Common.Backend.DataObjects
{
  public class UserNotification
  {
    public long id { get; set; }

    public string type { get; set; }

    public UserNotificationType Type { get; set; }

    public UserNotificationNewsfeed newsfeed { get; set; }

    public UserNotificationBubbleNewsfeed bubble_newsfeed { get; set; }

    public UserNotificationAlert alert { get; set; }
  }
}
