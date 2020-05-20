using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;

namespace VKClient.Common
{
  public class WebViewPage : PhoneApplicationPage
  {
    private const string SUPPORT_IN_APP_NAVIGATION_KEY = "SupportInAppNavigation";
    private bool _isInitialized;
    private bool _supportInAppNavigation;
    private bool _skipNextNavigation;
    internal WebBrowser WebView;
    private bool _contentLoaded;

    public WebViewPage()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.BackKeyPress += ((EventHandler<CancelEventArgs>) ((sender, e) =>
      {
        if (!this.WebView.CanGoBack)
          return;
        e.Cancel = true;
        this.WebView.GoBack();
      }));
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._isInitialized = true;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      if (queryString.ContainsKey("SupportInAppNavigation"))
        bool.TryParse(queryString["SupportInAppNavigation"], out this._supportInAppNavigation);
      this.WebView.Navigate(new Uri(HttpUtility.UrlDecode(queryString["Uri"])));
    }

    private void WebView_OnNavigating(object sender, NavigatingEventArgs e)
    {
      if (e.Uri.AbsoluteUri.Contains("blank.html"))
      {
        if (((Page) this).NavigationService.CanGoBack)
          ((Page) this).NavigationService.GoBackSafe();
        else
          Navigator.Current.NavigateToMainPage();
      }
      else
      {
        if (!this._supportInAppNavigation)
          return;
        if (!this._skipNextNavigation)
        {
          ((CancelEventArgs) e).Cancel = true;
          Uri uri = e.Uri;
          Navigator.Current.GetWithinAppNavigationUri(uri.AbsoluteUri, false, (Action<bool>) (isNavigated =>
          {
            if (isNavigated)
              return;
            this.WebView.Navigate(uri);
            this._skipNextNavigation = true;
          }));
        }
        this._skipNextNavigation = false;
      }
    }

    private void WebView_OnLoadCompleted(object sender, NavigationEventArgs e)
    {
      // ISSUE: method pointer
      this.WebView.LoadCompleted -= (new LoadCompletedEventHandler(this.WebView_OnLoadCompleted));
      ((UIElement) this.WebView).Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/WebViewPage.xaml", UriKind.Relative));
      this.WebView = (WebBrowser) base.FindName("WebView");
    }
  }
}
