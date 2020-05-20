using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class Auth2FAPage : PageBase
  {
    private bool _isInitialized;
    private Auth2FAViewModel _viewModel;
    private readonly ApplicationBarIconButton _sendButton;
    internal GenericHeaderUC ucHeader;
    internal TextBox textBoxConfirmationCode;
    internal TextBlock textBlockConfirmationCodeWatermark;
    internal TextBlock textBlockSendSms;
    private bool _contentLoaded;

    public Auth2FAPage()
    {
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      Uri uri = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton.IconUri = uri;
      string appBarConfirm = CommonResources.AppBar_Confirm;
      applicationBarIconButton.Text = appBarConfirm;
      this._sendButton = applicationBarIconButton;
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_SecurityCheck;
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.SupportMenu = false;
      this.SuppressMenu = true;
      base.Loaded+=(new RoutedEventHandler( this.OnLoaded));
      this.BuildAppBar();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      new DelayedExecutor(300).AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => ((Control) this.textBoxConfirmationCode).Focus()))));
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._sendButton.Click+=(new EventHandler(this.SendButton_OnClick));
      applicationBar.Buttons.Add(this._sendButton);
      this.ApplicationBar = ((IApplicationBar) applicationBar);
    }

    private void SendButton_OnClick(object sender, EventArgs eventArgs)
    {
      if (string.IsNullOrWhiteSpace(this._viewModel.Code))
        return;
      ((Control) this.textBoxConfirmationCode).IsEnabled = false;
      this._viewModel.Login((Action) (() => Execute.ExecuteOnUIThread((Action) (() => ((Control) this.textBoxConfirmationCode).IsEnabled = true))));
      ((Control) this).Focus();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      string index1 = "username";
      string username = queryString[index1];
      string index2 = "password";
      string password = queryString[index2];
      string index3 = "phoneMask";
      string phoneMask = queryString[index3];
      string index4 = "validationType";
      string validationType = queryString[index4];
      string index5 = "validationSid";
      string validationSid = queryString[index5];
      this._viewModel = new Auth2FAViewModel(username, password, phoneMask, validationType, validationSid);
      this.RestorePageUnboundState();
      base.DataContext = this._viewModel;
      this._isInitialized = true;
    }

    protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      base.HandleOnNavigatingFrom(e);
      if (e.NavigationMode != NavigationMode.Back || ParametersRepository.Contains("ValidationResponse"))
        return;
      ParametersRepository.SetParameterForId("ValidationResponse", new ValidationUserResponse()
      {
        IsSucceeded = false
      });
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      if (e.NavigationMode == NavigationMode.Back)
        return;
      this.SavePageUnboundState();
    }

    private void SavePageUnboundState()
    {
      IDictionary<string, object> state = this.State;
      string index = "Auth2FAPage_State";
      Auth2FAPage.PageState pageState = new Auth2FAPage.PageState();
      int num = this._viewModel.IsSMSMode ? 1 : 0;
      pageState.IsSMSMode = num != 0;
      state[index] = pageState;
    }

    private void RestorePageUnboundState()
    {
      if (!this.State.ContainsKey("Auth2FAPage_State"))
        return;
      Auth2FAPage.PageState pageState = this.State["Auth2FAPage_State"] as Auth2FAPage.PageState;
      if (pageState == null || !pageState.IsSMSMode)
        return;
      this._viewModel.IsSMSMode = true;
      this.SetSmsInputScope();
    }

    private void SetSmsInputScope()
    {
      InputScope inputScope = new InputScope();
      InputScopeName inputScopeName1 = new InputScopeName();
      int num = 52;
      inputScopeName1.NameValue = ((InputScopeNameValue) num);
      InputScopeName inputScopeName2 = inputScopeName1;
      inputScope.Names.Add(inputScopeName2);
      this.textBoxConfirmationCode.InputScope = inputScope;
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this._viewModel.Code = this.textBoxConfirmationCode.Text;
      this._sendButton.IsEnabled = (!string.IsNullOrWhiteSpace(this._viewModel.Code));
      ((UIElement) this.textBlockConfirmationCodeWatermark).Visibility = (string.IsNullOrEmpty(this.textBoxConfirmationCode.Text) ? Visibility.Visible : Visibility.Collapsed);
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter || string.IsNullOrWhiteSpace(this._viewModel.Code))
        return;
      this._viewModel.Login( null);
      ((Control) this).Focus();
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.textBoxConfirmationCode.Text))
        return;
      this.textBoxConfirmationCode.SelectAll();
    }

    private void SendSms_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._viewModel.IsSMSMode)
      {
        this._viewModel.SendSMS();
        ((Control) this.textBoxConfirmationCode).Focus();
      }
      else
      {
        List<MenuItem> menuItemList = new List<MenuItem>();
        MenuItem menuItem1 = new MenuItem();
        string auth2FaSendSmsCode = CommonResources.Auth2FA_SendSmsCode;
        menuItem1.Header = auth2FaSendSmsCode;
        MenuItem menuItem2 = menuItem1;
        menuItem2.Click += new RoutedEventHandler( this.SendSmsMenuItem_OnClick);
        MenuItem menuItem3 = menuItem2;
        menuItemList.Add(menuItem3);
        ContextMenu contextMenu1 = new ContextMenu();
        SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
        ((Control) contextMenu1).Background = ((Brush) solidColorBrush1);
        SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
        ((Control) contextMenu1).Foreground = ((Brush) solidColorBrush2);
        int num = 0;
        contextMenu1.IsZoomEnabled = num != 0;
        ContextMenu contextMenu2 = contextMenu1;
        foreach (MenuItem menuItem4 in menuItemList)
          ((PresentationFrameworkCollection<object>) contextMenu2.Items).Add(menuItem4);
        ContextMenuService.SetContextMenu((DependencyObject) this.textBlockSendSms, contextMenu2);
        contextMenu2.IsOpen = true;
      }
    }

    private void SendSmsMenuItem_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      this._viewModel.SendSMS();
      this.SetSmsInputScope();
      ((Control) this.textBoxConfirmationCode).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Auth2FAPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.textBoxConfirmationCode = (TextBox) base.FindName("textBoxConfirmationCode");
      this.textBlockConfirmationCodeWatermark = (TextBlock) base.FindName("textBlockConfirmationCodeWatermark");
      this.textBlockSendSms = (TextBlock) base.FindName("textBlockSendSms");
    }

    [DataContract]
    private class PageState
    {
      [DataMember]
      public bool IsSMSMode { get; set; }
    }
  }
}
