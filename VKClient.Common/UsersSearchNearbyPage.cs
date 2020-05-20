using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using Windows.System;

namespace VKClient.Common
{
    public class UsersSearchNearbyPage : PageBase
    {
        private UsersSearchNearbyViewModel _viewModel;
        internal VisualStateGroup CommonStates;
        internal VisualState Normal;
        internal VisualState Disabled;
        internal GenericHeaderUC ucHeader;
        internal ProgressRing progressRing;
        internal TextBlock textBlockDescription;
        internal TextBlock textBlockDisabled;
        internal Button buttonOpenSettings;
        internal ExtendedLongListSelector listUsers;
        private bool _contentLoaded;

        public UsersSearchNearbyPage()
        {
            this.InitializeComponent();
            this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_Nearby;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            this._viewModel = new UsersSearchNearbyViewModel();
            base.DataContext = this._viewModel;
            this._viewModel.LoadGeoposition(new Action<GeoPositionStatus>(this.HandlePositionStatus));
        }

        private void HandlePositionStatus(GeoPositionStatus status)
        {
            if (status == GeoPositionStatus.Ready)
            {
                this._viewModel.StopLoading();
                VisualStateManager.GoToState((Control)this, "Disabled", false);
            }
            else
            {
                VisualStateManager.GoToState((Control)this, "Normal", false);
                if (status != GeoPositionStatus.Initializing)
                    return;
                this._viewModel.StartLoading();
            }
        }

        private async void ButtonOpenSettings_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UsersSearchNearbyPage.xaml", UriKind.Relative));
            this.CommonStates = (VisualStateGroup)base.FindName("CommonStates");
            this.Normal = (VisualState)base.FindName("Normal");
            this.Disabled = (VisualState)base.FindName("Disabled");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.progressRing = (ProgressRing)base.FindName("progressRing");
            this.textBlockDescription = (TextBlock)base.FindName("textBlockDescription");
            this.textBlockDisabled = (TextBlock)base.FindName("textBlockDisabled");
            this.buttonOpenSettings = (Button)base.FindName("buttonOpenSettings");
            this.listUsers = (ExtendedLongListSelector)base.FindName("listUsers");
        }
    }
}
