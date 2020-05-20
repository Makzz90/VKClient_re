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
          return string.Format(CommonResources.Auth2FA_Sms_DescriptionFrm, this._phoneMask);
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
        base.NotifyPropertyChanged<Visibility>(() => this.SmsSendVisibility);
			}
    }

    public Visibility SmsSendVisibility
    {
      get
      {
        if (this._smsSentVisibility != Visibility.Visible)
          return Visibility.Visible;
        return Visibility.Collapsed;
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
        base.NotifyPropertyChanged<string>(() => this.DescriptionText);
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
			{
				return;
			}
			this._smsCodeSending = true;
			LoginService.Instance.SendSMS(this._validationSid, delegate(BackendResult<int, ResultCode> result)
			{
				Execute.ExecuteOnUIThread(async delegate
				{
					if (result.ResultCode == ResultCode.Succeeded)
					{
						this.SmsSentVisibility = 0;
						await Task.Delay(TimeSpan.FromSeconds(5.0));
						this.SmsSentVisibility = Visibility.Collapsed;
					}
					else if (result.ResultCode == ResultCode.Unauthorized)
					{
						MessageBox.Show(CommonResources.Auth2FA_IncorrectCodeMessage, CommonResources.Auth2FA_IncorrectCodeTitle, 0);
					}
					else
					{
						MessageBox.Show(CommonResources.Error_Connection, CommonResources.Error, 0);
					}
					this._smsCodeSending = false;
				});
			});
		}

    public void Login(Action callback = null)
    {
        if (this._loggingIn)
        {
            return;
        }
        if (string.IsNullOrEmpty(this.Code))
        {
            return;
        }
        this._loggingIn = true;
        base.SetInProgress(true, "");
        LoginService.Instance.GetAccessToken(this._username, this._password, this.Code, delegate(BackendResult<AutorizationData, ResultCode> result)
        {
            Execute.ExecuteOnUIThread(delegate
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    ServiceLocator.Resolve<IAppStateInfo>().HandleSuccessfulLogin(result.ResultData, true);
                }
                else if (result.ResultCode == ResultCode.Unauthorized)
                {
                    MessageBox.Show(CommonResources.Auth2FA_IncorrectCodeMessage, CommonResources.Auth2FA_IncorrectCodeTitle, 0);
                }
                else
                {
                    MessageBox.Show(CommonResources.Error_Connection, CommonResources.Error, 0);
                }
                this.SetInProgress(false, "");
                this._loggingIn = false;
                if (callback != null)
                {
                    callback.Invoke();
                }
            });
        });
    }
  }
}
