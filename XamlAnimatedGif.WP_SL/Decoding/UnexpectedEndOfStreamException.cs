using System;

namespace XamlAnimatedGif.Decoding
{
  public class UnexpectedEndOfStreamException : GifDecoderException
  {
    internal UnexpectedEndOfStreamException(string message)
      : base(message)
    {
    }

    internal UnexpectedEndOfStreamException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}
