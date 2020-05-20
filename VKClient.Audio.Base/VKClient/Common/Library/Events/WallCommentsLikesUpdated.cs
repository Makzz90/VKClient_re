namespace VKClient.Common.Library.Events
{
  public class WallCommentsLikesUpdated
  {
    public long OwnerId { get; set; }

    public long WallPostId { get; set; }

    public int CommentsCount { get; set; }

    public int LikesCount { get; set; }
  }
}
