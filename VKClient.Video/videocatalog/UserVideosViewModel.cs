using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Video.VideoCatalog
{
  public class UserVideosViewModel : ViewModelBase
  {
    private VideosOfOwnerViewModel _vidOfOwnerVM;
    private long _ownerId;
    private UserVideosViewModel.CurrentSource _videoListSource;

    public UserVideosViewModel.CurrentSource VideoListSource
    {
      get
      {
        return this._videoListSource;
      }
      set
      {
        this._videoListSource = value;
        this.NotifyPropertyChanged<UserVideosViewModel.CurrentSource>((Expression<Func<UserVideosViewModel.CurrentSource>>) (() => this.VideoListSource));
        this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.AddedForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.UploadedForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.AlbumsForeground));
        this.NotifyPropertyChanged<Visibility>((() => this.AddedVisibility));
        this.NotifyPropertyChanged<Visibility>((() => this.UploadedVisibility));
        this.NotifyPropertyChanged<Visibility>((() => this.AlbumsVisibility));
        this.LoadIfNeeded();
      }
    }

    public VideosOfOwnerViewModel VideosOfOwnerVM
    {
      get
      {
        return this._vidOfOwnerVM;
      }
    }

    public SolidColorBrush AddedForeground
    {
      get
      {
        return this.GetForeground(UserVideosViewModel.CurrentSource.Added);
      }
    }

    public SolidColorBrush UploadedForeground
    {
      get
      {
        return this.GetForeground(UserVideosViewModel.CurrentSource.Uploaded);
      }
    }

    public Visibility AddedVisibility
    {
      get
      {
        if (this.VideoListSource != UserVideosViewModel.CurrentSource.Added)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility UploadedVisibility
    {
      get
      {
        if (this.VideoListSource != UserVideosViewModel.CurrentSource.Uploaded)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility AlbumsVisibility
    {
      get
      {
        if (this.VideoListSource != UserVideosViewModel.CurrentSource.Albums)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public SolidColorBrush AlbumsForeground
    {
      get
      {
        return this.GetForeground(UserVideosViewModel.CurrentSource.Albums);
      }
    }

    public UserVideosViewModel(long ownerId)
    {
      this._ownerId = ownerId;
      this._vidOfOwnerVM = new VideosOfOwnerViewModel(ownerId, false, 0, false);
    }

    public void LoadIfNeeded()
    {
      if (this.VideoListSource == UserVideosViewModel.CurrentSource.Added && !this._vidOfOwnerVM.AllVideosVM.IsLoaded)
        this._vidOfOwnerVM.AllVideosVM.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
      if (this.VideoListSource == UserVideosViewModel.CurrentSource.Uploaded && !this._vidOfOwnerVM.UploadedVideosVM.IsLoaded)
        this._vidOfOwnerVM.UploadedVideosVM.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) null, false);
      if (this.VideoListSource != UserVideosViewModel.CurrentSource.Albums || this._vidOfOwnerVM.AlbumsVM.IsLoaded)
        return;
      this._vidOfOwnerVM.AlbumsVM.LoadData(false, false,  null, false);
    }

    private SolidColorBrush GetForeground(UserVideosViewModel.CurrentSource currentSource)
    {
      if (this.VideoListSource == currentSource)
        return (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      return (SolidColorBrush) Application.Current.Resources["PhoneCaptionGrayBrush"];
    }

    public enum CurrentSource
    {
      Added,
      Uploaded,
      Albums,
    }
  }
}
