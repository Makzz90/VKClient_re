using System;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class ChangePasswordViewModel : ViewModelBase
  {
    private DelayedExecutor _de = new DelayedExecutor(750);
    private string _oldPassword;
    private string _newPassword;
    private string _confirmNewPassword;
    private bool _isUpdating;

    public string OldPassword
    {
      get
      {
        return this._oldPassword;
      }
      set
      {
        this._oldPassword = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.OldPassword));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.ShowRecommendation));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ErrorString));
      }
    }

    public string NewPassword
    {
      get
      {
        return this._newPassword;
      }
      set
      {
        this._newPassword = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.NewPassword));
        this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.ShowRecommendation));
          this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ErrorString));
        }))));
      }
    }

    public string ConfirmNewPassword
    {
      get
      {
        return this._confirmNewPassword;
      }
      set
      {
        this._confirmNewPassword = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ConfirmNewPassword));
        this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.ShowRecommendation));
          this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ErrorString));
        }))));
      }
    }

    public string ErrorString
    {
      get
      {
        if (!string.IsNullOrEmpty(this.NewPassword) && this.NewPassword.Length < 6)
          return string.Format(CommonResources.Error_MinimumSymbolsFrm, (object) "6");
        if (!string.IsNullOrEmpty(this.NewPassword) && !string.IsNullOrEmpty(this.ConfirmNewPassword) && this.NewPassword != this.ConfirmNewPassword)
          return CommonResources.Settings_ChangePassword_PasswordsDoNotMatch;
        return "";
      }
    }

    public bool ShowRecommendation
    {
      get
      {
        return this.ErrorString == "";
      }
    }

    public bool CanUpdatePassword
    {
      get
      {
        if (this.ErrorString == "" && !string.IsNullOrEmpty(this.OldPassword) && !string.IsNullOrEmpty(this.NewPassword))
          return !this._isUpdating;
        return false;
      }
    }

    public void UpdatePassword(Action<bool> callbackRes)
    {
      if (this._isUpdating)
      {
        callbackRes(false);
      }
      else
      {
        this._isUpdating = true;
        this.SetInProgress(true, "");
        AccountService.Instance.ChangePassword(this.OldPassword, this.NewPassword, (Action<BackendResult<ChangePasswordResponse, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.SetInProgress(false, "");
          if (res.ResultCode == ResultCode.AccessDenied)
            new GenericInfoUC().ShowAndHideLater(CommonResources.Settings_ChangePassword_ErrorAccessDenied, null);
          else if (res.ResultCode != ResultCode.Succeeded)
            GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
          if (res.ResultCode == ResultCode.Succeeded)
          {
            AppGlobalStateManager.Current.HandleUserLogin(new AutorizationData()
            {
              access_token = res.ResultData.token,
              user_id = AppGlobalStateManager.Current.GlobalState.LoggedInUserId,
              secret = res.ResultData.secret ?? ""
            });
            int num = (int) MessageBox.Show(CommonResources.Settings_YourPasswordHasBeenChanged, CommonResources.Settings_ChangePassword, MessageBoxButton.OK);
            Navigator.Current.GoBack();
          }
          this._isUpdating = false;
          callbackRes(res.ResultCode == ResultCode.Succeeded);
        }))));
      }
    }
  }
}
