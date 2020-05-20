using Microsoft.Phone.Tasks;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
//
using System.Windows.Media;

namespace VKClient.Common
{
    public class SettingsPage : PageBase
    {
        private readonly PhotoChooserTask _photoChooserTask;
        private bool _isInitialized;
        private bool _contentLoaded;
        //
        internal RectangleGeometry rectangleGeometry;

        public SettingsPage()
        {
            PhotoChooserTask photoChooserTask = new PhotoChooserTask();
            int num = 1;
            photoChooserTask.ShowCamera = (num != 0);
            this._photoChooserTask = photoChooserTask;
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            this.InitializeComponent();
            ((ChooserBase<PhotoResult>)this._photoChooserTask).Completed += ((EventHandler<PhotoResult>)((o, e) =>
            {
                if (((TaskEventArgs)e).TaskResult != TaskResult.OK || ((TaskEventArgs)e).Error != null)
                    return;
                ParametersRepository.SetParameterForId("ChoosenNewProfilePhoto", e.ChosenPhoto);
            }));
            //
            this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            SettingsViewModel settingsViewModel = new SettingsViewModel();
            settingsViewModel.LoadCurrentUser();
            base.DataContext = settingsViewModel;
            this._isInitialized = true;
        }

        private void EditProfile_OnClicked(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToEditProfile();
        }

        private void Notifications_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsNotifications();
        }

        private void General_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsGeneral();
        }

        private void Account_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsAccount();
        }

        private void Privacy_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsPrivacy();
        }

        private void Blacklist_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToBlacklist();
        }

        private void Balance_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToBalance();
        }

        private void MoneyTransfers_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToTransfersListPage();
        }

        private void Diagnostics_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToDiagnostics();
        }

        private void Support_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToHelpPage();
        }

        private void About_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToAboutPage();
        }

        private void Logout_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show(CommonResources.Settings_LogOutMessage, CommonResources.Settings_LogOutTitle, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            ((Page)this).NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/SettingsPage.xaml", UriKind.Relative));
            //
            this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
        }
    }
}
