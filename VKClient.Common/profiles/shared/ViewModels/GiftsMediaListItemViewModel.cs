using System.Collections.Generic;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class GiftsMediaListItemViewModel : MediaListItemViewModelBase
  {
    private readonly List<string> _photos;

    public string ImageUrl1 { get; private set; }

    public string ImageUrl2 { get; private set; }

    public string ImageUrl3 { get; private set; }

    public override string Id
    {
      get
      {
        if (this._photos == null || this._photos.Count <= 0)
          return "";
        return string.Join(",", (IEnumerable<string>) this._photos);
      }
    }

    public GiftsMediaListItemViewModel(List<string> photos)
      : base(ProfileMediaListItemType.Gifts)
    {
      this._photos = photos;
      if (photos == null)
        return;
      if (photos.Count > 0)
        this.ImageUrl1 = photos[0];
      if (photos.Count > 1)
        this.ImageUrl2 = photos[1];
      if (photos.Count <= 2)
        return;
      this.ImageUrl3 = photos[2];
    }
  }
}
