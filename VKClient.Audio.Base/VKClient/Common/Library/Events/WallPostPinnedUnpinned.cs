namespace VKClient.Common.Library.Events
{
  public class WallPostPinnedUnpinned
  {
    public bool Pinned { get; set; }

    public long OwnerId { get; set; }

    public long PostId { get; set; }
  }
}
