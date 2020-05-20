using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class GroupsLists
  {
    public VKList<Group> Communities { get; set; }

    public CommunityInvitations Invitations { get; set; }
  }
}
