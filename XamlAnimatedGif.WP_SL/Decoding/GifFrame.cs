using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal class GifFrame : GifBlock
  {
    internal const int ImageSeparator = 44;

    public GifImageDescriptor Descriptor { get; private set; }

    public GifColor[] LocalColorTable { get; private set; }

    public IList<GifExtension> Extensions { get; private set; }

    public GifImageData ImageData { get; private set; }

    public GifGraphicControlExtension GraphicControl { get; set; }

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.GraphicRendering;
      }
    }

    private GifFrame()
    {
    }

    internal static async Task<GifFrame> ReadAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
    {
      GifFrame frame = new GifFrame();
      await frame.ReadInternalAsync(stream, controlExtensions).ConfigureAwait(false);
      return frame;
    }

    private async Task ReadInternalAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
    {
        this.Descriptor = await GifImageDescriptor.ReadAsync(stream).ConfigureAwait(false);
        if (this.Descriptor.HasLocalColorTable)
            this.LocalColorTable = await GifHelpers.ReadColorTableAsync(stream, this.Descriptor.LocalColorTableSize).ConfigureAwait(false);
        this.ImageData = await GifImageData.ReadAsync(stream).ConfigureAwait(false);
        this.Extensions = (IList<GifExtension>)controlExtensions.ToList<GifExtension>().AsReadOnly();
        this.GraphicControl = this.Extensions.OfType<GifGraphicControlExtension>().FirstOrDefault<GifGraphicControlExtension>();
    }
  }
}
