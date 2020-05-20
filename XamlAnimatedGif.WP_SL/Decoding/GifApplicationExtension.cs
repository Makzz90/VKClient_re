using System;
using System.IO;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal class GifApplicationExtension : GifExtension
  {
    internal const int ExtensionLabel = 255;

    public int BlockSize { get; private set; }

    public string ApplicationIdentifier { get; private set; }

    public byte[] AuthenticationCode { get; private set; }

    public byte[] Data { get; private set; }

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.SpecialPurpose;
      }
    }

    private GifApplicationExtension()
    {
    }

    internal static async Task<GifApplicationExtension> ReadAsync(Stream stream)
    {
      GifApplicationExtension ext = new GifApplicationExtension();
      await ext.ReadInternalAsync(stream).ConfigureAwait(false);
      return ext;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      byte[] bytes = new byte[12];
      await stream.ReadAllAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
      this.BlockSize = (int) bytes[0];
      if (this.BlockSize != 11)
        throw GifHelpers.InvalidBlockSizeException("Application Extension", 11, this.BlockSize);
      this.ApplicationIdentifier = GifHelpers.GetString(bytes, 1, 8);
      byte[] numArray = new byte[3];
      Array.Copy((Array) bytes, 9, (Array) numArray, 0, 3);
      this.AuthenticationCode = numArray;
      this.Data = await GifHelpers.ReadDataBlocksAsync(stream, false).ConfigureAwait(false);
    }
  }
}
