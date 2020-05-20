using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public interface ISupportOtherVideos
  {
      VKList<VKClient.Common.Backend.DataObjects.Video> OtherVideos { get; }
  }
}
