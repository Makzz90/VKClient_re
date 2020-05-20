using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        if (!this.IsDeactivated)
          return Visibility.Collapsed;
        return Visibility.Visible;
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
        if (this.IsDeactivated || !this.HasAvatar)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility NoPhotoVisibility
    {
      get
      {
        if (this.HasAvatar || this.IsDeactivated)
          return Visibility.Collapsed;
        return Visibility.Visible;
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
        if (this._userData != null && string.IsNullOrEmpty(this.OnlineStatus))
          return Visibility.Collapsed;
        return Visibility.Visible;
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
        if (user.online_mobile == 1 || user.last_seen != null && Enumerable.Contains<int>(VKConstants.MOBILE_ONLINE_TYPES, user.last_seen.platform))
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Visibility VerifiedVisibility
    {
      get
      {
        if (this._userData == null)
          return Visibility.Collapsed;
        if (!this._userData.IsVerified)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility AddPhotoIconVisibility
    {
      get
      {
        if (this._userData == null)
          return Visibility.Collapsed;
        if (this._userData.Id != AppGlobalStateManager.Current.LoggedInUserId)
          return Visibility.Collapsed;
        return Visibility.Visible;
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
        this.Name = string.Format("{0} {1}", this._userData.user.first_name, this._userData.user.last_name);
      this.ProfileImageClipRect = new Rect(0.0, 0.0, 480.0, 320.0);
      this.ProfileImageMargin = new Thickness(0.0);
      this.UpdateProfilePhoto(480.0, 1.5);
      if (this._userData == null || AppGlobalStateManager.Current.LoggedInUserId != this._userData.Id || (AppGlobalStateManager.Current.GlobalState.LoggedInUser == null || !(AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max != this._userData.user.photo_max)))
        return;
      AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = this._userData.user.photo_max;
      EventAggregator.Current.Publish(new BaseDataChangedEvent());
    }

    public UserProfileHeaderViewModel(string name)
    {
      this.Name = name;
    }

    private void UpdateProfilePhoto(double width, double ratio)
    {
      if (this._userData == null)
        return;
      double requiredHeight = width / ratio;
      CropPhoto cropPhoto = this._userData.user.crop_photo;
      string avatarUrl;
      if (cropPhoto != null)
      {
        bool flag = true;
        string appropriateForScaleFactor = cropPhoto.photo.GetAppropriateForScaleFactor(requiredHeight, 1);
        Photo photo = cropPhoto.photo;
        double num1 = photo.height > 0 ? (double) photo.width / (double) photo.height : 1.0;
        double width1 = width;
        double height = width1 / num1;
        if (num1 > ratio)
        {
          height = requiredHeight;
          width1 = requiredHeight * num1;
          flag = false;
        }
        this.ProfileImageWidth = (int) width1;
        this.ProfileImageHeight = (int) height;
        Rect croppingRectangle1 = cropPhoto.crop.GetCroppingRectangle(width1, height);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        Rect croppingRectangle2 = cropPhoto.rect.GetCroppingRectangle(((Rect) @croppingRectangle1).Width, ((Rect) @croppingRectangle1).Height);
        // ISSUE: explicit reference operation
        // ISSUE: variable of a reference type
        // ISSUE: explicit reference operation
        double num2 = croppingRectangle2.X + ((Rect)@croppingRectangle1).X;
        croppingRectangle2.X = num2;
        // ISSUE: explicit reference operation
        // ISSUE: variable of a reference type
        // ISSUE: explicit reference operation
        double num3 = croppingRectangle2.Y + ((Rect)@croppingRectangle1).Y;
        croppingRectangle2.Y = num3;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num4 = ((Rect) @croppingRectangle2).X + ((Rect) @croppingRectangle2).Width / 2.0;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num5 = ((Rect) @croppingRectangle2).Y + ((Rect) @croppingRectangle2).Height;
        if (flag)
        {
          // ISSUE: explicit reference operation
          double num6 = ((Rect) @croppingRectangle2).Height <= requiredHeight ? 2.0 : 2.56;
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          double num7 = num5 - (((Rect) @croppingRectangle2).Height - ((Rect) @croppingRectangle2).Height / num6);
          double val1 = requiredHeight / 2.0 - num7;
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          if (((Rect) @croppingRectangle2).Height > requiredHeight && num7 - ((Rect) @croppingRectangle2).Height / 2.0 >= 0.0)
          {
            // ISSUE: explicit reference operation
            val1 = -((Rect) @croppingRectangle2).Y;
          }
          double num8 = Math.Min(0.0, Math.Max(val1, requiredHeight - height));
          this.ProfileImageMargin = new Thickness(0.0, num8, 0.0, 0.0);
          this.ProfileImageClipRect = new Rect(0.0, -(num8 + 1.0), width, requiredHeight + 1.0);
        }
        else
        {
          double num6 = Math.Min(0.0, Math.Max(width / 2.0 - num4, width - width1));
          this.ProfileImageMargin = new Thickness(num6, 0.0, 0.0, 0.0);
          this.ProfileImageClipRect = new Rect(-(num6 + 1.0), 0.0, width + 1.0, requiredHeight);
        }
        // ISSUE: type reference
        // ISSUE: method reference
        this.NotifyPropertyChanged<Thickness>(() => this.ProfileImageMargin);
        // ISSUE: type reference
        // ISSUE: method reference
        this.NotifyPropertyChanged<Rect>(() => this.ProfileImageClipRect);
        avatarUrl = appropriateForScaleFactor;
      }
      else
      {
        avatarUrl = this._userData.user.photo_max ?? this._userData.user.photo_big;
        this.ProfileImageWidth = (int) width;
        this.ProfileImageHeight = (int) requiredHeight;
      }
      if (!ProfileHeaderViewModelBase.IsValidAvatarUrl(avatarUrl))
        return;
      this.ProfileImageUrl = avatarUrl;
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.ProfileImageUrl);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.ProfileImageWidth);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.ProfileImageHeight);
    }
  }
}
