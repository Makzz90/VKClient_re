using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;

namespace VKClient.Library
{
  public class SettingsViewModel : ViewModelBase, IHandle<BaseDataChangedEvent>, IHandle
  {
    private User _currentUser;
    //private bool _shownAppRestartNeededMB;
    //private bool _isInProgress;

    public string FullName
    {
      get
      {
        if (this.CurrentUser == null)
          return string.Empty;
        return string.Format("{0} {1}", this.CurrentUser.first_name, this.CurrentUser.last_name);
      }
    }

    public User CurrentUser
    {
      get
      {
        return this._currentUser;
      }
      set
      {
        this._currentUser = value;
        this.NotifyPropertyChanged<User>((Expression<Func<User>>) (() => this.CurrentUser));
        this.NotifyPropertyChanged<string>((() => this.FullName));
      }
    }

    public List<BGType> BackgroundTypes
    {
      get
      {
        return BGTypes.GetBGTypes();
      }
    }

    public List<BGType> AccentTypes
    {
      get
      {
        return AccentColorTypes.GetAccentTypes();
      }
    }

    public List<BGType> Languages
    {
      get
      {
        return LanguagesList.GetLanguages();
      }
    }

    public BGType Language
    {
        get
        {
            ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
            return Enumerable.FirstOrDefault<BGType>(this.Languages, (BGType l) => l.id == settings.LanguageSettings);
        }
        set
        {
        }
    }

    public BGType BackgroundType
    {
        get
        {
            ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
            return Enumerable.FirstOrDefault<BGType>(this.BackgroundTypes, (BGType b) => b.id == settings.BackgroundSettings);
        }
        set
        {
        }
    }

    public BGType AccentType
    {
        get
        {
            ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
            return Enumerable.FirstOrDefault<BGType>(this.AccentTypes, (BGType a) => a.id == settings.AccentSettings);
        }
        set
        {
        }
    }

    public BGType TileColor
    {
        get
        {
            ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
            return Enumerable.FirstOrDefault<BGType>(this.AccentTypes, (BGType a) => a.id == settings.TileSettings);
        }
        set
        {
            if (value != null)
            {
                ThemeSettings expr_08 = ThemeSettingsManager.GetThemeSettings();
                expr_08.TileSettings = value.id;
                ThemeSettingsManager.SetThemeSettings(expr_08);
                base.NotifyPropertyChanged<BGType>(() => this.TileColor);
                TileManager.Instance.UpdateTileColor();
            }
        }
    }

    public bool PushEnabled
    {
      get
      {
        if (!this.MessageNotificationsEnabled && !this.FriendsNotificationsEnabled && !this.MentionsNotificationsEnabled)
          return this.ReplyNotificationsEnabled;
        return true;
      }
    }

    public bool MessageTextEnabled
    {
      get
      {
        if (this.NotTempDisabled)
          return this.MessageNotificationsEnabled;
        return false;
      }
    }

    public bool MessageNotificationsEnabled
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.MessageNotificationsEnabled;
      }
      set
      {
        if (value == this.MessageNotificationsEnabled)
          return;
        AppGlobalStateManager.Current.GlobalState.MessageNotificationsEnabled = value;
        this.UpdatePushSettings();
      }
    }

    public bool MessageTextInNotification
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.MessageTextInNotification;
      }
      set
      {
        if (value == this.MessageTextInNotification)
          return;
        AppGlobalStateManager.Current.GlobalState.MessageTextInNotification = value;
        this.UpdatePushSettings();
      }
    }

    public bool FriendsNotificationsEnabled
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.FriendsNotificationsEnabled;
      }
      set
      {
        if (value == this.FriendsNotificationsEnabled)
          return;
        AppGlobalStateManager.Current.GlobalState.FriendsNotificationsEnabled = value;
        this.UpdatePushSettings();
      }
    }

    public bool MentionsNotificationsEnabled
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.MentionsNotificationsEnabled;
      }
      set
      {
        if (value == this.MentionsNotificationsEnabled)
          return;
        AppGlobalStateManager.Current.GlobalState.MentionsNotificationsEnabled = value;
        this.UpdatePushSettings();
      }
    }

    public bool ReplyNotificationsEnabled
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.ReplyNotificationsEnabled;
      }
      set
      {
        if (value == this.ReplyNotificationsEnabled)
          return;
        AppGlobalStateManager.Current.GlobalState.ReplyNotificationsEnabled = value;
        this.UpdatePushSettings();
      }
    }

    public bool TempDisabled
    {
      get
      {
        if (this.PushEnabled)
          return AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil >= DateTime.UtcNow;
        return false;
      }
    }

    public bool NotTempDisabled
    {
      get
      {
        return !this.TempDisabled;
      }
    }

    public string TempDisabledString
    {
      get
      {
        if (this.TempDisabled)
          return CommonResources.Settings_DisabledNotifications + " " + (AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil + (DateTime.Now - DateTime.UtcNow)).ToShortTimeString();
        return "";
      }
    }

    public Visibility TempDisabledVisibility
    {
      get
      {
        if (!this.TempDisabled)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public SettingsViewModel()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public void LoadCurrentUser()
    {
      User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
      if (loggedInUser == null)
      {
        UsersService instance = UsersService.Instance;
        List<long> userIds = new List<long>();
        userIds.Add(AppGlobalStateManager.Current.LoggedInUserId);
        Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          this.CurrentUser = res.ResultData.First<User>();
        });
        instance.GetUsers(userIds, callback);
      }
      else
        this.CurrentUser = loggedInUser;
    }

    private void UpdatePushSettings()
    {
    }

    private void NotifyProperties()
    {
      this.NotifyPropertyChanged<bool>((() => this.PushEnabled));
      this.NotifyPropertyChanged<bool>((() => this.TempDisabled));
      this.NotifyPropertyChanged<bool>((() => this.NotTempDisabled));
      this.NotifyPropertyChanged<string>((() => this.TempDisabledString));
      this.NotifyPropertyChanged<Visibility>((() => this.TempDisabledVisibility));
      this.NotifyPropertyChanged<bool>((() => this.MessageTextEnabled));
    }

    public void Disable(int seconds)
    {
      AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil = seconds != 0 ? DateTime.UtcNow + TimeSpan.FromSeconds((double) seconds) : DateTime.MinValue;
      this.NotifyProperties();
      AccountService.Instance.SetSilenceMode(AppGlobalStateManager.Current.GlobalState.NotificationsUri, seconds, (Action<BackendResult<object, ResultCode>>) (res => {}), 0, 0L);
    }

    internal void SetUserPicture(Stream stream, string fileName)
    {
    }

    public void Handle(BaseDataChangedEvent message)
    {
      User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
      if (loggedInUser == null)
        return;
      this.CurrentUser.photo_max = loggedInUser.photo_max;
      this.NotifyPropertyChanged<User>((Expression<Func<User>>) (() => this.CurrentUser));
    }
  }
}
