using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
    public class GroupDiscussionsPage : PageBase
    {
        //private readonly int OFFSET_KNOB;
        private bool _isInitialized;
        private ApplicationBarIconButton _appBarButtonRefresh;
        private ApplicationBarIconButton _appBarButtonAdd;
        private ApplicationBar _defaultAppBar;
        internal Grid LayoutRoot;
        internal Grid ContentPanel;
        internal ExtendedLongListSelector listBoxThemeHeaders;
        internal GenericHeaderUC Header;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;

        public GroupDiscussionsViewModel GroupDiscussionsVM
        {
            get
            {
                return base.DataContext as GroupDiscussionsViewModel;
            }
        }

        public GroupDiscussionsPage()
        {
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri1 = new Uri("Resources/appbar.refresh.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string appBarRefresh = CommonResources.AppBar_Refresh;
            applicationBarIconButton1.Text = appBarRefresh;
            this._appBarButtonRefresh = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            Uri uri2 = new Uri("Resources/appbar.add.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string appBarAdd = CommonResources.AppBar_Add;
            applicationBarIconButton2.Text = appBarAdd;
            this._appBarButtonAdd = applicationBarIconButton2;
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = appBarBgColor;
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = appBarFgColor;
            double num = 0.9;
            applicationBar.Opacity = num;
            this._defaultAppBar = applicationBar;
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            this.InitializeComponent();
            this.BuildAppBar();
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxThemeHeaders);
            // ISSUE: method pointer
            this.listBoxThemeHeaders.OnRefresh = delegate
            {
                this.GroupDiscussionsVM.LoadData(true, false);
            };
            // ISSUE: method pointer
            this.Header.OnHeaderTap = delegate
                  {
                      this.listBoxThemeHeaders.ScrollToTop();
                  };
        }

        private void BuildAppBar()
        {
            this._appBarButtonRefresh.Click += (new EventHandler(this._appBarButtonRefresh_Click));
            this._appBarButtonAdd.Click += (new EventHandler(this._appBarButtonAdd_Click));
            this._defaultAppBar.Opacity = 0.9;
        }

        private void _appBarButtonAdd_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToNewWallPost(this.GroupDiscussionsVM.GroupId, true, this.GroupDiscussionsVM.AdminLevel, this.GroupDiscussionsVM.IsPublicPage, true, false);
        }

        private void _appBarButtonRefresh_Click(object sender, EventArgs e)
        {
            this.GroupDiscussionsVM.LoadData(true, false);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                long gid = long.Parse(((Page)this).NavigationContext.QueryString["GroupId"]);
                int num1 = int.Parse(((Page)this).NavigationContext.QueryString["AdminLevel"]);
                bool flag1 = ((Page)this).NavigationContext.QueryString["IsPublicPage"] == bool.TrueString;
                bool flag2 = ((Page)this).NavigationContext.QueryString["CanCreateTopic"] == bool.TrueString;
                int adminLevel = num1;
                int num2 = flag1 ? 1 : 0;
                int num3 = flag2 ? 1 : 0;
                GroupDiscussionsViewModel discussionsViewModel = new GroupDiscussionsViewModel(gid, adminLevel, num2 != 0, num3 != 0);
                base.DataContext = discussionsViewModel;
                discussionsViewModel.LoadData(false, false);
                this._isInitialized = true;
            }
            this.UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            if (!this.GroupDiscussionsVM.CanCreateDiscussion || this._defaultAppBar.Buttons.Contains(this._appBarButtonAdd))
                return;
            this._defaultAppBar.Buttons.Insert(0, this._appBarButtonAdd);
            this.ApplicationBar = ((IApplicationBar)this._defaultAppBar);
        }

        private void listBoxThemeHeaders_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.GroupDiscussionsVM.DiscussionsVM.LoadMoreIfNeeded((e.ContentPresenter.Content as ThemeHeader));
        }

        private void listBoxThemeHeaders_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            this.listBoxThemeHeaders.SelectedItem = null;
        }

        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ThemeHeader dataContext = (sender as FrameworkElement).DataContext as ThemeHeader;
            if (dataContext == null)
                return;
            this.NavigateToDiscussion(false, dataContext);
        }

        private void Grid_Tap_2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ThemeHeader dataContext = (sender as FrameworkElement).DataContext as ThemeHeader;
            if (dataContext == null)
                return;
            this.NavigateToDiscussion(true, dataContext);
        }

        private void NavigateToDiscussion(bool loadFromEnd, ThemeHeader header)
        {
            this.GroupDiscussionsVM.NavigateToDiscusson(loadFromEnd, header);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/GroupDiscussionsPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
            this.listBoxThemeHeaders = (ExtendedLongListSelector)base.FindName("listBoxThemeHeaders");
            this.Header = (GenericHeaderUC)base.FindName("Header");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
        }
    }
}
