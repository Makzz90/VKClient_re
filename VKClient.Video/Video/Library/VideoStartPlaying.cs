namespace VKClient.Video.Library
{
  public class VideoStartPlaying
  {
    public VideoHeader Video { get; protected set; }

    public VideoStartPlaying(VideoHeader video)
    {
      this.Video = video;
    }
  }
}
