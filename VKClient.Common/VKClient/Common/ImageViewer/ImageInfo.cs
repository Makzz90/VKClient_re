using System;
using System.Windows.Media.Imaging;

namespace VKClient.Common.ImageViewer
{
  public class ImageInfo
  {
    public string Uri { get; set; }

    public Func<bool, BitmapSource> GetSourceFunc { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }
  }
}
