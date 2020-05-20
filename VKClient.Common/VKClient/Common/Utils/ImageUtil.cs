using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace VKClient.Common.Utils
{
  public class ImageUtil
  {
    public static byte[] TryImageToByteArray(BitmapSource imageIn)
    {
      try
      {
        if (imageIn == null)
          return new byte[0];
        MemoryStream memoryStream = new MemoryStream();
        new WriteableBitmap(imageIn).SaveJpeg((Stream) memoryStream, imageIn.PixelWidth, imageIn.PixelHeight, 0, 100);
        return memoryStream.ToArray();
      }
      catch
      {
        return new byte[0];
      }
    }

    public static BitmapImage ByteArrayToImage(byte[] byteArrayIn)
    {
      MemoryStream memoryStream1 = new MemoryStream(byteArrayIn);
      BitmapImage bitmapImage = new BitmapImage();
      MemoryStream memoryStream2 = memoryStream1;
      bitmapImage.SetSource((Stream) memoryStream2);
      return bitmapImage;
    }
  }
}
