using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;

namespace VKClient.Common.UC
{
    public class MenuUC : UserControl
    {
        private DateTime _lastMenuNavigationDateTime;
        private bool _isNavigating;
        internal ScrollViewer scrollViewer;
        internal MiniPlayerUC miniPlayerUC;
        private bool _contentLoaded;
        //
        internal RectangleGeometry rectangleGeometry;

        public static MenuUC Instance { get; private set; }

        public PageBase ParentPage
        {
            get
            {
                return (PageBase)((ContentControl)Application.Current.RootVisual).Content;
            }
        }

        public bool IsOnNewsPage
        {
            get
            {
                return this.ParentPage is NewsPage;
            }
        }

        public bool IsOnNotificationsPage
        {
            get
            {
                return this.ParentPage is FeedbackPage;
            }
        }

        public bool IsOnMessagesPage
        {
            get
            {
                return this.CheckOnPage("ConversationsPage");
            }
        }

        public bool IsOnFriendsPage
        {
            get
            {
                return this.CheckOnPage("FriendsPage");
            }
        }

        public bool IsOnFriendRequestsPage
        {
            get
            {
                return this.ParentPage is FriendRequestsPage;
            }
        }

        public bool IsOnCommunitiesPage
        {
            get
            {
                return this.CheckOnPage("GroupsListPage");
            }
        }

        public bool IsOnGroupInvitationsPage
        {
            get
            {
                return this.CheckOnPage("GroupInvitationsPage");
            }
        }

        public bool IsOnPhotosPage
        {
            get
            {
                return this.CheckOnPage("PhotosMainPage");
            }
        }

        public bool IsOnVideosPage
        {
            get
            {
                return this.CheckOnPage("VideoCatalogPage");
            }
        }

        public bool IsOnAudiosPage
        {
            get
            {
                return this.CheckOnPage("AudioPage");
            }
        }

        public bool IsOnGamesPage
        {
            get
            {
                return this.ParentPage is GamesMainPage;
            }
        }

        public bool IsOnFavoritesPage
        {
            get
            {
                return this.ParentPage is FavoritesPage;
            }
        }

        public bool IsOnSettingsPage
        {
            get
            {
                return this.CheckOnPage("SettingsPage");
            }
        }

        public bool IsOnLoggedInUserPage
        {
            get
            {
                return this.CheckOnProfilePage(AppGlobalStateManager.Current.LoggedInUserId);
            }
        }

        public bool IsOnBirthdaysPage
        {
            get
            {
                return this.ParentPage is BirthdaysPage;
            }
        }

        public bool IsOnAudioPlayerPage
        {
            get
            {
                return this.CheckOnPage("AudioPlayer");
            }
        }

        public MenuUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            MenuUC.Instance = this;
            base.CacheMode = ((CacheMode)new BitmapCache());
            base.Visibility = Visibility.Collapsed;
            base.DataContext = MenuViewModel.Instance;
            this.UpdateState();
            ((UIElement)this.miniPlayerUC.trackPanel).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((o, e) => this.NavigateToAudioPlayerPage()));
            //
            this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
        }

        private bool CheckOnPage(string pageName)
        {
            PageBase parentPage = this.ParentPage;
            return parentPage != null && (parentPage.CommonParameters.UserOrGroupId == 0L || parentPage.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId) && (!parentPage.CommonParameters.IsGroup && (parentPage.CommonParameters.UserId == 0L || parentPage.CommonParameters.UserId == AppGlobalStateManager.Current.LoggedInUserId)) && (this.ParentPage).GetType().Name.Contains(pageName);
        }

        private bool CheckOnProfilePage(long userId)
        {
            PageBase parentPage = this.ParentPage;
            return parentPage != null && (parentPage.CommonParameters.UserOrGroupId == userId || userId == AppGlobalStateManager.Current.LoggedInUserId && parentPage.CommonParameters.UserOrGroupId == 0L) && ((this.ParentPage).GetType().Name.Contains("ProfilePage") && !(this.ParentPage).GetType().Name.Contains("EditProfile"));
        }

        private bool CheckOnGiftsCatalogPage(long userId)
        {
            PageBase parentPage = this.ParentPage;
            return (parentPage != null ? (parentPage.CommonParameters.UserId == userId ? 1 : 0) : 0) != 0 && (this.ParentPage).GetType().Name.Contains("GiftsCatalogPage");
        }

        private static void PublishMenuItemClickedEvent(string itemName)
        {
            EventAggregator.Current.Publish(new MenuClickEvent()
            {
                item = itemName
            });
        }

        public void UpdateState()
        {
            MenuSectionName selectedSection = MenuSectionName.Unknown;
            if (this.IsOnNewsPage)
                selectedSection = MenuSectionName.News;
            else if (this.IsOnMessagesPage)
                selectedSection = MenuSectionName.Messages;
            else if (this.IsOnNotificationsPage)
                selectedSection = MenuSectionName.Notifications;
            else if (this.IsOnFriendsPage)
                selectedSection = MenuSectionName.Friends;
            else if (this.IsOnCommunitiesPage)
                selectedSection = MenuSectionName.Communities;
            else if (this.IsOnPhotosPage)
                selectedSection = MenuSectionName.Photos;
            else if (this.IsOnVideosPage)
                selectedSection = MenuSectionName.Videos;
            else if (this.IsOnAudiosPage)
                selectedSection = MenuSectionName.Audios;
            else if (this.IsOnGamesPage)
                selectedSection = MenuSectionName.Games;
            else if (this.IsOnFavoritesPage)
                selectedSection = MenuSectionName.Bookmarks;
            else if (this.IsOnSettingsPage)
                selectedSection = MenuSectionName.Settings;
            ((MenuViewModel)base.DataContext).UpdateSelectedItem(selectedSection);
        }

        private void NavigateOnMenuClick(Action navigateAction, bool needClearStack = true)
        {
            if ((DateTime.Now - this._lastMenuNavigationDateTime).TotalMilliseconds < 700.0 || this._isNavigating)
                return;
            this._isNavigating = true;
            this._lastMenuNavigationDateTime = DateTime.Now;
            PageBase parentPage = this.ParentPage;
            parentPage.PrepareForMenuNavigation((Action)(() =>
            {
                this._isNavigating = false;
                if (needClearStack)
                    WallPostVMCacheManager.ResetInstance();
                navigateAction();
                if (needClearStack)
                    return;
                Execute.ExecuteOnUIThread((Action)(async () =>
                {
                    await Task.Delay(1);
                    this.HandleSamePageNavigation(parentPage, true);
                }));
            }), needClearStack);
        }

        public void NavigateToUserProfile(long userId, string userName, bool isHoldingEvent)
        {
            if (this.CheckOnProfilePage(userId))
                this.HandleSamePageNavigation(null, false);
            else
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToUserProfile(userId, userName, "", false)), !isHoldingEvent);
        }

        public void NavigateToAudioPlayerPage()
        {
            if (this.IsOnAudioPlayerPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                PageBase parentPage = this.ParentPage;
                if (parentPage == null)
                    return;
                if (parentPage.IsMenuOpen)
                    parentPage.OpenCloseMenu(false, (Action)(() => Navigator.Current.NavigateToAudioPlayer(false)), false);
                else
                    Navigator.Current.NavigateToAudioPlayer(false);
            }
        }

        private void Profile_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnLoggedInUserPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("self");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToUserProfile(AppGlobalStateManager.Current.LoggedInUserId, AppGlobalStateManager.Current.GlobalState.LoggedInUser.Name, "", false)), !flag);
            }
        }

        private void Profile_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Profile_OnClicked(sender, null);
        }

        private void News_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            bool flag1 = e == null;
            bool flag2 = this.IsOnNewsPage;
            if (!flag2 & flag1)
            {
                using (IEnumerator<JournalEntry> enumerator = ((PhoneApplicationFrame)Application.Current.RootVisual).BackStack.GetEnumerator())
                {
                    while (((IEnumerator)enumerator).MoveNext())
                    {
                        if (enumerator.Current.Source.OriginalString.Contains("NewsPage.xaml"))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
            }
            if (flag2)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                MenuUC.PublishMenuItemClickedEvent("news");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToNewsFeed(0, false)), !flag1);
            }
        }

        private void News_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.News_OnClicked(sender, null);
        }

        private void Notifications_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnNotificationsPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("feedback");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToFeedback()), !flag);
            }
        }

        private void Notifications_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Notifications_OnClicked(sender, null);
        }

        private void Messages_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            bool flag1 = e == null;
            bool flag2 = this.IsOnMessagesPage;
            if (!flag2 & flag1)
            {
                using (IEnumerator<JournalEntry> enumerator = ((PhoneApplicationFrame)Application.Current.RootVisual).BackStack.GetEnumerator())
                {
                    while (((IEnumerator)enumerator).MoveNext())
                    {
                        if (enumerator.Current.Source.OriginalString.Contains("ConversationsPage.xaml"))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
            }
            if (flag2)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                MenuUC.PublishMenuItemClickedEvent("messages");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToConversations()), !flag1);
            }
        }

        private void Messages_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Messages_OnClicked(sender, null);
        }

        private void Friends_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnFriendsPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("friends");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToFriends(AppGlobalStateManager.Current.LoggedInUserId, "", false, FriendsPageMode.Default)), !flag);
            }
        }

        private void Friends_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Friends_OnClicked(sender, null);
        }

        private void FriendsRequests_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.FriendsRequests_OnClicked(sender, null);
        }

        private void FriendsRequests_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnFriendRequestsPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("friends_requests");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToFriendRequests(false)), !flag);
            }
        }

        private void Communities_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnCommunitiesPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("groups");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", false, 0, 0, "", false, "", 0L)), !flag);
            }
        }

        private void Communities_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Communities_OnClicked(sender, null);
        }

        private void GroupRequests_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnGroupInvitationsPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("group_requests");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToGroupInvitations()), !flag);
            }
        }

        private void GroupRequests_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.GroupRequests_Tap(sender, null);
        }

        private void Photos_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnPhotosPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("photos");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToPhotoAlbums(false, 0, false, 0)), !flag);
            }
        }

        private void Photos_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Photos_OnClicked(sender, null);
        }

        private void Videos_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnVideosPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("videos");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToVideoCatalog()), !flag);
            }
        }

        private void Videos_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Videos_OnClicked(sender, null);
        }

        private void Audios_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnAudiosPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("audios");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToAudio(0, 0, false, 0, 0, "")), !flag);
            }
        }

        private void Audios_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Audios_OnClicked(sender, null);
        }

        private void Games_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnGamesPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("games");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToGames(0, false)), !flag);
            }
        }

        private void Games_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Games_OnClicked(sender, null);
        }

        private void GamesRequests_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnGamesPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("games_requests");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToGames(0, false)), !flag);
            }
        }

        private void GamesRequests_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.GamesRequests_Tap(sender, null);
        }

        private void Bookmarks_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnFavoritesPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("favorites");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToFavorites()), !flag);
            }
        }

        private void Bookmarks_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Bookmarks_OnClicked(sender, null);
        }

        private void Settings_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.IsOnSettingsPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                bool flag = e == null;
                MenuUC.PublishMenuItemClickedEvent("settings");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToSettings()), !flag);
            }
        }

        private void Settings_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Settings_OnClicked(sender, null);
        }

        private void HandleSamePageNavigation(PageBase parentPage = null, bool withoutAnimation = false)
        {
            if (parentPage == null)
                parentPage = this.ParentPage;
            if (parentPage == null)
                return;
            parentPage.OpenCloseMenu(false, null, withoutAnimation);
        }

        internal void NavigateToBirthdays(bool isHoldingEvent)
        {
            if (this.IsOnBirthdaysPage)
            {
                this.HandleSamePageNavigation(null, false);
            }
            else
            {
                MenuUC.PublishMenuItemClickedEvent("birthdays");
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToBirthdaysPage()), !isHoldingEvent);
            }
        }

        public void NavigateToGiftsCatalog(long userId, bool isHoldingEvent)
        {
            if (this.CheckOnGiftsCatalogPage(userId))
                this.HandleSamePageNavigation(null, false);
            else
                this.NavigateOnMenuClick((Action)(() => Navigator.Current.NavigateToGiftsCatalog(userId, false)), !isHoldingEvent);
        }

        private void Search_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SearchHintsUC.ShowPopup();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MenuUC.xaml", UriKind.Relative));
            this.scrollViewer = (ScrollViewer)base.FindName("scrollViewer");
            this.miniPlayerUC = (MiniPlayerUC)base.FindName("miniPlayerUC");
            //
            this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
        }
    }
}
