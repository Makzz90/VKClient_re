using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class VideoAlbumMediaListItemViewModel : MediaListItemViewModelBase
  {
      private readonly VKClient.Common.Backend.DataObjects.Video _video;

    public string ImageUri { get; private set; }

    public override string Id
    {
      get
      {
        return this._video.ToString();
      }
    }

    public VideoAlbumMediaListItemViewModel(VKClient.Common.Backend.DataObjects.Video video)
      : base(ProfileMediaListItemType.VideoAlbum)
    {
      this._video = video;
      this.ImageUri = this._video.photo_640 ?? video.photo_320 ?? video.photo_130 ?? "";
    }
  }
}
