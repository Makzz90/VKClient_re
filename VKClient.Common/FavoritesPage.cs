using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.UC;
using VKClient.Photos.Library;
using VKClient.Video.Library;

namespace VKClient.Common
{
    public class FavoritesPage : PageBase
    {
        private bool _isInitialized;
        private bool _photosLoaded;
        private bool _videosLoaded;
        private bool _postsLoaded;
        private bool _usersLoaded;
        private bool _linksLoaded;
        private bool _productsLoaded;
        private int _initialOffset;
        internal Grid LayoutRoot;
        internal GenericHeaderUC ucHeader;
        internal Pivot pivot;
        internal PivotItem pivotItemPhotos;
        internal ExtendedLongListSelector photosListBox;
        internal PivotItem pivotItemVideos;
        internal ExtendedLongListSelector videosListBox;
        internal PivotItem pivotItemPosts;
        internal ViewportControl scrollPosts;
        internal MyVirtualizingStackPanel stackPanelPosts;
        internal MyVirtualizingPanel2 panelPosts;
        internal PivotItem pivotItemPersons;
        internal ExtendedLongListSelector usersListBox;
        internal PivotItem pivotItemLinks;
        internal ExtendedLongListSelector linksListBox;
        internal PivotItem pivotItemProducts;
        internal ExtendedLongListSelector productsListBox;
        internal PullToRefreshUC ucPullToRefresh;
        private bool _contentLoaded;

        private FavoritesViewModel FavVM
        {
            get
            {
                return base.DataContext as FavoritesViewModel;
            }
        }

        public FavoritesPage()
        {
            this.InitializeComponent();
            this.ucHeader.OnHeaderTap = new Action(this.HandleHeaderTap);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.photosListBox);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.videosListBox);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.panelPosts);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.linksListBox);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.usersListBox);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.productsListBox);
            this.photosListBox.OnRefresh = (Action)(() => this.FavVM.PhotosVM.LoadData(true, false, null, false));
            this.videosListBox.OnRefresh = (Action)(() => this.FavVM.VideosVM.LoadData(true, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)null, false));
            this.panelPosts.OnRefresh = (Action)(() => this.FavVM.PostsVM.LoadData(true, false, null, false));
            this.linksListBox.OnRefresh = (Action)(() => this.FavVM.LinksVM.LoadData(true, false, null, false));
            this.usersListBox.OnRefresh = (Action)(() => this.FavVM.UsersVM.LoadData(true, false, null, false));
            this.productsListBox.OnRefresh = (Action)(() => this.FavVM.ReloadProducts(false));
            this.scrollPosts.BindViewportBoundsTo((FrameworkElement)this.stackPanelPosts);
            this.panelPosts.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.scrollPosts), false);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.panelPosts);
            this.panelPosts.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.panelPosts_ScrollPositionChanged);
        }

        private void HandleHeaderTap()
        {
            if (this.pivot.SelectedItem == this.pivotItemPhotos)
                this.photosListBox.ScrollToTop();
            else if (this.pivot.SelectedItem == this.pivotItemVideos)
                this.videosListBox.ScrollToTop();
            else if (this.pivot.SelectedItem == this.pivotItemPosts)
                this.panelPosts.ScrollToBottom(false);
            else if (this.pivot.SelectedItem == this.pivotItemLinks)
                this.linksListBox.ScrollToTop();
            else if (this.pivot.SelectedItem == this.pivotItemProducts)
            {
                this.productsListBox.ScrollToTop();
            }
            else
            {
                if (this.pivot.SelectedItem != this.pivotItemPersons)
                    return;
                this.usersListBox.ScrollToTop();
            }
        }

        private void panelPosts_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
        {
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                base.DataContext = (new FavoritesViewModel());
                this._isInitialized = true;
            }
            CurrentMarketItemSource.Source = MarketItemSource.fave;
            if (e.NavigationMode == NavigationMode.New)
                this.pivot.SelectedIndex = AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection;
            this.ProcessInputParameters();
        }

        private void pivot_LoadedPivotItem_1(object sender, PivotItemEventArgs e)
        {
            if (e.Item == this.pivotItemPhotos && !this._photosLoaded)
            {
                this.FavVM.PhotosVM.LoadData(false, false, null, false);
                this._photosLoaded = true;
                AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 0;
            }
            else if (e.Item == this.pivotItemVideos && !this._videosLoaded)
            {
                this.FavVM.VideosVM.LoadData(false, false, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)null, false);
                this._videosLoaded = true;
                AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 1;
            }
            else if (e.Item == this.pivotItemPosts && !this._postsLoaded)
            {
                this.FavVM.PostsVM.LoadData(false, false, null, false);
                this._postsLoaded = true;
                AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 2;
            }
            else if (e.Item == this.pivotItemPersons && !this._usersLoaded)
            {
                this.FavVM.UsersVM.LoadData(false, false, null, false);
                this._usersLoaded = true;
                AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 3;
            }
            else if (e.Item == this.pivotItemLinks && !this._linksLoaded)
            {
                this.FavVM.LinksVM.LoadData(false, false, null, false);
                this._linksLoaded = true;
                AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 4;
            }
            else
            {
                if (e.Item != this.pivotItemProducts || this._productsLoaded)
                    return;
                this.FavVM.ProductsVM.LoadData(false, false, null, false);
                this._productsLoaded = true;
                AppGlobalStateManager.Current.GlobalState.FavoritesDefaultSection = 5;
            }
        }

        private void photos_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.FavVM.PhotosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            AlbumPhotoHeaderFourInARow dataContext = frameworkElement.DataContext as AlbumPhotoHeaderFourInARow;
            Photo photo = null;
            if (dataContext != null)
            {
                string str = frameworkElement.Tag.ToString();
                if (!(str == "1"))
                {
                    if (!(str == "2"))
                    {
                        if (!(str == "3"))
                        {
                            if (str == "4")
                                photo = dataContext.Photo4.Photo;
                        }
                        else
                            photo = dataContext.Photo3.Photo;
                    }
                    else
                        photo = dataContext.Photo2.Photo;
                }
                else
                    photo = dataContext.Photo1.Photo;
            }
            if (photo == null)
                return;
            List<Photo> photoList = new List<Photo>();
            foreach (AlbumPhotoHeaderFourInARow headerFourInArow in (Collection<AlbumPhotoHeaderFourInARow>)this.FavVM.PhotosVM.Collection)
                photoList.AddRange(headerFourInArow.GetAsPhotos());
            int num = photoList.IndexOf(photo);
            List<Photo> photos = new List<Photo>();
            int initialOffset = Math.Max(0, num - 20);
            for (int index = initialOffset; index < Math.Min(photoList.Count, num + 30); ++index)
                photos.Add(photoList[index]);
            this._initialOffset = initialOffset;
            Navigator.Current.NavigateToImageViewer(this.FavVM.PhotosVM.TotalCount, initialOffset, photos.IndexOf(photo), (List<long>)Enumerable.ToList<long>(Enumerable.Select<Photo, long>(photos, (Func<Photo, long>)(p => p.pid))), (List<long>)Enumerable.ToList<long>(Enumerable.Select<Photo, long>(photos, (Func<Photo, long>)(p => p.owner_id))), (List<string>)Enumerable.ToList<string>(Enumerable.Select<Photo, string>(photos, (Func<Photo, string>)(p => p.access_key))), photos, "PhotosByIdsForFavorites", false, false, new Func<int, Image>(this.GetPhotoById), null, false);
        }

        private Image GetPhotoById(int arg)
        {
            arg += this._initialOffset;
            int num1 = arg / 4;
            int num2 = arg % 4;
            return null;
        }

        private FrameworkElement SearchVisualTree(DependencyObject targetElement, DependencyObject comp)
        {
            FrameworkElement frameworkElement = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(targetElement);
            if (childrenCount == 0)
                return frameworkElement;
            for (int index = 0; index < childrenCount; ++index)
            {
                DependencyObject child = VisualTreeHelper.GetChild(targetElement, index);
                if ((child as FrameworkElement).DataContext == (comp as FrameworkElement).DataContext)
                    return child as FrameworkElement;
                frameworkElement = this.SearchVisualTree(child, comp);
                if (frameworkElement != null)
                    return frameworkElement;
            }
            return frameworkElement;
        }

        private void Videos_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.FavVM.VideosVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void Video_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoHeader selectedItem = this.videosListBox.SelectedItem as VideoHeader;
            if (selectedItem == null)
                return;
            this.videosListBox.SelectedItem = null;
            Navigator.Current.NavigateToVideoWithComments(selectedItem.VKVideo, selectedItem.VKVideo.owner_id, selectedItem.VKVideo.vid, selectedItem.VKVideo.access_key ?? "");
        }

        private void Users_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.FavVM.UsersVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FriendHeader selectedItem = this.usersListBox.SelectedItem as FriendHeader;
            if (selectedItem == null)
                return;
            this.usersListBox.SelectedItem = null;
            Navigator.Current.NavigateToUserProfile(selectedItem.UserId, selectedItem.FullName, "", false);
        }

        private void Links_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Link selectedItem = this.linksListBox.SelectedItem as Link;
            if (selectedItem == null)
                return;
            this.linksListBox.SelectedItem = null;
            if (string.IsNullOrWhiteSpace(selectedItem.url))
                return;
            Navigator.Current.NavigateToWebUri(selectedItem.url.Trim((char[])new char[1]
      {
        '/'
      }), false, false);
        }

        private void Links_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.FavVM.LinksVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void Products_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            this.FavVM.ProductsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void ProcessInputParameters()
        {
            Group parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (parameterForIdAndReset == null)
                return;
            FavoritesViewModel favVm = this.FavVM;
            ObservableCollection<IVirtualizable> observableCollection;
            if (favVm == null)
            {
                observableCollection = null;
            }
            else
            {
                GenericCollectionViewModel<WallData, IVirtualizable> postsVm = favVm.PostsVM;
                observableCollection = postsVm != null ? postsVm.Collection : null;
            }
            if (observableCollection == null)
                return;
            foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>)this.FavVM.PostsVM.Collection)
            {
                WallPostItem wallPostItem = virtualizable as WallPostItem;
                if (wallPostItem == null && virtualizable is NewsFeedAdsItem)
                    wallPostItem = (virtualizable as NewsFeedAdsItem).WallPostItem;
                if ((wallPostItem != null ? wallPostItem.LikesAndCommentsItem : null) != null && wallPostItem.LikesAndCommentsItem.ShareInGroupIfApplicable(parameterForIdAndReset.id, parameterForIdAndReset.name))
                    break;
                VideosNewsItem videosNewsItem = virtualizable as VideosNewsItem;
                if (videosNewsItem != null)
                    videosNewsItem.LikesAndCommentsItem.ShareInGroupIfApplicable(parameterForIdAndReset.id, parameterForIdAndReset.name);
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/FavoritesPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.pivot = (Pivot)base.FindName("pivot");
            this.pivotItemPhotos = (PivotItem)base.FindName("pivotItemPhotos");
            this.photosListBox = (ExtendedLongListSelector)base.FindName("photosListBox");
            this.pivotItemVideos = (PivotItem)base.FindName("pivotItemVideos");
            this.videosListBox = (ExtendedLongListSelector)base.FindName("videosListBox");
            this.pivotItemPosts = (PivotItem)base.FindName("pivotItemPosts");
            this.scrollPosts = (ViewportControl)base.FindName("scrollPosts");
            this.stackPanelPosts = (MyVirtualizingStackPanel)base.FindName("stackPanelPosts");
            this.panelPosts = (MyVirtualizingPanel2)base.FindName("panelPosts");
            this.pivotItemPersons = (PivotItem)base.FindName("pivotItemPersons");
            this.usersListBox = (ExtendedLongListSelector)base.FindName("usersListBox");
            this.pivotItemLinks = (PivotItem)base.FindName("pivotItemLinks");
            this.linksListBox = (ExtendedLongListSelector)base.FindName("linksListBox");
            this.pivotItemProducts = (PivotItem)base.FindName("pivotItemProducts");
            this.productsListBox = (ExtendedLongListSelector)base.FindName("productsListBox");
            this.ucPullToRefresh = (PullToRefreshUC)base.FindName("ucPullToRefresh");
        }
    }
}
