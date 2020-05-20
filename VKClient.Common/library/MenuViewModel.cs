using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
    public sealed class MenuViewModel : ViewModelBase, IHandle<BaseDataChangedEvent>, IHandle, IHandle<CountersChanged>
    {
        private static MenuViewModel _instance;

        public VKClient.Common.UC.BirthdaysViewModel BirthdaysVM { get; private set; }

        public MenuItemViewModel NewsItem { get; private set; }

        public MenuItemViewModel NotificationsItem { get; private set; }

        public MenuItemViewModel MessagesItem { get; private set; }

        public MenuItemViewModel FriendsItem { get; private set; }

        public MenuItemViewModel CommunitiesItem { get; private set; }

        public MenuItemViewModel PhotosItem { get; private set; }

        public MenuItemViewModel VideosItem { get; private set; }

        public MenuItemViewModel AudiosItem { get; private set; }

        public MenuItemViewModel GamesItem { get; private set; }

        public MenuItemViewModel BookmarksItem { get; private set; }

        public MenuItemViewModel SettingsItem { get; private set; }

        public static MenuViewModel Instance
        {
            get
            {
                if (MenuViewModel._instance == null)
                    MenuViewModel._instance = new MenuViewModel();
                return MenuViewModel._instance;
            }
            set
            {
                MenuViewModel._instance = value;
            }
        }

        public string UserPhoto
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max;
            }
        }

        public string UserName
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.LoggedInUser.Name;
            }
        }

        public int TotalCount
        {
            get
            {
                return this.NotificationsItem.Count + this.MessagesItem.Count + this.FriendsItem.Count + this.CommunitiesItem.Count + this.GamesItem.Count;
            }
        }

        public string TotalCountString
        {
            get
            {
                return MenuItemViewModel.FormatForUI(this.TotalCount);
            }
        }

        public bool HaveAnyNotifications
        {
            get
            {
                return this.TotalCount > 0;
            }
        }

        public Visibility HaveAnyNotificationsVisibility
        {
            get
            {
                if (!this.HaveAnyNotifications)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public MenuViewModel()
        {
            this.BirthdaysVM = new VKClient.Common.UC.BirthdaysViewModel();
            this.NewsItem = new MenuItemViewModel(MenuSectionName.News);
            this.NotificationsItem = new MenuItemViewModel(MenuSectionName.Notifications);
            this.MessagesItem = new MenuItemViewModel(MenuSectionName.Messages);
            this.FriendsItem = new MenuItemViewModel(MenuSectionName.Friends);
            this.CommunitiesItem = new MenuItemViewModel(MenuSectionName.Communities);
            this.PhotosItem = new MenuItemViewModel(MenuSectionName.Photos);
            this.VideosItem = new MenuItemViewModel(MenuSectionName.Videos);
            this.AudiosItem = new MenuItemViewModel(MenuSectionName.Audios);
            this.GamesItem = new MenuItemViewModel(MenuSectionName.Games);
            this.BookmarksItem = new MenuItemViewModel(MenuSectionName.Bookmarks);
            this.SettingsItem = new MenuItemViewModel(MenuSectionName.Settings);
            //
            EventAggregator.Current.Subscribe(this);
        }

        public void Handle(BaseDataChangedEvent message)
        {
            this.UpdateUserInformation();
            this.UpdateCounters();
            this.BirthdaysVM.UpdateData();
        }

        public void Handle(CountersChanged message)
        {
            this.UpdateCounters();
        }

        private void UpdateUserInformation()
        {
            base.NotifyPropertyChanged<string>(() => this.UserPhoto);
            base.NotifyPropertyChanged<string>(() => this.UserName);
        }

        private void UpdateCounters()
        {
            this.NotificationsItem.UpdateCount();
            this.MessagesItem.UpdateCount();
            this.FriendsItem.UpdateCount();
            this.CommunitiesItem.UpdateCount();
            this.GamesItem.UpdateCount();
            base.NotifyPropertyChanged<int>(() => this.TotalCount);
            base.NotifyPropertyChanged<string>(() => this.TotalCountString);
            base.NotifyPropertyChanged<bool>(() => this.HaveAnyNotifications);
            base.NotifyPropertyChanged<Visibility>(() => this.HaveAnyNotificationsVisibility);
        }

        public void UpdateSelectedItem(MenuSectionName selectedSection)
        {
            this.NewsItem.UpdateSelectionState(selectedSection);
            this.NotificationsItem.UpdateSelectionState(selectedSection);
            this.MessagesItem.UpdateSelectionState(selectedSection);
            this.FriendsItem.UpdateSelectionState(selectedSection);
            this.CommunitiesItem.UpdateSelectionState(selectedSection);
            this.PhotosItem.UpdateSelectionState(selectedSection);
            this.VideosItem.UpdateSelectionState(selectedSection);
            this.AudiosItem.UpdateSelectionState(selectedSection);
            this.GamesItem.UpdateSelectionState(selectedSection);
            this.BookmarksItem.UpdateSelectionState(selectedSection);
            this.SettingsItem.UpdateSelectionState(selectedSection);
        }
    }
}
