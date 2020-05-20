using Microsoft.Xna.Framework.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Photos.Library
{
    public class PhotoPickerAlbumsViewModel : ViewModelBase
    {
        private ObservableCollection<AlbumHeaderTwoInARow> _albums = new ObservableCollection<AlbumHeaderTwoInARow>();
        private bool _isLoaded;

        public ObservableCollection<AlbumHeaderTwoInARow> Albums
        {
            get
            {
                return this._albums;
            }
        }

        public string Title
        {
            get
            {
                return CommonResources.CHOOSEALBUM;
            }
        }

        public string FooterText
        {
            get
            {
                return PhotosMainViewModel.GetAlbumsTextForCount(this._albums.Sum<AlbumHeaderTwoInARow>((Func<AlbumHeaderTwoInARow, int>)(a => a.GetItemsCount())));
            }
        }

        public Visibility FooterTextVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.FooterText) || !this._isLoaded)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string StatusText
        {
            get
            {
                return "";
            }
        }

        public Visibility StatusTextVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        public ICommand TryAgainCmd
        {
            get
            {
                return (ICommand)null;
            }
        }

        public Visibility TryAgainVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        public PhotoPickerAlbumsViewModel()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    List<AlbumHeader> albumHeaders = new List<AlbumHeader>();
                    using (MediaLibrary mediaLibrary = new MediaLibrary())
                    {
                        using (PictureAlbum rootPictureAlbum = mediaLibrary.RootPictureAlbum)
                        {
                            PictureAlbumCollection pictureAlbumCollection = rootPictureAlbum != null ? rootPictureAlbum.Albums : (PictureAlbumCollection)null;
                            if (pictureAlbumCollection != null)
                            {
                                if (pictureAlbumCollection.Count > 0)
                                {
                                    using (IEnumerator<PictureAlbum> enumerator = pictureAlbumCollection.GetEnumerator())
                                    {
                                        while (((IEnumerator)enumerator).MoveNext())
                                        {
                                            PictureAlbum current = enumerator.Current;
                                            if (current!=null)
                                            {
                                                PictureCollection pictures = current.Pictures;
                                                if (pictures != null)
                                                {
                                                    string str = current.Name ?? "";
                                                    AlbumHeader albumHeader1 = new AlbumHeader();
                                                    albumHeader1.AlbumId = str;
                                                    albumHeader1.AlbumName = str;
                                                    int count = pictures.Count;
                                                    albumHeader1.PhotosCount = count;
                                                    AlbumHeader albumHeader2 = albumHeader1;
                                                    string albumName = albumHeader2.AlbumName;
                                                    if (!(albumName == "Camera Roll"))
                                                    {
                                                        if (!(albumName == "Saved Pictures"))
                                                        {
                                                            if (!(albumName == "Sample Pictures"))
                                                            {
                                                                if (albumName == "Screenshots")
                                                                {
                                                                    albumHeader2.AlbumName = CommonResources.AlbumScreenshots;
                                                                    albumHeader2.OrderNo = 3;
                                                                }
                                                                else
                                                                    albumHeader2.OrderNo = int.MaxValue;
                                                            }
                                                            else
                                                            {
                                                                albumHeader2.AlbumName = CommonResources.AlbumSamplePictures;
                                                                albumHeader2.OrderNo = 2;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            albumHeader2.AlbumName = CommonResources.AlbumSavedPictures;
                                                            albumHeader2.OrderNo = 1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        albumHeader2.AlbumName = CommonResources.AlbumCameraRoll;
                                                        albumHeader2.OrderNo = 0;
                                                    }
                                                    Picture picture = ((IEnumerable<Picture>)pictures).FirstOrDefault<Picture>();
                                                    if (picture!=null)
                                                    {
                                                        try
                                                        {
                                                            albumHeader2.ImageStream = picture.GetThumbnail();
                                                        }
                                                        catch
                                                        {
                                                        }
                                                        try
                                                        {
                                                            picture.Dispose();
                                                        }
                                                        catch
                                                        {
                                                        }
                                                    }
                                                    albumHeaders.Add(albumHeader2);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    stopwatch.Stop();
                    Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        this._albums.Clear();
                        foreach (IEnumerable<AlbumHeader> source in albumHeaders.OrderBy<AlbumHeader, int>((Func<AlbumHeader, int>)(ah => ah.OrderNo)).Partition<AlbumHeader>(2))
                        {
                            List<AlbumHeader> list = source.ToList<AlbumHeader>();
                            AlbumHeaderTwoInARow albumHeaderTwoInArow = new AlbumHeaderTwoInARow()
                            {
                                AlbumHeader1 = list[0]
                            };
                            if (list.Count > 1)
                                albumHeaderTwoInArow.AlbumHeader2 = list[1];
                            this._albums.Add(albumHeaderTwoInArow);
                        }
                        this._isLoaded = true;
                        this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.FooterText));
                        this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.FooterTextVisibility));
                    }));
                }
                catch (Exception ex)
                {
                    Logger.Instance.ErrorAndSaveToIso("Failed to read gallery albums", ex);
                }
            }));
        }
    }
}
