using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public class ServiceSwitchPage : PageBase
    {
        internal GenericHeaderUC Header;
        private bool _contentLoaded;

        public ServiceSwitchViewModel ViewModel
        {
            get
            {
                return base.DataContext as ServiceSwitchViewModel;
            }
        }

        public ServiceSwitchPage()
        {
            this.InitializeComponent();
            this.SuppressMenu = true;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            base.DataContext = (new ServiceSwitchViewModel((CommunityService)int.Parse(((Page)this).NavigationContext.QueryString["Service"]), (CommunityServiceState)int.Parse(((Page)this).NavigationContext.QueryString["CurrentState"])));
        }

        private void Disabled_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ViewModel.SaveResult(CommunityServiceState.Disabled);
        }

        private void Opened_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ViewModel.SaveResult(CommunityServiceState.Opened);
        }

        private void Limited_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ViewModel.SaveResult(CommunityServiceState.Limited);
        }

        private void Closed_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ViewModel.SaveResult(CommunityServiceState.Closed);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/ServiceSwitchPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)base.FindName("Header");
        }
    }
}
