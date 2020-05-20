using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Audio.ViewModels
{
    public class AllAudioViewModel : ViewModelBase, ICollectionDataProvider<List<AudioObj>, AudioHeader>, IHandle<AudioTrackAddedRemoved>, IHandle, ISupportReorder<AudioHeader>
    {
        protected long _userOrGroupId;
        protected bool _isGroup;
        private bool _isPickMode;
        private GenericCollectionViewModel<List<AudioObj>, AudioHeader> _allTracks;
        private AllAlbumsViewModel _allAlbumsVM;

        public long AlbumId { get; private set; }

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

        public GenericCollectionViewModel<List<AudioObj>, AudioHeader> AllTracks
        {
            get
            {
                return this._allTracks;
            }
            set
            {
                this._allTracks = value;
                base.NotifyPropertyChanged<GenericCollectionViewModel<List<AudioObj>, AudioHeader>>(() => this.AllTracks);
            }
        }

        public AllAlbumsViewModel AllAlbumsVM
        {
            get
            {
                return this._allAlbumsVM;
            }
        }

        public string Title
        {
            get
            {
                return ((string)CommonResources.Audio).ToUpperInvariant();
            }
        }

        public Func<List<AudioObj>, ListWithCount<AudioHeader>> ConverterFunc
        {
            get
            {
                return (Func<List<AudioObj>, ListWithCount<AudioHeader>>)(input =>
                {
                    ListWithCount<AudioHeader> listWithCount = new ListWithCount<AudioHeader>();
                    List<AudioHeader> audioHeaderList = new List<AudioHeader>((IEnumerable<AudioHeader>)Enumerable.Select<AudioObj, AudioHeader>(input, (Func<AudioObj, AudioHeader>)(i => new AudioHeader(i, 0L))));
                    listWithCount.List = audioHeaderList;
                    return listWithCount;
                });
            }
        }

        public Action<bool, GenericCollectionViewModel<List<AudioObj>, AudioHeader>> ReportBusyCallback
        {
            get
            {
                return (Action<bool, GenericCollectionViewModel<List<AudioObj>, AudioHeader>>)((b, c) => { });
            }
        }
        public bool IsSavedAudiosAlbum//mod
        {
            get
            {
                return this.AlbumId == AllAlbumsViewModel.SAVED_ALBUM_ID;
            }
        }

        public AllAudioViewModel(long userOrGroupId, bool isGroup, bool isPickMode, long albumId = 0, long exludeAlbumId = 0)
        {
            this._isGroup = isGroup;
            this._userOrGroupId = userOrGroupId;
            this._isPickMode = isPickMode;
            this.AlbumId = albumId;
            this.AllTracks = new GenericCollectionViewModel<List<AudioObj>, AudioHeader>((ICollectionDataProvider<List<AudioObj>, AudioHeader>)this)
            {
                NoContentText = CommonResources.NoContent_Audios,
                NoContentImage = "../Resources/NoContentImages/Audios.png"
            };
            this.AllTracks.LoadCount = 80;
            this._allAlbumsVM = new AllAlbumsViewModel(userOrGroupId, isGroup, isPickMode, exludeAlbumId);
            EventAggregator.Current.Subscribe(this);
        }

        public void LoadMore(object linkedItem)
        {
            this.AllTracks.LoadMoreIfNeeded(linkedItem);
        }

        public void GetData(GenericCollectionViewModel<List<AudioObj>, AudioHeader> caller, int offset, int count, Action<BackendResult<List<AudioObj>, ResultCode>> callback)
        {
            if (this.AlbumId == 0)
                AudioService.Instance.GetAllAudio((Action<BackendResult<List<AudioObj>, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded && AudioCacheManager.Instance.CachedListForCurrentUser.Count > 0 && (!this._isGroup && this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId) || this._userOrGroupId == 0L)
                        callback(new BackendResult<List<AudioObj>, ResultCode>(ResultCode.Succeeded, AudioCacheManager.Instance.CachedListForCurrentUser));
                    else
                        callback(res);
                }), new long?(this.UserOrGroupId), this.IsGroup, new long?(), offset, count);
            else if (this.AlbumId == AllAlbumsViewModel.RECOMMENDED_ALBUM_ID)
                AudioService.Instance.GetRecommended(this.UserOrGroupId, (long)offset, (long)count, callback);
            else if (this.AlbumId == AllAlbumsViewModel.POPULAR_ALBUM_ID)
            {
                AudioService.Instance.GetPopular(offset, count, callback);
            }
            else if (this.AlbumId == AllAlbumsViewModel.SAVED_ALBUM_ID)//mod
            {
                callback.Invoke(new BackendResult<List<AudioObj>, ResultCode>(ResultCode.Succeeded, AudioCacheManager.Instance.CachedListForCurrentUser));
                return;
            }
            else
            {
                long? nullable1 = new long?(this.UserOrGroupId);
                long? nullable2 = new long?(this.AlbumId);
                AudioService.Instance.GetAllAudio(callback, nullable1, this.IsGroup, nullable2, offset, count);
            }
        }

        public string GetFooterTextForCount(int count)
        {
            if (count <= 0)
                return AudioResources.NoTracks;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneTrackFrm, AudioResources.TwoFourTracksFrm, AudioResources.FiveTracksFrm, true, null, false);
        }

        public void Handle(AudioTrackAddedRemoved message)
        {
            if (!this.IsGroup && this.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    if (message.Added && this.AlbumId == 0)
                    {
                        this.AllTracks.LoadData(true, false, null, false);
                        return;
                    }
                    Func<AudioHeader, bool> arg_5E_1 = (((AudioHeader h) => h.Track.aid == message.Audio.aid));

                    AudioHeader audioHeader = Enumerable.FirstOrDefault<AudioHeader>(this.AllTracks.Collection, arg_5E_1);
                    if (audioHeader != null)
                    {//
                        if (message.IsSavedAudiosAlbum && this.AlbumId != AllAlbumsViewModel.SAVED_ALBUM_ID)
                        {
                            audioHeader.NotifyChanged();
                            return;
                        }//
                        this.AllTracks.Delete(audioHeader);
                    }
                });
            }
        }


        internal async void DeleteAudios(List<AudioHeader> list)//todo:IsSavedAudiosAlbum
        {
            AudioService.Instance.DeleteAudios((List<long>)Enumerable.ToList<long>(Enumerable.Select<AudioHeader, long>(list, (Func<AudioHeader, long>)(a => a.Track.aid))));
            await AudioCacheManager.Instance.ClearCache((IEnumerable<string>)Enumerable.Select<AudioHeader, string>(list, (Func<AudioHeader, string>)(h => h.Track.Id)));
            List<AudioHeader>.Enumerator enumerator = list.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    AudioHeader current1 = enumerator.Current;
                    EventAggregator current2 = EventAggregator.Current;
                    AudioTrackAddedRemoved trackAddedRemoved = new AudioTrackAddedRemoved();
                    trackAddedRemoved.Added = false;
                    AudioObj track = current1.Track;
                    trackAddedRemoved.Audio = track;
                    current2.Publish(trackAddedRemoved);
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        internal void MoveTracksToAlbum(List<AudioHeader> headersToMove, AudioAlbum pickedAlbum, Action<bool> callback)
        {
            AudioService.Instance.MoveToAlbum((List<long>)Enumerable.ToList<long>(Enumerable.Select<AudioHeader, long>(headersToMove, (Func<AudioHeader, long>)(h => h.Track.aid))), pickedAlbum.album_id, (Action<BackendResult<object, ResultCode>>)(res =>
          {
              if (res.ResultCode == ResultCode.Succeeded)
                  callback(true);
              else
                  callback(false);
          }));
        }

        public void Reordered(AudioHeader item, AudioHeader before, AudioHeader after)
        {
            if (item == null)
                return;
            AudioService.Instance.ReorderAudio(item.Track.aid, AppGlobalStateManager.Current.LoggedInUserId, this.AlbumId, after == null ? 0L : after.Track.aid, before == null ? 0L : before.Track.aid, (Action<BackendResult<long, ResultCode>>)(res => { }));
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<AudioObj>, AudioHeader> caller, int count)
        {
            return this.GetFooterTextForCount(count);
        }
    }
}
