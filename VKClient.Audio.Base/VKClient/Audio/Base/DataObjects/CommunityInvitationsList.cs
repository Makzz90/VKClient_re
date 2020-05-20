using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class CommunityInvitationsList
  {
    public int count { get; set; }

    public Group[] invitations { get; set; }

    public User[] inviters { get; set; }
  }
}
