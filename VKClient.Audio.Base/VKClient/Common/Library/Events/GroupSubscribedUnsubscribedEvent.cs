using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class GroupSubscribedUnsubscribedEvent
  {
    public Group group { get; set; }

    public bool IsSubscribed { get; set; }
  }
}
