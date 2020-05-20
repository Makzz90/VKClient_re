using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Localization;

namespace VKClient.Photos.Library
{
    public class PhotoAlbumViewModel : ViewModelBase, IHandle<PhotoUploadedToAlbum>, IHandle, IHandle<PhotoSetAsAlbumCover>, ICollectionDataProvider<PhotosListWithCount, AlbumPhotoHeaderFourInARow>
    {
        public static readonly int PhotosLoadCount = 40;
        private string _pageTitle = string.Empty;
        private string _albumName = string.Empty;
        private ObservableCollection<AlbumPhoto> _albumPhotos = new ObservableCollection<AlbumPhoto>();
        private double _headerOpacity = 1.0;
        private bool _isBusy;
        private readonly long _userOrGroupId;
        private readonly AlbumType _albumType;
        private readonly string _albumId;
        private readonly bool _isGroup;
        private readonly bool _forceCanUpload;
        private readonly int _adminLevel;
        private readonly PhotoAlbumViewModel.PhotoAlbumViewModelInput _inputData;
        private GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> _photosGenCol;
        private string _albumDescription;
        private int _photosCount;
        private string _thumbSrc;
        private bool _updatingCollection;
        private double _lastOffset;

        public GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> PhotosGenCol
        {
            get
            {
                return this._photosGenCol;
            }
        }

        public string FooterText
        {
            get
            {
                return this._photosGenCol.FooterText;
            }
        }

        public Visibility FooterTextVisibility
        {
            get
            {
                return this._photosGenCol.FooterTextVisibility;
            }
        }

        public string StatusText
        {
            get
            {
                return this._photosGenCol.StatusText;
            }
        }

        public Visibility StatusTextVisibility
        {
            get
            {
                return this._photosGenCol.StatusTextVisibility;
            }
        }

        public bool IsLoading
        {
            get
            {
                return this._photosGenCol.IsLoading;
            }
        }

        public Visibility IsLoadingVisibility
        {
            get
            {
                return this._photosGenCol.IsLoadingVisibility;
            }
        }

        public ICommand TryAgainCmd
        {
            get
            {
                return this._photosGenCol.TryAgainCmd;
            }
        }

        public Visibility TryAgainVisibility
        {
            get
            {
                return this._photosGenCol.TryAgainVisibility;
            }
        }

        public string AlbumId
        {
            get
            {
                return this._albumId;
            }
        }

        public AlbumType AType
        {
            get
            {
                return this._albumType;
            }
        }

        public ObservableCollection<AlbumPhoto> AlbumPhotos
        {
            get
            {
                return this._albumPhotos;
            }
        }

        public bool CanEditAlbum
        {
            get
            {
                if (this._albumType == AlbumType.NormalAlbum && (this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && !this._isGroup || this.EditableGroupAlbum))
                    return true;
                if (this._albumType == AlbumType.SavedPhotos)
                    return this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
                return false;
            }
        }

        public bool CanAddPhotos
        {
            get
            {
                if (this._albumType == AlbumType.SavedPhotos)
                    return false;
                if (!this.CanEditAlbum)
                    return this._forceCanUpload;
                return true;
            }
        }

        public bool CanRemovePhoto
        {
            get
            {
                if (this._albumType == AlbumType.PhotosWithUser)
                    return false;
                if (this.CanEditAlbum)
                    return true;
                if (!this._isGroup)
                    return this._userOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
                return false;
            }
        }

        public bool EditableGroupAlbum
        {
            get
            {
                if (this._isGroup)
                    return this._adminLevel > 1;
                return false;
            }
        }

        public string PageTitle
        {
            get
            {
                if (string.IsNullOrEmpty(this._pageTitle))
                    return PhotoResources.PhotosMainPage_MyPhotosTitle.ToUpperInvariant();
                return (this._pageTitle ?? "").ToUpperInvariant();
            }
            set
            {
                this._pageTitle = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PageTitle));
            }
        }

        public string Title
        {
            get
            {
                if (this.AlbumName != null)
                    return this.AlbumName.ToUpperInvariant();
                return "";
            }
        }

        public string AlbumName
        {
            get
            {
                return this._albumName;
            }
            set
            {
                this._albumName = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.AlbumName));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Title));
            }
        }

        public string AlbumDescription
        {
            get
            {
                return this._albumDescription;
            }
            set
            {
                this._albumDescription = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.AlbumDescription));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.HaveAlbumDescVisibility));
            }
        }

        public Visibility HaveAlbumDescVisibility
        {
            get
            {
                if (!string.IsNullOrEmpty(this.AlbumDescription))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public int PhotosCount
        {
            get
            {
                return this._photosCount;
            }
            private set
            {
                this._photosCount = value;
                this.NotifyPropertyChanged<int>((Expression<Func<int>>)(() => this.PhotosCount));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhotosCountStr));
            }
        }

        public string PhotosCountStr
        {
            get
            {
                if (this.PhotosCount <= 0)
                    return "";
                return CommonUtils.FormatPhotosCountString(this.PhotosCount);
            }
        }

        public string NoItemsDescription
        {
            get
            {
                return "";
            }
        }

        public Visibility NoItemsDescriptionVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        public Visibility NoContentBlockVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        public string NoContentImage
        {
            get
            {
                return "";
            }
        }

        public string NoContentText
        {
            get
            {
                return "";
            }
        }

        public Visibility NoContentNewsButtonsVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        private long OwnerId
        {
            get
            {
                if (this._userOrGroupId == 0L)
                    return AppGlobalStateManager.Current.LoggedInUserId;
                if (!this._isGroup)
                    return this._userOrGroupId;
                return -this._userOrGroupId;
            }
        }

        public string ThumbSrc
        {
            get
            {
                return this._thumbSrc;
            }
            set
            {
                if (!(this._thumbSrc != value))
                    return;
                this._thumbSrc = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.ThumbSrc));
            }
        }

        public double HeaderOpacity
        {
            get
            {
                return this._headerOpacity;
            }
            set
            {
                this._headerOpacity = value;
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.HeaderOpacity));
            }
        }

        public PhotoAlbumViewModel.PhotoAlbumViewModelInput InputData
        {
            get
            {
                return this._inputData;
            }
        }

        public Func<PhotosListWithCount, ListWithCount<AlbumPhotoHeaderFourInARow>> ConverterFunc
        {
            get
            {
                return (Func<PhotosListWithCount, ListWithCount<AlbumPhotoHeaderFourInARow>>)(plwc =>
                {
                    ListWithCount<AlbumPhotoHeaderFourInARow> listWithCount = new ListWithCount<AlbumPhotoHeaderFourInARow>();
                    if (this._lastOffset == 0.0)
                        this._albumPhotos.Clear();
                    List<Photo> response = plwc.response;
                    foreach (Photo photo in response)
                        this.AddPhotoToAlbumPhotos(new AlbumPhoto(photo, 0L), false);
                    foreach (IEnumerable<Photo> source in response.Partition<Photo>(4))
                    {
                        List<Photo> list = source.ToList<Photo>();
                        AlbumPhotoHeaderFourInARow headerFourInArow = new AlbumPhotoHeaderFourInARow(this.CanEditAlbum, this.CanRemovePhoto);
                        if (list.Count > 0)
                            headerFourInArow.Photo1 = new AlbumPhoto(list[0], 0L);
                        if (list.Count > 1)
                            headerFourInArow.Photo2 = new AlbumPhoto(list[1], 0L);
                        if (list.Count > 2)
                            headerFourInArow.Photo3 = new AlbumPhoto(list[2], 0L);
                        if (list.Count > 3)
                            headerFourInArow.Photo4 = new AlbumPhoto(list[3], 0L);
                        listWithCount.List.Add(headerFourInArow);
                    }
                    listWithCount.TotalCount = plwc.photosCount;
                    if (plwc.album != null && plwc.album.thumb_src != null)
                        this.ThumbSrc = plwc.album.thumb_src;
                    else if (this._albumPhotos.Count > 0)
                    {
                        Photo photo = this._albumPhotos.First<AlbumPhoto>().Photo;
                        this.ThumbSrc = photo.photo_807 ?? photo.src_big;
                    }
                    this.PhotosCount = plwc.photosCount;
                    return listWithCount;
                });
            }
        }

        public PhotoAlbumViewModel(PhotoAlbumViewModel.PhotoAlbumViewModelInput inputData)
        {
            this._inputData = inputData;
            this._userOrGroupId = inputData.UserOrGroupId;
            this._isGroup = inputData.IsGroup;
            this.PageTitle = inputData.PageTitle;
            this.AlbumName = inputData.AlbumName;
            this.AlbumDescription = inputData.AlbumDescription;
            this._albumType = inputData.AlbumType;
            this._albumId = inputData.AlbumId;
            this._photosCount = inputData.PhotosCount;
            this._adminLevel = inputData.AdminLevel;
            this._forceCanUpload = inputData.ForceCanUpload;
            this._albumPhotos.CollectionChanged += new NotifyCollectionChangedEventHandler(this._albumPhotos_CollectionChanged);
            this._photosGenCol = new GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow>((ICollectionDataProvider<PhotosListWithCount, AlbumPhotoHeaderFourInARow>)this);
            this._photosGenCol.PropertyChanged += new PropertyChangedEventHandler(this._photosGenCol_PropertyChanged);
            this._photosGenCol.LoadCount = 40;
            this._photosGenCol.ReloadCount = 40;
            EventAggregator.Current.Subscribe((object)this);
            if (!string.IsNullOrEmpty(this.AlbumName))
                return;
            this.LoadAlbumData();
        }

        private void _photosGenCol_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.FooterText));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.FooterTextVisibility));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.StatusText));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.StatusTextVisibility));
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsLoading));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.IsLoadingVisibility));
            this.NotifyPropertyChanged<ICommand>((Expression<Func<ICommand>>)(() => this.TryAgainCmd));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.TryAgainVisibility));
        }

        private void LoadAlbumData()
        {
            PhotosService.Current.GetAlbums(this._userOrGroupId, this._isGroup, long.Parse(this._albumId), (Action<BackendResult<VKList<Album>, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded || !res.ResultData.items.Any<Album>())
                    return;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    Album album = res.ResultData.items[0];
                    this.AlbumName = album.title;
                    this.AlbumDescription = album.description;
                    this.PhotosCount = album.size;
                }));
            }));
        }

        private void _albumPhotos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._updatingCollection || e.Action != NotifyCollectionChangedAction.Add)
                return;
            long afterPid = 0;
            long beforePid = 0;
            if (e.NewStartingIndex > 0)
                beforePid = this._albumPhotos[e.NewStartingIndex - 1].Photo.pid;
            if (e.NewStartingIndex < this._albumPhotos.Count - 1)
                afterPid = this._albumPhotos[e.NewStartingIndex + 1].Photo.pid;
            PhotosService.Current.ReorderPhotos(this.OwnerId, this._albumPhotos[e.NewStartingIndex].Photo.pid, beforePid, afterPid, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }));
            this.RebindHeadersToAlbumPhotos();
        }

        public void RefreshPhotos()
        {
            this._photosGenCol.LoadData(true, false, (Action<BackendResult<PhotosListWithCount, ResultCode>>)null, false);
        }

        public void LoadMorePhotos()
        {
            this._photosGenCol.LoadData(false, false, (Action<BackendResult<PhotosListWithCount, ResultCode>>)null, false);
        }

        public void DeletePhoto(Photo photo)
        {
            Func<AlbumPhoto, bool> func;
            PhotosService.Current.DeletePhoto(photo.pid, this.OwnerId, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    AlbumPhoto albumPhoto = this._albumPhotos.FirstOrDefault<AlbumPhoto>( (func = (Func<AlbumPhoto, bool>)(ap => ap.Photo.pid == photo.pid)));
                    if (albumPhoto != null)
                    {
                        this._albumPhotos.Remove(albumPhoto);
                        this.RebindHeadersToAlbumPhotos();
                        this.PhotosCount = this.PhotosCount - 1;
                        --this._photosGenCol.TotalCount;
                    }
                    EventAggregator.Current.Publish((object)new PhotoDeletedFromAlbum()
                    {
                        OwnerId = this.OwnerId,
                        AlbumId = this._albumId,
                        PhotoId = photo.pid
                    });
                    this.UpdateThumbAfterPhotosMoving();
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
            }))));
        }

        internal void DeletePhotos(List<AlbumPhoto> photosToBeDeleted)
        {
            PhotosService.Current.DeletePhotos(this.OwnerId, photosToBeDeleted.Select<AlbumPhoto, long>((Func<AlbumPhoto, long>)(p => p.Photo.pid)).ToList<long>());
            foreach (AlbumPhoto albumPhoto in photosToBeDeleted)
            {
                this._albumPhotos.Remove(albumPhoto);
                this.PhotosCount = this.PhotosCount - 1;
                --this._photosGenCol.TotalCount;
                EventAggregator.Current.Publish((object)new PhotoDeletedFromAlbum()
                {
                    OwnerId = this.OwnerId,
                    AlbumId = this._albumId,
                    PhotoId = albumPhoto.Photo.pid
                });
            }
            this.UpdateThumbAfterPhotosMoving();
            this.RebindHeadersToAlbumPhotos();
        }

        public void MakeCover(Photo photo)
        {
            PhotosService.Current.MakeCover(this._albumId, photo.pid, this.OwnerId, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }));
            EventAggregator.Current.Publish((object)new PhotoSetAsAlbumCover()
            {
                aid = this._albumId,
                pid = photo.pid,
                Photo = photo
            });
        }

        public void UploadPhoto(Stream photoDataStream, Action<BackendResult<Photo, ResultCode>> callback)
        {
            ImagePreprocessor.PreprocessImage(photoDataStream, VKConstants.ResizedImageSize, true, (Action<ImagePreprocessResult>)(pres =>
            {
                Stream stream = pres.Stream;
                byte[] numArray = new byte[stream.Length];
                stream.Read(numArray, 0, (int)stream.Length);
                stream.Close();
                this.SetInProgress(true, "");
                PhotosService.Current.UploadPhotoToAlbum(this._albumId, this._isGroup ? this._userOrGroupId : 0, numArray, (Action<BackendResult<Photo, ResultCode>>)(res =>
                {
                    this.SetInProgress(false, "");
                    if (res.ResultCode == ResultCode.Succeeded)
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            Photo resultData1 = res.ResultData;
                            EventAggregator current = EventAggregator.Current;
                            PhotoUploadedToAlbum photoUploadedToAlbum = new PhotoUploadedToAlbum();
                            photoUploadedToAlbum.photo = resultData1;
                            long pid = resultData1.pid;
                            photoUploadedToAlbum.pid = pid;
                            string str = resultData1.aid.ToString();
                            photoUploadedToAlbum.aid = str;
                            current.Publish((object)photoUploadedToAlbum);
                            if (string.IsNullOrEmpty(this.ThumbSrc))
                            {
                                EventAggregator.Current.Publish((object)new PhotoSetAsAlbumCover()
                                {
                                    aid = this._albumId,
                                    pid = resultData1.pid,
                                    Photo = resultData1
                                });
                            }
                            else
                            {
                                if (this._albumPhotos.Count <= 1)
                                    return;
                                Photo photo = this._albumPhotos[1].Photo;
                                if (!(this.ThumbSrc == photo.photo_75) && !(this.ThumbSrc == photo.photo_130) && (!(this.ThumbSrc == photo.photo_604) && !(this.ThumbSrc == photo.photo_807)) && (!(this.ThumbSrc == photo.photo_1280) && !(this.ThumbSrc == photo.photo_2560) && (!(this.ThumbSrc == photo.src) && !(this.ThumbSrc == photo.src_small))) && (!(this.ThumbSrc == photo.src_big) && !(this.ThumbSrc == photo.src_xbig) && !(this.ThumbSrc == photo.src_xxbig)))
                                    return;
                                Photo resultData2 = res.ResultData;
                                EventAggregator.Current.Publish((object)new PhotoSetAsAlbumCover()
                                {
                                    aid = this._albumId,
                                    Photo = resultData2,
                                    pid = resultData2.pid
                                });
                            }
                        }));
                    callback(res);
                }));
            }));
        }

        private void AddPhotoToAlbumPhotos(AlbumPhoto albumPhoto, bool insertAtTheBeginning = false)
        {
            this._updatingCollection = true;
            if (insertAtTheBeginning)
                this._albumPhotos.Insert(0, albumPhoto);
            else
                this._albumPhotos.Add(albumPhoto);
            this._updatingCollection = false;
        }

        private void RebindHeadersToAlbumPhotos()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this._photosGenCol._updatingCollection = true;
                this._photosGenCol.Collection.Clear();
                foreach (IEnumerable<AlbumPhoto> source in this._albumPhotos.Partition<AlbumPhoto>(4))
                {
                    List<AlbumPhoto> list = source.ToList<AlbumPhoto>();
                    AlbumPhotoHeaderFourInARow headerFourInArow = new AlbumPhotoHeaderFourInARow(this.CanEditAlbum, this.CanRemovePhoto);
                    if (list.Count > 0)
                        headerFourInArow.Photo1 = list[0];
                    if (list.Count > 1)
                        headerFourInArow.Photo2 = list[1];
                    if (list.Count > 2)
                        headerFourInArow.Photo3 = list[2];
                    if (list.Count > 3)
                        headerFourInArow.Photo4 = list[3];
                    this._photosGenCol.Collection.Add(headerFourInArow);
                }
                this._photosGenCol._updatingCollection = false;
                this._photosGenCol.NotifyChanged();
            }));
        }

        internal void MovePhotos(string albumId, List<long> pids, Action<bool> callback)
        {
            if (this._isBusy)
                return;
            this._isBusy = true;
            PhotosService.Current.MovePhotos(this.OwnerId, albumId, pids, (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
            {
                this._isBusy = false;
                this.SetInProgress(false, "");
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    EventAggregator.Current.Publish((object)new PhotosMovedToAlbum()
                    {
                        fromAlbumId = this._albumId,
                        toAlbumId = albumId,
                        photos = pids
                    });
                    List<AlbumPhoto> albumPhotoList = new List<AlbumPhoto>();
                    foreach (long pid1 in pids)
                    {
                        long pid = pid1;
                        AlbumPhoto albumPhoto = this._albumPhotos.FirstOrDefault<AlbumPhoto>((Func<AlbumPhoto, bool>)(p => p.Photo.pid == pid));
                        albumPhotoList.Add(albumPhoto);
                    }
                    foreach (AlbumPhoto albumPhoto in albumPhotoList)
                    {
                        this._albumPhotos.Remove(albumPhoto);
                        this.PhotosCount = this.PhotosCount - 1;
                        --this._photosGenCol.TotalCount;
                    }
                    this.RebindHeadersToAlbumPhotos();
                    callback(res.ResultCode == ResultCode.Succeeded);
                }));
                this._isBusy = false;
                this.SetInProgress(false, "");
            }));
        }

        public static void GetAlbumPhotos(AlbumType albumType, string albumId, long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
        {
            PhotosService current = PhotosService.Current;
            switch (albumType)
            {
                case AlbumType.AllPhotos:
                    current.GetAllPhotos(userOrGroupId, isGroup, offset, count, callback);
                    break;
                case AlbumType.ProfilePhotos:
                    current.GetProfilePhotos(userOrGroupId, offset, count, callback);
                    break;
                case AlbumType.PhotosWithUser:
                    current.GetUserPhotos(userOrGroupId, offset, count, callback);
                    break;
                case AlbumType.WallPhotos:
                    current.GetWallPhotos(userOrGroupId, isGroup, offset, count, callback);
                    break;
                case AlbumType.SavedPhotos:
                    current.GetSavedPhotos(userOrGroupId, isGroup, offset, count, callback);
                    break;
                case AlbumType.NormalAlbum:
                    current.GetAlbumPhotos(userOrGroupId, isGroup, albumId, offset, count, callback);
                    break;
            }
        }

        public void Handle(PhotoUploadedToAlbum message)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!(message.aid == this._albumId))
                    return;
                this.AddPhotoToAlbumPhotos(new AlbumPhoto(message.photo, 0L), true);
                this.PhotosCount = this.PhotosCount + 1;
                ++this._photosGenCol.TotalCount;
                this.RebindHeadersToAlbumPhotos();
            }));
        }

        public void GetData(GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> caller, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
        {
            this._lastOffset = (double)offset;
            PhotoAlbumViewModel.GetAlbumPhotos(this._albumType, this._albumId, this._userOrGroupId, this._isGroup, offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoPhotos;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePhotoFrm, CommonResources.TwoFourPhotosFrm, CommonResources.FivePhotosFrm, true, null, false);
        }

        public void Handle(PhotoSetAsAlbumCover message)
        {
            Photo photo1 = message.Photo;
            string str = photo1 != null ? photo1.photo_807 : (string)null;
            if (str == null)
            {
                Photo photo2 = message.Photo;
                str = photo2 != null ? photo2.src_big : (string)null;
            }
            this.ThumbSrc = str;
        }

        public void UpdateThumbAfterPhotosMoving()
        {
            if (!this._albumPhotos.Any<AlbumPhoto>())
            {
                EventAggregator.Current.Publish((object)new PhotoSetAsAlbumCover()
                {
                    aid = this._albumId
                });
            }
            else
            {
                ObservableCollection<AlbumPhoto> albumPhotos = this._albumPhotos;
                Func<AlbumPhoto, Photo> func = (Func<AlbumPhoto, Photo>)(p => p.Photo);
                if (albumPhotos.Select<AlbumPhoto, Photo>(func).Any<Photo>((Func<Photo, bool>)(photo =>
                {
                    if (!(this.ThumbSrc == photo.photo_75) && !(this.ThumbSrc == photo.photo_130) && (!(this.ThumbSrc == photo.photo_604) && !(this.ThumbSrc == photo.photo_807)) && (!(this.ThumbSrc == photo.photo_1280) && !(this.ThumbSrc == photo.photo_2560) && (!(this.ThumbSrc == photo.src) && !(this.ThumbSrc == photo.src_small))) && (!(this.ThumbSrc == photo.src_big) && !(this.ThumbSrc == photo.src_xbig)))
                        return this.ThumbSrc == photo.src_xxbig;
                    return true;
                })))
                    return;
                Photo photo1 = this._albumPhotos.First<AlbumPhoto>().Photo;
                EventAggregator.Current.Publish((object)new PhotoSetAsAlbumCover()
                {
                    aid = this._albumId,
                    Photo = photo1,
                    pid = photo1.pid
                });
            }
        }

        public class PhotoAlbumViewModelInput
        {
            private AlbumType _albumType = AlbumType.NormalAlbum;

            public long UserOrGroupId { get; set; }

            public bool IsGroup { get; set; }

            public string PageTitle { get; set; }

            public string AlbumName { get; set; }

            public string AlbumDescription { get; set; }

            public AlbumType AlbumType
            {
                get
                {
                    return this._albumType;
                }
                set
                {
                    this._albumType = value;
                }
            }

            public string AlbumId { get; set; }

            public int PhotosCount { get; set; }

            public int AdminLevel { get; set; }

            public bool ForceCanUpload { get; set; }
        }
    }
}
