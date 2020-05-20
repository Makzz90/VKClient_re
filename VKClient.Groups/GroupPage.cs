using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
    public class GroupPage : PageBase
    {
        private bool _isInitialized;
        private bool _forbidOverrideGoBack;
        private long _gid;
        private ApplicationBar _defaultAppBar;
        private readonly ApplicationBarIconButton _appBarButtonRefresh;
        private readonly ApplicationBarIconButton _appBarButtonAddNews;
        private readonly ApplicationBarIconButton _appBarButtonPin;
        private readonly ApplicationBarMenuItem _appBarMenuItemFaveUnfave;
        private readonly ApplicationBarMenuItem _appBarMenuItemSubscribeUnsubscribe;
        private readonly ApplicationBarMenuItem _appBarMenuItemLeaveGroup;
        private readonly ApplicationBarMenuItem _appBarMenuItemPinToStart;
        internal Grid LayoutRoot;
        internal GenericHeaderUC Header;
        internal Pivot pivot;
        internal PivotItem pivotItemMain;
        internal ViewportControl scrollViewer;
        internal MyVirtualizingStackPanel stackPanelMain;
        internal MyVirtualizingPanel2 wallPanel;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;
        //
        internal RectangleGeometry rectangleGeometry;
        internal RectangleGeometry rectangleGeometry2;

        public GroupViewModel GroupVM
        {
            get
            {
                return base.DataContext as GroupViewModel;
            }
        }

        public GroupPage()
        {
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = (appBarBgColor);
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = (appBarFgColor);
            this._defaultAppBar = applicationBar;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            string appBarRefresh = CommonResources.AppBar_Refresh;
            applicationBarIconButton1.Text = (appBarRefresh);
            Uri refreshUri = AppBarResources.RefreshUri;
            applicationBarIconButton1.IconUri = (refreshUri);
            this._appBarButtonRefresh = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            string mainPageNewsAddNews = CommonResources.MainPage_News_AddNews;
            applicationBarIconButton2.Text = (mainPageNewsAddNews);
            Uri addNewsUri = AppBarResources.AddNewsUri;
            applicationBarIconButton2.IconUri = (addNewsUri);
            this._appBarButtonAddNews = applicationBarIconButton2;
            ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
            string pinToStart1 = CommonResources.PinToStart;
            applicationBarIconButton3.Text = (pinToStart1);
            Uri pinToStartUri = AppBarResources.PinToStartUri;
            applicationBarIconButton3.IconUri = (pinToStartUri);
            this._appBarButtonPin = applicationBarIconButton3;
            ApplicationBarMenuItem applicationBarMenuItem1 = new ApplicationBarMenuItem();
            string addToBookmarks = CommonResources.AddToBookmarks;
            applicationBarMenuItem1.Text = (addToBookmarks);
            this._appBarMenuItemFaveUnfave = applicationBarMenuItem1;
            ApplicationBarMenuItem applicationBarMenuItem2 = new ApplicationBarMenuItem();
            string subscribeToNews = CommonResources.SubscribeToNews;
            applicationBarMenuItem2.Text = (subscribeToNews);
            this._appBarMenuItemSubscribeUnsubscribe = applicationBarMenuItem2;
            this._appBarMenuItemLeaveGroup = new ApplicationBarMenuItem();
            ApplicationBarMenuItem applicationBarMenuItem3 = new ApplicationBarMenuItem();
            string pinToStart2 = CommonResources.PinToStart;
            applicationBarMenuItem3.Text = (pinToStart2);
            this._appBarMenuItemPinToStart = applicationBarMenuItem3;
            // ISSUE: explicit constructor call
            //  base.\u002Ector();
            this.InitializeComponent();
            this.wallPanel.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.scrollViewer), false);
            this.scrollViewer.BindViewportBoundsTo((FrameworkElement)this.stackPanelMain);
            this.Header.OnHeaderTap = (Action)(() =>
            {
                if (this.pivot.SelectedItem != this.pivotItemMain)
                    return;
                this.wallPanel.ScrollToBottom(false);
            });
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.wallPanel);
            this.wallPanel.OnRefresh = (Action)(() => this.GroupVM.LoadGroupData(true, false));
            this.RegisterForCleanup((IMyVirtualizingPanel)this.wallPanel);
            this.wallPanel.DeltaOffset = -400.0;
            this.BuildAppBar();
            //
            this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry2.RadiusX = this.rectangleGeometry2.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry2.Rect.Width / 10.0 / 2.0;
        }

        private void BuildAppBar()
        {
            this._defaultAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
            this._appBarButtonRefresh.Click += (new EventHandler(this.AppBarButtonRefresh_OnClick));
            this._appBarMenuItemLeaveGroup.Click += (new EventHandler(this.AppBarMenuItemLeaveGroup_OnClick));
            this._appBarMenuItemPinToStart.Click += (new EventHandler(this.AppBarMenuItemPinToStart_OnClick));
            this._appBarButtonAddNews.Click += (new EventHandler(this.AppBarButtonAddNews_OnClick));
            this._appBarButtonPin.Click += (new EventHandler(this.AppBarButtonPin_OnClick));
            this._appBarMenuItemFaveUnfave.Click += (new EventHandler(this.AppBarMenuItemFaveUnfave_OnClick));
            this._appBarMenuItemSubscribeUnsubscribe.Click += (new EventHandler(this._appBarMenuItemSubscribeUnsubscribe_Click));
        }

        private void _appBarMenuItemSubscribeUnsubscribe_Click(object sender, EventArgs e)
        {
            this.GroupVM.SubscribeUnsubscribe();
        }

        private void AppBarMenuItemFaveUnfave_OnClick(object sender, EventArgs e)
        {
            this.GroupVM.FaveUnfave();
        }

        private void AppBarButtonAddNews_OnClick(object sender, EventArgs e)
        {
            this.GroupVM.HandleActionButton(ActionButtonType.WriteOnWall);
        }

        private void AppBarButtonPin_OnClick(object sender, EventArgs e)
        {
            this.GroupVM.PinToStart();
        }

        private void AppBarMenuItemPinToStart_OnClick(object sender, EventArgs e)
        {
            this.GroupVM.PinToStart();
        }

        private void AppBarMenuItemLeaveGroup_OnClick(object sender, EventArgs e)
        {
            this.GroupVM.HandleActionButton(ActionButtonType.Leave);
        }

        private void AppBarButtonRefresh_OnClick(object sender, EventArgs e)
        {
            this.GroupVM.LoadGroupData(true, false);
        }

        private void UpdateAppBar()
        {
            if (base.ImageViewerDecorator != null && base.ImageViewerDecorator.IsShown || base.IsMenuOpen)
                return;
            bool flag = SecondaryTileManager.Instance.TileExistsFor(this._gid, true);
            if (!this._defaultAppBar.MenuItems.Contains((object)this._appBarMenuItemPinToStart) && !flag)
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemPinToStart);
            if (flag)
                this._defaultAppBar.MenuItems.Remove((object)this._appBarMenuItemPinToStart);
            if (!this._defaultAppBar.MenuItems.Contains((object)this._appBarMenuItemFaveUnfave))
                this._defaultAppBar.MenuItems.Add((object)this._appBarMenuItemFaveUnfave);
            if (!this._defaultAppBar.MenuItems.Contains((object)this._appBarMenuItemSubscribeUnsubscribe))
                this._defaultAppBar.MenuItems.Insert(0, (object)this._appBarMenuItemSubscribeUnsubscribe);
            this._appBarMenuItemFaveUnfave.Text = (this.GroupVM.IsFavorite ? CommonResources.RemoveFromBookmarks : CommonResources.AddToBookmarks);
            this._appBarMenuItemSubscribeUnsubscribe.Text = (this.GroupVM.IsSubscribed ? CommonResources.UnsubscribeFromNews : CommonResources.SubscribeToNews);
            if ((this.GroupVM.CanPost || this.GroupVM.CanSuggestAPost) && !this._defaultAppBar.Buttons.Contains((object)this._appBarButtonAddNews))
                this._defaultAppBar.Buttons.Add((object)this._appBarButtonAddNews);
            if (!this.GroupVM.CanPost && !this.GroupVM.CanSuggestAPost)
                this._defaultAppBar.Buttons.Remove((object)this._appBarButtonAddNews);
            if (this.GroupVM.CanPost)
                this._appBarButtonAddNews.Text = (CommonResources.MainPage_News_AddNews);
            else if (this.GroupVM.CanSuggestAPost)
                this._appBarButtonAddNews.Text = (CommonResources.SuggestedNews_SuggestAPost);
            if (this._defaultAppBar.MenuItems.Count > 0 || this._defaultAppBar.Buttons.Count > 0)
            {
                base.ApplicationBar = ((IApplicationBar)this._defaultAppBar);
                base.ApplicationBar.Mode = (this._defaultAppBar.Buttons.Count == 0 ? (ApplicationBarMode)1 : (ApplicationBarMode)0);
            }
            else
                base.ApplicationBar = ((IApplicationBar)null);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                this._gid = long.Parse(base.NavigationContext.QueryString["GroupId"]);
                string name = base.NavigationContext.QueryString["Name"];
                if (base.NavigationContext.QueryString.ContainsKey("ForbidOverrideGoBack"))
                    bool.TryParse(base.NavigationContext.QueryString["ForbidOverrideGoBack"], out this._forbidOverrideGoBack);
                GroupViewModel groupViewModel = new GroupViewModel(this._gid, name);
                groupViewModel.PropertyChanged += new PropertyChangedEventHandler(this.gvm_PropertyChanged);
                base.DataContext = ((object)groupViewModel);
                groupViewModel.LoadGroupData(false, false);
                this._isInitialized = true;
            }
            this.ProcessInputParameters();
            CurrentMediaSource.AudioSource = StatisticsActionSource.wall_group;
            CurrentMediaSource.VideoSource = StatisticsActionSource.wall_group;
            CurrentMediaSource.GifPlaySource = StatisticsActionSource.wall_group;
            CurrentNewsFeedSource.Source = ViewPostSource.GroupWall;
        }

        private void ProcessInputParameters()
        {
            Group parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (parameterForIdAndReset == null)
                return;
            foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>)this.GroupVM.WallVM.Collection)
            {
                WallPostItem wallPostItem = virtualizable as WallPostItem;
                if (wallPostItem != null && wallPostItem.LikesAndCommentsItem != null && wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(parameterForIdAndReset.id, parameterForIdAndReset.name))
                    break;
            }
        }

        private void gvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "LeaveButtonVisibility") && !(e.PropertyName == "LeaveButtonText") && (!(e.PropertyName == "CanPost") && !(e.PropertyName == "IsFavorite")) && !(e.PropertyName == "IsSubscribed"))
                return;
            this.UpdateAppBar();
        }

        private void ActionButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.GroupVM.HandleActionButton(((sender as FrameworkElement).DataContext as ActionButton).ButtonType);
        }

        private void Button_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.GroupVM.HandleNavigateButton(((sender as FrameworkElement).DataContext as NavigateButton).ButtonType);
        }

        private void ButtonInformation_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ((sender as FrameworkElement).DataContext as InformationRow).Navigate();
        }

        private void AllPosts_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.GroupVM.ShowAllPosts = true;
        }

        private void GroupPosts_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.GroupVM.ShowAllPosts = false;
        }

        private void Avatar_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = (true);
            if (!this.GroupVM.HaveAvatar)
                return;
            Navigator.Current.NavigateToImageViewer("-6", 5, this._gid, true, -1, 0, new List<Photo>(), (Func<int, Image>)(ind => (Image)null));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Groups;component/GroupPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.Header = (GenericHeaderUC)base.FindName("Header");
            this.pivot = (Pivot)base.FindName("pivot");
            this.pivotItemMain = (PivotItem)base.FindName("pivotItemMain");
            this.scrollViewer = (ViewportControl)base.FindName("scrollViewer");
            this.stackPanelMain = (MyVirtualizingStackPanel)base.FindName("stackPanelMain");
            this.wallPanel = (MyVirtualizingPanel2)base.FindName("wallPanel");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
            //
            this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
            this.rectangleGeometry2 = (RectangleGeometry)base.FindName("rectangleGeometry2");
        }
    }
}
