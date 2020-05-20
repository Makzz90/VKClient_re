using VKClient.Common.Backend.DataObjects;

namespace VKClient.Photos.Library
{
  public class PhotoSetAsAlbumCover
  {
    public string aid { get; set; }

    public long pid { get; set; }

    public Photo Photo { get; set; }
  }
}
