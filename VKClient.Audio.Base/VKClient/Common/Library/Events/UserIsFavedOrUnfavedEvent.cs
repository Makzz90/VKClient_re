using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class UserIsFavedOrUnfavedEvent
  {
    public User user { get; set; }

    public bool IsFaved { get; set; }
  }
}
