using VKClient.Common.Framework;

namespace VKClient.Video.Library
{
  public class VideoResolution : ViewModelBase
  {
    public int Resolution { get; set; }

    public override string ToString()
    {
      return string.Format("{0}p", this.Resolution);
    }
  }
}
