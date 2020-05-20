using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.ViewModels
{
  public class StoreBannerHeader
  {
    private readonly StoreBanner _storeBanner;

    public StockItem StockItem
    {
      get
      {
        return this._storeBanner.stock_item;
      }
    }

    public string ImageUrl
    {
      get
      {
        double num = 480.0 * (double) ScaleFactor.GetRealScaleFactor() / 100.0;
        if (num <= 0.0)
          return "";
        if (num <= 480.0)
          return this._storeBanner.photo_480;
        if (num <= 640.0)
          return this._storeBanner.photo_640;
        if (num <= 960.0)
          return this._storeBanner.photo_960;
        return this._storeBanner.photo_1280;
      }
    }

    public StoreBannerHeader(StoreBanner storeBanner)
    {
      this._storeBanner = storeBanner;
    }
  }
}
