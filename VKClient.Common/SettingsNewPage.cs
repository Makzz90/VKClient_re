using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
    public class SettingsNewPage : PageBase
    {
        private PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
        {
            ShowCamera = true
        };
        private bool _isInitialized;
        internal Grid LayoutRoot;
        internal GenericHeaderUC Header;
        internal Grid ContentPanel;
        internal Border borderFeedback;
        internal TextBlock textBlockFeedback;
        internal Border borderGeneral;
        internal TextBlock textBlockGeneral;
        internal Border borderAccount;
        internal TextBlock textBlockAccount;
        internal Border borderPrivacy;
        internal TextBlock textBlockPrivacy;
        internal Border borderBlackList;
        internal TextBlock textBlockBlackList;
        internal Border borderQA;
        internal TextBlock textBlockQA;
        internal Border borderAbout;
        internal TextBlock textBlockAbout;
        private bool _contentLoaded;

        private SettingsNewViewModel ViewModel
        {
            get
            {
                return this.DataContext as SettingsNewViewModel;
            }
        }

        public SettingsNewPage()
        {
            this.InitializeComponent();
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(this._photoChooserTask_Completed);
            this.Header.TextBlockTitle.Text = CommonResources.Settings;
        }

        private void _photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;
            if (e.Error != null)
                Logger.Instance.Error("Error chosing photo", e.Error);
            else
                ParametersRepository.SetParameterForId("ChoosenNewProfilePhoto", (object)e.ChosenPhoto);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                this.DataContext = (object)new SettingsNewViewModel();
                this.ViewModel.LoadCurrentUser();
                this._isInitialized = true;
            }
            List<Stream> streamList = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            if (streamList == null || streamList.Count <= 0)
                return;
            //this.ViewModel.SetUserPicture(streamList[0], "newProfilePhoto.jpg");//TODO: функция SetUserPicture не имеет реализации (автор не сделал)
        }

        private void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToEditProfile();
        }

        private void Notifications_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsNotifications();
        }

        private void General_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsGeneral();
        }

        private void Account_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsAccount();
        }

        private void Privacy_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToSettingsPrivacy();
        }

        private void QA_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToHelpPage();
        }

        private void BlackList_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToBlackList();
        }

        private void Balance_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToBalance();
        }

        private void About_Tap(object sender, GestureEventArgs e)
        {
            Navigator.Current.NavigateToAboutPage();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(CommonResources.Settings_LogOutMessage, CommonResources.Settings_LogOutTitle, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this.NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/SettingsNewPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.Header = (GenericHeaderUC)this.FindName("Header");
            this.ContentPanel = (Grid)this.FindName("ContentPanel");
            this.borderFeedback = (Border)this.FindName("borderFeedback");
            this.textBlockFeedback = (TextBlock)this.FindName("textBlockFeedback");
            this.borderGeneral = (Border)this.FindName("borderGeneral");
            this.textBlockGeneral = (TextBlock)this.FindName("textBlockGeneral");
            this.borderAccount = (Border)this.FindName("borderAccount");
            this.textBlockAccount = (TextBlock)this.FindName("textBlockAccount");
            this.borderPrivacy = (Border)this.FindName("borderPrivacy");
            this.textBlockPrivacy = (TextBlock)this.FindName("textBlockPrivacy");
            this.borderBlackList = (Border)this.FindName("borderBlackList");
            this.textBlockBlackList = (TextBlock)this.FindName("textBlockBlackList");
            this.borderQA = (Border)this.FindName("borderQA");
            this.textBlockQA = (TextBlock)this.FindName("textBlockQA");
            this.borderAbout = (Border)this.FindName("borderAbout");
            this.textBlockAbout = (TextBlock)this.FindName("textBlockAbout");
        }
    }
}
