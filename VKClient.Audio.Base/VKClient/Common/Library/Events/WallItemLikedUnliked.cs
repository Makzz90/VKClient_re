namespace VKClient.Common.Library.Events
{
  public class WallItemLikedUnliked
  {
    public long OwnerId { get; set; }

    public long WallPostId { get; set; }

    public bool Liked { get; set; }
  }
}
