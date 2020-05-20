using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace VKClient.Common
{
  public class ShareExternalContentPage : PageBase
  {
    private bool _isInitialized;
    internal ShareContentUC ucShareContent;
    private bool _contentLoaded;

    public ShareExternalContentPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      IShareContentDataProvider contentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
      if (!(contentDataProvider is ShareExternalContentDataProvider))
      {
        Navigator.Current.GoBack();
      }
      else
      {
        this.ucShareContent.ShareContentDataProvider = contentDataProvider;
        base.DataContext = (new ViewModelBase());
        //(Application.Current as IAppStateInfo).ShareOperation =  null;
        this._isInitialized = true;
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/ShareExternalContentPage.xaml", UriKind.Relative));
      this.ucShareContent = (ShareContentUC) base.FindName("ucShareContent");
    }
  }
}
