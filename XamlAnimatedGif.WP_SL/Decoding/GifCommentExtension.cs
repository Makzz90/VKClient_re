using System.IO;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Decoding
{
  internal class GifCommentExtension : GifExtension
  {
    internal const int ExtensionLabel = 254;

    public string Text { get; private set; }

    internal override GifBlockKind Kind
    {
      get
      {
        return GifBlockKind.SpecialPurpose;
      }
    }

    private GifCommentExtension()
    {
    }

    internal static async Task<GifCommentExtension> ReadAsync(Stream stream)
    {
      GifCommentExtension comment = new GifCommentExtension();
      await comment.ReadInternalAsync(stream).ConfigureAwait(false);
      return comment;
    }

    private async Task ReadInternalAsync(Stream stream)
    {
      byte[] bytes = await GifHelpers.ReadDataBlocksAsync(stream, false).ConfigureAwait(false);
      if (bytes == null)
        return;
      this.Text = GifHelpers.GetString(bytes);
    }
  }
}
