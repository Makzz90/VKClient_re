using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class WelcomePage : PageBase
  {
    private bool _contentLoaded;

    public WelcomePage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
      this._manualHandleValidationParams = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
		{
			base.HandleOnNavigatedTo(e);
			base.DataContext=(new ViewModelBase());
			base.NavigationService.ClearBackStack();
			PushNotificationsManager.Instance.EnsureTheChannelIsClosed();
			AppGlobalStateManager.Current.HandleUserLogoutOrAuthorizationFailure();
			StickersSettings.Instance.Reset();
			SearchHintsUC.Reset();
			ContactsManager.Instance.DeleteAllContactsAsync();
			TileManager arg_70_0 = TileManager.Instance;
			int arg_70_1 = 0;
            Action arg_70_2 = new Action(() => { });
			
			arg_70_0.UpdateTileWithCount(arg_70_1, arg_70_2);
			NewsViewModel.Instance.Reset();
			WebBrowserExtensions.ClearCookiesAsync(new WebBrowser());
			BaseDataManager.Instance.NeedRefreshBaseData = true;
		}

    private void OnLogInClicked(object sender, RoutedEventArgs e)
    {
      ((Page) this).NavigationService.Navigate(new Uri(string.Format("/LoginPage.xaml"), UriKind.Relative));
    }

    private void OnSignUpClicked(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToRegistrationPage();
    }

    private void OnTermsClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://m.vk.com/terms", true, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient;component/WelcomePage.xaml", UriKind.Relative));
    }
  }
}
