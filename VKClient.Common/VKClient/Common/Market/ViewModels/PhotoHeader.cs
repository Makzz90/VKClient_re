using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Market.ViewModels
{
  public class PhotoHeader
  {
      public Photo Photo { get; set; }

      public double Width { get; set; }

    public double Height { get; set; }

    public string ImageUrl { get; set; }

    public PhotoHeader(Photo photo, double width)
    {
      this.Photo = photo;
      this.Width = width;
      this.Height = photo.width <= 0 || photo.height <= 0 ? this.Width : width / ((double) photo.width / (double) photo.height);
      this.ImageUrl = photo.GetAppropriateForScaleFactor(this.Height, 1);
    }
  }
}
