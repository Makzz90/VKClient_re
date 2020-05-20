using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using VKClient.Localization;

namespace VKClient
{
  public class LoginPage : PageBase
  {
    private DelayedExecutor _de = new DelayedExecutor(300);
    private bool _inProgress;
    private bool _isInitialized;
    private bool _isLoginButtonEnabled;
    private bool _loaded;
    internal ProgressIndicator progressIndicator;
    internal ScrollViewer scrollViewer;
    internal StackPanel stackPanel;
    internal TextBox textBoxUsername;
    internal PasswordBox passwordBox;
    internal Button buttonLogin;
    internal TextBlock textBlockForgotYourPassword;
    private bool _contentLoaded;

    public LoginPage()
    {
      this.InitializeComponent();
      this._manualHandleValidationParams = true;
      base.Loaded+=(new RoutedEventHandler( this.LoginPage_Loaded));
      this.SuppressMenu = true;
    }

    private void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._loaded)
        return;
      this._loaded = true;
      ((Control) this.textBoxUsername).Focus();
    }

    private void LogInButton_Click(object sender, RoutedEventArgs e)
    {
      this.PerformLoginAttempt();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
          this.RestorePageUnboundState();
          this._de.AddToDelayedExecution(delegate
          {
              Execute.ExecuteOnUIThread(delegate
              {
                  this.textBoxUsername.Focus();
              });
          });
          base.DataContext=(new ViewModelBase());
          this._isInitialized = true;
      }
      base.HandleValidationInputParams();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      this.SavePageUnboundState();
    }

    private void SavePageUnboundState()
    {
      this.State["MyState"] = new LoginPage.PageState()
      {
        Username = this.textBoxUsername.Text,
        Password = this.passwordBox.Password
      };
    }

    private void RestorePageUnboundState()
    {
      if (!this.State.ContainsKey("MyState"))
        return;
      LoginPage.PageState pageState = this.State["MyState"] as LoginPage.PageState;
      if (pageState == null)
        return;
      this.textBoxUsername.Text = pageState.Username;
      this.passwordBox.Password = pageState.Password;
      this.UpdateLoginButtonState();
    }

    private void PerformLoginAttempt()
    {
        if (this._inProgress || !this._isLoginButtonEnabled)
        {
            return;
        }
        this._inProgress = true;
        this.SetControlsState(false);
        Logger.Instance.Info("Trying to log in", new object[0]);
        this.ShowProgressIndicator(true, "");
        Logger.Instance.Info("Calling back end service", new object[0]);
        LoginService.Instance.GetAccessToken(this.textBoxUsername.Text, this.passwordBox.Password, delegate(BackendResult<AutorizationData, ResultCode> result)
        {
            Logger.Instance.Info("Back end service returned: " + result.ResultCode.ToString(), new object[0]);
            this.ShowProgressIndicator(false, "");
            ResultCode resultCode = result.ResultCode;
            if (resultCode != ResultCode.CommunicationFailed)
            {
                if (resultCode != ResultCode.Succeeded)
                {
                    if (resultCode != ResultCode.CaptchaControlCancelled)
                    {
                        Execute.ExecuteOnUIThread(delegate
                        {
                            ExtendedMessageBox.ShowSafe(AppResources.Login_Error_InvalidCredential, AppResources.Login_Error_Header);
                            this.SetControlsState(true);
                            this.passwordBox.Focus();
                        });
                    }
                    else
                    {
                        this.SetControlsState(true);
                    }
                }
                else
                {
                    ServiceLocator.Resolve<IAppStateInfo>().HandleSuccessfulLogin(result.ResultData, true);
                }
            }
            else
            {
                ExtendedMessageBox.ShowSafe(AppResources.FailedToConnect, AppResources.Login_Error_Header);
                this.SetControlsState(true);
            }
            this._inProgress = false;
        });
    }

    private void SetControlsState(bool enabled)
    {
        Execute.ExecuteOnUIThread(delegate
        {
            this.textBoxUsername.IsEnabled = enabled;
            this.passwordBox.IsEnabled = enabled;
            this.textBlockForgotYourPassword.IsHitTestVisible = enabled;
            this._isLoginButtonEnabled = enabled;
        });
    }

    private void ShowProgressIndicator(bool show, string text = "")
    {
        if (base.Dispatcher.CheckAccess())
        {
            this.DoShowProgressIndicator(show, text);
            return;
        }
        base.Dispatcher.BeginInvoke(delegate
        {
            this.DoShowProgressIndicator(show, text);
        });
    }

    private void DoShowProgressIndicator(bool show, string text)
    {
      this._progressBar.IsIndeterminate = show;
      ((UIElement) this._progressBar).Visibility = (show ? Visibility.Visible : Visibility.Collapsed);
    }

    private void textBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateLoginButtonState();
    }

    private void userPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.UpdateLoginButtonState();
    }

    private void UpdateLoginButtonState()
    {
      this._isLoginButtonEnabled = !string.IsNullOrWhiteSpace(this.textBoxUsername.Text) && !string.IsNullOrEmpty(this.passwordBox.Password);
    }

    private void textBoxUsername_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter || string.IsNullOrEmpty(this.textBoxUsername.Text))
        return;
      ((Control) this.passwordBox).Focus();
    }

    private void passwordBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter || !(this.textBoxUsername.Text != string.Empty) || !(this.passwordBox.Password != string.Empty))
        return;
      this.PerformLoginAttempt();
    }

    private void ForgotPasswordTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://m.vk.com/restore", true, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient;component/LoginPage.xaml", UriKind.Relative));
      this.progressIndicator = (ProgressIndicator) base.FindName("progressIndicator");
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.textBoxUsername = (TextBox) base.FindName("textBoxUsername");
      this.passwordBox = (PasswordBox) base.FindName("passwordBox");
      this.buttonLogin = (Button) base.FindName("buttonLogin");
      this.textBlockForgotYourPassword = (TextBlock) base.FindName("textBlockForgotYourPassword");
    }

    public class PageState
    {
      public string Username { get; set; }

      public string Password { get; set; }
    }
  }
}
