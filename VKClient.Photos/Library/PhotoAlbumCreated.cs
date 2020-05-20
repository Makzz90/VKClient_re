using VKClient.Common.Backend.DataObjects;

namespace VKClient.Photos.Library
{
  public class PhotoAlbumCreated
  {
    public Album Album { get; set; }

    public int EventSource { get; set; }
  }
}
