using System;
using System.IO;
using System.Linq.Expressions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Photos.Library
{
  public class AlbumHeader : ViewModelBase
  {
    private string _albumName = string.Empty;
    private string _imageUri = string.Empty;
    private string _imageUri2 = "";
    private string _imageUri3 = "";
    private string _imageUriSmall = "";
    private int _orderNo;
    private Stream _imageStream;
    private int _photosCount;
    private Album _album;

    public int OrderNo
    {
      get
      {
        return this._orderNo;
      }
      set
      {
        this._orderNo = value;
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
        this.NotifyPropertyChanged<string>((() => this.AlbumName));
      }
    }

    public Stream ImageStream
    {
      get
      {
        return this._imageStream;
      }
      set
      {
        this._imageStream = value;
        this.NotifyPropertyChanged<Stream>((Expression<Func<Stream>>) (() => this.ImageStream));
      }
    }

    public string AlbumDesc
    {
      get
      {
        if (this.Album == null)
          return "";
        return this.Album.description ?? "";
      }
    }

    public string ImageUri
    {
      get
      {
        return this._imageUri;
      }
      set
      {
        this._imageUri = value;
        if (this.Album != null)
          this.Album.thumb_src = this._imageUri;
        this.NotifyPropertyChanged<string>((() => this.ImageUri));
        this.NotifyPropertyChanged<string>((() => this.ImageUriNoPlaceholder));
      }
    }

    public string ImageUriSmall
    {
      get
      {
        return this._imageUriSmall;
      }
      set
      {
        this._imageUriSmall = value;
        this.NotifyPropertyChanged<string>((() => this.ImageUriSmall));
      }
    }

    public string ImageUri2
    {
      get
      {
        return this._imageUri2;
      }
      set
      {
        this._imageUri2 = value;
        this.NotifyPropertyChanged<string>((() => this.ImageUri2));
      }
    }

    public string ImageUri3
    {
      get
      {
        return this._imageUri3;
      }
      set
      {
        this._imageUri3 = value;
        this.NotifyPropertyChanged<string>((() => this.ImageUri3));
      }
    }

    public string ImageUriNoPlaceholder
    {
      get
      {
        if (this.ImageUri != null && (this.ImageUri.EndsWith("vk.com/images/m_noalbum.png") || this.ImageUri.EndsWith(".gif")))
          return "";
        return this.ImageUri;
      }
    }

    public int PhotosCount
    {
      get
      {
        return this._photosCount;
      }
      set
      {
        this._photosCount = Math.Max(value, 0);
        this.NotifyPropertyChanged<string>((() => this.PhotosCountString));
      }
    }

    public string AlbumId { get; set; }

    public AlbumType AlbumType { get; set; }

    public string PhotosCountString
    {
      get
      {
        return CommonUtils.FormatPhotosCountString(this.PhotosCount);
      }
    }

    public Album Album
    {
      get
      {
        return this._album;
      }
      set
      {
        if (value != null)
          this._album = value.Copy();
        else
          this._album =  null;
      }
    }

    public void ReadDataFromAlbumField()
    {
      this.AlbumName = this.Album.title;
      this.PhotosCount = this.Album.size;
      this.ImageUri = this.Album.thumb_src;
      this.ImageUriSmall = this.Album.thumb_src_small;
      this.AlbumId = this.Album.aid;
      this.NotifyPropertyChanged<string>((() => this.AlbumDesc));
    }
  }
}
