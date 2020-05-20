using System;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class PhotoMediaListItemViewModel : MediaListItemViewModelBase, IHaveUniqueKey
  {
    private readonly double _containerWidth;
    private const int CONTAINER_HEIGHT = 160;
    private string _imageUri;

    public override double ContainerWidth
    {
      get
      {
        return this._containerWidth;
      }
    }

    public override double ContainerHeight
    {
      get
      {
        return 160.0;
      }
    }

    public override Thickness ItemMargin
    {
      get
      {
        return new Thickness(0.0, 0.0, 0.0, 4.0);
      }
    }

    public override string Id
    {
      get
      {
        return this.Photo.ToString();
      }
    }

    public long OwnerId { get; private set; }

    public Photo Photo { get; private set; }

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

    public PhotoMediaListItemViewModel(Photo photo)
      : base(ProfileMediaListItemType.Photo)
    {
      this.Photo = photo;
      this.OwnerId = photo.owner_id;
      int num1 = photo.width;
      int num2 = photo.height;
      this.ImageUri = this.Photo.GetAppropriateForScaleFactor(160.0, 1);
      if (double.IsNaN((double) num1) || double.IsInfinity((double) num1) || num1 == 0)
        num1 = 160;
      if (double.IsNaN((double) num2) || double.IsInfinity((double) num2) || num2 == 0)
        num2 = 160;
      this._containerWidth = Math.Round((double) num1 / (double) num2 * 160.0, MidpointRounding.AwayFromZero);
    }

    public string GetKey()
    {
      return this.Photo.ToString();
    }
  }
}
