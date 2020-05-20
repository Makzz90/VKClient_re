using System;
using VKClient.Common.Backend;

namespace VKClient.Audio.Base.Events
{
  public class VideoUploaded
  {
    public Guid guid { get; set; }

    public SaveVideoResponse SaveVidResp { get; set; }
  }
}
