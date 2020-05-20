using System;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Video.Library;
using VKClient.Video.Localization;

namespace VKClient.Video.VideoCatalog
{
    public class VideoAlbumsListViewModel : ViewModelBase, ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>, IHandle<VideoAlbumEditedEvent>, IHandle, IHandle<VideoAlbumAddedDeletedEvent>
    {
        private long _ownerId;
        private GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> _albumsGenCol;
        private bool _forceAllowCreateAlbum;

        public GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> AlbumsGenCol
        {
            get
            {
                return this._albumsGenCol;
            }
        }

        public string Title
        {
            get
            {
                return CommonResources.VideoCatalog_Albums.ToUpperInvariant();
            }
        }

        public Func<VKList<VideoAlbum>, ListWithCount<AlbumHeader>> ConverterFunc
        {
            get
            {
                return (Func<VKList<VideoAlbum>, ListWithCount<AlbumHeader>>)(list =>
                {
                    ListWithCount<AlbumHeader> listWithCount = new ListWithCount<AlbumHeader>();
                    foreach (VideoAlbum va in list.items)
                        listWithCount.List.Add(new AlbumHeader(va, false, this._forceAllowCreateAlbum));
                    listWithCount.TotalCount = list.count;
                    return listWithCount;
                });
            }
        }

        public VideoAlbumsListViewModel(long ownerId, bool forceAllowCreateAlbum)
        {
            this._ownerId = ownerId;
            this._forceAllowCreateAlbum = forceAllowCreateAlbum;
            this._albumsGenCol = new GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader>((ICollectionDataProvider<VKList<VideoAlbum>, AlbumHeader>)this);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> caller, int offset, int count, Action<BackendResult<VKList<VideoAlbum>, ResultCode>> callback)
        {
            VideoService.Instance.GetAlbums(Math.Abs(this._ownerId), this._ownerId < 0, false, offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<VKList<VideoAlbum>, AlbumHeader> caller, int count)
        {
            if (count <= 0)
                return VideoResources.NoAlbums;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, VideoResources.OneAlbumFrm, VideoResources.TwoFourAlbumsFrm, VideoResources.FiveAlbumsFrm, true, (string)null, false);
        }

        public void Handle(VideoAlbumAddedDeletedEvent message)
        {
            if (message.OwnerId != this._ownerId)
                return;
            this.AlbumsGenCol.LoadData(true, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>)null, false);
        }

        public void Handle(VideoAlbumEditedEvent message)
        {
            if (message.OwnerId != this._ownerId)
                return;
            AlbumHeader albumHeader = this.AlbumsGenCol.Collection.FirstOrDefault<AlbumHeader>((Func<AlbumHeader, bool>)(a =>
            {
                if (a.VideoAlbum.album_id == message.AlbumId)
                    return a.VideoAlbum.owner_id == message.OwnerId;
                return false;
            }));
            if (albumHeader == null)
                return;
            albumHeader.VideoAlbum.privacy = message.Privacy.ToStringList();
            albumHeader.VideoAlbum.title = message.Name;
        }
    }
}
