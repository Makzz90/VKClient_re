namespace VKClient.Audio.Base.Events
{
  public class PhotoDeletedFromAlbum
  {
    public long OwnerId { get; set; }

    public string AlbumId { get; set; }

    public long PhotoId { get; set; }
  }
}
