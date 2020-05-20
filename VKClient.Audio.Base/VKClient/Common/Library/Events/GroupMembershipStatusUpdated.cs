namespace VKClient.Common.Library.Events
{
  public class GroupMembershipStatusUpdated
  {
    public long GroupId { get; private set; }

    public bool Joined { get; private set; }

    public GroupMembershipStatusUpdated(long groupId, bool joined)
    {
      this.GroupId = groupId;
      this.Joined = joined;
    }
  }
}
