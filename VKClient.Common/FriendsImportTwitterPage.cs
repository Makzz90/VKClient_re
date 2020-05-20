using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library.FriendsImport;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FriendsImportTwitterPage : PageBase
  {
    private bool _isInitialized;
    internal FriendsImportUC ucFriendsImport;
    private bool _contentLoaded;

    public FriendsImportTwitterPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      if (queryString.ContainsKey("oauthToken") && queryString.ContainsKey("oauthVerifier"))
      {
        TwitterFriendsImportProvider.Instance.SetAuthCompletionData(queryString["oauthToken"], queryString["oauthVerifier"]);
        this.ucFriendsImport.SetFriendsImportProvider((IFriendsImportProvider) TwitterFriendsImportProvider.Instance);
      }
      base.DataContext = (new FriendsImportPageViewModel());
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FriendsImportTwitterPage.xaml", UriKind.Relative));
      this.ucFriendsImport = (FriendsImportUC) base.FindName("ucFriendsImport");
    }
  }
}
