namespace VKClient.Audio.Base.Events
{
  public class VideoCommentIsAddedDeleted
  {
    public bool IsAdded { get; set; }

    public long OwnerId { get; set; }

    public long VideoId { get; set; }
  }
}
