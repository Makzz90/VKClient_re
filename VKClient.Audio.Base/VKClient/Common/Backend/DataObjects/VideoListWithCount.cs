using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class VideoListWithCount
  {
    public int VideosCount { get; set; }

    public List<Video> response { get; set; }
  }
}
