namespace VKClient.Common.Backend.DataObjects
{
  public sealed class CommunityInvitations
  {
    public int count { get; set; }

    public CommunityInvitation first_invitation { get; set; }
  }
}
