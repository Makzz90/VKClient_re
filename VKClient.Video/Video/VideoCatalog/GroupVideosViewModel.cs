using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.Utils;
using VKClient.Video.Library;

namespace VKClient.Video.VideoCatalog
{
  public class GroupVideosViewModel : ViewModelBase, ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>
  {
    private CollectionObject<OwnerHeaderWithSubscribeViewModel> _headerCollObj = new CollectionObject<OwnerHeaderWithSubscribeViewModel>();
    private CollectionObject<SectionHeaderViewModel> _albumsHeaderCollObj = new CollectionObject<SectionHeaderViewModel>();
    private CollectionObject<AlbumsListHorizontalViewModel> _albumsCollObj = new CollectionObject<AlbumsListHorizontalViewModel>();
    private CollectionObject<SectionHeaderViewModel> _videosHeaderCollObj = new CollectionObject<SectionHeaderViewModel>();
    private GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> _videosGenCol;
    private long _ownerId;
    private long _albumId;
    private bool _loadedBaseInfo;
    private bool _canUploadVideo;
    private bool _switchingAlbums;

    public string Title
    {
      get
      {
        return CommonResources.Videos.ToUpperInvariant();
      }
    }

    public bool CanUploadVideo
    {
      get
      {
        return this._canUploadVideo;
      }
      private set
      {
        this._canUploadVideo = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanUploadVideo));
      }
    }

    public GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> VideosGenCol
    {
      get
      {
        return this._videosGenCol;
      }
    }

    public Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>>) (vData =>
        {
          ListWithCount<VideoHeader> listWithCount = new ListWithCount<VideoHeader>();
          foreach (VKClient.Common.Backend.DataObjects.Video video in vData.items)
          {
            VideoHeader videoHeader = new VideoHeader(video, (List<MenuItemData>) null, vData.profiles, vData.groups, StatisticsActionSource.videos_group, Math.Abs(this._ownerId).ToString() + "_" + this._albumId.ToString(), false, 0L);
            listWithCount.List.Add(videoHeader);
          }
          listWithCount.TotalCount = vData.count;
          return listWithCount;
        });
      }
    }

    public GroupVideosViewModel(long ownerId)
    {
      this._ownerId = ownerId;
      this._videosGenCol = new GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>((ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>) this);
      this._videosGenCol.MergedCollection.Merge((INotifyCollectionChanged) this._headerCollObj.Coll);
      this._videosGenCol.MergedCollection.Merge((INotifyCollectionChanged) this._albumsHeaderCollObj.Coll);
      this._videosGenCol.MergedCollection.Merge((INotifyCollectionChanged) this._albumsCollObj.Coll);
      this._videosGenCol.MergedCollection.Merge((INotifyCollectionChanged) this._videosHeaderCollObj.Coll);
      this._videosGenCol.MergedCollection.Merge((INotifyCollectionChanged) this._videosGenCol.Collection);
      this._albumId = VideoAlbum.ADDED_ALBUM_ID;
    }

    public void GetData(GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> caller, int offset, int count, Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>> callback)
    {
      if (caller.Refresh && !this._switchingAlbums)
        this._loadedBaseInfo = false;
      if (!this._loadedBaseInfo)
        VideoService.Instance.GetVideoDataExt(this._ownerId, (Action<BackendResult<GetVideosDataExtResponse, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          VKList<VKClient.Common.Backend.DataObjects.Video> resultData = (VKList<VKClient.Common.Backend.DataObjects.Video>) null;
          if (res.ResultCode == ResultCode.Succeeded)
          {
            resultData = this._albumId == VideoAlbum.ADDED_ALBUM_ID ? res.ResultData.AddedVideos : res.ResultData.UploadedVideos;
            this._headerCollObj.Data = (OwnerHeaderWithSubscribeViewModel) null;
            this._albumsHeaderCollObj.Data = (SectionHeaderViewModel) null;
            this._albumsCollObj.Data = (AlbumsListHorizontalViewModel) null;
            this._videosHeaderCollObj.Data = (SectionHeaderViewModel) null;
            OwnerHeaderWithSubscribeViewModel subscribeViewModel = new OwnerHeaderWithSubscribeViewModel(res.ResultData.Group);
            this.CanUploadVideo = res.ResultData.Group != null && res.ResultData.Group.can_upload_video == 1;
            this._headerCollObj.Data = subscribeViewModel;
            if (res.ResultData.Albums.items.Count > 0)
            {
              this._albumsHeaderCollObj.Data = new SectionHeaderViewModel()
              {
                Count = res.ResultData.Albums.count,
                Title = CommonResources.VideoCatalog_Albums,
                ShowAllVisibility = Visibility.Visible,
                ShowOptionsVisibility = Visibility.Collapsed,
                OnHeaderTap = (Action) (() => Navigator.Current.NavigateToVideoAlbumsList(this._ownerId, this.CanUploadVideo))
              };
              this._albumsCollObj.Data = new AlbumsListHorizontalViewModel(this._ownerId, res.ResultData.Albums, new List<User>(), new List<Group>());
            }
            CollectionObject<SectionHeaderViewModel> collectionObject = this._videosHeaderCollObj;
            SectionHeaderViewModel sectionHeaderViewModel = new SectionHeaderViewModel();
            sectionHeaderViewModel.Title = CommonResources.Profile_Videos;
            int count1 = resultData.count;
            sectionHeaderViewModel.Count = count1;
            int num1 = 1;
            sectionHeaderViewModel.ShowAllVisibility = (Visibility) num1;
            int num2 = 0;
            sectionHeaderViewModel.ShowOptionsVisibility = (Visibility) num2;
            sectionHeaderViewModel.HeaderOptions = new List<HeaderOption>()
            {
              new HeaderOption()
              {
                ID = VideoAlbum.ADDED_ALBUM_ID,
                Name = CommonResources.VideoCatalog_Added
              },
              new HeaderOption()
              {
                ID = VideoAlbum.UPLOADED_ALBUM_ID,
                Name = CommonResources.VideoCatalog_Uploaded
              }
            };
            collectionObject.Data = sectionHeaderViewModel;
            this._videosHeaderCollObj.Data.PropertyChanged += new PropertyChangedEventHandler(this.VideosHeader_Data_PropertyChanged);
            this._videosHeaderCollObj.Data.SelectedOption = this._videosHeaderCollObj.Data.HeaderOptions.First<HeaderOption>((Func<HeaderOption, bool>) (ho => ho.ID == this._albumId));
            this._loadedBaseInfo = true;
          }
          callback(new BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>(res.ResultCode, resultData));
        }))));
      else
        VideoService.Instance.GetVideos(Math.Abs(this._ownerId), this._ownerId < 0L, offset, count, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            this._videosHeaderCollObj.Data.Count = res.ResultData.count;
          callback(res);
        }), this._albumId, true);
    }

    private void VideosHeader_Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedOption"))
        return;
      this._albumId = this._videosHeaderCollObj.Data.SelectedOption.ID;
      if (this.VideosGenCol.IsLoading)
        return;
      this._switchingAlbums = true;
      this.VideosGenCol.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>) (res => this._switchingAlbums = false), false);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoVideos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true, null, false);
    }
  }
}
