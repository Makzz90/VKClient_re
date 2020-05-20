using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProductMediaListItemViewModel : MediaListItemViewModelBase
  {
    private const double CONTAINER_WIDTH = 180.0;
    private const double CONTAINER_HEIGHT = 236.0;
    private const double IMAGE_WIDTH_HEIGHT = 236.0;
    private readonly Product _product;
    private string _imageUri;

    public override double ContainerWidth
    {
      get
      {
        return 180.0;
      }
    }

    public override double ContainerHeight
    {
      get
      {
        return 236.0;
      }
    }

    public override Thickness ItemMargin
    {
      get
      {
        return new Thickness(0.0, 0.0, 0.0, 16.0);
      }
    }

    public override string Id
    {
      get
      {
        return this._product.ToString();
      }
    }

    public Product Product
    {
      get
      {
        return this._product;
      }
    }

    public string ImageUri
    {
      get
      {
        return this._imageUri;
      }
      set
      {
        if (this._imageUri == value)
          return;
        this._imageUri = value;
        this.NotifyPropertyChanged("ImageUri");
      }
    }

    public string Title { get; set; }

    public string Price { get; set; }

    public Visibility PriceVisibility
    {
      get
      {
        if (this._product.price == null || string.IsNullOrEmpty(this._product.price.text))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public ProductMediaListItemViewModel(Product product)
      : base(ProfileMediaListItemType.Product)
    {
      this._product = product;
      Photo photo = product.photos == null || product.photos.Count <= 0 ?  null : product.photos[0];
      if (photo != null)
        this.ImageUri = photo.GetAppropriateForScaleFactor(236.0, 1);
      this.Title = product.title;
      this.Price = product.price != null ? product.price.text : "";
    }
  }
}
