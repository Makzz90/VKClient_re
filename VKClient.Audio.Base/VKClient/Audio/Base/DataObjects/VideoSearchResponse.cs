using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class VideoSearchResponse
  {
    public List<Video> MyVideos { get; set; }

    public List<Video> GlobalVideos { get; set; }

    public int TotalCount { get; set; }
  }
}
