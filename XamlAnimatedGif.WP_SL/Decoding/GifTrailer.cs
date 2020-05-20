using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal class GifTrailer : GifBlock
  {
    internal const int TrailerByte = 59;

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.Other;
      }
    }

    private GifTrailer()
    {
    }

    internal static Task<GifTrailer> ReadAsync()
    {
      return Task.FromResult<GifTrailer>(new GifTrailer());
    }
  }
}
