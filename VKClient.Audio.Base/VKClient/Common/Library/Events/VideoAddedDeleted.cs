namespace VKClient.Common.Library.Events
{
  public class VideoAddedDeleted
  {
    public bool IsAdded { get; set; }

    public long VideoId { get; set; }

    public long OwnerId { get; set; }

    public long TargetId { get; set; }

    public long AlbumId { get; set; }

    public bool IsDeletedPermanently { get; set; }
  }
}
