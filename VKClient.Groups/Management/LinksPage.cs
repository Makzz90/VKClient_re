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
    public class LinksPage : PageBase
    {
        private bool _isInitialized;
        internal GenericHeaderUC Header;
        internal ExtendedLongListSelector List;
        internal PullToRefreshUC PullToRefresh;
        private bool _contentLoaded;

        private LinksViewModel ViewModel
        {
            get
            {
                return ((FrameworkElement)this).DataContext as LinksViewModel;
            }
        }

        public LinksPage()
        {
            this.InitializeComponent();
            this.Header.OnHeaderTap += (Action)(() => this.List.ScrollToTop());
            this.PullToRefresh.TrackListBox((ISupportPullToRefresh)this.List);
            this.List.OnRefresh = (Action)(() => this.ViewModel.Links.LoadData(true, false, (Action<BackendResult<System.Collections.Generic.List<Group>, ResultCode>>)null, false));
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            LinksViewModel linksViewModel = new LinksViewModel(long.Parse(((Page)this).NavigationContext.QueryString["CommunityId"]));
            ((FrameworkElement)this).DataContext=((object)linksViewModel);
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri=(uri);
            string appBarAdd = CommonResources.AppBar_Add;
            applicationBarIconButton1.Text=(appBarAdd);
            ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
            applicationBarIconButton2.Click+=((EventHandler)((p, f) => Navigator.Current.NavigateToCommunityManagementLinkCreation(this.ViewModel.CommunityId, (GroupLink)null)));
            this.ApplicationBar=((IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
            this.ApplicationBar.Buttons.Add((object)applicationBarIconButton2);
            linksViewModel.Links.LoadData(true, false, (Action<BackendResult<System.Collections.Generic.List<Group>, ResultCode>>)null, false);
            this._isInitialized = true;
        }

        private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
        {
            this.ViewModel.Links.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LinkHeader selectedItem = this.List.SelectedItem as LinkHeader;
            if (selectedItem == null)
                return;
            this.List.SelectedItem=((object)null);
            Navigator.Current.NavigateToWebUri(selectedItem.Url, false, false);
        }

        private void ContextMenu_OnEditClicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = (menuItem != null ? ((FrameworkElement)menuItem).Parent : (DependencyObject)null) as ContextMenu;
            FrameworkElement frameworkElement = (contextMenu != null ? contextMenu.Owner : (DependencyObject)null) as FrameworkElement;
            LinkHeader linkHeader = (frameworkElement != null ? frameworkElement.DataContext : (object)null) as LinkHeader;
            if (linkHeader == null)
                return;
            Navigator.Current.NavigateToCommunityManagementLinkCreation(this.ViewModel.CommunityId, linkHeader.Link);
        }

        private void ContextMenu_OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = (menuItem != null ? ((FrameworkElement)menuItem).Parent : (DependencyObject)null) as ContextMenu;
            FrameworkElement frameworkElement = (contextMenu != null ? contextMenu.Owner : (DependencyObject)null) as FrameworkElement;
            LinkHeader linkHeader = (frameworkElement != null ? frameworkElement.DataContext : (object)null) as LinkHeader;
            if (linkHeader == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.LinkRemoving, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this.ViewModel.DeleteLink(linkHeader);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Groups;component/Management/LinksPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)((FrameworkElement)this).FindName("Header");
            this.List = (ExtendedLongListSelector)((FrameworkElement)this).FindName("List");
            this.PullToRefresh = (PullToRefreshUC)((FrameworkElement)this).FindName("PullToRefresh");
        }
    }
}
