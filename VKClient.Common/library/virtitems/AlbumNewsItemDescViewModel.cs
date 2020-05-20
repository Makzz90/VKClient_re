using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
  public class AlbumNewsItemDescViewModel
  {
    private readonly bool _isBig;
    private readonly Album _album;
    private readonly MarketAlbum _marketAlbum;

    public Visibility BigPreviewVisibility
    {
      get
      {
        return this._isBig.ToVisiblity();
      }
    }

    public Visibility SmallPreviewVisibility
    {
      get
      {
        return (!this._isBig).ToVisiblity();
      }
    }

    public Visibility ShowPlaySmallIconVisibility
    {
      get
      {
        return Visibility.Visible;
      }
    }

    public string Title
    {
      get
      {
        if (this._album != null)
          return this._album.title;
        if (this._marketAlbum != null)
          return this._marketAlbum.title;
        return "";
      }
    }

    public string Subtitle
    {
      get
      {
        if (this._album != null && this._album.size > 0)
          return UIStringFormatterHelper.FormatNumberOfSomething(this._album.size, CommonResources.OnePhotoFrm, CommonResources.TwoFourPhotosFrm, CommonResources.FivePhotosFrm, true,  null, false);
        if (this._marketAlbum != null && this._marketAlbum.count > 0)
          return UIStringFormatterHelper.FormatNumberOfSomething(this._marketAlbum.count, CommonResources.OneProductItemFrm, CommonResources.TwoFourProductItemsFrm, CommonResources.FiveProductItemsFrm, true,  null, false);
        return "";
      }
    }

    public string SmallPreviewIcon
    {
      get
      {
        if (this._album != null)
          return "/Resources/WallPost/AttachPhotoAlbumSmall.png";
        return this._marketAlbum != null ? "/Resources/WallPost/AttachProductCollectionSmall.png" : "";
      }
    }

    public string Size
    {
      get
      {
        if (this._album != null && this._album.size > 0)
          return UIStringFormatterHelper.FormatForUIShort((long) this._album.size);
        if (this._marketAlbum != null && this._marketAlbum.count > 0)
          return UIStringFormatterHelper.FormatForUIShort((long) this._marketAlbum.count);
        return "";
      }
    }

    private AlbumNewsItemDescViewModel(bool isBig)
    {
      this._isBig = isBig;
    }

    public AlbumNewsItemDescViewModel(Album album, bool isBig)
      : this(isBig)
    {
      this._album = album;
    }

    public AlbumNewsItemDescViewModel(MarketAlbum marketAlbum, bool isBig)
      : this(isBig)
    {
      this._marketAlbum = marketAlbum;
    }
  }
}
