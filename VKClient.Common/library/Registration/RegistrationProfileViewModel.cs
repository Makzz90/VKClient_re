using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationProfileViewModel : ViewModelBase, ICompleteable, IBinarySerializable
  {
    private string _firstName = "";
    private string _lastName = "";
    private string _fullAvatarUri = "";
    private bool _isMale;
    private bool _isGenderSet;
    private bool _havePhoto;
    private Rect _photoCropRect;

    public bool HavePhoto
    {
      get
      {
        return this._havePhoto;
      }
      set
      {
        this._havePhoto = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.AvatarUri));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.HavePhoto));
      }
    }

    public Rect CropPhotoRect
    {
      get
      {
        return this._photoCropRect;
      }
    }

    public string FullAvatarUri
    {
      get
      {
        return this._fullAvatarUri;
      }
    }

    public bool IsCompleted
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.FirstName) && !string.IsNullOrWhiteSpace(this.LastName))
          return this._isGenderSet;
        return false;
      }
    }

    public string AvatarUri
    {
      get
      {
        if (this._havePhoto)
          return string.Concat("cropped", this._fullAvatarUri);
        return "";
      }
    }

    public string FirstName
    {
      get
      {
        return this._firstName;
      }
      set
      {
        this._firstName = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.FirstName));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
      }
    }

    public string LastName
    {
      get
      {
        return this._lastName;
      }
      set
      {
        this._lastName = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.LastName));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
      }
    }

    public bool IsMale
    {
      get
      {
        if (this._isGenderSet)
          return this._isMale;
        return false;
      }
      set
      {
        this._isMale = value;
        this._isGenderSet = true;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsMale));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsFemale));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
      }
    }

    public bool IsFemale
    {
      get
      {
        if (this._isGenderSet)
          return !this._isMale;
        return false;
      }
      set
      {
        this._isMale = !value;
        this._isGenderSet = true;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsMale));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsFemale));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsCompleted));
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this._firstName);
      writer.WriteString(this._lastName);
      writer.Write(this._isMale);
      writer.Write(this._isGenderSet);
      writer.Write(this._havePhoto);
      writer.Write(this._fullAvatarUri);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._firstName = reader.ReadString();
      this._lastName = reader.ReadString();
      this._isMale = reader.ReadBoolean();
      this._isGenderSet = reader.ReadBoolean();
      this._havePhoto = reader.ReadBoolean();
      this._fullAvatarUri = reader.ReadString();
    }

    internal void DeletePhoto()
    {
      this.HavePhoto = false;
    }

    internal void SetUserPhoto(Stream stream, Rect rect)
    {
        this._photoCropRect = rect;
        ImagePreprocessor.PreprocessImage(stream, 1500000, false, (Action<ImagePreprocessResult>)(imPR => Execute.ExecuteOnUIThread((Action)(() =>
        {
            BitmapImage bitmapImage = new BitmapImage();
            Stream stream1 = imPR.Stream;
            bitmapImage.SetSource(stream1);
            WriteableBitmap bmp = new WriteableBitmap((BitmapSource)bitmapImage);
            WriteableBitmap bitmap = bmp.Crop(new Rect()
            {
                Width = (double)bmp.PixelWidth * rect.Width,
                Height = (double)bmp.PixelHeight * rect.Height,
                Y = rect.Top * (double)bmp.PixelHeight,
                X = rect.Left * (double)bmp.PixelWidth
            });
            try
            {
                if (!string.IsNullOrWhiteSpace(this._fullAvatarUri))
                    File.Delete(this._fullAvatarUri);
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.SaveJpeg((Stream)memoryStream, bitmap.PixelWidth, bitmap.PixelHeight, 0, 90);
                    memoryStream.Position = 0L;
                    this._fullAvatarUri = Guid.NewGuid().ToString();
                    ImageCache.Current.TrySetImageForUri("cropped" + this._fullAvatarUri, (Stream)memoryStream);
                    stream.Position = 0L;
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(this._fullAvatarUri, FileMode.Create, FileAccess.Write))
                        StreamUtils.CopyStream(stream, (Stream)storageFileStream, (Action<double>)null, (Cancellation)null, 0L);
                    this.HavePhoto = true;
                }
            }
            catch
            {
            }
        }))));
    }
  }
}
