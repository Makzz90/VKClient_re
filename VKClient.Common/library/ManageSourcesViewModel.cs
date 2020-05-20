using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Common.Library
{
    public class ManageSourcesViewModel : ViewModelBase, IHandle<UserIsSubcribedUnsubscribedToEvent>, IHandle, IHandle<GroupSubscribedUnsubscribedEvent>, ICollectionDataProvider<ProfilesAndGroups, FriendHeader>, ICollectionDataProvider<ProfilesAndGroups, GroupHeader>
    {
        private GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> _friendsVM;
        private GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> _groupsVM;
        private AsyncHelper<BackendResult<ProfilesAndGroups, ResultCode>> _helperGetBanned;
        private int _selectedCount;
        private ManageSourcesMode _manageSourcesMode;

        public GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> FriendsVM
        {
            get
            {
                return this._friendsVM;
            }
        }

        public string Title
        {
            get
            {
                if (this._manageSourcesMode != ManageSourcesMode.ManageHiddenNewsSources)
                    return CommonResources.Settings_Nofications_Sources.ToUpperInvariant();
                return CommonResources.HiddenSources.ToUpperInvariant();
            }
        }

        public GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> GroupsVM
        {
            get
            {
                return this._groupsVM;
            }
        }

        public int SelectedCount
        {
            get
            {
                return this._selectedCount;
            }
            set
            {
                this._selectedCount = value;
                this.NotifyPropertyChanged<int>(() => this.SelectedCount);
            }
        }

        public Func<ProfilesAndGroups, ListWithCount<FriendHeader>> ConverterFunc
        {
            get
            {
                return (Func<ProfilesAndGroups, ListWithCount<FriendHeader>>)(pg =>
                {
                    ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
                    listWithCount.TotalCount = pg.profiles.Count;
                    listWithCount.List = ((IEnumerable<FriendHeader>)Enumerable.Select<User, FriendHeader>(pg.profiles, (Func<User, FriendHeader>)(p => new FriendHeader(p, false)
                    {
                        IsInSelectedState = true
                    }))).ToList<FriendHeader>();
                    List<FriendHeader>.Enumerator enumerator = listWithCount.List.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                            enumerator.Current.PropertyChanged += new PropertyChangedEventHandler(this.p_PropertyChanged);
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                    return listWithCount;
                });
            }
        }

        Func<ProfilesAndGroups, ListWithCount<GroupHeader>> ICollectionDataProvider<ProfilesAndGroups, GroupHeader>.ConverterFunc
        {
            get
            {
                return (Func<ProfilesAndGroups, ListWithCount<GroupHeader>>)(pg =>
                {
                    ListWithCount<GroupHeader> listWithCount = new ListWithCount<GroupHeader>();
                    listWithCount.TotalCount = pg.groups.Count;
                    listWithCount.List = ((IEnumerable<GroupHeader>)Enumerable.Select<Group, GroupHeader>(pg.groups, (Func<Group, GroupHeader>)(g => new GroupHeader(g, null)))).ToList<GroupHeader>();
                    List<GroupHeader>.Enumerator enumerator = listWithCount.List.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                            enumerator.Current.PropertyChanged += new PropertyChangedEventHandler(this.p_PropertyChanged);
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                    return listWithCount;
                });
            }
        }

        public ManageSourcesViewModel(ManageSourcesMode mode)
        {
            this._manageSourcesMode = mode;
            this._friendsVM = new GenericCollectionViewModel<ProfilesAndGroups, FriendHeader>((ICollectionDataProvider<ProfilesAndGroups, FriendHeader>)this);
            this._groupsVM = new GenericCollectionViewModel<ProfilesAndGroups, GroupHeader>((ICollectionDataProvider<ProfilesAndGroups, GroupHeader>)this);
            if (this._manageSourcesMode == ManageSourcesMode.ManagePushNotificationsSources)
            {
                this._friendsVM.NoItemsDescription = string.Format(CommonResources.NewsNotifications_Desc, (Environment.NewLine + Environment.NewLine));
                this._groupsVM.NoItemsDescription = string.Format(CommonResources.NewsNotifications_Desc, (Environment.NewLine + Environment.NewLine));
            }
            this._helperGetBanned = new AsyncHelper<BackendResult<ProfilesAndGroups, ResultCode>>((Action<Action<BackendResult<ProfilesAndGroups, ResultCode>>>)(a => NewsFeedService.Current.GetBanned(a)));
            EventAggregator.Current.Subscribe(this);
        }

        public void DeleteSelected()
        {
            List<FriendHeader> list1 = ((IEnumerable<FriendHeader>)Enumerable.Where<FriendHeader>(this._friendsVM.Collection, (Func<FriendHeader, bool>)(fh => fh.IsSelected))).ToList<FriendHeader>();
            List<GroupHeader> list2 = ((IEnumerable<GroupHeader>)Enumerable.Where<GroupHeader>(this._groupsVM.Collection, (Func<GroupHeader, bool>)(gh => gh.IsSelected))).ToList<GroupHeader>();
            switch (this._manageSourcesMode)
            {
                case ManageSourcesMode.ManageHiddenNewsSources:
                    NewsFeedService.Current.DeleteBan(((IEnumerable<long>)Enumerable.Select<FriendHeader, long>(list1, (Func<FriendHeader, long>)(fh => fh.UserId))).ToList<long>(), ((IEnumerable<long>)Enumerable.Select<GroupHeader, long>(list2, (Func<GroupHeader, long>)(g => g.Group.id))).ToList<long>(), (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        Execute.ExecuteOnUIThread(delegate
                        {
                            EventAggregator.Current.Publish(new HiddenNewsSourcesCountUpdated
                            {
                                UpdatedCount = this._friendsVM.Collection.Count + this._groupsVM.Collection.Count
                            });
                            if (NewsViewModel.Instance.NewsSource.Alias == "news")
                            {
                                NewsViewModel.Instance.ReloadNews(true, true, false);
                            }
                        });
                    }
                }));
                    break;
                case ManageSourcesMode.ManagePushNotificationsSources:
                    this.DoUnsubscribe(((IEnumerable<long>)Enumerable.Select<FriendHeader, long>(list1, (Func<FriendHeader, long>)(fh => fh.UserId))).Union<long>((IEnumerable<long>)Enumerable.Select<GroupHeader, long>(list2, (Func<GroupHeader, long>)(g => -g.Group.id))).ToList<long>().Partition<long>(25), 0);
                    break;
            }
            List<FriendHeader>.Enumerator enumerator1 = list1.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                    this._friendsVM.Delete(enumerator1.Current);
            }
            finally
            {
                enumerator1.Dispose();
            }
            List<GroupHeader>.Enumerator enumerator2 = list2.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                    this._groupsVM.Delete(enumerator2.Current);
            }
            finally
            {
                enumerator2.Dispose();
            }
            this.SelectedCount = this.SelectedCount - (list1.Count + list2.Count);
        }

        private void DoUnsubscribe(IEnumerable<IEnumerable<long>> listsToBeUnsubscribed, int ind)
        {
            if (ind >= listsToBeUnsubscribed.Count<IEnumerable<long>>())
                return;
            WallService.Current.WallSubscriptionsUnsubscribe(listsToBeUnsubscribed.ToList<IEnumerable<long>>()[ind].ToList<long>(), (Action<BackendResult<ResponseWithId, ResultCode>>)(unsubscribeRes => this.DoUnsubscribe(listsToBeUnsubscribed, ind + 1)));
        }

        public void GetData(GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> caller, int offset, int count, Action<BackendResult<ProfilesAndGroups, ResultCode>> callback)
        {
            switch (this._manageSourcesMode)
            {
                case ManageSourcesMode.ManageHiddenNewsSources:
                    this._helperGetBanned.RunAction(callback, false);
                    break;
                case ManageSourcesMode.ManagePushNotificationsSources:
                    WallService.Current.GetWallSubscriptionsProfiles(offset, count, (Action<BackendResult<VKList<User>, ResultCode>>)(res =>
                    {
                        ProfilesAndGroups resultData = null;
                        if (res.ResultCode == ResultCode.Succeeded)
                            resultData = new ProfilesAndGroups()
                            {
                                profiles = res.ResultData.items
                            };
                        callback(new BackendResult<ProfilesAndGroups, ResultCode>(res.ResultCode, resultData));
                    }));
                    break;
            }
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoPersons;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
        }

        public void GetData(GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> caller, int offset, int count, Action<BackendResult<ProfilesAndGroups, ResultCode>> callback)
        {
            switch (this._manageSourcesMode)
            {
                case ManageSourcesMode.ManageHiddenNewsSources:
                    this._helperGetBanned.RunAction(callback, false);
                    break;
                case ManageSourcesMode.ManagePushNotificationsSources:
                    WallService.Current.GetWallSubscriptionsGroups(offset, count, (Action<BackendResult<VKList<Group>, ResultCode>>)(res =>
                    {
                        ProfilesAndGroups resultData = null;
                        if (res.ResultCode == ResultCode.Succeeded)
                            resultData = new ProfilesAndGroups()
                            {
                                groups = res.ResultData.items
                            };
                        callback(new BackendResult<ProfilesAndGroups, ResultCode>(res.ResultCode, resultData));
                    }));
                    break;
            }
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoCommunites;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCommunityFrm, CommonResources.TwoFourCommunitiesFrm, CommonResources.FiveCommunitiesFrm, true, null, false);
        }

        private void p_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsSelected"))
                return;
            bool flag = false;
            if (sender is FriendHeader)
                flag = (sender as FriendHeader).IsSelected;
            if (sender is GroupHeader)
                flag = (sender as GroupHeader).IsSelected;
            if (flag)
                this.SelectedCount = this.SelectedCount + 1;
            else
                this.SelectedCount = this.SelectedCount - 1;
        }

        public void Handle(UserIsSubcribedUnsubscribedToEvent message)
        {
            if (this._manageSourcesMode != ManageSourcesMode.ManagePushNotificationsSources)
                return;
            if (message.IsSubscribed)
            {
                this.FriendsVM.LoadData(true, false, null, false);
            }
            else
            {
                FriendHeader friendHeader = (FriendHeader)Enumerable.FirstOrDefault<FriendHeader>(this.FriendsVM.Collection, (Func<FriendHeader, bool>)(fh => fh.UserId == message.user.id));
                if (friendHeader == null)
                    return;
                this.FriendsVM.Delete(friendHeader);
            }
        }

        public void Handle(GroupSubscribedUnsubscribedEvent message)
        {
            if (this._manageSourcesMode != ManageSourcesMode.ManagePushNotificationsSources)
                return;
            if (message.IsSubscribed)
            {
                this.GroupsVM.LoadData(true, false, null, false);
            }
            else
            {
                GroupHeader groupHeader = (GroupHeader)Enumerable.FirstOrDefault<GroupHeader>(this.GroupsVM.Collection, (Func<GroupHeader, bool>)(fh => fh.Group.id == message.group.id));
                if (groupHeader == null)
                    return;
                this.GroupsVM.Delete(groupHeader);
            }
        }

        //
        internal double px_per_tick = 62.0 / 10.0 / 2.0;

        public double UserAvatarRadius
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick;
            }
        }
        //
    }
}
