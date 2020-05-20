using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class GroupFavedUnfavedEvent
  {
    public Group group { get; set; }

    public bool IsFaved { get; set; }
  }
}
