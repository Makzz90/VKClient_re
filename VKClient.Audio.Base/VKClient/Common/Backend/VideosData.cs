using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class VideosData
  {
    public VKList<Video> AddedVideos { get; set; }

    public int UploadedVideosCount { get; set; }

    public int VideoAlbumsCount { get; set; }
  }
}
