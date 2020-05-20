using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.ImageViewer;

namespace VKClient.Common.CommonExtensions
{
  public static class ImageExtensions
  {
    public static void ResizeToFit(this Image image, Photo photo, Size containerSize)
    {
      if (photo == null)
        return;
      Size photoSize = new Size((double) photo.width, (double) photo.height);
      image.ResizeToFit(photoSize, containerSize);
    }

    public static void ResizeToFit(this Image image, Size photoSize, Size containerSize)
    {
      if (photoSize.Width <= 0.0 || photoSize.Height <= 0.0)
        return;
      Rect fill = RectangleUtils.ResizeToFill(containerSize, photoSize);
      Rect rect = new Rect(-fill.X, -fill.Y, containerSize.Width, containerSize.Height);
      image.Width = fill.Width;
      image.Height = fill.Height;
      image.Margin = new Thickness(fill.X, fill.Y, 0.0, 0.0);
      image.Clip = (Geometry) new RectangleGeometry()
      {
        Rect = rect
      };
    }
  }
}
