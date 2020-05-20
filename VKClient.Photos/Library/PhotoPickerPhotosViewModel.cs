using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.ImageEditor;

namespace VKClient.Photos.Library
{
    public class PhotoPickerPhotosViewModel : ViewModelBase, IPhotoPickerPhotosViewModel
    {
        private ObservableCollection<AlbumPhotoHeaderFourInARow> _photos = new ObservableCollection<AlbumPhotoHeaderFourInARow>();
        private ObservableCollection<AlbumPhoto> _albumPhotos = new ObservableCollection<AlbumPhoto>();
        private List<AlbumPhoto> _selectedPhotos = new List<AlbumPhoto>();
        private HashSet<long> _timestamps = new HashSet<long>();
        private int _recentlyAddedImageInd = -1;
        private int _countToLoad = 100;
        private string _albumId;
        private bool _isLoaded;
        private int _maxAllowedToSelect;
        private ImageEditorViewModel _imageEditor;
        private bool _ownPhotoPick;
        private int _totalCount;
        private bool _isLoading;

        public List<AlbumPhoto> SelectedPhotos
        {
            get
            {
                return this._selectedPhotos;
            }
        }

        public ObservableCollection<AlbumPhotoHeaderFourInARow> Photos
        {
            get
            {
                return this._photos;
            }
        }

        public ObservableCollection<AlbumPhoto> AlbumPhotos
        {
            get
            {
                return this._albumPhotos;
            }
            set
            {
                this._albumPhotos = value;
                this.NotifyPropertyChanged<ObservableCollection<AlbumPhoto>>((Expression<Func<ObservableCollection<AlbumPhoto>>>)(() => this.AlbumPhotos));
            }
        }

        public int MaxAllowedToSelect
        {
            get
            {
                return this._maxAllowedToSelect;
            }
        }

        public int SelectedCount
        {
            get
            {
                return this._selectedPhotos.Count;
            }
        }

        public bool CanTakePicture
        {
            get
            {
                return this._albumId == "Camera Roll";
            }
        }

        private bool IsLoaded
        {
            get
            {
                return this._isLoaded;
            }
            set
            {
                if (this._isLoaded == value)
                    return;
                this._isLoaded = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsLoaded));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.PhotosCountStr));
            }
        }

        public string PhotosCountStr
        {
            get
            {
                if (this._isLoaded)
                    return CommonUtils.FormatPhotosCountString(this._photos.Sum<AlbumPhotoHeaderFourInARow>((Func<AlbumPhotoHeaderFourInARow, int>)(p => p.GetItemsCount())));
                return "";
            }
        }

        public int PhotosCount
        {
            get
            {
                return this._photos.Sum<AlbumPhotoHeaderFourInARow>((Func<AlbumPhotoHeaderFourInARow, int>)(p => p.GetItemsCount()));
            }
        }

        public string Title
        {
            get
            {
                return this.AlbumName.ToUpperInvariant();
            }
        }

        public string AlbumName
        {
            get
            {
                string albumId = this._albumId;
                if (albumId == "Camera Roll")
                    return CommonResources.AlbumCameraRoll.ToLowerInvariant();
                if (albumId == "Saved Pictures")
                    return CommonResources.AlbumSavedPictures.ToLowerInvariant();
                if (albumId == "Sample Pictures")
                    return CommonResources.AlbumSamplePictures.ToLowerInvariant();
                if (albumId == "Screenshots")
                    return CommonResources.AlbumScreenshots.ToLowerInvariant();
                return this._albumId;
            }
        }

        public string AlbumId
        {
            get
            {
                return this._albumId;
            }
            set
            {
                if (!(this._albumId != value))
                    return;
                this._albumId = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.AlbumId));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.AlbumName));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Title));
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.CanTakePicture));
                this.LoadData(true, (Action)null);
            }
        }

        public ImageEditorViewModel ImageEditor
        {
            get
            {
                return this._imageEditor;
            }
        }

        public bool OwnPhotoPick
        {
            get
            {
                return this._ownPhotoPick;
            }
        }

        public int TotalCount
        {
            get
            {
                return this._totalCount;
            }
        }

        public int RecentlyAddedImageInd
        {
            get
            {
                return this._recentlyAddedImageInd;
            }
        }

        public int CountToLoad
        {
            get
            {
                return this._countToLoad;
            }
            set
            {
                this._countToLoad = value;
            }
        }

        public bool SuppressEXIFFetch
        {
            get
            {
                return this.ImageEditor.SuppressParseEXIF;
            }
            set
            {
                this.ImageEditor.SuppressParseEXIF = value;
            }
        }

        public PhotoPickerPhotosViewModel(int maxAllowedToSelect, bool ownPhotoPick)
        {
            this._albumId = "Camera Roll";
            this._maxAllowedToSelect = maxAllowedToSelect;
            this._ownPhotoPick = ownPhotoPick;
            this._imageEditor = new ImageEditorViewModel();
        }

        public void CleanupSession()
        {
            this._imageEditor.CleanupSession();
        }

        public void LoadData(bool refresh = true, Action callback = null)
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            if (refresh)
            {
                this._imageEditor.ResetCachedMediaLibrary();
                this._photos.Clear();
                this._albumPhotos.Clear();
            }
            this.IsLoaded = false;
            this.SetInProgress(true, "");
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    List<AlbumPhoto> photoHeaders = new List<AlbumPhoto>();
                    HashSet<long> longSet1 = new HashSet<long>();
                    using (MediaLibrary mediaLibrary = new MediaLibrary())
                    {
                        using (PictureAlbum pictureAlbum = ((IEnumerable<PictureAlbum>)mediaLibrary.RootPictureAlbum.Albums).FirstOrDefault<PictureAlbum>((Func<PictureAlbum, bool>)(a => a.Name == this._albumId)))
                        {
                            this._recentlyAddedImageInd = -1;
                            if ((pictureAlbum!= null))
                            {
                                int count = pictureAlbum.Pictures.Count;
                                this._totalCount = count;
                                int num1;
                                for (int i = count - 1 - this._albumPhotos.Count; i >= 0; i = num1 - 1)
                                {
                                    double width;
                                    double height;
                                    using (Picture picture = pictureAlbum.Pictures[i])
                                    {
                                        HashSet<long> longSet2 = longSet1;
                                        DateTime date = picture.Date;
                                        long ticks1 = date.Ticks;
                                        longSet2.Add(ticks1);
                                        if (this._timestamps != null)
                                        {
                                            HashSet<long> timestamps = this._timestamps;
                                            date = picture.Date;
                                            long ticks2 = date.Ticks;
                                            if (!timestamps.Contains(ticks2))
                                                this._recentlyAddedImageInd = count - 1 - this._albumPhotos.Count - i;
                                        }
                                        width = (double)picture.Width;
                                        height = (double)picture.Height;
                                    }
                                    AlbumPhoto albumPhoto = new AlbumPhoto(this._albumId, i, (Func<AlbumPhoto, bool, Stream>)((ap, preview) => this._imageEditor.GetImageStream(ap.AlbumId, ap.SeqNo, preview)), width, height);
                                    if (this._selectedPhotos.Any<AlbumPhoto>((Func<AlbumPhoto, bool>)(p =>
                                    {
                                        if (p.SeqNo == i)
                                            return p.AlbumId == this._albumId;
                                        return false;
                                    })))
                                        albumPhoto.IsSelected = true;
                                    albumPhoto.PropertyChanged += new PropertyChangedEventHandler(this.albumPhoto_PropertyChanged);
                                    photoHeaders.Add(albumPhoto);
                                    if (photoHeaders.Count != this._countToLoad)
                                        num1 = i;
                                    else
                                        break;
                                }
                                if (refresh)
                                {
                                    this._timestamps = longSet1;
                                }
                                else
                                {
                                    foreach (long num2 in longSet1)
                                    {
                                        if (!this._timestamps.Contains(num2))
                                            this._timestamps.Add(num2);
                                    }
                                }
                                Execute.ExecuteOnUIThread((Action)(() =>
                                {
                                    foreach (AlbumPhoto albumPhoto in photoHeaders)
                                        this.AlbumPhotos.Add(albumPhoto);
                                    foreach (IEnumerable<AlbumPhoto> photos in photoHeaders.Partition<AlbumPhoto>(4))
                                        this._photos.Add(new AlbumPhotoHeaderFourInARow(photos)
                                        {
                                            SelectionOpacity = this._ownPhotoPick ? 0.0 : 1.0
                                        });
                                    if (this._albumPhotos.Count == this.PhotosCount)
                                        this.IsLoaded = true;
                                    this.SetInProgress(false, "");
                                    if (callback != null)
                                        callback();
                                    this._isLoading = false;
                                }));
                            }
                            else
                            {
                                this.SetInProgress(false, "");
                                if (callback != null)
                                    callback();
                                this._isLoading = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("Failed to read gallery photos ", ex);
                }
            }));
        }

        private void albumPhoto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsSelected"))
                return;
            AlbumPhoto ap = sender as AlbumPhoto;
            if (ap == null)
                return;
            if (ap.IsSelected && !this._selectedPhotos.Any<AlbumPhoto>((Func<AlbumPhoto, bool>)(p =>
            {
                if (p.AlbumId == ap.AlbumId)
                    return p.SeqNo == ap.SeqNo;
                return false;
            })))
            {
                if (this._selectedPhotos.Count == this._maxAllowedToSelect)
                {
                    ap.IsSelected = false;
                }
                else
                {
                    this._selectedPhotos.Add(ap);
                    this.NotifySelectionChanged();
                }
            }
            if (ap.IsSelected)
                return;
            AlbumPhoto albumPhoto = this._selectedPhotos.FirstOrDefault<AlbumPhoto>((Func<AlbumPhoto, bool>)(p =>
            {
                if (p.AlbumId == ap.AlbumId)
                    return p.SeqNo == ap.SeqNo;
                return false;
            }));
            if (albumPhoto == null)
                return;
            this._selectedPhotos.Remove(albumPhoto);
            this.NotifySelectionChanged();
        }

        private void NotifySelectionChanged()
        {
            this.NotifyPropertyChanged<int>((Expression<Func<int>>)(() => this.SelectedCount));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Title));
        }

        private AlbumPhoto GetAlbumPhotoBySeqNo(int seqNo)
        {
            int index = (this._totalCount - seqNo - 1) / 4;
            if (index < this._photos.Count)
                return this._photos[index].GetAsAlbumPhotos().FirstOrDefault<AlbumPhoto>((Func<AlbumPhoto, bool>)(p => p.SeqNo == seqNo));
            return (AlbumPhoto)null;
        }

        internal void HandleEffectUpdate(string albumId, int seqNo)
        {
            if (!(this._albumId == albumId))
                return;
            AlbumPhoto albumPhotoBySeqNo = this.GetAlbumPhotoBySeqNo(seqNo);
            if (albumPhotoBySeqNo == null)
                return;
            albumPhotoBySeqNo.NotifyUpdateThumbnail();
        }
    }
}
