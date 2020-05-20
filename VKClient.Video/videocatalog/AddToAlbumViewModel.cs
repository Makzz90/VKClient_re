using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Video.Library;
using VKClient.Video.Localization;

namespace VKClient.Video.VideoCatalog
{
    public class AddToAlbumViewModel : ViewModelBase, ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>, IHandle<VideoAlbumAddedDeletedEvent>, IHandle
    {
        private Dictionary<long, bool> _addedRemovedDict = new Dictionary<long, bool>();
        private List<long> _albumsByVideoIds = new List<long>();
        private List<HeaderOption> _headerOptions = new List<HeaderOption>();
        private GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> _albumsVM;
        private long _ownerId;
        private long _vid;
        private HeaderOption _selectedOption;
        private HeaderOption _myAlbumsOption;
        private bool _headerOptionsInitialized;

        public GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> AlbumsVM
        {
            get
            {
                return this._albumsVM;
            }
        }

        public long TargetId
        {
            get
            {
                return this._selectedOption.ID;
            }
        }

        public string Title
        {
            get
            {
                return CommonResources.VideoNew_AddToAlbum.ToUpperInvariant();
            }
        }

        public List<HeaderOption> HeaderOptions
        {
            get
            {
                return this._headerOptions;
            }
            set
            {
                this._headerOptions = value;
                this.NotifyPropertyChanged<List<HeaderOption>>((Expression<Func<List<HeaderOption>>>)(() => this.HeaderOptions));
            }
        }

        public HeaderOption SelectedOption
        {
            get
            {
                return this._selectedOption;
            }
            set
            {
                if (this._selectedOption == value)
                    return;
                this.Save((Action<bool>)(res => { }));
                this._selectedOption = value;
                this.NotifyPropertyChanged<HeaderOption>((Expression<Func<HeaderOption>>)(() => this.SelectedOption));
                this._addedRemovedDict.Clear();
                this._albumsVM.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>)null, false);
            }
        }

        public Func<VKList<VideoAlbum>, ListWithCount<AlbumHeader>> ConverterFunc
        {
            get
            {
                return (Func<VKList<VideoAlbum>, ListWithCount<AlbumHeader>>)(list =>
                {
                    ListWithCount<AlbumHeader> listWithCount = new ListWithCount<AlbumHeader>();
                    listWithCount.TotalCount = list.count;
                    foreach (VideoAlbum va in list.items)
                    {
                        if (va.id != VideoAlbum.UPLOADED_ALBUM_ID)
                        {
                            AlbumHeader albumHeader = new AlbumHeader(va, false, false);
                            albumHeader.IsSelected = this._albumsByVideoIds.Contains(va.id);
                            albumHeader.PropertyChanged += new PropertyChangedEventHandler(this.ah_PropertyChanged);
                            listWithCount.List.Add(albumHeader);
                        }
                    }
                    return listWithCount;
                });
            }
        }

        public AddToAlbumViewModel(long ownerId, long vid)
        {
            this._ownerId = ownerId;
            this._vid = vid;
            this._myAlbumsOption = new HeaderOption()
            {
                ID = AppGlobalStateManager.Current.LoggedInUserId,
                Name = CommonResources.VideoCatalog_MyAlbums
            };
            this._selectedOption = this._myAlbumsOption;
            this._headerOptions.Add(this._selectedOption);
            this._albumsVM = new GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader>((ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>)this);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> caller, int offset, int count, Action<BackendResult<VKList<VideoAlbum>, ResultCode>> callback)
        {
            if (offset == 0)
                VideoService.Instance.GetAddToAlbumInfo(this.TargetId, this._ownerId, this._vid, (Action<BackendResult<GetAddToAlbumInfoResponse, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        this._albumsByVideoIds = res.ResultData.AlbumsByVideo;
                        this.InitializeHeaderOptionsBasedOnGroupData(res.ResultData.Groups.items);
                    }
                    callback(new BackendResult<VKList<VideoAlbum>, ResultCode>(res.ResultCode, res.ResultData.Albums));
                }));
            else
                VideoService.Instance.GetAlbums(Math.Abs(this.TargetId), this.TargetId < 0, true, offset, count, callback);
        }

        private void InitializeHeaderOptionsBasedOnGroupData(List<Group> list)
        {
            if (this._headerOptionsInitialized)
                return;
            List<HeaderOption> headerOptionList = new List<HeaderOption>();
            headerOptionList.Add(this._myAlbumsOption);
            foreach (Group group in list.Where<Group>((Func<Group, bool>)(g => g.can_upload_video == 1)))
                headerOptionList.Add(new HeaderOption()
                {
                    ID = -group.id,
                    Name = group.name
                });
            this.HeaderOptions = headerOptionList;
            this._headerOptionsInitialized = true;
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.VideoCatalog_NoAlbums;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, VideoResources.OneAlbumFrm, VideoResources.TwoFourAlbumsFrm, VideoResources.FiveAlbumsFrm, true, (string)null, false);
        }

        private void ah_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AlbumHeader albumHeader = sender as AlbumHeader;
            if (albumHeader == null)
                return;
            if (!this._addedRemovedDict.Keys.Contains<long>(albumHeader.VideoAlbum.id))
            {
                if (albumHeader.IsSelected)
                    this._addedRemovedDict[albumHeader.VideoAlbum.id] = true;
                else
                    this._addedRemovedDict[albumHeader.VideoAlbum.id] = false;
            }
            else
                this._addedRemovedDict.Remove(albumHeader.VideoAlbum.id);
        }

        public void Save(Action<bool> callback)
        {
            List<long> list1 = this._addedRemovedDict.Where<KeyValuePair<long, bool>>((Func<KeyValuePair<long, bool>, bool>)(kvp => kvp.Value)).Select<KeyValuePair<long, bool>, long>((Func<KeyValuePair<long, bool>, long>)(kvp => kvp.Key)).ToList<long>();
            List<long> list2 = this._addedRemovedDict.Where<KeyValuePair<long, bool>>((Func<KeyValuePair<long, bool>, bool>)(kvp => !kvp.Value)).Select<KeyValuePair<long, bool>, long>((Func<KeyValuePair<long, bool>, long>)(kvp => kvp.Key)).ToList<long>();
            if (list1.Count > 0 || list2.Count > 0)
                VideoService.Instance.AddRemoveToAlbums(this._vid, this._ownerId, this.TargetId, list1, list2, (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                        Execute.ExecuteOnUIThread((Action)(() => GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null)));
                    callback(res.ResultCode == ResultCode.Succeeded);
                }));
            else
                callback(true);
        }

        public void Handle(VideoAlbumAddedDeletedEvent message)
        {
            if (!message.IsAdded || message.OwnerId != this.TargetId)
                return;
            this.AlbumsVM.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>)null, false);
        }
    }
}
