using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Localization;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Audio.ViewModels
{
    public class AllAlbumsViewModel : ViewModelBase, ICollectionDataProvider<ListResponse<AudioAlbum>, AudioAlbumHeader>, IHandle<AudioAlbumCreatedEditedDeleted>, IHandle
    {
        public static readonly long SAVED_ALBUM_ID = -99;//mod
        public static readonly long RECOMMENDED_ALBUM_ID = -100;
        public static readonly long POPULAR_ALBUM_ID = -101;
        protected long _userOrGroupId;
        protected bool _isGroup;
        private bool _isPickMode;
        private long _excludeAlbumId;
        private GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbumHeader> _allAlbums;
        private bool _addToOffset;
        private bool _substractFromOffset;

        public long UserOrGroupId
        {
            get
            {
                return this._userOrGroupId;
            }
        }

        public bool IsGroup
        {
            get
            {
                return this._isGroup;
            }
        }

        public GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbumHeader> AllAlbums
        {
            get
            {
                return this._allAlbums;
            }
            set
            {
                this._allAlbums = value;
                base.NotifyPropertyChanged<GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbumHeader>>(() => this.AllAlbums);
            }
        }

        public Func<ListResponse<AudioAlbum>, ListWithCount<AudioAlbumHeader>> ConverterFunc
        {
            get
            {
                return (Func<ListResponse<AudioAlbum>, ListWithCount<AudioAlbumHeader>>)(input => new ListWithCount<AudioAlbumHeader>()
                {
                    TotalCount = input.TotalCount,
                    List = ((IEnumerable<AudioAlbumHeader>)Enumerable.Select<AudioAlbum, AudioAlbumHeader>(input.Data, (Func<AudioAlbum, AudioAlbumHeader>)(a => new AudioAlbumHeader(a)))).ToList<AudioAlbumHeader>()
                });
            }
        }

        public Action<bool, GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbum>> ReportBusyCallback
        {
            get
            {
                return (Action<bool, GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbum>>)((b, c) => { });
            }
        }

        public AllAlbumsViewModel(long userOrGroupId, bool isGroup, bool isPickMode, long excludeAlbumId = 0)
        {
            this._isGroup = isGroup;
            this._userOrGroupId = userOrGroupId;
            this._isPickMode = isPickMode;
            this._excludeAlbumId = excludeAlbumId;
            this.AllAlbums = new GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbumHeader>((ICollectionDataProvider<ListResponse<AudioAlbum>, AudioAlbumHeader>)this);
            EventAggregator.Current.Subscribe(this);
        }

        public void LoadMore(object linkedItem)
        {
            this.AllAlbums.LoadMoreIfNeeded(linkedItem);
        }

        public void GetData(GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbumHeader> caller, int offset, int count, Action<BackendResult<ListResponse<AudioAlbum>, ResultCode>> callback)
        {
            if (offset == 0)
            {
                this._addToOffset = false;
                this._substractFromOffset = false;
            }
            if (this._addToOffset)
                ++offset;
            if (this._substractFromOffset)
            {
                --offset;
                --offset;
                --offset;
            }
            AudioService.Instance.GetUserAlbums((Action<BackendResult<ListResponse<AudioAlbum>, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    if (!this._isPickMode && offset == 0 && !this.IsGroup)
                    {
                        //mod
                        ++res.ResultData.TotalCount;
                        res.ResultData.Data.Insert(0, new AudioAlbum()
                        {
                            title = VKClient.Common.Localization.CommonResources.Audio_Cached,
                            album_id = AllAlbumsViewModel.SAVED_ALBUM_ID
                        });
                        //
                        ++res.ResultData.TotalCount;
                        res.ResultData.Data.Insert(0, new AudioAlbum()
                        {
                            title = AudioResources.Recommended,
                            album_id = AllAlbumsViewModel.RECOMMENDED_ALBUM_ID
                        });
                        ++res.ResultData.TotalCount;
                        res.ResultData.Data.Insert(1, new AudioAlbum()
                        {
                            title = AudioResources.Popular,
                            album_id = AllAlbumsViewModel.POPULAR_ALBUM_ID
                        });
                        this._substractFromOffset = true;
                    }
                    if (this._excludeAlbumId != 0L)
                    {
                        --res.ResultData.TotalCount;
                        AudioAlbum audioAlbum = (AudioAlbum)Enumerable.FirstOrDefault<AudioAlbum>(res.ResultData.Data, (Func<AudioAlbum, bool>)(a => a.album_id == this._excludeAlbumId));
                        if (audioAlbum != null)
                        {
                            res.ResultData.Data.Remove(audioAlbum);
                            this._addToOffset = true;
                        }
                    }
                }
                callback(res);
            }), new long?(this.UserOrGroupId), this.IsGroup, offset, count);
        }

        public string GetFooterTextForCount(int count)
        {
            if (count <= 0)
                return AudioResources.NoAlbums;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneAlbumFrm, AudioResources.TwoFourAlbumsFrm, AudioResources.FiveAlbumsFrm, true, null, false);
        }

        internal void CreateEditAlbum(AudioAlbum album)
        {
            if (album.album_id == 0L)
            {
                AudioService.Instance.CreateAlbum(album.title, (Action<BackendResult<AudioAlbum, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    album.owner_id = AppGlobalStateManager.Current.LoggedInUserId;
                    album.album_id = res.ResultData.album_id;
                    EventAggregator.Current.Publish(new AudioAlbumCreatedEditedDeleted()
                    {
                        Created = new bool?(true),
                        Album = album
                    });
                }));
            }
            else
            {
                AudioService.Instance.EditAlbum(album.album_id, album.title, (Action<BackendResult<object, ResultCode>>)(res => { }));
                EventAggregator.Current.Publish(new AudioAlbumCreatedEditedDeleted()
                {
                    Created = new bool?(false),
                    Album = album
                });
            }
        }

        public void Handle(AudioAlbumCreatedEditedDeleted message)
        {
            if (message.Album.owner_id != this.UserOrGroupId)
                return;
            bool? created = message.Created;
            if (created.HasValue)
            {
                created = message.Created;
                if (created.Value)
                {
                    int ind = 0;
                    if (this._allAlbums.Collection.Count > 0 && this._allAlbums.Collection.First<AudioAlbumHeader>().Album.album_id == AllAlbumsViewModel.RECOMMENDED_ALBUM_ID)
                        ++ind;
                    if (this._allAlbums.Collection.Count > 1 && this._allAlbums.Collection[1].Album.album_id == AllAlbumsViewModel.POPULAR_ALBUM_ID)
                        ++ind;
                    this._allAlbums.Insert(new AudioAlbumHeader(message.Album), ind);
                    return;
                }
            }
            created = message.Created;
            if (created.HasValue)
            {
                AudioAlbumHeader audioAlbumHeader = (AudioAlbumHeader)Enumerable.FirstOrDefault<AudioAlbumHeader>(this._allAlbums.Collection, (Func<AudioAlbumHeader, bool>)(a => a.Album.album_id == message.Album.album_id));
                if (audioAlbumHeader == null)
                    return;
                audioAlbumHeader.Title = message.Album.title;
            }
            else
            {
                AudioAlbumHeader audioAlbumHeader = (AudioAlbumHeader)Enumerable.FirstOrDefault<AudioAlbumHeader>(this._allAlbums.Collection, (Func<AudioAlbumHeader, bool>)(a => a.Album.album_id == message.Album.album_id));
                if (audioAlbumHeader == null)
                    return;
                this._allAlbums.Delete(audioAlbumHeader);
            }
        }

        internal void DeleteAlbum(AudioAlbumHeader h)
        {
            AudioService.Instance.DeleteAlbum(h.Album.album_id, (Action<BackendResult<object, ResultCode>>)(res => { }));
            EventAggregator.Current.Publish(new AudioAlbumCreatedEditedDeleted()
            {
                Album = h.Album,
                Created = new bool?()
            });
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<ListResponse<AudioAlbum>, AudioAlbumHeader> caller, int count)
        {
            return this.GetFooterTextForCount(count);
        }
    }
}
