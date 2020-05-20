using VKClient.Audio.Base.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public class VideoAlbumEditedEvent
  {
    public long AlbumId { get; set; }

    public long OwnerId { get; set; }

    public PrivacyInfo Privacy { get; set; }

    public string Name { get; set; }
  }
}
