using System;
using System.Threading.Tasks;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class Auth2FAViewModel : ViewModelBase
  {
    private Visibility _smsSentVisibility = Visibility.Collapsed;
    private const string VALIDATION_TYPE_CODE = "2fa_app";
    private const string VALIDATION_TYPE_SMS = "2fa_sms";
    private readonly string _username;
    private readonly string _password;
    private readonly string _phoneMask;
    private string _validationType;
    private readonly string _validationSid;
    private bool _smsCodeSending;
    private bool _loggingIn;

    public string DescriptionText
    {
      get
      {
        if (this._validationType == "2fa_app")
          return CommonResources.Auth2FA_App_Description;
        if (this._validationType == "2fa_sms")
          return string.Format(CommonResources.Auth2FA_Sms_DescriptionFrm, (object) this._phoneMask);
        return "";
      }
    }

    public Visibility SmsSentVisibility
    {
      get
      {
        return this._smsSentVisibility;
      }
      set
      {
        this._smsSentVisibility = value;
        this.NotifyPropertyChanged("SmsSentVisibility");
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.SmsSendVisibility));
      }
    }

    public Visibility SmsSendVisibility
    {
      get
      {
        return this._smsSentVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public bool IsSMSMode
    {
      get
      {
        return this._validationType == "2fa_sms";
      }
      set
      {
        this._validationType = value ? "2fa_sms" : "2fa_app";
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.DescriptionText));
      }
    }

    public string Code { get; set; }

    public Auth2FAViewModel(string username, string password, string phoneMask, string validationType, string validationSid)
    {
      this._username = username;
      this._password = password;
      this._phoneMask = phoneMask;
      this._validationType = validationType;
      this._validationSid = validationSid;
    }

    public void SendSMS()
    {
      this.IsSMSMode = true;
      if (this._smsCodeSending)
        return;
      this._smsCodeSending = true;
      LoginService.Instance.SendSMS(this._validationSid, (Action<BackendResult<int, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (async () =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this.SmsSentVisibility = Visibility.Visible;
          await Task.Delay(TimeSpan.FromSeconds(5.0));
          this.SmsSentVisibility = Visibility.Collapsed;
        }
        else if (result.ResultCode == ResultCode.Unauthorized)
        {
          int num1 = (int) MessageBox.Show(CommonResources.Auth2FA_IncorrectCodeMessage, CommonResources.Auth2FA_IncorrectCodeTitle, MessageBoxButton.OK);
        }
        else
        {
          int num2 = (int) MessageBox.Show(CommonResources.Error_Connection, CommonResources.Error, MessageBoxButton.OK);
        }
        this._smsCodeSending = false;
      }))));
    }

    public void Login(Action callback = null)
    {
      if (this._loggingIn || string.IsNullOrEmpty(this.Code))
        return;
      this._loggingIn = true;
      this.SetInProgress(true, "");
      LoginService.Instance.GetAccessToken(this._username, this._password, this.Code, (Action<BackendResult<AutorizationData, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
          ServiceLocator.Resolve<IAppStateInfo>().HandleSuccessfulLogin(result.ResultData, true);
        else if (result.ResultCode == ResultCode.Unauthorized)
        {
          int num1 = (int) MessageBox.Show(CommonResources.Auth2FA_IncorrectCodeMessage, CommonResources.Auth2FA_IncorrectCodeTitle, MessageBoxButton.OK);
        }
        else
        {
          int num2 = (int) MessageBox.Show(CommonResources.Error_Connection, CommonResources.Error, MessageBoxButton.OK);
        }
        this.SetInProgress(false, "");
        this._loggingIn = false;
        if (callback == null)
          return;
        callback();
      }))));
    }
  }
}
