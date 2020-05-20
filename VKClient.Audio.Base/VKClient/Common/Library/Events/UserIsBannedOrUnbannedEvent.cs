using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class UserIsBannedOrUnbannedEvent
  {
    public User user { get; set; }

    public bool IsBanned { get; set; }
  }
}
