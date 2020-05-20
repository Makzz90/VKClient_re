using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public class SettingsAccountViewModel : ViewModelBase, IHandle<HiddenNewsSourcesCountUpdated>, IHandle, IHandle<ShortNameChangedEvent>
  {
    private int _hiddenSourcesCount = -1;
    private bool _isLoading;
    private SettingsAccountInfo _settingsAccountInfo;

    public List<BGType> ShowByDefaultTypes
    {
      get
      {
        return ShowByDefaultTypesList.GetShowByDefaultTypesList();
      }
    }

    public BGType ShowByDefaultType
    {
      get
      {
        if (!this.ShowOnlyMyPosts)
          return this.ShowByDefaultTypes[0];
        return this.ShowByDefaultTypes[1];
      }
      set
      {
        if (value == null)
          return;
        if (value.id == 0)
          this.ShowOnlyMyPosts = false;
        else
          this.ShowOnlyMyPosts = true;
      }
    }

    public string PhoneNumberStr
    {
      get
      {
        string str = this._settingsAccountInfo == null ? " " : (string.IsNullOrEmpty(this._settingsAccountInfo.Account.phone) ? CommonResources.SettingsAccount_SetUp : this._settingsAccountInfo.Account.phone);
        if (this._settingsAccountInfo != null && this._settingsAccountInfo.Account.phone_status == "waiting")
          str = str + " (" + CommonResources.SettingsAccount_PhoneNumberWaiting + ")";
        return str;
      }
    }

    public string EmailStr
    {
      get
      {
        string str = this._settingsAccountInfo == null ? " " : (string.IsNullOrEmpty(this._settingsAccountInfo.Account.email) ? CommonResources.SettingsAccount_SetUp : this._settingsAccountInfo.Account.email);
        if (this._settingsAccountInfo != null && this._settingsAccountInfo.Account.email_status == "need_confirmation")
          str = str + " (" + CommonResources.SettingsAccount_EmailNeedsConfirmation + ")";
        return str;
      }
    }

    public string ShortNameStr
    {
      get
      {
        if (this._settingsAccountInfo != null && !string.IsNullOrEmpty(this._settingsAccountInfo.ProfileInfo.screen_name))
          return "@" + this._settingsAccountInfo.ProfileInfo.screen_name;
        return CommonResources.SettingsAccount_SetUp;
      }
    }

    public string ShortName
    {
      get
      {
        if (this._settingsAccountInfo != null)
          return this._settingsAccountInfo.ProfileInfo.screen_name;
        return "";
      }
    }

    public bool ShowOnlyMyPosts
    {
      get
      {
        if (this._settingsAccountInfo != null)
          return this._settingsAccountInfo.Account.own_posts_default == 1;
        return false;
      }
      set
      {
        if (this._isLoading || this._settingsAccountInfo == null || value == this.ShowOnlyMyPosts)
          return;
        this._isLoading = true;
        AccountService.Instance.SetInfo("own_posts_default", value ? "1" : "0", (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
          else
            this._settingsAccountInfo.Account.own_posts_default = value ? 1 : 0;
          this._isLoading = false;
          this.NotifyPropertyChanged<bool>(() => this.ShowOnlyMyPosts);
          this.NotifyPropertyChanged<BGType>(() => this.ShowByDefaultType);
        }));
      }
    }

    public bool PostComments
    {
      get
      {
        if (this._settingsAccountInfo != null)
          return this._settingsAccountInfo.Account.no_wall_replies == 0;
        return false;
      }
      set
      {
        if (this._isLoading || this._settingsAccountInfo == null || value == this.PostComments)
          return;
        this._isLoading = true;
        AccountService.Instance.SetInfo("no_wall_replies", value ? "0" : "1", (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
          else
            this._settingsAccountInfo.Account.no_wall_replies = value ? 0 : 1;
          this._isLoading = false;
          this.NotifyPropertyChanged<bool>(() => this.PostComments);
        }));
      }
    }

    public bool IsLoaded
    {
      get
      {
        return this._settingsAccountInfo != null;
      }
    }

    public string NewsFilterDescStr
    {
      get
      {
        int hiddenSourcesCount = this._hiddenSourcesCount;
        if (hiddenSourcesCount > 0)
          return CommonResources.SettingsAccount_NotShown + ": " + hiddenSourcesCount.ToString();
        return " ";
      }
    }

    public SettingsAccountViewModel()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public void LoadData()
    {
        if (this._isLoading)
        {
            return;
        }
        this._isLoading = true;
        base.SetInProgress(true, "");
        AccountService.Instance.GetSettingsAccountInfo(delegate(BackendResult<SettingsAccountInfo, ResultCode> res)
        {
            base.SetInProgress(false, "");
            if (res.ResultCode != ResultCode.Succeeded)
            {
                GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
            }
            else
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    this.ReadData(res.ResultData);
                });
            }
            this._isLoading = false;
        });
    }

    private void ReadData(SettingsAccountInfo settingsAccountInfo)
    {
      this._settingsAccountInfo = settingsAccountInfo;
      this._hiddenSourcesCount = (this._settingsAccountInfo.NewsBanned.groups == null ? 0 : this._settingsAccountInfo.NewsBanned.groups.Count) + (this._settingsAccountInfo.NewsBanned.members == null ? 0 : this._settingsAccountInfo.NewsBanned.members.Count);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.PhoneNumberStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.EmailStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.NewsFilterDescStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.PostComments);
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.ShowOnlyMyPosts);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.ShortNameStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.IsLoaded);
    }

    internal void HandlePhoneNumberTap()
    {
      if (this._settingsAccountInfo == null || string.IsNullOrEmpty(this._settingsAccountInfo.Account.change_phone_url))
        return;
      Navigator.Current.NavigationToValidationPage(this._settingsAccountInfo.Account.change_phone_url);
    }

    internal void HandleEmailTap()
    {
      if (this._settingsAccountInfo == null || string.IsNullOrEmpty(this._settingsAccountInfo.Account.change_email_url))
        return;
      Navigator.Current.NavigationToValidationPage(this._settingsAccountInfo.Account.change_email_url);
    }

    internal void HandleValidationResponse(ValidationUserResponse validationResponse)
    {
      if (this._settingsAccountInfo == null)
        return;
      if (!string.IsNullOrEmpty(validationResponse.phone))
        this._settingsAccountInfo.Account.phone = validationResponse.phone;
      if (!string.IsNullOrEmpty(validationResponse.phone_status))
        this._settingsAccountInfo.Account.phone_status = validationResponse.phone_status;
      if (!string.IsNullOrEmpty(validationResponse.email))
        this._settingsAccountInfo.Account.email = validationResponse.email;
      if (!string.IsNullOrEmpty(validationResponse.email_status))
        this._settingsAccountInfo.Account.email_status = validationResponse.email_status;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.PhoneNumberStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.EmailStr);
    }

    public void Handle(HiddenNewsSourcesCountUpdated message)
    {
      if (this._settingsAccountInfo == null || this._hiddenSourcesCount == -1)
        return;
      this._hiddenSourcesCount = message.UpdatedCount;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.NewsFilterDescStr);
    }

    public void Handle(ShortNameChangedEvent message)
    {
      if (this._settingsAccountInfo == null)
        return;
      this._settingsAccountInfo.ProfileInfo.screen_name = message.ShortName;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.ShortNameStr);
    }
  }
}
