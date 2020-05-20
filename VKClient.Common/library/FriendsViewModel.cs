using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class FriendsViewModel : ViewModelBase, IHandle<CountersChanged>, IHandle, IHandle<FriendRemoved>, IHandle<FriendRequestAcceptedDeclined>, ICollectionDataProvider<List<User>, Group<FriendHeader>>, ICollectionDataProvider<List<User>, FriendHeader>, ICollectionDataProvider<List<FriendsList>, FriendHeader>
    {
        private long _uid;
        private long _lid;
        private bool _isLoading;
        private string _name;
        private FriendsViewModel.Mode _mode;
        private GenericCollectionViewModel<List<User>, Group<FriendHeader>> _allFriendsVM;
        private GenericCollectionViewModel<List<User>, FriendHeader> _onlineFriendsVM;
        private GenericCollectionViewModel<List<User>, FriendHeader> _commonFriendsVM;
        private GenericCollectionViewModel<List<FriendsList>, FriendHeader> _friendsListVM;
        private AsyncHelper<BackendResult<List<User>, ResultCode>> _helperGetCurrentUserFriends;
        private AsyncHelper<BackendResult<List<User>, ResultCode>> _helperFriendsFromList;
        private AsyncHelper<BackendResult<FriendsAndMutualFriends, ResultCode>> _helperGetFriendsAndMutual;
        private FriendRequests _requestsViewModel;
        private bool _getAllFriendsCalled;
        private bool _getOnlineCalled;
        private bool _getCommonCalled;

        public GenericCollectionViewModel<List<User>, Group<FriendHeader>> AllFriendsVM
        {
            get
            {
                return this._allFriendsVM;
            }
        }

        public GenericCollectionViewModel<List<User>, FriendHeader> OnlineFriendsVM
        {
            get
            {
                return this._onlineFriendsVM;
            }
        }

        public GenericCollectionViewModel<List<User>, FriendHeader> CommonFriendsVM
        {
            get
            {
                return this._commonFriendsVM;
            }
        }

        public GenericCollectionViewModel<List<FriendsList>, FriendHeader> FriendListsVM
        {
            get
            {
                return this._friendsListVM;
            }
        }

        public bool OwnFriends
        {
            get
            {
                if (this._uid != 0L)
                    return this._uid == AppGlobalStateManager.Current.LoggedInUserId;
                return true;
            }
        }

        public string Title
        {
            get
            {
                string str = "";
                if (this._mode == FriendsViewModel.Mode.Friends)
                    str = this.OwnFriends || string.IsNullOrEmpty(this._name) ? CommonResources.FriendsPage_OwnFriendsTitle : string.Format(CommonResources.FriendsPage_TitleFrm, this._name);
                if (this._mode == FriendsViewModel.Mode.Lists)
                    str = this._name ?? "";
                return str.ToUpperInvariant();
            }
        }

        public FriendsViewModel.Mode FriendsMode
        {
            get
            {
                return this._mode;
            }
        }

        public Visibility FriendsListsVisibility
        {
            get
            {
                if (!this.OwnFriends)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility CommonFriendsVisibility
        {
            get
            {
                if (this.OwnFriends)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility AllFriendsCountVisibility
        {
            get
            {
                if (this._lid == 0L)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility RequestsBlockVisibility
        {
            get
            {
                if (!this.OwnFriends || this.RequestsViewModel == null || this.RequestsViewModel.count <= 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public FriendRequests RequestsViewModel
        {
            get
            {
                return this._requestsViewModel;
            }
            set
            {
                this._requestsViewModel = value;
                this.NotifyPropertyChanged<FriendRequests>(() => this.RequestsViewModel);
                this.NotifyPropertyChanged<Visibility>(() => this.RequestsBlockVisibility);
                FriendsCache.Instance.SetFriends(null, value);
            }
        }

        internal List<User> AllFriendsRaw
        {
            get
            {
                List<User> userList = new List<User>();
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                IEnumerator<Group<FriendHeader>> enumerator1 = ((Collection<Group<FriendHeader>>)this._allFriendsVM.Collection).GetEnumerator();
                try
                {
                    while (enumerator1.MoveNext())
                    {
                        IEnumerator<FriendHeader> enumerator2 = ((Collection<FriendHeader>)enumerator1.Current).GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                FriendHeader current = enumerator2.Current;
                                string key = current.GetKey();
                                if (!dictionary.ContainsKey(key))
                                {
                                    userList.Add(current.User);
                                    dictionary[key] = key;
                                }
                            }
                        }
                        finally
                        {
                            if (enumerator2 != null)
                                enumerator2.Dispose();
                        }
                    }
                }
                finally
                {
                    if (enumerator1 != null)
                        enumerator1.Dispose();
                }
                return userList;
            }
        }

        public Func<List<User>, ListWithCount<Group<FriendHeader>>> ConverterFunc
        {
            get
            {
                return (Func<List<User>, ListWithCount<Group<FriendHeader>>>)(users =>
                {
                    ListWithCount<Group<FriendHeader>> listWithCount = new ListWithCount<Group<FriendHeader>>();
                    listWithCount.TotalCount = users.Count;
                    bool isMenuEnabled = this._mode == FriendsViewModel.Mode.Lists;
                    bool flag = listWithCount.TotalCount >= 20;
                    List<FriendHeader> friendHeaderList = new List<FriendHeader>();
                    Group<FriendHeader> group = new Group<FriendHeader>("", false);
                    if (this._lid == 0L && this._uid == AppGlobalStateManager.Current.LoggedInUserId)
                    {
                        List<FriendHeader> list = Enumerable.ToList<FriendHeader>(Enumerable.Select<User, FriendHeader>(((IEnumerable<User>)users).Take<User>(flag ? 5 : 20), (Func<User, FriendHeader>)(s => new FriendHeader(s, isMenuEnabled))));
                        list.ForEach((Action<FriendHeader>)(f => f.Initial = new char?()));
                        Action<FriendHeader> action = new Action<FriendHeader>(((Collection<FriendHeader>)group).Add);
                        list.ForEach(action);
                    }
                    friendHeaderList.AddRange((IEnumerable<FriendHeader>)Enumerable.Select<User, FriendHeader>(users, (Func<User, FriendHeader>)(s => new FriendHeader(s, isMenuEnabled))));
                    IEnumerable<Group<FriendHeader>> groups = null;
                    if (flag)
                    {
                        Func<FriendHeader, string> orderFunc = null;
                        orderFunc = AppGlobalStateManager.Current.GlobalState.FriendListOrder != 0 ? (Func<FriendHeader, string>)(fh => fh.LastName) : (Func<FriendHeader, string>)(fh => fh.FullName);
                        groups = (IEnumerable<Group<FriendHeader>>)Enumerable.Select<IGrouping<char?, FriendHeader>, Group<FriendHeader>>(Enumerable.OrderBy<IGrouping<char?, FriendHeader>, char?>(Enumerable.GroupBy<FriendHeader, char?>(friendHeaderList, (Func<FriendHeader, char?>)(fh => fh.Initial)), (Func<IGrouping<char?, FriendHeader>, char?>)(g => g.Key)), (Func<IGrouping<char?, FriendHeader>, Group<FriendHeader>>)(g => new Group<FriendHeader>(g.Key.ToString(), (IEnumerable<FriendHeader>)Enumerable.OrderBy<FriendHeader, string>(g, (Func<FriendHeader, string>)orderFunc), false)));
                    }
                    if (((Collection<FriendHeader>)group).Count > 0)
                        listWithCount.List.Add(group);
                    if (!flag && this._uid != AppGlobalStateManager.Current.LoggedInUserId)
                        listWithCount.List.Add(new Group<FriendHeader>("", (IEnumerable<FriendHeader>)friendHeaderList, false));
                    if (groups != null)
                    {
                        IEnumerator<Group<FriendHeader>> enumerator = groups.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                Group<FriendHeader> current = enumerator.Current;
                                listWithCount.List.Add(current);
                            }
                        }
                        finally
                        {
                            if (enumerator != null)
                                enumerator.Dispose();
                        }
                    }
                    return listWithCount;
                });
            }
        }

        Func<List<User>, ListWithCount<FriendHeader>> ICollectionDataProvider<List<User>, FriendHeader>.ConverterFunc
        {
            get
            {
                bool isMenuEnabled = (this._mode == FriendsViewModel.Mode.Lists ? 1 : 0) != 0;
                return (Func<List<User>, ListWithCount<FriendHeader>>)(users => new ListWithCount<FriendHeader>() { TotalCount = users.Count, List = new List<FriendHeader>((IEnumerable<FriendHeader>)Enumerable.Select<User, FriendHeader>(users, (Func<User, FriendHeader>)(u => new FriendHeader(u, isMenuEnabled)))) });
            }
        }

        Func<List<FriendsList>, ListWithCount<FriendHeader>> ICollectionDataProvider<List<FriendsList>, FriendHeader>.ConverterFunc
        {
            get
            {
                return (Func<List<FriendsList>, ListWithCount<FriendHeader>>)(fl =>
                {
                    ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
                    string str = CommonResources.Birthdays_Title.Substring(0, 1) + CommonResources.Birthdays_Title.Substring(1).ToLowerInvariant();
                    FriendsList cachedAllowedList = new FriendsList() { id = -1, name = str };
                    listWithCount.List.Add(new FriendHeader(cachedAllowedList));
                    List<FriendsList>.Enumerator enumerator = fl.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            FriendsList current = enumerator.Current;
                            listWithCount.List.Add(new FriendHeader(current));
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                    listWithCount.TotalCount = listWithCount.List.Count;
                    return listWithCount;
                });
            }
        }

        public FriendsViewModel(long uid, string name)
        {
            this._uid = uid;
            this._name = name;
            this._mode = FriendsViewModel.Mode.Friends;
            this.Initialize();
        }

        public FriendsViewModel(long lid, string listName, bool list)
        {
            this._lid = lid;
            this._name = listName;
            this._mode = FriendsViewModel.Mode.Lists;
            this.Initialize();
        }

        private void Initialize()
        {
            EventAggregator.Current.Subscribe(this);
            this._helperGetFriendsAndMutual = new AsyncHelper<BackendResult<FriendsAndMutualFriends, ResultCode>>((Action<Action<BackendResult<FriendsAndMutualFriends, ResultCode>>>)(a => UsersService.Instance.GetFriendsAndMutual(this._uid, a)));
            this._helperGetCurrentUserFriends = new AsyncHelper<BackendResult<List<User>, ResultCode>>((Action<Action<BackendResult<List<User>, ResultCode>>>)(a => UsersService.Instance.GetFriendsWithRequests( ( Action<BackendResult<AllFriendsList, ResultCode>>) (async res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    FriendsCache.Instance.SetFriends(res.ResultData.friends, res.ResultData.requests);
                    await ContactsManager.Instance.SyncContactsAsync(res.ResultData.friends);
                    this.RequestsViewModel = res.ResultData.requests;
                    CountersManager.Current.Counters.friends = this.RequestsViewModel.menu_counter;
                    EventAggregator.Current.Publish(new CountersChanged(CountersManager.Current.Counters));
                }
                Action<BackendResult<List<User>, ResultCode>> action = a;
                BackendResult<List<User>, ResultCode> backendResult = new BackendResult<List<User>, ResultCode>();
                backendResult.Error = res.Error;
                backendResult.ResultCode = res.ResultCode;
                AllFriendsList resultData = res.ResultData;
                List<User> userList = resultData != null ? resultData.friends : null;
                backendResult.ResultData = userList;
                action(backendResult);
            }))));
            this._helperFriendsFromList = new AsyncHelper<BackendResult<List<User>, ResultCode>>((Action<Action<BackendResult<List<User>, ResultCode>>>)(a => UsersService.Instance.GetFriendsForList(this._lid, a)));
            this._allFriendsVM = new GenericCollectionViewModel<List<User>, Group<FriendHeader>>((ICollectionDataProvider<List<User>, Group<FriendHeader>>)this)
            {
                NeedCollectionCountBeforeFullyLoading = true,
                RecreateCollectionOnRefresh = true
            };
            this._onlineFriendsVM = new GenericCollectionViewModel<List<User>, FriendHeader>((ICollectionDataProvider<List<User>, FriendHeader>)this)
            {
                NeedCollectionCountBeforeFullyLoading = true
            };
            this._commonFriendsVM = new GenericCollectionViewModel<List<User>, FriendHeader>((ICollectionDataProvider<List<User>, FriendHeader>)this)
            {
                NeedCollectionCountBeforeFullyLoading = true
            };
            this._friendsListVM = new GenericCollectionViewModel<List<FriendsList>, FriendHeader>((ICollectionDataProvider<List<FriendsList>, FriendHeader>)this)
            {
                NeedCollectionCountBeforeFullyLoading = true
            };
            if (this._lid != 0L)
                return;
            this._allFriendsVM.NoContentText = CommonResources.NoContent_Friends;
            this._allFriendsVM.NoContentImage = "../Resources/NoContentImages/Friends.png";
        }

        public void LoadFriends()
        {
            this._allFriendsVM.LoadData(false, false, null, false);
        }

        public void RefreshFriends(bool suppressLoadingMessage = true)
        {
            this._allFriendsVM.LoadData(true, suppressLoadingMessage, null, false);
            this._onlineFriendsVM.LoadData(true, suppressLoadingMessage, null, false);
        }

        internal void RemoveFriendFromList(long uid)
        {
            List<User> allFriendsRaw = this.AllFriendsRaw;
            User user = (User)Enumerable.FirstOrDefault<User>(allFriendsRaw, (Func<User, bool>)(u => u.uid == uid));
            if (user == null)
                return;
            allFriendsRaw.Remove(user);
            UsersService.Instance.EditList(this._lid, this._name ?? "", (List<long>)Enumerable.ToList<long>(Enumerable.Select<User, long>(allFriendsRaw, (Func<User, long>)(u => u.uid))), (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.RefreshFriends(true);
            }));
        }

        public void Handle(CountersChanged message)
        {
            // ISSUE: method reference
            this.NotifyPropertyChanged<FriendRequests>(() => this.RequestsViewModel);
            // ISSUE: method reference
            this.NotifyPropertyChanged<Visibility>(() => this.RequestsBlockVisibility);
        }

        public void Handle(FriendRemoved message)
        {
            if (this._uid == AppGlobalStateManager.Current.LoggedInUserId)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    this._allFriendsVM.LoadData(true, false, null, false);
                    this._onlineFriendsVM.LoadData(true, false, null, false);
                });
            }
        }

        public async void GetData(GenericCollectionViewModel<List<User>, Group<FriendHeader>> caller, int offset, int count, Action<BackendResult<List<User>, ResultCode>> callback)
        {
            if (this.OwnFriends)
            {
                switch (this._mode)
                {
                    case FriendsViewModel.Mode.Friends:
                        bool isCachedFriendsLoaded = false;
                        SavedContacts friends = (SavedContacts)await FriendsCache.Instance.GetFriends();
                        if (!this._allFriendsVM.Refresh && (DateTime.UtcNow - friends.SyncedDate).TotalMinutes < 60.0)
                        {
                            callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded, friends.SavedUsers));
                            this.RequestsViewModel = friends.Requests;
                            isCachedFriendsLoaded = true;
                        }
                        this._helperGetCurrentUserFriends.RunAction((Action<BackendResult<List<User>, ResultCode>>)(result =>
                        {
                            if (isCachedFriendsLoaded)
                                return;
                            callback(result);
                        }), this._getAllFriendsCalled);
                        break;
                    case FriendsViewModel.Mode.Lists:
                        this._helperFriendsFromList.RunAction(callback, this._getAllFriendsCalled);
                        break;
                }
            }
            else
                this._helperGetFriendsAndMutual.RunAction((Action<BackendResult<FriendsAndMutualFriends, ResultCode>>)(backendResult =>
                {
                    if (backendResult.ResultCode == ResultCode.Succeeded)
                        callback(new BackendResult<List<User>, ResultCode>()
                        {
                            ResultData = backendResult.ResultData.friends,
                            ResultCode = ResultCode.Succeeded
                        });
                    else
                        callback(new BackendResult<List<User>, ResultCode>(backendResult.ResultCode));
                }), this._getAllFriendsCalled);
            this._getAllFriendsCalled = true;
        }

        public string GetFooterTextForCountFriends(int count)
        {
            if (count <= 0)
                return CommonResources.NoFriends;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, true, null, false);
        }

        public void GetData(GenericCollectionViewModel<List<User>, FriendHeader> caller, int offset, int count, Action<BackendResult<List<User>, ResultCode>> callback)
        {
            if (caller == this._onlineFriendsVM)
            {
                if (this.OwnFriends)
                {
                    Action<BackendResult<List<User>, ResultCode>> callback1 = (Action<BackendResult<List<User>, ResultCode>>)(backendResult =>
                    {
                        if (backendResult.ResultCode == ResultCode.Succeeded)
                            callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded)
                            {
                                ResultData = (List<User>)Enumerable.ToList<User>(Enumerable.Where<User>(backendResult.ResultData, (Func<User, bool>)(u => u.online == 1)))
                            });
                        else
                            callback(backendResult);
                    });
                    switch (this._mode)
                    {
                        case FriendsViewModel.Mode.Friends:
                            this._helperGetCurrentUserFriends.RunAction(callback1, this._getOnlineCalled);
                            break;
                        case FriendsViewModel.Mode.Lists:
                            this._helperFriendsFromList.RunAction(callback1, this._getOnlineCalled);
                            break;
                    }
                }
                else
                    this._helperGetFriendsAndMutual.RunAction((Action<BackendResult<FriendsAndMutualFriends, ResultCode>>)(backendResult =>
                    {
                        if (backendResult.ResultCode == ResultCode.Succeeded)
                            callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded)
                            {
                                ResultData = (List<User>)Enumerable.ToList<User>(Enumerable.Where<User>(backendResult.ResultData.friends, (Func<User, bool>)(u => u.online == 1)))
                            });
                        else
                            callback(new BackendResult<List<User>, ResultCode>(backendResult.ResultCode));
                    }), this._getOnlineCalled);
                this._getOnlineCalled = true;
            }
            else
            {
                if (caller != this._commonFriendsVM)
                    return;
                this._helperGetFriendsAndMutual.RunAction((Action<BackendResult<FriendsAndMutualFriends, ResultCode>>)(backendResult =>
                {
                    if (backendResult.ResultCode == ResultCode.Succeeded)
                    {
                        ILookup<long, User> lookup = (ILookup<long, User>)Enumerable.ToLookup<User, long>(backendResult.ResultData.friends, (Func<User, long>)(u => u.uid));
                        List<User> userList = new List<User>();
                        List<long>.Enumerator enumerator = backendResult.ResultData.mutualFriends.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                long current = enumerator.Current;
                                User user = lookup[current].FirstOrDefault<User>();
                                if (user != null)
                                    userList.Add(user);
                            }
                        }
                        finally
                        {
                            enumerator.Dispose();
                        }
                        callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded)
                        {
                            ResultData = userList
                        });
                    }
                    else
                        callback(new BackendResult<List<User>, ResultCode>(backendResult.ResultCode));
                }), this._getCommonCalled);
                this._getCommonCalled = true;
            }
        }

        public void GetData(GenericCollectionViewModel<List<FriendsList>, FriendHeader> caller, int offset, int count, Action<BackendResult<List<FriendsList>, ResultCode>> callback)
        {
            UsersService.Instance.GetLists(callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, Group<FriendHeader>> caller, int count)
        {
            return this.GetFooterTextForCountFriends(count);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, FriendHeader> caller, int count)
        {
            return this.GetFooterTextForCountFriends(count);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<FriendsList>, FriendHeader> caller, int count)
        {
            return FriendsViewModel.GetListsCountStr(count);
        }

        private static string GetListsCountStr(int count)
        {
            if (count <= 0)
                return CommonResources.NoFriendsLists;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendListFrm, CommonResources.TwoFourFriendsListsFrm, CommonResources.FiveFriendsListsFrm, true, null, false);
        }

        public void Handle(FriendRequestAcceptedDeclined message)
        {
            if (!this.OwnFriends || !message.Accepted)
                return;
            this._allFriendsVM.LoadData(true, false, null, false);
            this._onlineFriendsVM.LoadData(true, false, null, false);
        }

        public enum Mode
        {
            Friends,
            Lists,
        }
    }
}
