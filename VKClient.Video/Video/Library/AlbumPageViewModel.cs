using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using VKClient.Common;
using VKClient.Common.Framework;

namespace VKClient.Video.Library
{
  public class AlbumPageViewModel : ViewModelBase
  {
    private readonly long _userOrGroupId;
    private readonly bool _isGroup;
    private ObservableCollection<Group<VideoHeader>> _videosInAlbum;
    private bool isLoading;
    private long _albumId;
    private int _loadedPages;

    public ObservableCollection<Group<VideoHeader>> VideosInAlbum
    {
      get
      {
        return this._videosInAlbum;
      }
      set
      {
        this._videosInAlbum = value;
        this.NotifyPropertyChanged<ObservableCollection<Group<VideoHeader>>>((Expression<Func<ObservableCollection<Group<VideoHeader>>>>) (() => this.VideosInAlbum));
      }
    }

    public AlbumPageViewModel(long userOrGroupId, bool isGroup)
    {
      this.VideosInAlbum = new ObservableCollection<Group<VideoHeader>>();
      ((Collection<Group<VideoHeader>>) this.VideosInAlbum).Add(new Group<VideoHeader>(string.Empty, (IEnumerable<VideoHeader>) new List<VideoHeader>(), false));
      this._userOrGroupId = userOrGroupId;
      this._isGroup = isGroup;
    }

    public void LoadVideos(long albumId)
    {
      this._albumId = albumId;
      this.VideosInAlbum = new ObservableCollection<Group<VideoHeader>>();
      ((Collection<Group<VideoHeader>>) this.VideosInAlbum).Add(new Group<VideoHeader>(string.Empty, (IEnumerable<VideoHeader>) new List<VideoHeader>(), false));
      this._loadedPages = 0;
      this.LoadVideosFromBackend(0);
    }

    private void LoadVideosFromBackend(int offset)
    {
      if (this.isLoading)
        return;
      this.isLoading = true;
    }

    internal void LoadMoreVideos()
    {
      this.LoadVideosFromBackend(this._loadedPages * VKConstants.VideosReadCount);
    }
  }
}
