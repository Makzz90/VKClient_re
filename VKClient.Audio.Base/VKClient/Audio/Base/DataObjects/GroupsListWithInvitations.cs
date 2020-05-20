using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class GroupsListWithInvitations
  {
    public VKList<Group> groups { get; set; }

    public CommunityInvitations invitations { get; set; }
  }
}
