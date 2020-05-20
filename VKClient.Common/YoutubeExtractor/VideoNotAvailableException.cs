using System;

namespace YoutubeExtractor
{
  public class VideoNotAvailableException : Exception
  {
    public VideoNotAvailableException()
    {
    }

    public VideoNotAvailableException(string message)
      : base(message)
    {
    }
  }
}
