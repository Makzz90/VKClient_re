using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
  public sealed class FriendRecommendationItem : INotifyPropertyChanged, IHandle<FriendRequestSent>, IHandle, IHandle<SubscriptionCancelled>
  {
    private const double PhotoWidth = 256.0;
    private const double PhotoHeight = 214.0;
    private string _name;
    private string _description;
    private string _addToFriendsButtonIcon;
    private SolidColorBrush _addToFriendsButtonForeground;
    private Thickness _nameBlockMargin;
    private string _photoImageSource;
    private double _photoImageWidth;
    private double _photoImageHeight;
    private Thickness _photoImageMargin;
    private Rect _photoImageClipRect;
    private FriendRecommendationItemType _type;

    public string Name
    {
      get
      {
        return this._name;
      }
      set
      {
        if (this._name == value)
          return;
        this._name = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Name));
      }
    }

    public string Description
    {
      get
      {
        return this._description;
      }
      set
      {
        if (this._description == value)
          return;
        this._description = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Description));
      }
    }

    public string AddToFriendsButtonIcon
    {
      get
      {
        return this._addToFriendsButtonIcon;
      }
      set
      {
        if (this._addToFriendsButtonIcon == value)
          return;
        this._addToFriendsButtonIcon = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.AddToFriendsButtonIcon));
      }
    }

    public SolidColorBrush AddToFriendsButtonForeground
    {
      get
      {
        return this._addToFriendsButtonForeground;
      }
      set
      {
        if (this._addToFriendsButtonForeground == value)
          return;
        this._addToFriendsButtonForeground = value;
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.AddToFriendsButtonForeground));
      }
    }

    public Thickness NameBlockMargin
    {
      get
      {
        return this._nameBlockMargin;
      }
      set
      {
        if (this._nameBlockMargin == value)
          return;
        this._nameBlockMargin = value;
        this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>) (() => this.NameBlockMargin));
      }
    }

    public string PhotoImageSource
    {
      get
      {
        return this._photoImageSource;
      }
      set
      {
        if (value == null || value.Contains("deactivated"))
          value = "../Resources/deactivatedUser.png";
        else if (value.Contains("camera"))
          value = "../Resources/Photo_Placeholder.png";
        if (this._photoImageSource == value)
          return;
        this._photoImageSource = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.PhotoImageSource));
      }
    }

    public double PhotoImageWidth
    {
      get
      {
        return this._photoImageWidth;
      }
      set
      {
        if (this._photoImageWidth == value)
          return;
        this._photoImageWidth = value;
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.PhotoImageWidth));
      }
    }

    public double PhotoImageHeight
    {
      get
      {
        return this._photoImageHeight;
      }
      set
      {
        if (this._photoImageHeight == value)
          return;
        this._photoImageHeight = value;
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.PhotoImageHeight));
      }
    }

    public Thickness PhotoImageMargin
    {
      get
      {
        return this._photoImageMargin;
      }
      set
      {
        if (this._photoImageMargin == value)
          return;
        this._photoImageMargin = value;
        this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>) (() => this.PhotoImageMargin));
      }
    }

    public Rect PhotoImageClipRect
    {
      get
      {
        return this._photoImageClipRect;
      }
      set
      {
        if (this._photoImageClipRect == value)
          return;
        this._photoImageClipRect = value;
        this.NotifyPropertyChanged<Rect>((System.Linq.Expressions.Expression<Func<Rect>>) (() => this.PhotoImageClipRect));
      }
    }

    public FriendRecommendationItemType Type
    {
      get
      {
        return this._type;
      }
      set
      {
        if (this._type == value)
          return;
        this._type = value;
        this.NotifyPropertyChanged<FriendRecommendationItemType>((System.Linq.Expressions.Expression<Func<FriendRecommendationItemType>>) (() => this.Type));
      }
    }

    public bool IsHandled { get; set; }

    public long UserId { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public FriendRecommendationItem(User user)
    {
      if (user == null)
      {
        this.Type = FriendRecommendationItemType.ContactsSyncPromo;
      }
      else
      {
        this.AddToFriendsButtonIcon = "../Resources/New/FriendAdd.png";
        this.AddToFriendsButtonForeground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 81, (byte) 129, (byte) 184));
        this.Name = user.Name;
        this.UserId = user.id;
        this.Description = user.description;
        this.NameBlockMargin = new Thickness(0.0, !string.IsNullOrEmpty(this.Description) ? 223.0 : 235.0, 50.0, 0.0);
        CropPhoto cropPhoto = user.crop_photo;
        if (cropPhoto != null)
        {
          Photo photo = cropPhoto.photo;
          double num1 = photo.height > 0 ? (double) photo.width / (double) photo.height : 1.0;
          double num2 = 256.0;
          double num3 = num2 / num1;
          bool flag = true;
          if (num1 > 128.0 / 107.0)
          {
            flag = false;
            num3 = 214.0;
            num2 = 214.0 * num1;
          }
          this.PhotoImageWidth = Math.Round(num2);
          this.PhotoImageHeight = Math.Round(num3);
          Rect croppingRectangle1 = cropPhoto.crop.GetCroppingRectangle(num2, num3);
          Rect croppingRectangle2 = cropPhoto.rect.GetCroppingRectangle(croppingRectangle1.Width, croppingRectangle1.Height);
          croppingRectangle2.X += croppingRectangle1.X;
          croppingRectangle2.Y += croppingRectangle1.Y;
          double num4 = croppingRectangle2.X + croppingRectangle2.Width / 2.0;
          double num5 = croppingRectangle2.Y + croppingRectangle2.Height;
          if (flag)
          {
            double num6 = croppingRectangle2.Height > 214.0 ? 2.56 : 2.0;
            double num7 = num5 - (croppingRectangle2.Height - croppingRectangle2.Height / num6);
            double val1 = 107.0 - num7;
            if (croppingRectangle2.Height > 214.0 && num7 - croppingRectangle2.Height / 2.0 >= 0.0)
              val1 = -croppingRectangle2.Y;
            double top = Math.Min(0.0, Math.Max(val1, 214.0 - num3));
            this.PhotoImageMargin = new Thickness(0.0, top, 0.0, 0.0);
            this.PhotoImageClipRect = new Rect(0.0, -(top + 1.0), 256.0, 215.0);
          }
          else
          {
            double left = Math.Min(0.0, Math.Max(128.0 - num4, 256.0 - num2));
            this.PhotoImageMargin = new Thickness(left, 0.0, 0.0, 0.0);
            this.PhotoImageClipRect = new Rect(-(left + 1.0), 0.0, 257.0, 214.0);
          }
          this.PhotoImageSource = cropPhoto.photo.GetAppropriateForScaleFactor(214.0, 1);
        }
        else
        {
          this.PhotoImageClipRect = new Rect(0.0, 0.0, 10000.0, 10000.0);
          this.PhotoImageSource = user.photo_400_orig ?? user.photo_200_orig ?? user.photo_max;
          this.PhotoImageWidth = 256.0;
          this.PhotoImageHeight = double.NaN;
        }
        EventAggregator.Current.Subscribe((object) this);
      }
    }

    public void Handle(FriendRequestSent message)
    {
      if (message.UserId != this.UserId)
        return;
      this.IsHandled = true;
      this.AddToFriendsButtonIcon = "../Resources/New/FriendAdded.png";
      this.AddToFriendsButtonForeground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 198, (byte) 201, (byte) 204));
    }

    public void Handle(SubscriptionCancelled message)
    {
      if (message.UserId != this.UserId)
        return;
      this.IsHandled = false;
      this.AddToFriendsButtonIcon = "../Resources/New/FriendAdd.png";
      this.AddToFriendsButtonForeground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte) 81, (byte) 129, (byte) 184));
    }

    public void NotifyPropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess || this.PropertyChanged == null)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs((propertyExpression.Body as MemberExpression).Member.Name));
      }));
    }
  }
}
