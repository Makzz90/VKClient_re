using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal abstract class GifBlock
  {
    internal abstract GifBlockKind Kind { get; }

    internal static async Task<GifBlock> ReadAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
    {
      int blockId = await stream.ReadByteAsync(new CancellationToken()).ConfigureAwait(false);
      if (blockId < 0)
        throw GifHelpers.UnexpectedEndOfStreamException();
      switch (blockId)
      {
        case 33:
          return (GifBlock) await GifExtension.ReadAsync(stream, controlExtensions).ConfigureAwait(false);
        case 44:
          return (GifBlock) await GifFrame.ReadAsync(stream, controlExtensions).ConfigureAwait(false);
        case 59:
          return (GifBlock) await GifTrailer.ReadAsync().ConfigureAwait(false);
        default:
          throw GifHelpers.UnknownBlockTypeException(blockId);
      }
    }
  }
}
