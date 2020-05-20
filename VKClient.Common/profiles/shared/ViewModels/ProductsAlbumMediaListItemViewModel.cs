using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProductsAlbumMediaListItemViewModel : MediaListItemViewModelBase
  {
    private readonly Product _product;

    public string ImageUri { get; private set; }

    public override string Id
    {
      get
      {
        return this._product.ToString();
      }
    }

    public ProductsAlbumMediaListItemViewModel(Product product)
      : base(ProfileMediaListItemType.ProductsAlbum)
    {
      this._product = product;
      Photo photo = product.photos == null || product.photos.Count <= 0 ?  null : product.photos[0];
      if (photo == null)
        return;
      this.ImageUri = photo.GetAppropriateForScaleFactor(120.0, 1);
    }
  }
}
