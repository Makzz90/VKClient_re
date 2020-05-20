using System.Collections.Generic;

namespace VKClient.Photos.Library
{
  public class PhotosMovedToAlbum
  {
    public string fromAlbumId { get; set; }

    public string toAlbumId { get; set; }

    public List<long> photos { get; set; }
  }
}
