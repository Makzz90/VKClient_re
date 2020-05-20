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
    public partial class LoginPage : PageBase
    {
        private DelayedExecutor _de = new DelayedExecutor(300);
        private bool _inProgress;
        private bool _isInitialized;
        private bool _isLoginButtonEnabled;
        private bool _loaded;

        public LoginPage()
        {
            this.InitializeComponent();
            this._manualHandleValidationParams = true;
            this.Loaded += new RoutedEventHandler(this.LoginPage_Loaded);
            this.SuppressMenu = true;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._loaded)
                return;
            this._loaded = true;
            this.textBoxUsername.Focus();
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
                this._de.AddToDelayedExecution((Action)(() => Execute.ExecuteOnUIThread((Action)(() => this.textBoxUsername.Focus()))));
                this.DataContext = (object)new ViewModelBase();
                this._isInitialized = true;
            }
            this.HandleValidationInputParams();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this.SavePageUnboundState();
        }

        private void SavePageUnboundState()
        {
            this.State["MyState"] = (object)new LoginPage.PageState()
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
                return;
            this._inProgress = true;
            this.SetControlsState(false);
            Logger.Instance.Info("Trying to log in");
            this.ShowProgressIndicator(true, "");
            Logger.Instance.Info("Calling back end service");
            //wtf
            Logger.Instance.Error("WTF " + this.textBoxUsername.Text + " " + this.passwordBox.Password);
            //
            LoginService.Instance.GetAccessToken(this.textBoxUsername.Text, this.passwordBox.Password, (Action<BackendResult<AutorizationData, ResultCode>>)(result =>
            {
                Logger.Instance.Info("Back end service returned: " + result.ResultCode.ToString());
                this.ShowProgressIndicator(false, "");
                switch (result.ResultCode)
                {
                    case ResultCode.CommunicationFailed:
                        ExtendedMessageBox.ShowSafe(AppResources.FailedToConnect, AppResources.Login_Error_Header);
                        this.SetControlsState(true);
                        break;
                    case ResultCode.Succeeded:
                        ServiceLocator.Resolve<IAppStateInfo>().HandleSuccessfulLogin(result.ResultData, true);
                        break;
                    case ResultCode.CaptchaControlCancelled:
                        this.SetControlsState(true);
                        break;
                    default:
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            ExtendedMessageBox.ShowSafe(AppResources.Login_Error_InvalidCredential, AppResources.Login_Error_Header);
                            this.SetControlsState(true);
                            this.passwordBox.Focus();
                        }));
                        break;
                }
                this._inProgress = false;
            }));
        }

        private void SetControlsState(bool enabled)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.textBoxUsername.IsEnabled = enabled;
                this.passwordBox.IsEnabled = enabled;
                this.textBlockForgotYourPassword.IsHitTestVisible = enabled;
                this._isLoginButtonEnabled = enabled;
            }));
        }

        private void ShowProgressIndicator(bool show, string text = "")
        {
            if (this.Dispatcher.CheckAccess())
                this.DoShowProgressIndicator(show, text);
            else
                this.Dispatcher.BeginInvoke((Action)(() => this.DoShowProgressIndicator(show, text)));
        }

        private void DoShowProgressIndicator(bool show, string text)
        {
            this._progressBar.IsIndeterminate = show;
            this._progressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
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
            this.passwordBox.Focus();
        }

        private void passwordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !(this.textBoxUsername.Text != string.Empty) || !(this.passwordBox.Password != string.Empty))
                return;
            this.PerformLoginAttempt();
        }

        private void ForgotPasswordTap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToWebUri("https://m.vk.com/restore", true, false);
        }

        public class PageState
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }
    }
}
