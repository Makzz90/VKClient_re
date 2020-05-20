using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class NewsViewModel : ViewModelBase, ICollectionDataProvider<NewsFeedData, IVirtualizable>, IHandle<WallPostAddedOrEdited>, IHandle, IHandle<AdReportedEvent>, IHandle<NewsfeedTopEnabledDisabledEvent>
    {
        private string _from = "";
        private DateTime _lastNewsFeedUpdateDateTime = DateTime.MinValue;
        private bool _keepScrollPosition = true;
        private DateTime _lastNotificationsSyncDateTime = DateTime.MinValue;
        private DateTime _lastFreshNewsCheckDateTime = DateTime.MinValue;
        private static NewsViewModel _instance;
        private const int NEWSFEED_LOAD_COUNT = 20;
        private NewsFeedType? _newsFeedType;
        private PickableItem _newsSource;
        private NewsFeedSectionsAndLists _sectionsAndLists;
        private FreshNewsState _freshNewsState;
        private bool _isLoadingFreshNews;
        private FreshNewsData _freshNewsData;
        private bool _setFeedTypePending;

        public DateTime NavigatedFromNewsfeedTime = DateTime.Now;

        public static NewsViewModel Instance
        {
            get
            {
                if (NewsViewModel._instance == null)
                    NewsViewModel._instance = new NewsViewModel();
                return NewsViewModel._instance;
            }
        }

        public GenericCollectionViewModel<NewsFeedData, IVirtualizable> NewsFeedVM { get; private set; }

        public string Title
        {
            get
            {
                PickableItem newsSource = this.NewsSource;
                return (newsSource != null ? newsSource.Name.ToUpperInvariant() : null) ?? NewsSources.NewsFeed.Title.ToUpperInvariant();
            }
        }

        public PickableItem NewsSource
        {
            get
            {
                return this._newsSource;
            }
            set
            {
                int num = this._newsSource == null ? 1 : 0;
                this._newsSource = value;
                AppGlobalStateManager.Current.GlobalState.SelectedNewsSource = this._newsSource;
                if (num == 0 && AppGlobalStateManager.Current.LoggedInUserId != 0L)
                    this.ReloadNews(true, true, false);
                if (this._newsSource.ID == -10L)
                {
                    this.NewsFeedVM.NoContentText = CommonResources.NoContent_News;
                    this.NewsFeedVM.NoContentImage = "../Resources/NoContentImages/News.png";
                }
                else
                {
                    this.NewsFeedVM.NoContentText = "";
                    this.NewsFeedVM.NoContentImage = null;
                }
                base.NotifyPropertyChanged<PickableItem>(() => this.NewsSource);
                base.NotifyPropertyChanged<string>(() => this.Title);
                this.UpdateCurrentNewsFeedSource();
            }
        }

        private bool IsNewsFeedExpired
        {
            get
            {
                int autoReloadInterval = this.GetNewsfeedAutoReloadInterval();
                if (autoReloadInterval <= 0)
                    return false;
                return (DateTime.Now - this._lastNewsFeedUpdateDateTime).TotalSeconds >= (double)autoReloadInterval;
            }
        }

        public bool ForceNewsFeedUpdate
        {
            get
            {
                return this.IsNewsFeedExpired;
            }
        }

        public bool? TopFeedPromoAnswer { get; set; }

        public long TopFeedPromoId { get; set; }

        public FreshNewsState FreshNewsState
        {
            get
            {
                return this._freshNewsState;
            }
            set
            {
                this._freshNewsState = value;
                Action<FreshNewsState> stateChangedCallback = this.FreshNewsStateChangedCallback;
                if (stateChangedCallback == null)
                    return;
                stateChangedCallback(this._freshNewsState);
            }
        }

        public NewsFeedConsts NewsFeedConsts { get; private set; }

        public Action<FreshNewsState> FreshNewsStateChangedCallback { get; set; }

        public Action KeepScrollPositionChanged { get; set; }

        public Action<UserNotification> ShowNewsfeedTopPromoAction { get; set; }

        public bool KeepScrollPosition
        {
            get
            {
                return this._keepScrollPosition;
            }
            private set
            {
                this._keepScrollPosition = value;
                Action scrollPositionChanged = this.KeepScrollPositionChanged;
                if (scrollPositionChanged == null)
                    return;
                scrollPositionChanged();
            }
        }

        private bool SyncNotifications
        {
            get
            {
                int totalSeconds = (int)(DateTime.Now - this._lastNotificationsSyncDateTime).TotalSeconds;
                NewsFeedConsts newsFeedConsts = this.NewsFeedConsts;
                int? nullable = newsFeedConsts != null ? new int?(newsFeedConsts.notifications_sync_interval) : new int?();
                int valueOrDefault = nullable.GetValueOrDefault();
                if (totalSeconds <= valueOrDefault)
                    return false;
                return nullable.HasValue;
            }
        }

        public Func<NewsFeedData, ListWithCount<IVirtualizable>> ConverterFunc
        {
            get
            {
                return (Func<NewsFeedData, ListWithCount<IVirtualizable>>)(newsFeedData =>
                {
                    this._from = newsFeedData.next_from;
                    if (newsFeedData.FeedType.HasValue)
                    {
                        NewsFeedType? feedType = newsFeedData.FeedType;
                        bool num = feedType.GetValueOrDefault() == NewsFeedType.top ? (feedType.HasValue ? true : false) : false;
                        AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled = num;
                        this._newsFeedType = newsFeedData.FeedType;
                    }
                    if (newsFeedData.consts != null)
                        this.NewsFeedConsts = newsFeedData.consts;
                    if (newsFeedData.lists != null)
                        this._sectionsAndLists = newsFeedData.lists;
                    return this.ConvertToVirtualizableList(newsFeedData);
                });
            }
        }

        public bool AreFreshNewsUpToDate
        {
            get
            {
                double totalSeconds = (DateTime.Now - this._lastFreshNewsCheckDateTime).TotalSeconds;
                NewsFeedConsts newsFeedConsts = this.NewsFeedConsts;
                int? nullable1 = newsFeedConsts != null ? new int?(newsFeedConsts.fresh_news_expiration_timeout) : new int?();
                double? nullable2 = nullable1.HasValue ? new double?(nullable1.GetValueOrDefault()) : new double?();
                double valueOrDefault = nullable2.GetValueOrDefault();
                if (totalSeconds >= valueOrDefault)
                    return false;
                return nullable2.HasValue;
            }
        }

        public bool HasFreshNewsToInsert
        {
            get
            {
                if (this._freshNewsData != null)
                    return this._freshNewsData.Items.Count > 0;
                return false;
            }
        }

        public NewsViewModel()
        {
            this.NewsFeedVM = new GenericCollectionViewModel<NewsFeedData, IVirtualizable>((ICollectionDataProvider<NewsFeedData, IVirtualizable>)this)
            {
                NoContentText = CommonResources.NoContent_News,
                NoContentImage = "../Resources/NoContentImages/News.png",
                NoContentNewsButtonsVisibility = Visibility.Visible,
                LoadCount = 20,
                ReloadCount = 20
            };
            this.InitializeNewsSource();
            EventAggregator.Current.Subscribe(this);
        }

        public ObservableCollection<PickableNewsfeedSourceItemViewModel> GetSectionsAndLists()
        {
            return NewsSources.GetNewsSources(this._sectionsAndLists, this.NewsSource);
        }

        private int GetNewsfeedAutoReloadInterval()
        {
            if (this.NewsFeedConsts == null)
                return 0;
            if (!AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled)
                return this.NewsFeedConsts.newsfeed_auto_reload_interval;
            return this.NewsFeedConsts.newsfeed_top_auto_reload_interval;
        }

        public void ReloadNews(bool clearCollectionBeforeLoad = true, bool resetFreshNewsStateBeforeLoad = true, bool suppressLoadingMessage = false)
        {
            if (resetFreshNewsStateBeforeLoad)
                this.FreshNewsState = FreshNewsState.NoNews;
            this.NewsFeedVM.LoadData(true, suppressLoadingMessage, (Action<BackendResult<NewsFeedData, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!resetFreshNewsStateBeforeLoad)
                    this.FreshNewsState = FreshNewsState.NoNews;
                this._lastFreshNewsCheckDateTime = DateTime.Now;
                Action<UserNotification> newsfeedTopPromoAction = this.ShowNewsfeedTopPromoAction;
                if (newsfeedTopPromoAction == null)
                    return;
                UserNotification newsfeedNotification = NewsViewModel.TryGetBubbleNewsfeedNotification(result.ResultData);
                newsfeedTopPromoAction(newsfeedNotification);
            }))), clearCollectionBeforeLoad);
        }

        private static UserNotification TryGetBubbleNewsfeedNotification(NewsFeedData newsFeedData)
        {
            if (newsFeedData == null)
                return null;
            VKList<UserNotification> notifications = newsFeedData.notifications;
            if (notifications == null)
                return null;
            List<UserNotification> items = notifications.items;
            if (items == null)
                return null;
            return (UserNotification)Enumerable.FirstOrDefault<UserNotification>(items, (Func<UserNotification, bool>)(n =>
            {
                if (n.Type == UserNotificationType.bubble_newsfeed)
                    return n.bubble_newsfeed != null;
                return false;
            }));
        }

        private void InitializeNewsSource()
        {
            PickableNewsfeedSourceItemViewModel sourceItemViewModel = ((IEnumerable<PickableNewsfeedSourceItemViewModel>)NewsSources.GetPredefinedNewsSources()).FirstOrDefault<PickableNewsfeedSourceItemViewModel>();
            this.NewsSource = sourceItemViewModel != null ? sourceItemViewModel.PickableItem : null;
        }

        public void UpdateCurrentNewsFeedSource()
        {
            Array values = Enum.GetValues(typeof(NewsSourcesPredefined));
            long id = this.NewsSource.ID;
            CurrentNewsFeedSource.FeedSource = NewsSourcesPredefined.CustomList;
            IEnumerator enumerator = values.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    if (id == (long)((int)enumerator.Current) )
                        CurrentNewsFeedSource.FeedSource = (NewsSourcesPredefined)id;
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        public void GetData(GenericCollectionViewModel<NewsFeedData, IVirtualizable> caller, int offset, int count, Action<BackendResult<NewsFeedData, ResultCode>> callback)
        {
            if (offset > 0 && string.IsNullOrWhiteSpace(this._from))
            {
                callback(new BackendResult<NewsFeedData, ResultCode>(ResultCode.Succeeded, new NewsFeedData()));
            }
            else
            {
                string str = offset == 0 ? "" : this._from;
                NewsFeedGetParams newsFeedGetParams1 = new NewsFeedGetParams();
                newsFeedGetParams1.from = str;
                newsFeedGetParams1.count = count;
                long id = this.NewsSource.ID;
                newsFeedGetParams1.NewsListId = id;
                NewsFeedGetParams newsFeedGetParams2 = newsFeedGetParams1;
                if (!string.IsNullOrWhiteSpace(str) || this._setFeedTypePending)
                    newsFeedGetParams2.FeedType = (new NewsFeedType?(AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled ? NewsFeedType.top : NewsFeedType.recent));
                if (this._setFeedTypePending)
                {
                    this._setFeedTypePending = false;
                    newsFeedGetParams2.UpdateFeedType = true;
                }
                if (string.IsNullOrWhiteSpace(str))
                {
                    newsFeedGetParams2.SyncNotifications = true;
                    this._lastNotificationsSyncDateTime = DateTime.Now;
                    this._lastNewsFeedUpdateDateTime = DateTime.Now;
                    bool? nullable = this.TopFeedPromoAnswer;
                    if (nullable.HasValue)
                    {
                        newsFeedGetParams2.TopFeedPromoAnswer = this.TopFeedPromoAnswer;
                        newsFeedGetParams2.TopFeedPromoId = this.TopFeedPromoId;
                        nullable = new bool?();
                        this.TopFeedPromoAnswer = nullable;
                    }
                }
                NewsFeedService.Current.GetNewsFeed(newsFeedGetParams2, callback);
            }
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<NewsFeedData, IVirtualizable> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoNews;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneNewsFrm, CommonResources.TwoFourNewsFrm, CommonResources.FiveNewsFrm, true, null, false);
        }

        private ListWithCount<IVirtualizable> ConvertToVirtualizableList(NewsFeedData newsFeedData)
        {
            if (newsFeedData == null)
                return new ListWithCount<IVirtualizable>();
            ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>() { TotalCount = newsFeedData.TotalCount };
            VKList<UserNotification> notifications = newsFeedData.notifications;
            List<UserNotification> userNotificationList = notifications != null ? notifications.items : null;
            if (userNotificationList != null)
            {
                IEnumerator<UserNotification> enumerator1 = ((IEnumerable<UserNotification>)Enumerable.Where<UserNotification>(userNotificationList, (Func<UserNotification, bool>)(notification =>
                {
                    if (notification.Type == UserNotificationType.newsfeed)
                        return notification.newsfeed != null;
                    return false;
                }))).GetEnumerator();
                try
                {
                    while (enumerator1.MoveNext())
                    {
                        UserNotification notification = enumerator1.Current;
                        UserNotificationNewsfeed newsfeed = notification.newsfeed;
                        List<long> userIds = newsfeed.user_ids ?? new List<long>();
                        List<long> groupIds = newsfeed.group_ids ?? new List<long>();
                        List<User> profiles = newsFeedData.notifications.profiles;
                        List<User> userList;
                        if (profiles == null)
                        {
                            userList = null;
                        }
                        else
                        {
                            Func<User, bool> func = (Func<User, bool>)(user => userIds.Contains(user.id));
                            userList = ((IEnumerable<User>)Enumerable.Where<User>(profiles, (Func<User, bool>)func)).ToList<User>();
                        }
                        List<User> users = userList;
                        List<Group> groups1 = newsFeedData.notifications.groups;
                        List<Group> groupList;
                        if (groups1 == null)
                        {
                            groupList = null;
                        }
                        else
                        {
                            Func<Group, bool> func = (Func<Group, bool>)(group => groupIds.Contains(group.id));
                            groupList = ((IEnumerable<Group>)Enumerable.Where<Group>(groups1, (Func<Group, bool>)func)).ToList<Group>();
                        }
                        List<Group> groups2 = groupList;
                        listWithCount.List.Add((IVirtualizable)new UserNotificationNewsfeedItem(480.0, new Thickness(), notification, users, groups2, (Action)(() =>
                        {
                            IEnumerator<IVirtualizable> enumerator = ((Collection<IVirtualizable>)this.NewsFeedVM.Collection).GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    IVirtualizable current = enumerator.Current;
                                    UserNotificationNewsfeedItem notificationNewsfeedItem = current as UserNotificationNewsfeedItem;
                                    if (notificationNewsfeedItem != null && notificationNewsfeedItem.Id == notification.id)
                                    {
                                        this.NewsFeedVM.Delete(current);
                                        break;
                                    }
                                }
                            }
                            finally
                            {
                                if (enumerator != null)
                                    enumerator.Dispose();
                            }
                        })));
                    }
                }
                finally
                {
                    if (enumerator1 != null)
                        enumerator1.Dispose();
                }
            }
            List<NewsItem>.Enumerator enumerator3 = newsFeedData.items.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    NewsItem current = enumerator3.Current;
                    IVirtualizable virtualizable = null;
                    NewsItemDataWithUsersAndGroupsInfo usersAndGroupsInfo1 = new NewsItemDataWithUsersAndGroupsInfo();
                    usersAndGroupsInfo1.NewsItem = current;
                    List<User> profiles = newsFeedData.profiles;
                    usersAndGroupsInfo1.Profiles = profiles;
                    List<Group> groups = newsFeedData.groups;
                    usersAndGroupsInfo1.Groups = groups;
                    NewsItemDataWithUsersAndGroupsInfo usersAndGroupsInfo2 = usersAndGroupsInfo1;
                    string postType = current.post_type;
                    // ISSUE: reference to a compiler-generated method
                    /*
                    uint stringHash = <PrivateImplementationDetails>.ComputeStringHash(postType);
                    if (stringHash <= 2166822627U)
                    {
                        if ((int)stringHash != 708376497)
                        {
                            if ((int)stringHash != 1513400942)
                            {
                                if ((int)stringHash != -2128144669 || !(postType == "photo"))
                                    goto label_37;
                            }
                            else if (postType == "friends_recomm")
                            {
                                virtualizable = (IVirtualizable)new FriendsRecommendationsNewsItem(usersAndGroupsInfo2);
                                goto label_37;
                            }
                            else
                                goto label_37;
                        }
                        else if (postType == "ads")
                        {
                            virtualizable = (IVirtualizable)new NewsFeedAdsItem(480.0, new Thickness(), current, newsFeedData.profiles, newsFeedData.groups);
                            WallPostItem wallPostItem = ((NewsFeedAdsItem)virtualizable).WallPostItem;
                            if (wallPostItem != null && this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID)
                            {
                                // ISSUE: method pointer
                                wallPostItem.HideSourceItemsCallback = new Action<long, User, Group>(this.HideSourceItemsCallback);
                                wallPostItem.IgnoreNewsfeedItemCallback = new Action<NewsFeedIgnoreItemData>(this.IgnoreNewsfeedAdsItem);
                                goto label_37;
                            }
                            else
                                goto label_37;
                        }
                        else
                            goto label_37;
                    }
                    else if (stringHash <= 2674473766U)
                    {
                        if ((int)stringHash != -1981285817)
                        {
                            if ((int)stringHash != -1620493530 || !(postType == "wall_photo"))
                                goto label_37;
                        }
                        else if (postType == "post")
                        {
                            virtualizable = (IVirtualizable)this.CreateWallPostItem(usersAndGroupsInfo2);
                            goto label_37;
                        }
                        else
                            goto label_37;
                    }
                    else if ((int)stringHash != -822539412)
                    {
                        if ((int)stringHash != -504881604 || !(postType == "photo_tag"))
                            goto label_37;
                    }
                    else if (postType == "video")
                    {
                        virtualizable = this.CreateVideoNewsItem(usersAndGroupsInfo2);
                        goto label_37;
                    }
                    else
                        goto label_37;
                    */
                    if (postType == "post")
                    {
                        virtualizable = (IVirtualizable)this.CreateWallPostItem(usersAndGroupsInfo2);
                        goto label_37;
                    }
                    else if (postType == "video")
                    {
                        virtualizable = this.CreateVideoNewsItem(usersAndGroupsInfo2);
                        goto label_37;
                    }
                    else if (postType == "photo" || postType == "wall_photo" || postType == "photo_tag")
                    {
                        //goto label_37;
                    }
                    else if (postType == "ads")
                    {
                        if (!AppGlobalStateManager.Current.GlobalState.HideADs)
                        {
                            virtualizable = new NewsFeedAdsItem(480.0, new Thickness(), current, newsFeedData.profiles, newsFeedData.groups);
                            WallPostItem wallPostItem = ((NewsFeedAdsItem)virtualizable).WallPostItem;
                            if (wallPostItem != null && this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID)
                            {
                                wallPostItem.HideSourceItemsCallback = new Action<long, User, Group>(this.HideSourceItemsCallback);
                                wallPostItem.IgnoreNewsfeedItemCallback = new Action<NewsFeedIgnoreItemData>(this.IgnoreNewsfeedAdsItem);
                            }
                        }
                        goto label_37;
                    }
                    else if (postType == "friends_recomm")
                    {
                        if (!AppGlobalStateManager.Current.GlobalState.HideFriendsRecommended)
                            virtualizable = new FriendsRecommendationsNewsItem(usersAndGroupsInfo2);
                        goto label_37;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("NewsViewModel.ConvertToVirtualizableList " + postType);
                    }
                    virtualizable = new PhotoVideoNewsItem(480.0, new Thickness(), usersAndGroupsInfo2, false, null, true, new Action<NewsFeedIgnoreItemData>(this.IgnoreNewsFeedItem), null);
                label_37:
                    if (virtualizable != null)
                        listWithCount.List.Add(virtualizable);
                }
            }
            finally
            {
                enumerator3.Dispose();
            }
            return listWithCount;
        }

        private void IgnoreNewsFeedItem(NewsFeedIgnoreItemData ignoreItemData)
        {
            if (ignoreItemData == null)
                return;
            for (int index = 0; index < this.NewsFeedVM.Collection.Count; ++index)
            {
                IVirtualizable virtualizable = this.NewsFeedVM.Collection[index];
                ICanHideFromNewsfeed hideFromNewsfeed = virtualizable as ICanHideFromNewsfeed;
                NewsFeedIgnoreItemData feedIgnoreItemData = hideFromNewsfeed != null ? hideFromNewsfeed.GetIgnoreItemData() : null;
                if (feedIgnoreItemData != null && !(feedIgnoreItemData.Type != ignoreItemData.Type) && (feedIgnoreItemData.OwnerId == ignoreItemData.OwnerId && feedIgnoreItemData.ItemId == ignoreItemData.ItemId))
                {
                    this.NewsFeedVM.Delete(virtualizable);
                    NewsFeedService.Current.IgnoreItem(feedIgnoreItemData.Type, feedIgnoreItemData.OwnerId, feedIgnoreItemData.ItemId, (Action<BackendResult<bool, ResultCode>>)(result => { }));
                    if (!AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled)
                        break;
                    Execute.ExecuteOnUIThread((Action)(() => new GenericInfoUC(2000).ShowAndHideLater(CommonResources.SimilarPostsShownLess, null)));
                    break;
                }
            }
        }

        private void IgnoreNewsfeedAdsItem(NewsFeedIgnoreItemData ignoreItemData)
        {
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            string str = "";
            IEnumerator<IVirtualizable> enumerator1 = ((Collection<IVirtualizable>)this.NewsFeedVM.Collection).GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    IVirtualizable current = enumerator1.Current;
                    NewsFeedAdsItem newsFeedAdsItem = current as NewsFeedAdsItem;
                    WallPostItem wallPostItem = newsFeedAdsItem != null ? newsFeedAdsItem.WallPostItem : null;
                    long? nullable = wallPostItem != null ? new long?(wallPostItem.WallPost.to_id) : new long?();
                    long ownerId = ignoreItemData.OwnerId;
                    if ((nullable.GetValueOrDefault() == ownerId ? (nullable.HasValue ? 1 : 0) : 0) != 0 && wallPostItem.WallPost.id == ignoreItemData.ItemId)
                    {
                        str = newsFeedAdsItem.NewsItem.ads[0].ad_data;
                        virtualizableList.Add(current);
                    }
                }
            }
            finally
            {
                if (enumerator1 != null)
                    enumerator1.Dispose();
            }
            List<IVirtualizable>.Enumerator enumerator2 = virtualizableList.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                    ((Collection<IVirtualizable>)this.NewsFeedVM.Collection).Remove(enumerator2.Current);
            }
            finally
            {
                enumerator2.Dispose();
            }
            AdsIntService.HideAd(str, "ad", (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => { }));
        }

        private void HideSourceItemsCallback(long ownerId, User user = null, Group group = null)
        {
            if (user != null || group != null)
            {
                string str = user != null ? user.first_name : group.name;
                if (!string.IsNullOrEmpty(str) && str.Length > 140)
                    str = string.Format("{0}...", str.Substring(0, 140));
                string format = user == null ? CommonResources.HideFromNewsCommunityConfirmationMessage : (!user.IsFriend ? (user.IsFemale ? CommonResources.HideFromNewsUserFemaleConfirmationMessage : CommonResources.HideFromNewsUserMaleConfirmationMessage) : (user.IsFemale ? CommonResources.HideFromNewsUserFriendFemaleConfirmationMessage : CommonResources.HideFromNewsUserFriendMaleConfirmationMessage));
                string confirmationTitle = CommonResources.HideFromNewsConfirmationTitle;
                if (MessageBox.Show(string.Format(format, str), confirmationTitle, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                    return;
            }
            List<long> longList1 = new List<long>();
            List<long> longList2 = new List<long>();
            if (ownerId > 0L)
                longList1.Add(ownerId);
            else
                longList2.Add(-ownerId);
            List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
            string str1 = "";
            IEnumerator<IVirtualizable> enumerator1 = ((Collection<IVirtualizable>)this.NewsFeedVM.Collection).GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    IVirtualizable current = enumerator1.Current;
                    if (current is WallPostItem)
                    {
                        WallPostItem wallPostItem = current as WallPostItem;
                        if (wallPostItem.WallPost.to_id == ownerId)
                            virtualizableList.Add((IVirtualizable)wallPostItem);
                    }
                    if (current is PhotoVideoNewsItem)
                    {
                        PhotoVideoNewsItem photoVideoNewsItem = current as PhotoVideoNewsItem;
                        if (photoVideoNewsItem.SourceId == ownerId)
                            virtualizableList.Add((IVirtualizable)photoVideoNewsItem);
                    }
                    if (current is NewsFeedAdsItem && (current as NewsFeedAdsItem).WallPostItem.WallPost.to_id == ownerId)
                    {
                        str1 = (current as NewsFeedAdsItem).NewsItem.ads[0].ad_data;
                        virtualizableList.Add(current);
                    }
                    if (current is VideosNewsItem && (current as VideosNewsItem).OwnerId == ownerId)
                        virtualizableList.Add(current);
                }
            }
            finally
            {
                if (enumerator1 != null)
                    enumerator1.Dispose();
            }
            List<IVirtualizable>.Enumerator enumerator2 = virtualizableList.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                    this.NewsFeedVM.Delete(enumerator2.Current);
            }
            finally
            {
                enumerator2.Dispose();
            }
            if (str1 == string.Empty)
                NewsFeedService.Current.AddBan(longList1, longList2, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => { }));
            else
                AdsIntService.HideAd(str1, "source", (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => { }));
        }

        private IVirtualizable CreateVideoNewsItem(NewsItemDataWithUsersAndGroupsInfo post)
        {
            VideosNewsItem videosNewsItem = new VideosNewsItem(480.0, new Thickness(), post, null, null);
            if (this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID)
            {
                videosNewsItem.HideSourceItemsCallback = new Action<long, User, Group>(this.HideSourceItemsCallback);
            }
            return videosNewsItem;
        }

        private WallPostItem CreateWallPostItem(NewsItemDataWithUsersAndGroupsInfo post)
        {
            if (post.NewsItem.marked_as_ads != null && post.NewsItem.marked_as_ads > 0 && AppGlobalStateManager.Current.GlobalState.HideADs)
                return null;
            WallPostItem wallPostItem = new WallPostItem(480.0, new Thickness(), true, post, new Action<WallPostItem>(this.itemDeleted), false, null, false, false, true, true, null, null) { IgnoreNewsfeedItemCallback = new Action<NewsFeedIgnoreItemData>(this.IgnoreNewsFeedItem) };
            if (this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID)
            {
                wallPostItem.HideSourceItemsCallback = new Action<long, User, Group>(this.HideSourceItemsCallback);
            }
            return wallPostItem;
        }

        private void itemDeleted(WallPostItem obj)
        {
            this.NewsFeedVM.Delete((IVirtualizable)obj);
        }

        internal void EnsureUpToDate()
        {
            if (this.NewsFeedVM.Collection.Count != 0)
                return;
            this.ReloadNews(true, true, false);
        }

        public void Reset()
        {
            this.NewsFeedVM.Collection.Clear();
            this.NewsSource = NewsSources.NewsFeed.PickableItem;
            this._from = "";
            this._freshNewsData = null;
        }

        public void CheckForFreshNewsIfNeeded(double scrollPosition)
        {
            if (this._isLoadingFreshNews || ((Collection<IVirtualizable>)this.NewsFeedVM.Collection).Count == 0)
                return;
            DateTime dateTimeNow = DateTime.Now;
            double totalSeconds = (dateTimeNow - this._lastFreshNewsCheckDateTime).TotalSeconds;
            NewsFeedConsts newsFeedConsts = this.NewsFeedConsts;
            int? nullable1 = newsFeedConsts != null ? new int?(newsFeedConsts.fresh_news_check_interval) : new int?();
            double? nullable2 = nullable1.HasValue ? new double?(nullable1.GetValueOrDefault()) : new double?();
            double valueOrDefault = nullable2.GetValueOrDefault();
            if ((totalSeconds < valueOrDefault ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                return;
            this._isLoadingFreshNews = true;
            this.CheckForFreshNews(scrollPosition, (Action<FreshNewsState?>)(state =>
            {
                if (state.HasValue)
                    this.FreshNewsState = state.Value;
                this._lastFreshNewsCheckDateTime = dateTimeNow;
                this._isLoadingFreshNews = false;
            }), (Action)(() => this._isLoadingFreshNews = false));
        }

        private void CheckForFreshNews(double scrollPosition, Action<FreshNewsState?> successCallback, Action errorCallback)
        {
            NewsFeedGetParams newsFeedGetParams1 = new NewsFeedGetParams();
            NewsFeedConsts newsFeedConsts = this.NewsFeedConsts;
            int num1 = newsFeedConsts != null ? newsFeedConsts.fresh_news_load_count : 25;
            newsFeedGetParams1.count = num1;
            long id = this.NewsSource.ID;
            newsFeedGetParams1.NewsListId = id;
            NewsFeedGetParams newsFeedGetParams2 = newsFeedGetParams1;
            if (this.SyncNotifications)
            {
                newsFeedGetParams2.SyncNotifications = true;
                this._lastNotificationsSyncDateTime = DateTime.Now;
            }
            if (this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID && this._newsFeedType.HasValue && this._newsFeedType.Value == NewsFeedType.top)
            {
                int totalSeconds = (int)(DateTime.Now - this.NavigatedFromNewsfeedTime).TotalSeconds;
                string postId;
                int postPosition;
                this.GetCurrentPostInfo(scrollPosition, out postId, out postPosition);
                newsFeedGetParams2.UpdateAwayTime = totalSeconds;
                newsFeedGetParams2.UpdatePosition = postPosition;
                newsFeedGetParams2.UpdatePost = postId;
            }
            NewsFeedService.Current.GetNewsFeed(newsFeedGetParams2, (Action<BackendResult<NewsFeedData, ResultCode>>)(result =>
            {
                if (result.ResultCode != ResultCode.Succeeded)
                    Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        Action action = errorCallback;
                        if (action == null)
                            return;
                        action();
                    }));
                else
                    Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        NewsFeedData newsfeedData = result.ResultData;
                        if (newsfeedData != null && !newsfeedData.notifications_updated)
                            this.CreateNewNotificationsFromExisting(newsfeedData);
                        List<IVirtualizable> virtualizableItems = this.ConvertToVirtualizableList(newsfeedData).List.ToList<IVirtualizable>();
                        List<long> newNotificationIds = null;
                        List<long> existingNotificationIds = null;
                        List<IHaveUniqueKey> haveUniqueKeyList1 = new List<IHaveUniqueKey>();
                        List<IHaveUniqueKey> haveUniqueKeyList2 = new List<IHaveUniqueKey>();
                        Func<IVirtualizable, bool> func = (Func<IVirtualizable, bool>)(item =>
                        {
                            if (item is IHaveUniqueKey && !(item is NewsFeedAdsItem))
                                return !(item is FriendsRecommendationsNewsItem);
                            return false;
                        });
                        IEnumerator<IVirtualizable> enumerator1 = ((IEnumerable<IVirtualizable>)Enumerable.Where<IVirtualizable>(this.NewsFeedVM.Collection, (Func<IVirtualizable, bool>)func)).GetEnumerator();
                        try
                        {
                            while (enumerator1.MoveNext())
                            {
                                IVirtualizable current = enumerator1.Current;
                                UserNotificationNewsfeedItem notificationNewsfeedItem = current as UserNotificationNewsfeedItem;
                                if (notificationNewsfeedItem != null)
                                {
                                    if (existingNotificationIds == null)
                                        existingNotificationIds = new List<long>();
                                    existingNotificationIds.Add(notificationNewsfeedItem.Id);
                                }
                                else
                                    haveUniqueKeyList1.Add((IHaveUniqueKey)current);
                            }
                        }
                        finally
                        {
                            if (enumerator1 != null)
                                enumerator1.Dispose();
                        }
                        IEnumerator<IVirtualizable> enumerator2 = ((IEnumerable<IVirtualizable>)Enumerable.Where<IVirtualizable>(virtualizableItems, (Func<IVirtualizable, bool>)func)).GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                IVirtualizable current = enumerator2.Current;
                                UserNotificationNewsfeedItem notificationNewsfeedItem = current as UserNotificationNewsfeedItem;
                                if (notificationNewsfeedItem != null)
                                {
                                    if (newNotificationIds == null)
                                        newNotificationIds = new List<long>();
                                    newNotificationIds.Add(notificationNewsfeedItem.Id);
                                }
                                else
                                    haveUniqueKeyList2.Add((IHaveUniqueKey)current);
                            }
                        }
                        finally
                        {
                            if (enumerator2 != null)
                                enumerator2.Dispose();
                        }
                        if (haveUniqueKeyList1.Count == 0)
                        {
                            Action<FreshNewsState?> action = successCallback;
                            if (action == null)
                                return;
                            FreshNewsState? nullable = new FreshNewsState?();
                            action(nullable);
                        }
                        else if (haveUniqueKeyList2.Count == 0)
                        {
                            Action<FreshNewsState?> action = successCallback;
                            if (action == null)
                                return;
                            FreshNewsState? nullable = new FreshNewsState?();
                            action(nullable);
                        }
                        else
                        {
                            Action updatePendingFreshNewsDataAction = (Action)(() =>
                            {
                                if (newsfeedData == null)
                                    return;
                                this._freshNewsData = new FreshNewsData((IEnumerable<IVirtualizable>)virtualizableItems, newsfeedData.next_from);
                            });
                            Action action1 = (Action)(() =>
                            {
                                updatePendingFreshNewsDataAction();
                                Action<FreshNewsState?> action = successCallback;
                                if (action != null)
                                {
                                    FreshNewsState? nullable = new FreshNewsState?(FreshNewsState.Reload);
                                    action(nullable);
                                }
                                Action<UserNotification> newsfeedTopPromoAction = this.ShowNewsfeedTopPromoAction;
                                if (newsfeedTopPromoAction == null)
                                    return;
                                UserNotification newsfeedNotification = NewsViewModel.TryGetBubbleNewsfeedNotification(newsfeedData);
                                newsfeedTopPromoAction(newsfeedNotification);
                            });
                            Action action3 = (Action)(() =>
                            {
                                updatePendingFreshNewsDataAction();
                                Action<FreshNewsState?> action = successCallback;
                                if (action != null)
                                {
                                    FreshNewsState? nullable = new FreshNewsState?(FreshNewsState.ForcedReload);
                                    action(nullable);
                                }
                                Action<UserNotification> newsfeedTopPromoAction = this.ShowNewsfeedTopPromoAction;
                                if (newsfeedTopPromoAction == null)
                                    return;
                                UserNotification newsfeedNotification = NewsViewModel.TryGetBubbleNewsfeedNotification(newsfeedData);
                                newsfeedTopPromoAction(newsfeedNotification);
                            });
                            if (!NewsViewModel.GetNotificationsUpToDate(newNotificationIds, existingNotificationIds))
                            {
                                action3();
                            }
                            else
                            {
                                int num2 = -1;
                                IHaveUniqueKey haveUniqueKey = haveUniqueKeyList1[0];
                                for (int index = 0; index < haveUniqueKeyList2.Count; ++index)
                                {
                                    if (haveUniqueKeyList2[index].GetKey() == haveUniqueKey.GetKey())
                                    {
                                        num2 = index;
                                        break;
                                    }
                                }
                                if (num2 == 0)
                                {
                                    Action<FreshNewsState?> action2 = successCallback;
                                    if (action2 == null)
                                        return;
                                    FreshNewsState? nullable = new FreshNewsState?();
                                    action2(nullable);
                                }
                                else
                                {
                                    if (newNotificationIds != null && newNotificationIds.Count > 0 || existingNotificationIds != null && existingNotificationIds.Count > 0)
                                        action1();
                                    else if (num2 == -1)
                                    {
                                        action1();
                                    }
                                    else
                                    {
                                        bool flag = true;
                                        for (int index = num2; index < Math.Min(haveUniqueKeyList2.Count, haveUniqueKeyList1.Count); ++index)
                                        {
                                            if (haveUniqueKeyList2[index].GetKey() != haveUniqueKeyList1[index - num2].GetKey())
                                            {
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (!flag)
                                        {
                                            action1();
                                        }
                                        else
                                        {
                                            this._freshNewsData = null;
                                            for (int index = num2 - 1; index >= 0; --index)
                                                this.NewsFeedVM.Insert(virtualizableItems[index], 0);
                                            Action<FreshNewsState?> action2 = successCallback;
                                            if (action2 != null)
                                            {
                                                FreshNewsState? nullable = new FreshNewsState?(FreshNewsState.Insert);
                                                action2(nullable);
                                            }
                                            Action<UserNotification> newsfeedTopPromoAction = this.ShowNewsfeedTopPromoAction;
                                            if (newsfeedTopPromoAction == null)
                                                return;
                                            UserNotification newsfeedNotification = NewsViewModel.TryGetBubbleNewsfeedNotification(newsfeedData);
                                            newsfeedTopPromoAction(newsfeedNotification);
                                        }
                                    }
                                }
                            }
                        }
                    }));
            }));
        }

        private void GetCurrentPostInfo(double scrollPosition, out string postId, out int postPosition)
        {
            postId = "";
            postPosition = -1;
            double num = 0.0;
            IEnumerator<IVirtualizable> enumerator = ((Collection<IVirtualizable>)this.NewsFeedVM.Collection).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    IVirtualizable current = enumerator.Current;
                    IHaveNewsfeedItemId haveNewsfeedItemId = current as IHaveNewsfeedItemId;
                    if (haveNewsfeedItemId != null)
                    {
                        postPosition = postPosition + 1;
                        if (num >= scrollPosition)
                        {
                            postId = haveNewsfeedItemId.NewsfeedItemId;
                            break;
                        }
                        num += current.FixedHeight;
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
        }

        private void CreateNewNotificationsFromExisting(NewsFeedData newsfeedData)
        {
            IEnumerator<IVirtualizable> enumerator = this.NewsFeedVM.Collection.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    UserNotificationNewsfeedItem current = enumerator.Current as UserNotificationNewsfeedItem;
                    UserNotificationNewsfeed notificationNewsfeed;
                    if (current == null)
                    {
                        notificationNewsfeed = null;
                    }
                    else
                    {
                        UserNotification userNotification = current.UserNotification;
                        notificationNewsfeed = userNotification != null ? userNotification.newsfeed : null;
                    }
                    if (notificationNewsfeed == null)
                        break;
                    if (newsfeedData.notifications == null)
                        newsfeedData.notifications = new VKList<UserNotification>()
                        {
                            groups = current.Groups,
                            profiles = current.Users,
                            items = new List<UserNotification>()
                        };
                    List<UserNotification> items = newsfeedData.notifications.items;
                    UserNotification userNotification1 = new UserNotification();
                    userNotification1.Type = UserNotificationType.newsfeed;
                    UserNotificationNewsfeed newsfeed = current.UserNotification.newsfeed;
                    userNotification1.newsfeed = newsfeed;
                    items.Add(userNotification1);
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
        }

        private static bool GetNotificationsUpToDate(List<long> newNotificationIds, List<long> existingNotificationIds)
        {
            if (newNotificationIds == null)
            {
                return true;
            }
            
            List<long> expr_1E = existingNotificationIds;
            int? num = (expr_1E != null) ? new int?(expr_1E.Count) : default(int?);
            return newNotificationIds.Count == num.GetValueOrDefault() && num.HasValue && !Enumerable.Any<long>(Enumerable.Where<long>(newNotificationIds, (long id, int i) => id != existingNotificationIds[i]));
        }

        public void ReplaceAllWithPendingFreshNews()
        {
            if (this._freshNewsData == null || this._freshNewsData.Items.Count == 0)
                return;
            while (((Collection<IVirtualizable>)this.NewsFeedVM.Collection).Count > 0)
                this.NewsFeedVM.Delete(this.NewsFeedVM.Collection.Last<IVirtualizable>());
            for (int index = this._freshNewsData.Items.Count - 1; index >= 0; --index)
                this.NewsFeedVM.Insert(this._freshNewsData.Items[index], 0);
            this.NewsFeedVM.TotalCount = 0;
            this._from = this._freshNewsData.NextFrom;
            this.FreshNewsState = FreshNewsState.NoNews;
            this._lastNewsFeedUpdateDateTime = DateTime.Now;
        }

        public void Handle(WallPostAddedOrEdited message)
        {
            if (this.NewsSource == null || message.Edited && message.NewlyAddedWallPost.IsPostponed)
                return;
            WallPostItem wallPostItem1 = this.CreateWallPostItem(new NewsItemDataWithUsersAndGroupsInfo() { WallPost = message.NewlyAddedWallPost, Profiles = message.Users, Groups = message.Groups });
            WallPost newWallPost = message.NewlyAddedWallPost;
            IVirtualizable virtualizable = (IVirtualizable)Enumerable.FirstOrDefault<IVirtualizable>(this.NewsFeedVM.Collection, (Func<IVirtualizable, bool>)(wp =>
            {
                WallPostItem wallPostItem2 = wp as WallPostItem;
                if (wallPostItem2 == null)
                    return false;
                WallPost wallPost = wallPostItem2.WallPost;
                if (wallPost.id == newWallPost.id)
                    return wallPost.to_id == newWallPost.to_id;
                return false;
            }));
            if (virtualizable == null)
            {
                if (this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID)
                {
                    this.KeepScrollPosition = false;
                    this.NewsFeedVM.Insert((IVirtualizable)wallPostItem1, 0);
                    this.KeepScrollPosition = true;
                }
                else
                    new DelayedExecutor(500).AddToDelayedExecution((Action)(() =>
                    {
                        PageBase currentPage = FramePageUtils.CurrentPage;
                        if (currentPage == null || (currentPage).GetType() == typeof(NewsPage))
                            return;
                        Execute.ExecuteOnUIThread((Action)(() => GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.WallPost, 0, "")));
                    }));
            }
            else
            {
                this.KeepScrollPosition = false;
                int ind = this.NewsFeedVM.Collection.IndexOf(virtualizable);
                this.NewsFeedVM.Delete(virtualizable);
                this.NewsFeedVM.Insert(wallPostItem1, ind);
                this.KeepScrollPosition = true;
            }
        }

        public void Handle(AdReportedEvent message)
        {
            this.NewsFeedVM.Delete(Enumerable.FirstOrDefault<IVirtualizable>(this.NewsFeedVM.Collection, (Func<IVirtualizable, bool>)(item =>
            {
                if (item is NewsFeedAdsItem)
                return (item as NewsFeedAdsItem).NewsItem.ads[0].ad_data == message.AdData;
                return false;
            })));
        }

        public void Handle(NewsfeedTopEnabledDisabledEvent message)
        {
            this.UpdateFeedType();
        }

        public void UpdateFeedType()
        {
            if (this.NewsSource == null)
                return;
            this._setFeedTypePending = true;
            if (this.NewsSource.ID == NewsSources.NewsFeed.PickableItem.ID)
                this.ReloadNews(true, true, false);
            else
                this.NewsSource = NewsSources.NewsFeed.PickableItem;
        }
    }
}
