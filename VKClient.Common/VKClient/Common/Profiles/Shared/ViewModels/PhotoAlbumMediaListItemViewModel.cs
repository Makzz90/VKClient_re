using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class PhotoAlbumMediaListItemViewModel : MediaListItemViewModelBase
  {
    private const int UC_HEIGHT = 120;
    private readonly Photo _photo;

    public string ImageUri { get; private set; }

    public override string Id
    {
      get
      {
        return this._photo.ToString();
      }
    }

    public PhotoAlbumMediaListItemViewModel(Photo photo)
      : base(ProfileMediaListItemType.PhotoAlbum)
    {
      this._photo = photo;
      this.ImageUri = this._photo.GetAppropriateForScaleFactor(120.0, 1);
    }
  }
}
