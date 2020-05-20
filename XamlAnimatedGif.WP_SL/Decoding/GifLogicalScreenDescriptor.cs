using System;
using System.IO;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal class GifLogicalScreenDescriptor : IGifRect
  {
    public int Width { get; private set; }

    public int Height { get; private set; }

    public bool HasGlobalColorTable { get; private set; }

    public int ColorResolution { get; private set; }

    public bool IsGlobalColorTableSorted { get; private set; }

    public int GlobalColorTableSize { get; private set; }

    public int BackgroundColorIndex { get; private set; }

    public double PixelAspectRatio { get; private set; }

    int IGifRect.Left
    {
      get
      {
        return 0;
      }
    }

    int IGifRect.Top
    {
      get
      {
        return 0;
      }
    }

    internal static async Task<GifLogicalScreenDescriptor> ReadAsync(Stream stream)
    {
      GifLogicalScreenDescriptor descriptor = new GifLogicalScreenDescriptor();
      await descriptor.ReadInternalAsync(stream).ConfigureAwait(false);
      return descriptor;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      byte[] bytes = new byte[7];
      await stream.ReadAllAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
      this.Width = (int) BitConverter.ToUInt16(bytes, 0);
      this.Height = (int) BitConverter.ToUInt16(bytes, 2);
      byte num = bytes[4];
      this.HasGlobalColorTable = ((uint) num & 128U) > 0U;
      this.ColorResolution = (((int) num & 112) >> 4) + 1;
      this.IsGlobalColorTableSorted = ((uint) num & 8U) > 0U;
      this.GlobalColorTableSize = 1 << ((int) num & 7) + 1;
      this.BackgroundColorIndex = (int) bytes[5];
      this.PixelAspectRatio = (int) bytes[5] == 0 ? 0.0 : (double) (15 + (int) bytes[5]) / 64.0;
    }
  }
}
