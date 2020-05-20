using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Photos.Library
{
  public class AlbumPhoto : ViewModelBase
  {
    private static double _reqDimension = -1.0;
    private Photo _photo;
    private bool _isSelected;
    private string _albumId;
    private int _seqNo;
    private Func<AlbumPhoto, bool, Stream> _getImageFunc;

    public long MessageId { get; private set; }

    private static double RequiredDimension
    {
      get
      {
        if (AlbumPhoto._reqDimension == -1.0)
          AlbumPhoto._reqDimension = 117.0 * ((double) ScaleFactor.GetRealScaleFactor() / 100.0);
        return AlbumPhoto._reqDimension;
      }
    }

    public Photo Photo
    {
      get
      {
        return this._photo;
      }
    }

    public string Description
    {
      get
      {
        return this._photo.text ?? "";
      }
    }

    public string DateStr
    {
      get
      {
        return UIStringFormatterHelper.FormatDateTimeForUI(this._photo.created);
      }
    }

    public string Src
    {
      get
      {
        if (this._photo != null)
          return this._photo.photo_130;
        return  null;
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        if (this._isSelected == value)
          return;
        this._isSelected = value;
        base.NotifyPropertyChanged<bool>(() => this.IsSelected);
        base.NotifyPropertyChanged<string>(() => this.SelectUnselectImageUri);
        base.NotifyPropertyChanged<string>(() => this.SelectionStateIconUri);
        base.NotifyPropertyChanged<Visibility>(() => this.IsSelectedVisibility);
        base.NotifyPropertyChanged<Visibility>(() => this.IsNotSelectedVisibility);
      }
    }

    public string SelectUnselectImageUri
    {
      get
      {
        return !this.IsSelected ? "/VKClient.Common;component/Resources/PhotoChooser-Check-WXGA.png" : "/VKClient.Common;component/Resources/PhotoChooser-Checked-WXGA.png";
      }
    }

    public string SelectionStateIconUri
    {
      get
      {
        return !this.IsSelected ? "/Resources/AttachmentPicker/Unselected56px.png" : "/Resources/AttachmentPicker/Selected56px.png";
      }
    }

    public Visibility IsSelectedVisibility
    {
      get
      {
        if (!this._isSelected)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility IsNotSelectedVisibility
    {
      get
      {
        if (this._isSelected)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public int SeqNo
    {
      get
      {
        return this._seqNo;
      }
    }

    public string AlbumId
    {
      get
      {
        return this._albumId;
      }
    }

    public Stream ThumbnailStream
    {
      get
      {
        return this._getImageFunc(this, true);
      }
    }

    public Stream ImageStream
    {
      get
      {
        return this._getImageFunc(this, false);
      }
    }

    public double Width { get; set; }

    public double Height { get; set; }

    public AlbumPhoto(Photo photo, long messageId = 0)
    {
      this._photo = photo;
      this.MessageId = messageId;
    }

    public AlbumPhoto(string albumId, int seqNo, Func<AlbumPhoto, bool, Stream> getImageFunc, double width, double height)
    {
      this._albumId = albumId;
      this._seqNo = seqNo;
      this._getImageFunc = getImageFunc;
      this.Width = width;
      this.Height = height;
    }

    public void NotifyUpdateThumbnail()
    {
        base.NotifyPropertyChanged<Stream>(() => this.ThumbnailStream);
    }
  }
}
