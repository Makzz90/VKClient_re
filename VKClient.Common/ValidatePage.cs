using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Framework;

namespace VKClient.Common
{
    public class ValidatePage : PhoneApplicationPage
    {
        private const string REDIRECT_URL = "https://oauth.vk.com/blank.html";
        private const string VALIDATION_RESPONSE_KEY = "ValidationResponse";
        private const string MONEY_TRANSFER_ACCEPTED_RESPONSE_KEY = "MoneyTransferAcceptedResponse";
        private const string MONEY_TRANSFER_SENT_RESPONSE_KEY = "MoneyTransferSentResponse";
        private bool _isInitialized;
        private string _scopes;
        private bool _revoke;
        private string _validationUri;
        private bool _isMoneyTransfer;
        private bool _isAcceptMoneyTransfer;
        private long _transferId;
        private long _fromId;
        private long _toId;
        internal WebBrowser webBrowser;
        private bool _contentLoaded;

        public ValidatePage()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            this.BackKeyPress += ((EventHandler<CancelEventArgs>)((sender, e) =>
            {
                if (!this.webBrowser.CanGoBack)
                    return;
                e.Cancel = true;
                this.webBrowser.GoBack();
            }));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (this._isInitialized)
                return;
            this.SetupSystemTray();
            if (((Page)this).NavigationContext.QueryString.ContainsKey("ValidationUri"))
            {
                this._validationUri = ((Page)this).NavigationContext.QueryString["ValidationUri"];
                if (((Page)this).NavigationContext.QueryString.ContainsKey("IsMoneyTransfer"))
                    this._isMoneyTransfer = true;
                else if (((Page)this).NavigationContext.QueryString.ContainsKey("IsAcceptMoneyTransfer"))
                {
                    this._isMoneyTransfer = true;
                    this._isAcceptMoneyTransfer = true;
                    long.TryParse(((Page)this).NavigationContext.QueryString["TransferId"], out this._transferId);
                    long.TryParse(((Page)this).NavigationContext.QueryString["FromId"], out this._fromId);
                    long.TryParse(((Page)this).NavigationContext.QueryString["ToId"], out this._toId);
                }
            }
            else
            {
                this._scopes = ((Page)this).NavigationContext.QueryString["Scopes"];
                this._revoke = ((Page)this).NavigationContext.QueryString["Revoke"] == bool.TrueString;
            }
            this.InitializeWebBrowser();
            this._isInitialized = true;
        }

        private void SetupSystemTray()
        {
            SystemTray.Opacity = 0.0;
            SystemTray.ForegroundColor = (((SolidColorBrush)Application.Current.Resources["PhoneSystemTrayForegroundBrush"]).Color);
            SystemTray.IsVisible = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode != NavigationMode.Back)
                return;
            // ISSUE: method pointer
            this.webBrowser.NavigationFailed -= (new NavigationFailedEventHandler(this.BrowserOnNavigationFailed));
            this.webBrowser.Navigating -= (new EventHandler<NavigatingEventArgs>(this.BrowserOnNavigating));
            if (!this._isMoneyTransfer)
            {
                if (ParametersRepository.Contains("ValidationResponse"))
                    return;
                ParametersRepository.SetParameterForId("ValidationResponse", new ValidationUserResponse()
                {
                    IsSucceeded = false
                });
            }
            else if (this._isAcceptMoneyTransfer)
            {
                if (ParametersRepository.Contains("MoneyTransferAcceptedResponse"))
                    return;
                ParametersRepository.SetParameterForId("MoneyTransferAcceptedResponse", new MoneyTransferAcceptedResponse()
                {
                    IsSucceeded = false
                });
            }
            else
            {
                if (ParametersRepository.Contains("MoneyTransferSentResponse"))
                    return;
                ParametersRepository.SetParameterForId("MoneyTransferSentResponse", new MoneyTransferAcceptedResponse()
                {
                    IsSucceeded = false
                });
            }
        }

        private void InitializeWebBrowser()
        {
            if (string.IsNullOrEmpty(this._validationUri))
                return;
            string validationUri = this._validationUri;
            // ISSUE: method pointer
            this.webBrowser.NavigationFailed += (new NavigationFailedEventHandler(this.BrowserOnNavigationFailed));
            this.webBrowser.Navigating += (new EventHandler<NavigatingEventArgs>(this.BrowserOnNavigating));
            // ISSUE: method pointer
            this.webBrowser.LoadCompleted += (new LoadCompletedEventHandler(this.BrowserOnLoadCompleted));
            this.webBrowser.Navigate(new Uri(validationUri));
        }

        private void BrowserOnLoadCompleted(object sender, NavigationEventArgs navigationEventArgs)
        {
            // ISSUE: method pointer
            this.webBrowser.LoadCompleted -= (new LoadCompletedEventHandler(this.BrowserOnLoadCompleted));
            ((UIElement)this.webBrowser).Visibility = Visibility.Visible;
        }

        private void BrowserOnNavigating(object sender, NavigatingEventArgs args)
        {
            string absoluteUri = args.Uri.AbsoluteUri;
            if (!absoluteUri.StartsWith("https://oauth.vk.com/blank.html"))
                return;
            Dictionary<string, string> dictionary = ValidatePage.ExploreQueryString(absoluteUri.Substring(absoluteUri.IndexOf('#') + 1));
            if (!this._isMoneyTransfer)
                ValidatePage.ProcessResult((IDictionary<string, string>)dictionary);
            else if (this._isAcceptMoneyTransfer)
                this.ProcessMoneyTransferAcceptResult((IDictionary<string, string>)dictionary);
            else
                ValidatePage.ProcessMoneyTransferSendResult((IDictionary<string, string>)dictionary);
            ((Page)this).NavigationService.GoBackSafe();
        }

        private static void ProcessResult(IDictionary<string, string> dict)
        {
            ValidationUserResponse validationUserResponse = new ValidationUserResponse();
            if (dict.ContainsKey("success"))
            {
                validationUserResponse.IsSucceeded = true;
                if (dict.ContainsKey("access_token"))
                    validationUserResponse.access_token = dict["access_token"];
                if (dict.ContainsKey("user_id"))
                {
                    long result = 0;
                    if (long.TryParse(dict["user_id"], out result))
                        validationUserResponse.user_id = result;
                }
                if (dict.ContainsKey("phone"))
                    validationUserResponse.phone = dict["phone"];
                if (dict.ContainsKey("phone_status"))
                    validationUserResponse.phone_status = dict["phone_status"];
                if (dict.ContainsKey("email"))
                    validationUserResponse.phone = dict["email"];
                if (dict.ContainsKey("email_status"))
                    validationUserResponse.phone_status = dict["email_status"];
            }
            ParametersRepository.SetParameterForId("ValidationResponse", validationUserResponse);
        }

        private void ProcessMoneyTransferAcceptResult(IDictionary<string, string> dict)
        {
            MoneyTransferAcceptedResponse acceptedResponse = new MoneyTransferAcceptedResponse();
            if (dict.ContainsKey("success"))
            {
                acceptedResponse.IsSucceeded = true;
                acceptedResponse.TransferId = this._transferId;
                acceptedResponse.FromId = this._fromId;
                acceptedResponse.ToId = this._toId;
            }
            ParametersRepository.SetParameterForId("MoneyTransferAcceptedResponse", acceptedResponse);
        }

        private static void ProcessMoneyTransferSendResult(IDictionary<string, string> dict)
        {
            MoneyTransferSentResponse transferSentResponse = new MoneyTransferSentResponse();
            if (dict.ContainsKey("success"))
                transferSentResponse.IsSucceeded = true;
            ParametersRepository.SetParameterForId("MoneyTransferSentResponse", transferSentResponse);
        }

        private void BrowserOnNavigationFailed(object sender, NavigationFailedEventArgs navigationFailedEventArgs)
        {
        }

        public static Dictionary<string, string> ExploreQueryString(string queryString)
        {
            string[] strArray1 = queryString.Split('&');
            Dictionary<string, string> dictionary = new Dictionary<string, string>(strArray1.Length);
            foreach (string str in strArray1)
            {
                char[] chArray = new char[1] { '=' };
                string[] strArray2 = str.Split(chArray);
                dictionary.Add(strArray2[0], strArray2[1]);
            }
            return dictionary;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/ValidatePage.xaml", UriKind.Relative));
            this.webBrowser = (WebBrowser)base.FindName("webBrowser");
        }
    }
}
