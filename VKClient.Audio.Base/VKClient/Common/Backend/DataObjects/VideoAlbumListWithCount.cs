using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class VideoAlbumListWithCount
  {
    public int AlbumsCount { get; set; }

    public List<VideoAlbum> response { get; set; }
  }
}
