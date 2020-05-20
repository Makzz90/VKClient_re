using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public class BlacklistPage : PageBase
    {
        private bool _isInitialized;
        internal GenericHeaderUC Header;
        internal ExtendedLongListSelector List;
        internal PullToRefreshUC PullToRefresh;
        private bool _contentLoaded;

        private BlacklistViewModel ViewModel
        {
            get
            {
                return ((FrameworkElement)this).DataContext as BlacklistViewModel;
            }
        }

        public BlacklistPage()
        {
            this.InitializeComponent();
            this.Header.OnHeaderTap += (Action)(() => this.List.ScrollToTop());
            this.PullToRefresh.TrackListBox((ISupportPullToRefresh)this.List);
            this.List.OnRefresh = (Action)(() => this.ViewModel.Users.LoadData(true, false, (Action<BackendResult<BlockedUsers, ResultCode>>)null, false));
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            long communityId = long.Parse(((Page)this).NavigationContext.QueryString["CommunityId"]);
            GroupType communityType = (GroupType)int.Parse(((Page)this).NavigationContext.QueryString["CommunityType"]);
            BlacklistViewModel blacklistViewModel = new BlacklistViewModel(communityId);
            ((FrameworkElement)this).DataContext=((object)blacklistViewModel);
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri=(uri);
            string appBarAdd = CommonResources.AppBar_Add;
            applicationBarIconButton1.Text=(appBarAdd);
            ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
            applicationBarIconButton2.Click+=((EventHandler)((p, f) => Navigator.Current.NavigateToCommunitySubscribers(this.ViewModel.CommunityId, communityType, false, true, true)));
            this.ApplicationBar=((IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
            this.ApplicationBar.Buttons.Add((object)applicationBarIconButton2);
            blacklistViewModel.Users.LoadData(true, false, (Action<BackendResult<BlockedUsers, ResultCode>>)null, false);
            this._isInitialized = true;
        }

        private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
        {
            this.ViewModel.Users.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LinkHeader selectedItem = this.List.SelectedItem as LinkHeader;
            if (selectedItem == null)
                return;
            this.List.SelectedItem=((object)null);
            Navigator.Current.NavigateToUserProfile(selectedItem.Id, "", "", false);
        }

        private void ContextMenu_OnEditClicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = (menuItem != null ? ((FrameworkElement)menuItem).Parent : (DependencyObject)null) as ContextMenu;
            FrameworkElement frameworkElement = (contextMenu != null ? contextMenu.Owner : (DependencyObject)null) as FrameworkElement;
            LinkHeader linkHeader = (frameworkElement != null ? frameworkElement.DataContext : (object)null) as LinkHeader;
            if (linkHeader == null)
                return;
            Navigator.Current.NavigateToCommunityManagementBlockEditing(this.ViewModel.CommunityId, linkHeader.User, linkHeader.User.ban_info.manager);
        }

        private void ContextMenu_OnUnblockClicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = (menuItem != null ? ((FrameworkElement)menuItem).Parent : (DependencyObject)null) as ContextMenu;
            FrameworkElement frameworkElement = (contextMenu != null ? contextMenu.Owner : (DependencyObject)null) as FrameworkElement;
            LinkHeader linkHeader = (frameworkElement != null ? frameworkElement.DataContext : (object)null) as LinkHeader;
            if (linkHeader == null)
                return;
            this.ViewModel.UnblockUser(linkHeader);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Groups;component/Management/BlacklistPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)((FrameworkElement)this).FindName("Header");
            this.List = (ExtendedLongListSelector)((FrameworkElement)this).FindName("List");
            this.PullToRefresh = (PullToRefreshUC)((FrameworkElement)this).FindName("PullToRefresh");
        }
    }
}
