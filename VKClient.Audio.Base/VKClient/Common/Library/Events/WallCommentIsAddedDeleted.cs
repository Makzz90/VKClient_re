namespace VKClient.Common.Library.Events
{
  public class WallCommentIsAddedDeleted
  {
    public long OwnerId { get; set; }

    public long WallPostId { get; set; }

    public bool Added { get; set; }
  }
}
