using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class EditPrivacyViewModel : ViewModelBase, IHandle<FinishedLoadingFriendsAndListsCacheEvent>, IHandle
    {
        private ObservableCollection<Group<FriendHeader>> _allowedDeniedCollection = new ObservableCollection<Group<FriendHeader>>();
        private string _key = "";
        private List<string> _supportedValues = new List<string>();
        private static FriendsAndLists _friendsAndListsCached;
        private static long _friendsAndListsCachedOwnerId;
        //private string p;
        private PrivacyInfo _inputPrivacyInfo;
        private string _privacyQuestion;
        private PrivacyType _privacyType;
        private Action<PrivacyInfo> _saveCallback;
        private PrivacyType _previousPrivacyType;
        private static bool _isLoadingCache;
        private int _idOfGroupForUsersPick;
        //private EditPrivacyViewModel editPrivacyViewModel;
        //private Action<PrivacyInfo> action;

        public static FriendsAndLists FriendsAndListsCached
        {
            get
            {
                return EditPrivacyViewModel._friendsAndListsCached;
            }
        }

        public string PrivacyQuestion
        {
            get
            {
                return this._privacyQuestion;
            }
            set
            {
                this._privacyQuestion = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.PrivacyQuestion));
            }
        }

        public string PrivacyQuestionLowerCase
        {
            get
            {
                if (this._privacyQuestion == null)
                    return "";
                return this._privacyQuestion.ToLowerInvariant();
            }
        }

        public string Key
        {
            get
            {
                return this._key;
            }
        }

        public ObservableCollection<Group<FriendHeader>> AllowedDeniedCollection
        {
            get
            {
                return this._allowedDeniedCollection;
            }
        }

        public PrivacyType PrivacyType
        {
            get
            {
                return this._privacyType;
            }
            set
            {
                if (this._privacyType == value)
                    return;
                this._previousPrivacyType = this._privacyType;
                this._privacyType = value;
                this.GenerateAllowedDenied();
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.AllUsers));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FriendsOfFriends));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.OnlyMe));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FriendsOfFriendsOnly));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.Friends));
                this.Save();
            }
        }

        public bool AllUsers
        {
            get
            {
                return this.PrivacyType == PrivacyType.AllUsers;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.AllUsers;
            }
        }

        public Visibility AllUsersVisibility
        {
            get
            {
                return this.CheckSupported("all");
            }
        }

        public bool Friends
        {
            get
            {
                return this.PrivacyType == PrivacyType.Friends;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.Friends;
            }
        }

        public Visibility FriendsVisibility
        {
            get
            {
                return this.CheckSupported("friends");
            }
        }

        public bool FriendsOfFriends
        {
            get
            {
                return this.PrivacyType == PrivacyType.FriendsOfFriends;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.FriendsOfFriends;
            }
        }

        public Visibility FriendsOfFriendsVisibility
        {
            get
            {
                return this.CheckSupported("friends_of_friends");
            }
        }

        public bool FriendsOfFriendsOnly
        {
            get
            {
                return this.PrivacyType == PrivacyType.FriendsOfFriendsOnly;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.FriendsOfFriendsOnly;
            }
        }

        public Visibility FriendsOfFriendsOnlyVisibility
        {
            get
            {
                return this._supportedValues == null || !this._supportedValues.Contains("friends_of_friends_only") ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool OnlyMe
        {
            get
            {
                return this.PrivacyType == PrivacyType.OnlyMe;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.OnlyMe;
            }
        }

        public Visibility OnlyMeVisibility
        {
            get
            {
                return this.CheckSupported("only_me");
            }
        }

        public bool Nobody
        {
            get
            {
                return this.PrivacyType == PrivacyType.Nobody;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.Nobody;
            }
        }

        public Visibility NobodyVisibility
        {
            get
            {
                return this._supportedValues == null || !this._supportedValues.Contains("nobody") ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool CertainUsers
        {
            get
            {
                return this.PrivacyType == PrivacyType.CertainUsers;
            }
            set
            {
                if (!value)
                    return;
                this.PrivacyType = PrivacyType.CertainUsers;
                Group<FriendHeader> g = this.AllowedDeniedCollection.First<Group<FriendHeader>>();
                if (g.Count != 0)
                    return;
                this.InitiatePickUsersFor(g);
            }
        }

        public Visibility CertainUsersVisibility
        {
            get
            {
                return this.CheckSupported("some");
            }
        }

        public bool DenyIsApplicable
        {
            get
            {
                if (this.PrivacyType != PrivacyType.OnlyMe)
                    return this.CertainUsersVisibility == Visibility.Visible;
                return false;
            }
        }

        public string UserFriendlyDesc
        {
            get
            {
                return this.BuildUserFriendlyDesc();
            }
        }

        public bool IsOKState
        {
            get
            {
                if (this.PrivacyType == PrivacyType.CertainUsers)
                {
                    ObservableCollection<Group<FriendHeader>> source = this._allowedDeniedCollection;
                    Func<Group<FriendHeader>, bool> predicate = (Func<Group<FriendHeader>, bool>)(g => g.Id == 0);
                    //Func<Group<FriendHeader>, bool> predicate = null;
                    if (!source.FirstOrDefault<Group<FriendHeader>>(predicate).Any<FriendHeader>())
                        return false;
                }
                return true;
            }
        }

        private bool IsCachedDataReady
        {
            get
            {
                bool flag = EditPrivacyViewModel._friendsAndListsCached == null || EditPrivacyViewModel._friendsAndListsCachedOwnerId != AppGlobalStateManager.Current.LoggedInUserId;
                if (!flag)
                {
                    IEnumerable<long> parentList1 = EditPrivacyViewModel._friendsAndListsCached.friends.Select<User, long>((Func<User, long>)(u => u.id));
                    IEnumerable<long> parentList2 = EditPrivacyViewModel._friendsAndListsCached.friendLists.Select<FriendsList, long>((Func<FriendsList, long>)(l => l.id));
                    flag = !ListUtils.ListContainsAllOfAnother<long>(parentList1, (IEnumerable<long>)this._inputPrivacyInfo.AllowedUsers) || !ListUtils.ListContainsAllOfAnother<long>(parentList1, (IEnumerable<long>)this._inputPrivacyInfo.DeniedUsers) || !ListUtils.ListContainsAllOfAnother<long>(parentList2, (IEnumerable<long>)this._inputPrivacyInfo.AllowedLists) || !ListUtils.ListContainsAllOfAnother<long>(parentList2, (IEnumerable<long>)this._inputPrivacyInfo.DeniedLists);
                }
                return !flag;
            }
        }

        public EditPrivacyViewModel(EditPrivacyViewModel vm, Action<PrivacyInfo> saveCallback)
        {
            this.PrivacyQuestion = vm.PrivacyQuestion;
            this._inputPrivacyInfo = new PrivacyInfo(vm._inputPrivacyInfo.ToString());
            this._key = vm.Key;
            this._supportedValues = vm._supportedValues;
            this._privacyType = this._inputPrivacyInfo.PrivacyType;
            this._saveCallback = saveCallback;
            this.GenerateAllowedDenied();
            EventAggregator.Current.Subscribe((object)this);
        }

        public EditPrivacyViewModel(string title, PrivacyInfo privacyInfo, string key = "", List<string> supportedValues = null)
        {
            this.PrivacyQuestion = title;
            this.ReadFromPrivacyInfo(privacyInfo);
            this._key = key;
            if (supportedValues != null)
                this._supportedValues = supportedValues;
            EventAggregator.Current.Subscribe((object)this);
        }

        public Visibility CheckSupported(string privacyTypeStr)
        {
            return (this._supportedValues.IsNullOrEmpty() ? 1 : (this._supportedValues.Contains(privacyTypeStr) ? 1 : 0)) == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private string BuildUserFriendlyDesc()
        {
            string str1 = "";
            switch (this.PrivacyType)
            {
                case PrivacyType.AllUsers:
                    str1 = CommonResources.Privacy_AllUsers;
                    break;
                case PrivacyType.Friends:
                    str1 = CommonResources.Privacy_Friends;
                    break;
                case PrivacyType.FriendsOfFriends:
                    str1 = CommonResources.Privacy_FriendsOfFriends;
                    break;
                case PrivacyType.OnlyMe:
                    str1 = CommonResources.Privacy_OnlyMe;
                    break;
                case PrivacyType.FriendsOfFriendsOnly:
                    str1 = CommonResources.Privacy_FriendsOfFriends;
                    break;
                case PrivacyType.Nobody:
                    str1 = CommonResources.Privacy_Nobody;
                    break;
            }
            string lowerInvariant = str1.ToLowerInvariant();
            string str2 = "";
            Group<FriendHeader> source1 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 0));
            if (source1 != null)
                str2 = source1.Select<FriendHeader, string>((Func<FriendHeader, string>)(fh => fh.FullName)).ToList<string>().GetCommaSeparated(", ");
            string str3 = "";
            Group<FriendHeader> source2 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 1));
            if (source2 != null && source2.Any<FriendHeader>())
                str3 = CommonResources.Privacy_Excluding + " " + source2.Select<FriendHeader, string>((Func<FriendHeader, string>)(fr => fr.FullNameGen)).ToList<string>().GetCommaSeparated(", ");
            if (str3 != "")
            {
                if (lowerInvariant != "")
                    lowerInvariant += ",";
                if (str2 != "")
                    str2 += ",";
            }
            return string.Join(" ", new List<string>()
      {
        lowerInvariant,
        str2,
        str3
      }.Where<string>((Func<string, bool>)(s => !string.IsNullOrEmpty(s))));
        }

        public void ReadFromPrivacyInfo(PrivacyInfo privacyInfo)
        {
            this._allowedDeniedCollection.Clear();
            this._inputPrivacyInfo = privacyInfo;
            this._privacyType = privacyInfo.PrivacyType;
            this.GenerateAllowedDenied();
        }

        public void Handle(FinishedLoadingFriendsAndListsCacheEvent message)
        {
            if (!message.Success)
                return;
            this.LoadAllowedDenied();
        }

        private void GenerateAllowedDenied()
        {
            if (!this.IsCachedDataReady)
                EditPrivacyViewModel.PrepareCache();
            else
                this.LoadAllowedDenied();
        }

        private void LoadAllowedDenied()
        {
            this._inputPrivacyInfo = this.ReadCurrentStateIntoPrivacyInfo();
            this._allowedDeniedCollection.Clear();
            if (this.PrivacyType == PrivacyType.CertainUsers)
            {
                Group<FriendHeader> group = new Group<FriendHeader>("", false);
                group.Id = 0;
                this.AddListsAndUsersToGroup(group, this._inputPrivacyInfo.AllowedLists, this._inputPrivacyInfo.AllowedUsers);
                this._allowedDeniedCollection.Add(group);
            }
            if (this.DenyIsApplicable)
            {
                Group<FriendHeader> group = new Group<FriendHeader>(CommonResources.Privacy_DeniedTo, false);
                group.Id = 1;
                this.AddListsAndUsersToGroup(group, this._inputPrivacyInfo.DeniedLists, this._inputPrivacyInfo.DeniedUsers);
                this._allowedDeniedCollection.Add(group);
            }
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UserFriendlyDesc));
        }

        private PrivacyInfo ReadCurrentStateIntoPrivacyInfo()
        {
            PrivacyInfo privacyInfo = new PrivacyInfo(this._inputPrivacyInfo.ToString());
            privacyInfo.PrivacyType = this.PrivacyType;
            Group<FriendHeader> source1 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 0));
            if (source1 != null)
            {
                privacyInfo.AllowedUsers = source1.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriend)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.UserId)).ToList<long>();
                privacyInfo.AllowedLists = source1.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriendList)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.FriendListId)).ToList<long>();
            }
            Group<FriendHeader> source2 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 1));
            if (source2 != null)
            {
                privacyInfo.DeniedUsers = source2.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriend)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.UserId)).ToList<long>();
                privacyInfo.DeniedLists = source2.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriendList)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.FriendListId)).ToList<long>();
            }
            return privacyInfo;
        }

        private void AddListsAndUsersToGroup(Group<FriendHeader> group, List<long> lists, List<long> users)
        {
            foreach (long list in lists)
            {
                long al_id = list;
                FriendsList cachedAllowedList = EditPrivacyViewModel._friendsAndListsCached.friendLists.FirstOrDefault<FriendsList>((Func<FriendsList, bool>)(l => l.id == al_id));
                if (cachedAllowedList != null)
                    group.Add(new FriendHeader(cachedAllowedList));
            }
            foreach (long user1 in users)
            {
                long al_uid = user1;
                User user2 = EditPrivacyViewModel._friendsAndListsCached.friends.FirstOrDefault<User>((Func<User, bool>)(f => f.id == al_uid));
                if (user2 != null)
                    group.Add(new FriendHeader(user2, false));
            }
        }

        private static void PrepareCache()
        {
            if (EditPrivacyViewModel._isLoadingCache)
                return;
            EditPrivacyViewModel._isLoadingCache = true;
            UsersService.Instance.GetFriendsAndLists((Action<BackendResult<FriendsAndLists, ResultCode>>)(res =>
            {
                bool flag = res.ResultCode == ResultCode.Succeeded;
                if (flag)
                {
                    EditPrivacyViewModel._friendsAndListsCached = res.ResultData;
                    EditPrivacyViewModel._friendsAndListsCachedOwnerId = AppGlobalStateManager.Current.LoggedInUserId;
                }
                EventAggregator.Current.Publish((object)new FinishedLoadingFriendsAndListsCacheEvent()
                {
                    Success = flag
                });
                EditPrivacyViewModel._isLoadingCache = false;
            }));
        }

        internal void InitiatePickUsersFor(Group<FriendHeader> g)
        {
            string customTitle = g.Id == 0 ? CommonResources.Privacy_AllowedTo : CommonResources.Privacy_DeniedTo;
            this._idOfGroupForUsersPick = g.Id;
            Navigator.Current.NavigateToPickUser(false, 0L, true, 0, PickUserMode.PickForPrivacy, customTitle, 0);
        }

        internal void HandlePickedUsers(List<User> pickedUsers, List<FriendsList> pickedLists)
        {
            Group<FriendHeader> source1 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == this._idOfGroupForUsersPick));
            Group<FriendHeader> source2 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id != this._idOfGroupForUsersPick));
            if (source1 != null)
            {
                if (!pickedUsers.IsNullOrEmpty())
                {
                    foreach (User pickedUser in pickedUsers)
                    {
                        User u = pickedUser;
                        if (!source1.Any<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.UserId == u.id)))
                            source1.Add(new FriendHeader(u, false));
                        if (source2 != null)
                        {
                            FriendHeader friendHeader = source2.FirstOrDefault<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.UserId == u.id));
                            if (friendHeader != null)
                                source2.Remove(friendHeader);
                        }
                    }
                }
                if (!pickedLists.IsNullOrEmpty())
                {
                    foreach (FriendsList pickedList in pickedLists)
                    {
                        FriendsList l = pickedList;
                        if (!source1.Any<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.FriendListId == l.id)))
                            source1.Add(new FriendHeader(l));
                        if (source2 != null)
                        {
                            FriendHeader friendHeader = source2.FirstOrDefault<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.FriendListId == l.id));
                            if (friendHeader != null)
                                source2.Remove(friendHeader);
                        }
                    }
                }
            }
            this.SetToPreviousIfNeeded();
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UserFriendlyDesc));
            this.GenerateAllowedDenied();
            this.Save();
        }

        internal void Remove(FriendHeader fh)
        {
            foreach (Collection<FriendHeader> allowedDenied in (Collection<Group<FriendHeader>>)this.AllowedDeniedCollection)
                allowedDenied.Remove(fh);
            this.SetToPreviousIfNeeded();
            this.GenerateAllowedDenied();
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UserFriendlyDesc));
            this.Save();
        }

        private void Save()
        {
            if (!this.IsOKState)
                return;
            this._saveCallback(this.GetAsPrivacyInfo());
        }

        private void SetToPreviousIfNeeded()
        {
            Group<FriendHeader> group = this.AllowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 0));
            this.AllowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 1));
            if (group == null || group.Count != 0 || !this.CertainUsers)
                return;
            this.PrivacyType = this._previousPrivacyType;
        }

        public PrivacyInfo GetAsPrivacyInfo()
        {
            PrivacyInfo privacyInfo = this.ReadCurrentStateIntoPrivacyInfo();
            privacyInfo.CleanupAllowedDeniedArraysBasedOnPrivacyType();
            return privacyInfo;
        }
    }
}
