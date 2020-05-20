using System;

namespace VKClient.Common.Backend.DataObjects
{
  public sealed class CommunityInvitation
  {
    public Group community { get; set; }

    public User inviter { get; set; }

    public Action<CommunityInvitations> InvitationHandledAction { get; set; }
  }
}
