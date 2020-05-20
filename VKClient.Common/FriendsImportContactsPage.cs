using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library.FriendsImport;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FriendsImportContactsPage : PageBase
  {
    private bool _isInitialized;
    internal FriendsImportUC ucFriendsImport;
    private bool _contentLoaded;

    public FriendsImportContactsPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this.ucFriendsImport.SetFriendsImportProvider((IFriendsImportProvider) ContactsFriendsImportProvider.Instance);
      base.DataContext = (new FriendsImportPageViewModel());
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FriendsImportContactsPage.xaml", UriKind.Relative));
      this.ucFriendsImport = (FriendsImportUC) base.FindName("ucFriendsImport");
    }
  }
}
