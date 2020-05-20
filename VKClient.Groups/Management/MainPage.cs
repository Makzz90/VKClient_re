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
    public class MainPage : PageBase
    {
        internal GenericHeaderUC Header;
        private bool _contentLoaded;

        public MainViewModel ViewModel
        {
            get
            {
                return base.DataContext as MainViewModel;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            long id = long.Parse(base.NavigationContext.QueryString["CommunityId"]);
            GroupType groupType = (GroupType)int.Parse(base.NavigationContext.QueryString["CommunityType"]);
            bool flag = base.NavigationContext.QueryString["IsAdministrator"].ToLower() == "true";
            base.DataContext = new MainViewModel(id, groupType, flag);
        }

        private void Information_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementInformation(this.ViewModel.Id);
        }

        private void Services_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServices(this.ViewModel.Id);
        }

        private void Managers_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementManagers(this.ViewModel.Id, this.ViewModel.Type);
        }

        private void Requests_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementRequests(this.ViewModel.Id);
        }

        private void Invitations_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementInvitations(this.ViewModel.Id);
        }

        private void Members_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunitySubscribers(this.ViewModel.Id, this.ViewModel.Type, true, false, false);
        }

        private void Blacklist_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementBlacklist(this.ViewModel.Id, this.ViewModel.Type);
        }

        private void Links_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementLinks(this.ViewModel.Id);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/MainPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)base.FindName("Header");
        }
    }
}
