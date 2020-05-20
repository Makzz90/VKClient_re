namespace VKClient.Audio.Base.Events
{
  public class VideoCommentsLikesUpdated
  {
    public long OwnerId { get; set; }

    public long VideoId { get; set; }

    public int CommentsCount { get; set; }

    public int LikesCount { get; set; }
  }
}
