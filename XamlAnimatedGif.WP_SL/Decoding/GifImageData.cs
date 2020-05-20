using System.IO;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal class GifImageData
  {
    public byte LzwMinimumCodeSize { get; set; }

    public long CompressedDataStartOffset { get; set; }

    private GifImageData()
    {
    }

    internal static async Task<GifImageData> ReadAsync(Stream stream)
    {
      GifImageData imgData = new GifImageData();
      await imgData.ReadInternalAsync(stream).ConfigureAwait(false);
      return imgData;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      this.LzwMinimumCodeSize = (byte) stream.ReadByte();
      this.CompressedDataStartOffset = stream.Position;
      byte[] numArray = await GifHelpers.ReadDataBlocksAsync(stream, true).ConfigureAwait(false);
    }
  }
}
