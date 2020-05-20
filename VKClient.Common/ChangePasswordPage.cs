using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class ChangePasswordPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBarIconButton _appBarButtonCheck;
    private ApplicationBarIconButton _appBarButtonCancel;
    private ApplicationBar _appBar;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    internal PasswordBox textBoxOldPassword;
    internal PasswordBox textBoxNewPassword;
    internal PasswordBox textBoxConfirmNewPassword;
    private bool _contentLoaded;

    private ChangePasswordViewModel VM
    {
      get
      {
        return base.DataContext as ChangePasswordViewModel;
      }
    }

    public ChangePasswordPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string chatEditAppBarSave = CommonResources.ChatEdit_AppBar_Save;
      applicationBarIconButton1.Text = chatEditAppBarSave;
      this._appBarButtonCheck = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      this._appBarButtonCancel = applicationBarIconButton2;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBar = applicationBar;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.Settings_ChangePassword.ToUpperInvariant());
      this.SuppressMenu = true;
      this.ucHeader.HideSandwitchButton = true;
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.ChangePasswordPage_Loaded));
    }

    private void ChangePasswordPage_Loaded(object sender, RoutedEventArgs e)
    {
      ((Control) this.textBoxOldPassword).Focus();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click+=(new EventHandler(this._appBarButtonCheck_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this._appBarButtonCancel_Click));
      this._appBar.Buttons.Add(this._appBarButtonCheck);
      this._appBar.Buttons.Add(this._appBarButtonCancel);
      this.ApplicationBar = ((IApplicationBar) this._appBar);
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      this.VM.UpdatePassword((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() => this.UpdateAppBar()))));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        base.DataContext = (new ChangePasswordViewModel());
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanUpdatePassword;
    }

    private void textBoxOldPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.VM.OldPassword = this.textBoxOldPassword.Password;
      this.UpdateAppBar();
    }

    private void textBoxNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.VM.NewPassword = this.textBoxNewPassword.Password;
      this.UpdateAppBar();
    }

    private void textBoxConfirmNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.VM.ConfirmNewPassword = this.textBoxConfirmNewPassword.Password;
      this.UpdateAppBar();
    }

    private void textBoxOldPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this.textBoxNewPassword).Focus();
    }

    private void textBoxNewPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this.textBoxConfirmNewPassword).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/ChangePasswordPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.textBoxOldPassword = (PasswordBox) base.FindName("textBoxOldPassword");
      this.textBoxNewPassword = (PasswordBox) base.FindName("textBoxNewPassword");
      this.textBoxConfirmNewPassword = (PasswordBox) base.FindName("textBoxConfirmNewPassword");
    }
  }
}
