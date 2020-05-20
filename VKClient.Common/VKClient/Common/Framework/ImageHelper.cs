using Microsoft.Xna.Framework.Media;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
  public class ImageHelper
  {
    public static void SaveImage(BitmapImage image)
    {
      byte[] byteArray = ImageUtil.TryImageToByteArray((BitmapSource) image);
      if (byteArray == null || byteArray.Length == 0)
        return;
      string name = "VK_Saved_Photo_ " + DateTime.Now.Ticks.ToString();
      using (MediaLibrary mediaLibrary = new MediaLibrary())
        mediaLibrary.SavePicture(name, byteArray);
      new GenericInfoUC().ShowAndHideLater(CommonResources.PhotoIsSaved, null);
    }
  }
}
