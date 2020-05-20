using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using Windows.UI.Notifications;

namespace VKClient.Common
{
    public partial class WelcomePage : PageBase
    {

        public WelcomePage()
        {
            this.InitializeComponent();
            this.SuppressMenu = true;
            this._manualHandleValidationParams = true;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            this.DataContext = (object)new ViewModelBase();
            this.NavigationService.ClearBackStack();
            ToastNotificationManager.History.Clear();
            PushNotificationsManager.Instance.EnsureTheChannelIsClosed();
            AppGlobalStateManager.Current.HandleUserLogoutOrAuthorizationFailure();
            StickersSettings.Instance.Reset();
            SearchHintsUC.Reset();
            ContactsManager.Instance.DeleteAllContactsAsync();
            TileManager.Instance.UpdateTileWithCount(0, (Action)(() => { }));
            NewsViewModel.Instance.Reset();
            new WebBrowser().ClearCookiesAsync();
            BaseDataManager.Instance.NeedRefreshBaseData = true;
        }

        private void OnLogInClicked(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri(string.Format("/LoginPage.xaml"), UriKind.Relative));
        }

        private void OnSignUpClicked(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToRegistrationPage();
        }

        private void OnTermsClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToWebUri("https://m.vk.com/terms", true, false);
        }
    }
}
