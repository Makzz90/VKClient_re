namespace VKClient.Audio.Base.Events
{
  public sealed class CommunityLinkDeleted
  {
    public long CommunityId { get; set; }

    public long LinkId { get; set; }
  }
}
