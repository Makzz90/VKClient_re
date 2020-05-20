using System;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class SettingsChangeShortNameViewModel : ViewModelBase
  {
    private DelayedExecutor _de = new DelayedExecutor(500);
    private string _availableNames = "";
    private string _reason = "";
    private SettingsChangeShortNameViewModel.NameStatus _status;
    private string _shortName;
    private string _currentShortName;
    private bool _isSaving;

    public string ShortName
    {
      get
      {
        return this._shortName;
      }
      set
      {
        this._shortName = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ShortName));
        this.DelayFetchNameStatus(this._shortName);
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.UserProvidedNewName));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveReasonOrIsFree));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.YourLink));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveShortName));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.AtShortName));
      }
    }

    public string AtShortName
    {
      get
      {
        return "@" + this.ShortName;
      }
    }

    public bool HaveShortName
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.ShortName) || this.ShortName.Length <= 4)
          return false;
        if (this._status != SettingsChangeShortNameViewModel.NameStatus.Free && this._status != SettingsChangeShortNameViewModel.NameStatus.Unknown)
          return !this.UserProvidedNewName;
        return true;
      }
    }

    public bool HaveReasonOrIsFree
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.Reason) || this.IsFree)
          return this.UserProvidedNewName;
        return false;
      }
    }

    public string ShortNameDescStr
    {
      get
      {
        return string.Format(CommonResources.Settings_ShortName_DescStr, (object) "");
      }
    }

    public SettingsChangeShortNameViewModel.NameStatus CheckNameStatus
    {
      get
      {
        return this._status;
      }
      set
      {
        this._status = value;
        this.NotifyPropertyChanged<SettingsChangeShortNameViewModel.NameStatus>((System.Linq.Expressions.Expression<Func<SettingsChangeShortNameViewModel.NameStatus>>) (() => this.CheckNameStatus));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsFree));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveReasonOrIsFree));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanSave));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.UserProvidedNewName));
        this.NotifyPropertyChanged(this.YourLink);
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveShortName));
      }
    }

    public bool HaveAvailableNames
    {
      get
      {
        if (!string.IsNullOrEmpty(this.AvailableNames))
          return !string.IsNullOrWhiteSpace(this.Reason);
        return false;
      }
    }

    public string AvailableNames
    {
      get
      {
        return this._availableNames;
      }
      set
      {
        this._availableNames = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.AvailableNames));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveAvailableNames));
      }
    }

    public string YourLink
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.ShortName))
          return "vk.com/" + this.ShortName;
        return "";
      }
    }

    public string Reason
    {
      get
      {
        return this._reason;
      }
      set
      {
        this._reason = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Reason));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveAvailableNames));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveReasonOrIsFree));
      }
    }

    public bool IsFree
    {
      get
      {
        return this._status == SettingsChangeShortNameViewModel.NameStatus.Free;
      }
    }

    public bool IsSaving
    {
      get
      {
        return this._isSaving;
      }
      set
      {
        this._isSaving = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSaving));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanSave));
      }
    }

    public bool CanSave
    {
      get
      {
        if (this.IsFree && !this.IsSaving)
          return this.UserProvidedNewName;
        return false;
      }
    }

    public bool UserProvidedNewName
    {
      get
      {
        return this.ShortName != this._currentShortName;
      }
    }

    public SettingsChangeShortNameViewModel(string currentName)
    {
      this._currentShortName = currentName;
      this.ShortName = this._currentShortName;
    }

    private void DelayFetchNameStatus(string shortName)
    {
      this.CheckNameStatus = SettingsChangeShortNameViewModel.NameStatus.Unknown;
      this.Reason = "";
      if (string.IsNullOrEmpty(shortName))
        return;
      this._de.AddToDelayedExecution((Action) (() => AccountService.Instance.CheckShortName(shortName, (Action<BackendResult<CheckNameResponse, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!(this.ShortName == shortName))
          return;
        this.AvailableNames = "";
        if (res.ResultCode != ResultCode.Succeeded)
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
        else if (res.ResultData.status == 1)
        {
          this.Reason = "";
          this.CheckNameStatus = SettingsChangeShortNameViewModel.NameStatus.Free;
        }
        else
        {
          this.Reason = res.ResultData.reason;
          this.CheckNameStatus = SettingsChangeShortNameViewModel.NameStatus.Busy;
          if (res.ResultData.suggestions == null || res.ResultData.suggestions.items == null)
            return;
          this.AvailableNames = res.ResultData.suggestions.items.GetCommaSeparated(", ");
        }
      }))))));
    }

    public void SaveShortName(Action<bool> callback)
    {
      if (this._isSaving)
      {
        callback(false);
      }
      else
      {
        this.IsSaving = true;
        this.SetInProgress(true, "");
        AccountService.Instance.SetShortName(this.ShortName, (Action<BackendResult<SaveProfileResponse, ResultCode>>) (res =>
        {
          this.SetInProgress(false, "");
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            this.IsSaving = false;
            if (res.ResultCode == ResultCode.Succeeded && res.ResultData.changed == 1)
            {
              callback(true);
              EventAggregator.Current.Publish((object) new ShortNameChangedEvent()
              {
                ShortName = this.ShortName
              });
              Navigator.Current.GoBack();
            }
            else
            {
              callback(false);
              new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
            }
          }));
        }));
      }
    }

    public enum NameStatus
    {
      Unknown,
      Free,
      Busy,
    }
  }
}
