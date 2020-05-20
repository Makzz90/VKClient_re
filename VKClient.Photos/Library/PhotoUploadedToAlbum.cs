using VKClient.Common.Backend.DataObjects;

namespace VKClient.Photos.Library
{
  public class PhotoUploadedToAlbum
  {
    public string aid { get; set; }

    public long pid { get; set; }

    public Photo photo { get; set; }
  }
}
