using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Market.ViewModels
{
  public class PhotoHeader
  {
      public Photo Photo { get; private set; }

      public double Width { get; private set; }

      public double Height { get; private set; }

      public string ImageUrl { get; private set; }

    public PhotoHeader(Photo photo, double width)
    {
      this.Photo = photo;
      this.Width = width;
      this.Height = photo.width <= 0 || photo.height <= 0 ? this.Width : width / ((double) photo.width / (double) photo.height);
      this.ImageUrl = photo.GetAppropriateForScaleFactor(this.Height, 1);
    }
  }
}
