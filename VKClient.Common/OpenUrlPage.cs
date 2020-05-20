using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;

namespace VKClient.Common
{
  public class OpenUrlPage : PageBase
  {
    private bool _contentLoaded;

    public OpenUrlPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (e.NavigationMode == NavigationMode.Back)
      {
        Navigator.Current.NavigateToMainPage();
      }
      else
      {
        string uri = ((Page) this).NavigationContext.QueryString["Uri"];
        if (!string.IsNullOrEmpty(uri))
          uri = HttpUtility.UrlDecode(uri);
        Navigator.Current.NavigateToWebUri(uri, false, true);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/OpenUrlPage.xaml", UriKind.Relative));
    }
  }
}
