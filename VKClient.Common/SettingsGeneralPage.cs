using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using Windows.System;

namespace VKClient.Common
{
    public class SettingsGeneralPage : PageBase
    {
        private bool _isInitialized;
        internal Grid LayoutRoot;
        internal GenericHeaderUC Header;
        internal Grid ContentPanel;
        private bool _contentLoaded;

        private SettingsGeneralViewModel VM
        {
            get
            {
                return base.DataContext as SettingsGeneralViewModel;
            }
        }

        public SettingsGeneralPage()
        {
            this.InitializeComponent();
            this.Header.textBlockTitle.Text = (CommonResources.NewSettings_General.ToUpperInvariant());
        }

        private async void ConfigureLockScreenTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
        }

        private void ClearMusicCacheTap(object sender, GestureEventArgs e)//mod
        {
            this.VM.ClearMusicCache();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            base.DataContext = (new SettingsGeneralViewModel());
            this._isInitialized = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/SettingsGeneralPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.Header = (GenericHeaderUC)base.FindName("Header");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
        }

        //
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = sender as Slider;
            VM.UserAvatarRadius = (int)s.Value;
        }
        private void Slider_ValueChangedNotify(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = sender as Slider;
            VM.NotifyRadius = (int)s.Value;
        }
        //
    }
}
