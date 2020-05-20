using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class VideoMediaListItemViewModel : MediaListItemViewModelBase
  {
    private const double CONTAINER_WIDTH = 256.0;
    private const double CONTAINER_HEIGHT = 200.0;
    private readonly VKClient.Common.Backend.DataObjects.Video _video;
    private string _imageUri;

    public override double ContainerWidth
    {
      get
      {
        return 256.0;
      }
    }

    public override double ContainerHeight
    {
      get
      {
        return 200.0;
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
        return this._video.ToString();
      }
    }

    public VKClient.Common.Backend.DataObjects.Video Video
    {
      get
      {
        return this._video;
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

    public string Duration { get; set; }

    public Visibility DurationVisibility
    {
      get
      {
        if (this._video.duration <= 0)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public VideoMediaListItemViewModel(VKClient.Common.Backend.DataObjects.Video video)
      : base(ProfileMediaListItemType.Video)
    {
      this._video = video;
      this.ImageUri = video.photo_640 ?? video.photo_320 ?? video.photo_130;
      this.Title = video.title;
      this.Duration = UIStringFormatterHelper.FormatDuration(video.duration);
    }
  }
}
