using System.IO;

namespace VKClient.Common.Framework
{
  public class ImagePreprocessResult
  {
    public Stream Stream { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public ImagePreprocessResult(Stream stream, int width, int height)
    {
      this.Stream = stream;
      this.Width = width;
      this.Height = height;
    }
  }
}
