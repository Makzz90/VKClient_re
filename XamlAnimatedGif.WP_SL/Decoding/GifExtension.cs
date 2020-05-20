using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal abstract class GifExtension : GifBlock
  {
    internal const int ExtensionIntroducer = 33;

    internal static async Task<GifExtension> ReadAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
    {
      int label = stream.ReadByte();
      if (label < 0)
        throw GifHelpers.UnexpectedEndOfStreamException();
      switch (label)
      {
        case 254:
          return (GifExtension) await GifCommentExtension.ReadAsync(stream).ConfigureAwait(false);
        case (int) byte.MaxValue:
          return (GifExtension) await GifApplicationExtension.ReadAsync(stream).ConfigureAwait(false);
        case 1:
          return (GifExtension) await GifPlainTextExtension.ReadAsync(stream, controlExtensions).ConfigureAwait(false);
        case 249:
          return (GifExtension) await GifGraphicControlExtension.ReadAsync(stream).ConfigureAwait(false);
        default:
          throw GifHelpers.UnknownExtensionTypeException(label);
      }
    }
  }
}
