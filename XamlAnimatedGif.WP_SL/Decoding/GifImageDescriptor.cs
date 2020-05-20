using System;
using System.IO;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal class GifImageDescriptor : IGifRect
  {
    public int Left { get; private set; }

    public int Top { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public bool HasLocalColorTable { get; private set; }

    public bool Interlace { get; private set; }

    public bool IsLocalColorTableSorted { get; private set; }

    public int LocalColorTableSize { get; private set; }

    private GifImageDescriptor()
    {
    }

    internal static async Task<GifImageDescriptor> ReadAsync(Stream stream)
    {
      GifImageDescriptor descriptor = new GifImageDescriptor();
      await descriptor.ReadInternalAsync(stream).ConfigureAwait(false);
      return descriptor;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      byte[] bytes = new byte[9];
      await stream.ReadAllAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
      this.Left = (int) BitConverter.ToUInt16(bytes, 0);
      this.Top = (int) BitConverter.ToUInt16(bytes, 2);
      this.Width = (int) BitConverter.ToUInt16(bytes, 4);
      this.Height = (int) BitConverter.ToUInt16(bytes, 6);
      byte num = bytes[8];
      this.HasLocalColorTable = ((uint) num & 128U) > 0U;
      this.Interlace = ((uint) num & 64U) > 0U;
      this.IsLocalColorTableSorted = ((uint) num & 32U) > 0U;
      this.LocalColorTableSize = 1 << ((int) num & 7) + 1;
    }
  }
}
