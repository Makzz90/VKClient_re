using System;

namespace YoutubeExtractor
{
  public class YoutubeParseException : Exception
  {
    public YoutubeParseException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
