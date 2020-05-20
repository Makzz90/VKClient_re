using VKClient.Common.Backend.DataObjects;

namespace VKClient.Video.Library
{
  public class MoveVideoToAlbum : VideoStartPlaying
  {
    public VideoAlbum Album { get; private set; }

    public MoveVideoToAlbum(VideoHeader video, VideoAlbum album)
      : base(video)
    {
      this.Album = album;
    }
  }
}
