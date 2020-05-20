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
    public class ManagersPage : PageBase
    {
        private bool _isInitialized;
        internal GenericHeaderUC Header;
        internal ExtendedLongListSelector List;
        internal PullToRefreshUC PullToRefresh;
        private bool _contentLoaded;

        private ManagersViewModel ViewModel
        {
            get
            {
                return base.DataContext as ManagersViewModel;
            }
        }
        public ManagersPage()
        {
            this.InitializeComponent();
            Action a = new Action(this.List.ScrollToTop);
            GenericHeaderUC expr_12 = this.Header;
            expr_12.OnHeaderTap = (Action)Delegate.Combine(expr_12.OnHeaderTap, a);
            this.PullToRefresh.TrackListBox(this.List);
            this.List.OnRefresh = delegate
            {
                this.ViewModel.Managers.LoadData(true, false, null, false);
            };
        }


        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            long communityId = long.Parse(((Page)this).NavigationContext.QueryString["CommunityId"]);
            GroupType communityType = (GroupType)int.Parse(((Page)this).NavigationContext.QueryString["CommunityType"]);
            int num = (int)communityType;
            ManagersViewModel managersViewModel = new ManagersViewModel(communityId, (GroupType)num);
            base.DataContext = managersViewModel;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri;
            string appBarAdd = CommonResources.AppBar_Add;
            applicationBarIconButton1.Text = appBarAdd;
            ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
            applicationBarIconButton2.Click += ((EventHandler)((p, f) => Navigator.Current.NavigateToCommunitySubscribers(this.ViewModel.CommunityId, communityType, false, true, false)));
            base.ApplicationBar = ((IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
            base.ApplicationBar.Buttons.Add(applicationBarIconButton2);
            managersViewModel.Managers.LoadData(true, false, null, false);
            this._isInitialized = true;
        }

        private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
        {
            this.ViewModel.Managers.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LinkHeader selectedItem = this.List.SelectedItem as LinkHeader;
            if (selectedItem == null)
                return;
            this.List.SelectedItem = null;
            Navigator.Current.NavigateToUserProfile(selectedItem.Id, "", "", false);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/ManagersPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)base.FindName("Header");
            this.List = (ExtendedLongListSelector)base.FindName("List");
            this.PullToRefresh = (PullToRefreshUC)base.FindName("PullToRefresh");
        }
    }
}
