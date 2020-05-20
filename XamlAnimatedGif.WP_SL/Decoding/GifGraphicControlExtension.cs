using System;
using System.IO;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal class GifGraphicControlExtension : GifExtension
  {
    internal const int ExtensionLabel = 249;

    public int BlockSize { get; private set; }

    public GifFrameDisposalMethod DisposalMethod { get; private set; }

    public bool UserInput { get; private set; }

    public bool HasTransparency { get; private set; }

    public int Delay { get; private set; }

    public int TransparencyIndex { get; private set; }

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.Control;
      }
    }

    private GifGraphicControlExtension()
    {
    }

    internal static async Task<GifGraphicControlExtension> ReadAsync(Stream stream)
    {
      GifGraphicControlExtension ext = new GifGraphicControlExtension();
      await ext.ReadInternalAsync(stream).ConfigureAwait(false);
      return ext;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      byte[] bytes = new byte[6];
      await stream.ReadAllAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
      this.BlockSize = (int) bytes[0];
      if (this.BlockSize != 4)
        throw GifHelpers.InvalidBlockSizeException("Graphic Control Extension", 4, this.BlockSize);
      byte num = bytes[1];
      this.DisposalMethod = (GifFrameDisposalMethod) (((int) num & 28) >> 2);
      this.UserInput = ((uint) num & 2U) > 0U;
      this.HasTransparency = ((uint) num & 1U) > 0U;
      this.Delay = (int) BitConverter.ToUInt16(bytes, 2) * 10;
      this.TransparencyIndex = (int) bytes[4];
    }
  }
}
