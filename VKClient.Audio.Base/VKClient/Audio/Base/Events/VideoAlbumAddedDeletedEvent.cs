namespace VKClient.Audio.Base.Events
{
  public class VideoAlbumAddedDeletedEvent
  {
    public long AlbumId { get; set; }

    public long OwnerId { get; set; }

    public bool IsAdded { get; set; }
  }
}
