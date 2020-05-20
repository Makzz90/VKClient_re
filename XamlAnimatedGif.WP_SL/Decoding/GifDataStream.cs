using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal class GifDataStream
  {
    public GifHeader Header { get; private set; }

    public GifColor[] GlobalColorTable { get; set; }

    public IList<GifFrame> Frames { get; set; }

    public IList<GifExtension> Extensions { get; set; }

    public ushort RepeatCount { get; set; }

    private GifDataStream()
    {
    }

    internal static async Task<GifDataStream> ReadAsync(Stream stream)
    {
      GifDataStream file = new GifDataStream();
      await file.ReadInternalAsync(stream).ConfigureAwait(false);
      return file;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
        this.Header = await GifHeader.ReadAsync(stream).ConfigureAwait(false);
        if (this.Header.LogicalScreenDescriptor.HasGlobalColorTable)
            this.GlobalColorTable = await GifHelpers.ReadColorTableAsync(stream, this.Header.LogicalScreenDescriptor.GlobalColorTableSize).ConfigureAwait(false);
        await this.ReadFramesAsync(stream).ConfigureAwait(false);
        GifApplicationExtension ext = this.Extensions.OfType<GifApplicationExtension>().FirstOrDefault<GifApplicationExtension>(new Func<GifApplicationExtension, bool>(GifHelpers.IsNetscapeExtension));
        this.RepeatCount = ext != null ? GifHelpers.GetRepeatCount(ext) : (ushort)1;
    }

    private async Task ReadFramesAsync(Stream stream)
    {
      List<GifFrame> frames = new List<GifFrame>();
      List<GifExtension> controlExtensions = new List<GifExtension>();
      List<GifExtension> specialExtensions = new List<GifExtension>();
      GifBlock gifBlock;
      do
      {
        gifBlock = await GifBlock.ReadAsync(stream, (IEnumerable<GifExtension>) controlExtensions).ConfigureAwait(false);
        if (gifBlock.Kind == GifBlockKind.GraphicRendering)
          controlExtensions = new List<GifExtension>();
        if (gifBlock is GifFrame)
          frames.Add((GifFrame) gifBlock);
        else if (gifBlock is GifExtension)
        {
          GifExtension gifExtension = (GifExtension) gifBlock;
          switch (gifExtension.Kind)
          {
            case GifBlockKind.Control:
              controlExtensions.Add(gifExtension);
              continue;
            case GifBlockKind.SpecialPurpose:
              specialExtensions.Add(gifExtension);
              continue;
            default:
              continue;
          }
        }
      }
      while (!(gifBlock is GifTrailer));
      this.Frames = (IList<GifFrame>) frames.AsReadOnly();
      this.Extensions = (IList<GifExtension>) specialExtensions.AsReadOnly();
    }
  }
}
