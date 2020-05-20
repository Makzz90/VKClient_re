using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public class PhotoAlbumCreated
  {
    public Album Album { get; set; }

    public int EventSource { get; set; }
  }
}
