using System.Collections.Generic;

namespace VKClient.Audio.Base.Events
{
  public class PhotosMovedToAlbum
  {
    public string fromAlbumId { get; set; }

    public string toAlbumId { get; set; }

    public List<long> photos { get; set; }
  }
}
