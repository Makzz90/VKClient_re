using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class UserIsSubcribedUnsubscribedToEvent
  {
    public User user { get; set; }

    public bool IsSubscribed { get; set; }
  }
}
