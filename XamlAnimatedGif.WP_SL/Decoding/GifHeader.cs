using System.IO;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal class GifHeader : GifBlock
  {
    public string Signature { get; private set; }

    public string Version { get; private set; }

    public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.Other;
      }
    }

    private GifHeader()
    {
    }

    internal static async Task<GifHeader> ReadAsync(Stream stream)
    {
      GifHeader header = new GifHeader();
      await header.ReadInternalAsync(stream).ConfigureAwait(false);
      return header;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      this.Signature = await GifHelpers.ReadStringAsync(stream, 3).ConfigureAwait(false);
      if (this.Signature != "GIF")
        throw GifHelpers.InvalidSignatureException(this.Signature);
      this.Version = await GifHelpers.ReadStringAsync(stream, 3).ConfigureAwait(false);
      if (this.Version != "87a" && this.Version != "89a")
        throw GifHelpers.UnsupportedVersionException(this.Version);
      this.LogicalScreenDescriptor = await GifLogicalScreenDescriptor.ReadAsync(stream).ConfigureAwait(false);
    }
  }
}
