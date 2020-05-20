using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal class GifPlainTextExtension : GifExtension
  {
    internal const int ExtensionLabel = 1;

    public int BlockSize { get; private set; }

    public int Left { get; private set; }

    public int Top { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int CellWidth { get; private set; }

    public int CellHeight { get; private set; }

    public int ForegroundColorIndex { get; private set; }

    public int BackgroundColorIndex { get; private set; }

    public string Text { get; private set; }

    public IList<GifExtension> Extensions { get; private set; }

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.GraphicRendering;
      }
    }

    private GifPlainTextExtension()
    {
    }

    internal static async Task<GifPlainTextExtension> ReadAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
    {
      GifPlainTextExtension plainText = new GifPlainTextExtension();
      await plainText.ReadInternalAsync(stream, controlExtensions).ConfigureAwait(false);
      return plainText;
    }

    private async Task ReadInternalAsync(Stream stream, IEnumerable<GifExtension> controlExtensions)
    {
        byte[] bytes = new byte[13];
        await stream.ReadAllAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        this.BlockSize = (int)bytes[0];
        if (this.BlockSize != 12)
            throw GifHelpers.InvalidBlockSizeException("Plain Text Extension", 12, this.BlockSize);
        this.Left = (int)BitConverter.ToUInt16(bytes, 1);
        this.Top = (int)BitConverter.ToUInt16(bytes, 3);
        this.Width = (int)BitConverter.ToUInt16(bytes, 5);
        this.Height = (int)BitConverter.ToUInt16(bytes, 7);
        this.CellWidth = (int)bytes[9];
        this.CellHeight = (int)bytes[10];
        this.ForegroundColorIndex = (int)bytes[11];
        this.BackgroundColorIndex = (int)bytes[12];
        this.Text = GifHelpers.GetString(await GifHelpers.ReadDataBlocksAsync(stream, false).ConfigureAwait(false));
        this.Extensions = (IList<GifExtension>)controlExtensions.ToList<GifExtension>().AsReadOnly();
    }
  }
}
