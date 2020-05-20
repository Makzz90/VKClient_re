using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Users.ViewModels
{
  public sealed class UserProfileHeaderViewModel : ProfileHeaderViewModelBase
  {
    private const int IMAGE_WIDTH = 480;
    private const double IMAGE_RATIO = 1.5;
    private readonly UserData _userData;

    private bool IsDeleted
    {
      get
      {
        if (this._userData == null || this._userData.user == null)
          return false;
        return this._userData.user.deactivated == "deleted";
      }
    }

    private bool IsBanned
    {
      get
      {
        if (this._userData == null || this._userData.user == null)
          return false;
        return this._userData.user.deactivated == "banned";
      }
    }

    private bool IsDeactivated
    {
      get
      {
        if (this._userData == null || this._userData.user == null)
          return false;
        if (!this.IsDeleted)
          return this.IsBanned;
        return true;
      }
    }

    public Visibility DeactivatedPhotoVisibility
    {
      get
      {
        return !this.IsDeactivated ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override bool HasAvatar
    {
      get
      {
        if (this._userData == null)
          return true;
        return ProfileHeaderViewModelBase.IsValidAvatarUrl(this.ProfileImageUrl ?? this._userData.user.photo_max ?? this._userData.user.photo_big);
      }
    }

    public Visibility AvatarVisibility
    {
      get
      {
        return this.IsDeactivated || !this.HasAvatar ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility NoPhotoVisibility
    {
      get
      {
        return this.HasAvatar || this.IsDeactivated ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override string ProfileImageUrl { get; protected set; }

    public override string Name { get; protected set; }

    public int ProfileImageWidth { get; private set; }

    public int ProfileImageHeight { get; private set; }

    public Thickness ProfileImageMargin { get; private set; }

    public Rect ProfileImageClipRect { get; private set; }

    public Visibility OnlineStatusVisibility
    {
      get
      {
        return this._userData != null && string.IsNullOrEmpty(this.OnlineStatus) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string OnlineStatus
    {
      get
      {
        if (this._userData == null)
          return string.Empty;
        this._userData.user.last_seen.online = (long) this._userData.user.online;
        return this._userData.user.last_seen.GetUserStatusString(this._userData.user.sex == 2).ToLowerInvariant();
      }
    }

    public Visibility MobileVisibility
    {
      get
      {
        if (this._userData == null)
          return Visibility.Collapsed;
        User user = this._userData.user;
        return user.online_mobile == 1 || user.last_seen != null && ((IEnumerable<int>) VKConstants.MOBILE_ONLINE_TYPES).Contains<int>(user.last_seen.platform) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility VerifiedVisibility
    {
      get
      {
        return this._userData == null || !this._userData.IsVerified ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility AddPhotoIconVisibility
    {
      get
      {
        return this._userData == null || this._userData.Id != AppGlobalStateManager.Current.LoggedInUserId ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public double NameWidth
    {
      get
      {
        double num = 464.0;
        if (this._userData == null)
          return num;
        if (this._userData.IsVerified)
          num -= 44.0;
        if (this._userData.Id == AppGlobalStateManager.Current.LoggedInUserId)
          num -= 64.0;
        return num;
      }
    }

    public UserProfileHeaderViewModel(UserData userData)
    {
      this._userData = userData;
      if (this._userData != null)
        this.Name = string.Format("{0} {1}", (object) this._userData.user.first_name, (object) this._userData.user.last_name);
      this.ProfileImageClipRect = new Rect(0.0, 0.0, 480.0, 320.0);
      this.ProfileImageMargin = new Thickness(0.0);
      this.UpdateProfilePhoto(480.0, 1.5);
      if (this._userData == null || AppGlobalStateManager.Current.LoggedInUserId != this._userData.Id || (AppGlobalStateManager.Current.GlobalState.LoggedInUser == null || !(AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max != this._userData.user.photo_max)))
        return;
      AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = this._userData.user.photo_max;
      EventAggregator.Current.Publish((object) new BaseDataChangedEvent());
    }

    public UserProfileHeaderViewModel(string name)
    {
      this.Name = name;
    }

    private void UpdateProfilePhoto(double width, double ratio)
    {
      if (this._userData == null)
        return;
      double num1 = width / ratio;
      CropPhoto cropPhoto = this._userData.user.crop_photo;
      string avatarUrl;
      if (cropPhoto != null)
      {
        bool flag = true;
        string appropriateForScaleFactor = cropPhoto.photo.GetAppropriateForScaleFactor(num1, 1);
        Photo photo = cropPhoto.photo;
        double num2 = photo.height > 0 ? (double) photo.width / (double) photo.height : 1.0;
        double width1 = width;
        double height = width1 / num2;
        if (num2 > ratio)
        {
          height = num1;
          width1 = num1 * num2;
          flag = false;
        }
        this.ProfileImageWidth = (int) width1;
        this.ProfileImageHeight = (int) height;
        Rect croppingRectangle1 = cropPhoto.crop.GetCroppingRectangle(width1, height);
        Rect croppingRectangle2 = cropPhoto.rect.GetCroppingRectangle(croppingRectangle1.Width, croppingRectangle1.Height);
        croppingRectangle2.X += croppingRectangle1.X;
        croppingRectangle2.Y += croppingRectangle1.Y;
        double num3 = croppingRectangle2.X + croppingRectangle2.Width / 2.0;
        double num4 = croppingRectangle2.Y + croppingRectangle2.Height;
        if (flag)
        {
          double num5 = croppingRectangle2.Height <= num1 ? 2.0 : 2.56;
          double num6 = num4 - (croppingRectangle2.Height - croppingRectangle2.Height / num5);
          double val1 = num1 / 2.0 - num6;
          if (croppingRectangle2.Height > num1 && num6 - croppingRectangle2.Height / 2.0 >= 0.0)
            val1 = -croppingRectangle2.Y;
          double top = Math.Min(0.0, Math.Max(val1, num1 - height));
          this.ProfileImageMargin = new Thickness(0.0, top, 0.0, 0.0);
          this.ProfileImageClipRect = new Rect(0.0, -(top + 1.0), width, num1 + 1.0);
        }
        else
        {
          double left = Math.Min(0.0, Math.Max(width / 2.0 - num3, width - width1));
          this.ProfileImageMargin = new Thickness(left, 0.0, 0.0, 0.0);
          this.ProfileImageClipRect = new Rect(-(left + 1.0), 0.0, width + 1.0, num1);
        }
        this.NotifyPropertyChanged<Thickness>((System.Linq.Expressions.Expression<Func<Thickness>>) (() => this.ProfileImageMargin));
        this.NotifyPropertyChanged<Rect>((System.Linq.Expressions.Expression<Func<Rect>>) (() => this.ProfileImageClipRect));
        avatarUrl = appropriateForScaleFactor;
      }
      else
      {
        avatarUrl = this._userData.user.photo_max ?? this._userData.user.photo_big;
        this.ProfileImageWidth = (int) width;
        this.ProfileImageHeight = (int) num1;
      }
      if (!ProfileHeaderViewModelBase.IsValidAvatarUrl(avatarUrl))
        return;
      this.ProfileImageUrl = avatarUrl;
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ProfileImageUrl));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.ProfileImageWidth));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.ProfileImageHeight));
    }
  }
}
