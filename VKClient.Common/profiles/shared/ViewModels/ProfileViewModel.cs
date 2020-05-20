using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Groups.ViewModels;
using VKClient.Common.Profiles.Users.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Library;
using Windows.System;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
    public class ProfileViewModel : ViewModelBase, ICollectionDataProvider<WallData, IVirtualizable>, IHandle<WallPostDeleted>, IHandle, IHandle<WallPostAddedOrEdited>, IHandle<WallPostPinnedUnpinned>, IHandle<GroupMembershipStatusUpdated>, IHandle<FriendRequestAcceptedDeclined>, IHandle<FriendRemoved>, IHandle<FriendRequestSent>, IHandle<SubscriptionCancelled>, IHandle<BaseDataChangedEvent>, IHandle<CommunityLinkAddedOrEdited>, IHandle<CommunityLinkDeleted>, IHandle<CommunityManagerChanged>, IHandle<CommunityInformationChanged>, IHandle<CommunityServicesChanged>
    {
        private readonly List<long> _serviceUserIds = new List<long>() { 100L, 101L, 333L };
        private readonly ProfileMediaViewModelFacade _mediaViewModel = new ProfileMediaViewModelFacade();
        private string _nameHeader;
        private IProfileData _profileData;
        private UserData _userData;
        private GroupData _groupData;
        private bool _isLoading;
        private ProfileLoadingStatus _previousStatus;
        private ProfileLoadingStatus _status;
        private const long SERVICE_USER_ID_ADMINISTRATION = 100;
        private const long SERVICE_USER_ID_COMMUNITY_COMMENTS_BACKWARD_COMPATIBILITY = 101;
        private const long SERVICE_USER_ID_MOBILE_SUPPORT = 333;
        private ProfileHeaderViewModelBase _headerViewModel;
        private MembershipInfoBase _membershipViewModel;
        private ProfileInfoViewModelBase _infoViewModel;
        private bool _isProfileAppDataLoaded;
        private ProfileAppViewModel _appViewModel;
        private SuggestedPostponedPostsViewModel _suggestedPostponedViewModel;
        private AllProfilePostsToggleViewModel _postsToggleViewModel;
        private WallData _ownerWallDataPrefetched;
        private WallData _allWallDataPrefetched;
        private const string FILTER_OWNER = "owner";
        private const string FILTER_ALL = "all";
        private bool _isSaving;

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        public long Id { get; private set; }

        public string NameHeader
        {
            get
            {
                return this._nameHeader;
            }
            private set
            {
                this._nameHeader = value;
                this.NotifyPropertyChanged("NameHeader");
            }
        }

        public bool IsServiceProfile
        {
            get
            {
                return this._serviceUserIds.Contains(this.Id);
            }
        }

        public bool CanAnimateHeaderOpacity
        {
            get
            {
                if (this.Id <= 0L)
                    return this.HasCover;
                return true;
            }
        }

        private bool HasCover
        {
            get
            {
                GroupData groupData = this._groupData;
                string str;
                if (groupData == null)
                {
                    str = null;
                }
                else
                {
                    CoverImage coverImage = groupData.CoverImage;
                    str = coverImage != null ? coverImage.url : null;
                }
                return !string.IsNullOrEmpty(str);
            }
        }

        public ProfileHeaderViewModelBase HeaderViewModel
        {
            get
            {
                return this._headerViewModel;
            }
            private set
            {
                this._headerViewModel = value;
                this.NotifyPropertyChanged("HeaderViewModel");
                base.NotifyPropertyChanged<ProfileHeaderViewModelBase>(() => this.UserHeaderViewModel);
                base.NotifyPropertyChanged<ProfileHeaderViewModelBase>(() => this.GroupHeaderViewModel);
            }
        }

        public ProfileHeaderViewModelBase UserHeaderViewModel
        {
            get
            {
                return this._headerViewModel as UserProfileHeaderViewModel;
            }
        }

        public ProfileHeaderViewModelBase GroupHeaderViewModel
        {
            get
            {
                return this._headerViewModel as GroupProfileHeaderViewModel;
            }
        }

        public Visibility UserHeaderVisibility { get; private set; }

        public Visibility GroupHeaderVisibility { get; private set; }

        public string LoadingStatusText
        {
            get
            {
                switch (this._status)
                {
                    case ProfileLoadingStatus.Loading:
                        return CommonResources.Loading;
                    case ProfileLoadingStatus.LoadingFailed:
                        return CommonResources.FailedToConnectError;
                    default:
                        if (this._userData != null)
                        {
                            switch (this._status)
                            {
                                case ProfileLoadingStatus.Deleted:
                                    return string.Format("{0} {1}", CommonResources.UserDeleted, CommonResources.InformationUnavailable);
                                case ProfileLoadingStatus.Banned:
                                    return string.Format("{0} {1}", CommonResources.UserBanned, CommonResources.InformationUnavailable);
                                case ProfileLoadingStatus.Blacklisted:
                                    return string.Format("{0} {1}", string.Format(CommonResources.UserBlacklisted, this._userData.user.first_name), CommonResources.InformationUnavailable);
                                case ProfileLoadingStatus.Service:
                                    switch (this.Id)
                                    {
                                        case 101:
                                            return CommonResources.User101_Description;
                                        case 333:
                                            return CommonResources.User333_Description;
                                        default:
                                            return "";
                                    }
                            }
                        }
                        if (this._groupData != null)
                        {
                            switch (this._status)
                            {
                                case ProfileLoadingStatus.Deleted:
                                case ProfileLoadingStatus.Banned:
                                    return string.Format("{0} {1}", CommonResources.Group_Banned, CommonResources.InformationUnavailable);
                                case ProfileLoadingStatus.Blacklisted:
                                    return this.ComposeGroupBlacklistedText();
                                case ProfileLoadingStatus.Private:
                                    return CommonResources.Group_Private;
                            }
                        }
                        return "";
                }
            }
        }

        public Visibility LoadingStatusVisibility
        {
            get
            {
                if (this.User100DescriptionVisibility != Visibility.Visible)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility User100DescriptionVisibility
        {
            get
            {
                if (this._status != ProfileLoadingStatus.Loading && this._status != ProfileLoadingStatus.LoadingFailed && (this._userData != null && this.Id == 100L))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility ChoosePhotoMenuItemVisibility
        {
            get
            {
                if (this.Id >= 0L)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public MembershipInfoBase MembershipViewModel
        {
            get
            {
                return this._membershipViewModel;
            }
            private set
            {
                this._membershipViewModel = value;
                this.NotifyPropertyChanged("MembershipViewModel");
            }
        }

        private bool IsAllDataBlockerStatus
        {
            get
            {
                if (this._status != ProfileLoadingStatus.Loading && this._status != ProfileLoadingStatus.LoadingFailed && (this._previousStatus != ProfileLoadingStatus.Deleted && this._previousStatus != ProfileLoadingStatus.Banned))
                    return this._previousStatus == ProfileLoadingStatus.Blacklisted;
                return true;
            }
        }

        public Visibility MembershipVisibility
        {
            get
            {
                if (this.IsAllDataBlockerStatus || !this.CanShowMembership)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private bool CanShowMembership
        {
            get
            {
                if (this.Id > 0L)
                {
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Blacklisted:
                        case ProfileLoadingStatus.Private:
                        case ProfileLoadingStatus.Service:
                            return false;
                        default:
                            return true;
                    }
                }
                else
                {
                    if (this.Id >= 0L)
                        return false;
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Private:
                            return false;
                        default:
                            return true;
                    }
                }
            }
        }

        public ProfileInfoViewModelBase InfoViewModel
        {
            get
            {
                return this._infoViewModel;
            }
            private set
            {
                this._infoViewModel = value;
                this.NotifyPropertyChanged("InfoViewModel");
            }
        }

        public Visibility InfoVisibility
        {
            get
            {
                if (this.IsAllDataBlockerStatus || !this.CanShowInfo)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private bool CanShowInfo
        {
            get
            {
                if (this.Id > 0L)
                {
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Blacklisted:
                        case ProfileLoadingStatus.Private:
                        case ProfileLoadingStatus.Service:
                            return false;
                        default:
                            return true;
                    }
                }
                else
                {
                    if (this.Id >= 0L)
                        return false;
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Private:
                            return false;
                        default:
                            return true;
                    }
                }
            }
        }

        public ProfileAppViewModel AppViewModel
        {
            get
            {
                return this._appViewModel;
            }
            private set
            {
                this._appViewModel = value;
                this.NotifyPropertyChanged("AppViewModel");
            }
        }

        public Visibility AppVisibility
        {
            get
            {
                bool arg_39_0;
                if (!this.IsAllDataBlockerStatus && this.CanShowApp)
                {
                    GroupData expr_1B = this._profileData as GroupData;
                    object arg_34_0;
                    if (expr_1B == null)
                    {
                        arg_34_0 = null;
                    }
                    else
                    {
                        Group expr_27 = expr_1B.group;
                        arg_34_0 = ((expr_27 != null) ? expr_27.app_button : null);
                    }
                    arg_39_0 = (arg_34_0 != null);
                }
                else
                {
                    arg_39_0 = false;
                }
                return arg_39_0.ToVisiblity();
            }
        }

        private bool CanShowApp
        {
            get
            {
                if (this.Id > 0L || this.Id >= 0L)
                    return false;
                switch (this._status)
                {
                    case ProfileLoadingStatus.Deleted:
                    case ProfileLoadingStatus.Banned:
                    case ProfileLoadingStatus.Private:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public ProfileMediaViewModelFacade MediaViewModel
        {
            get
            {
                return this._mediaViewModel;
            }
        }

        public Visibility MediaVisibility
        {
            get
            {
                if (this.IsAllDataBlockerStatus || !this.CanShowMedia || !this._mediaViewModel.CanDisplay)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private bool CanShowMedia
        {
            get
            {
                if (this.Id > 0L)
                {
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Blacklisted:
                        case ProfileLoadingStatus.Private:
                        case ProfileLoadingStatus.Service:
                            return false;
                        default:
                            return true;
                    }
                }
                else
                {
                    if (this.Id >= 0L)
                        return false;
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Private:
                            return false;
                        default:
                            return true;
                    }
                }
            }
        }

        public SuggestedPostponedPostsViewModel SuggestedPostponedViewModel
        {
            get
            {
                return this._suggestedPostponedViewModel;
            }
            private set
            {
                this._suggestedPostponedViewModel = value;
                this.NotifyPropertyChanged("SuggestedPostponedViewModel");
            }
        }

        public Visibility SuggestedPostponedVisibility
        {
            get
            {
                if (this.IsAllDataBlockerStatus || !this.CanShowSuggestedPostponed || (this._suggestedPostponedViewModel == null || !this._suggestedPostponedViewModel.CanDisplay))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private bool CanShowSuggestedPostponed
        {
            get
            {
                switch (this._status)
                {
                    case ProfileLoadingStatus.Deleted:
                    case ProfileLoadingStatus.Banned:
                    case ProfileLoadingStatus.Blacklisted:
                    case ProfileLoadingStatus.Private:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public AllProfilePostsToggleViewModel PostsToggleViewModel
        {
            get
            {
                return this._postsToggleViewModel;
            }
            private set
            {
                this._postsToggleViewModel = value;
                this.NotifyPropertyChanged("PostsToggleViewModel");
            }
        }

        public Visibility PostsVisibility
        {
            get
            {
                if (this.IsAllDataBlockerStatus || !this.CanShowPosts)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility PostsNotLoadingVisibility
        {
            get
            {
                if ((this._allWallDataPrefetched == null || this._allWallDataPrefetched.TotalCount <= 0) && (this._ownerWallDataPrefetched == null || this._ownerWallDataPrefetched.TotalCount <= 0))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private bool CanShowPosts
        {
            get
            {
                if (this.Id > 0L)
                {
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Blacklisted:
                        case ProfileLoadingStatus.Private:
                        case ProfileLoadingStatus.Service:
                            return false;
                        default:
                            return true;
                    }
                }
                else
                {
                    if (this.Id >= 0L)
                        return false;
                    switch (this._status)
                    {
                        case ProfileLoadingStatus.Deleted:
                        case ProfileLoadingStatus.Banned:
                        case ProfileLoadingStatus.Private:
                            return false;
                        default:
                            return true;
                    }
                }
            }
        }

        public bool CanPost
        {
            get
            {
                if (this._profileData != null && this._profileData.CanPost && !this.IsServiceProfile)
                    return !this._profileData.IsDeactivated;
                return false;
            }
        }

        public bool CanSuggestAPost
        {
            get
            {
                if (this._profileData != null && this._profileData.CanSuggestAPost && !this.IsServiceProfile)
                    return !this._profileData.IsDeactivated;
                return false;
            }
        }

        public bool CanSendGift
        {
            get
            {
                return this._profileData != null && this.Id != AppGlobalStateManager.Current.LoggedInUserId && (!this.IsServiceProfile && this.Id >= 0L) && (this._userData != null && this._userData.user.blacklisted != 1 && !this._profileData.IsDeactivated);
            }
        }

        public bool CanFaveUnfave
        {
            get
            {
                return this._profileData != null && this.Id != AppGlobalStateManager.Current.LoggedInUserId && !this.IsServiceProfile && (this.Id >= 0L || this._groupData == null || this._groupData.group.Privacy != GroupPrivacy.Private) && (!this._profileData.IsDeactivated || this._profileData.IsFavorite);
            }
        }

        public bool CanSubscribeUnsubscribe
        {
            get
            {
                if (this._profileData == null || this.Id == AppGlobalStateManager.Current.LoggedInUserId || this.IsServiceProfile)
                    return false;
                if (this.Id < 0L)
                {
                    if (this._groupData == null || !this._groupData.group.IsMember && this._groupData.group.Privacy != GroupPrivacy.Public)
                        return false;
                    BanInfo banInfo = this._groupData.group.ban_info;
                    if (banInfo != null && banInfo.end_date == 0L)
                        return false;
                }
                else if (this.Id > 0L && (this._userData == null || this._userData.user.blacklisted == 1))
                    return false;
                return !this._profileData.IsDeactivated;
            }
        }

        public bool CanBanUnban
        {
            get
            {
                if (this._profileData != null && this.Id > 0L && (this.Id != AppGlobalStateManager.Current.LoggedInUserId && !this.IsServiceProfile))
                    return !this._profileData.IsDeactivated;
                return false;
            }
        }

        public bool CanPinToStart
        {
            get
            {
                if (this._profileData != null && !this.IsTileExist() && (this._status != ProfileLoadingStatus.Banned && this._status != ProfileLoadingStatus.Deleted))
                    return !this.IsServiceProfile;
                return false;
            }
        }

        public bool CanUserUnsubscribe
        {
            get
            {
                if (this._profileData != null && this.Id > 0L && (this.Id != AppGlobalStateManager.Current.LoggedInUserId && this._profileData.areFriendsStatus == 1) && (this._status == ProfileLoadingStatus.Banned || this._status == ProfileLoadingStatus.Deleted))
                    return !this.IsServiceProfile;
                return false;
            }
        }

        public bool CanRemoveFromFriends
        {
            get
            {
                if (this._profileData != null && this.Id > 0L && (this.Id != AppGlobalStateManager.Current.LoggedInUserId && this._profileData.areFriendsStatus == 3) && (this._status == ProfileLoadingStatus.Banned || this._status == ProfileLoadingStatus.Deleted))
                    return !this.IsServiceProfile;
                return false;
            }
        }

        public bool CanEditProfile
        {
            get
            {
                return this.Id == AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public bool IsAdministrator
        {
            get
            {
                GroupData groupData = this._groupData;
                if (groupData == null)
                    return false;
                Group group = groupData.group;
                int? nullable = group != null ? new int?(group.admin_level) : new int?();
                int num = 3;
                if (nullable.GetValueOrDefault() < num)
                    return false;
                return nullable.HasValue;
            }
        }

        public bool CanManageCommunity
        {
            get
            {
                GroupData groupData = this._groupData;
                int num1;
                if (groupData == null)
                {
                    num1 = 0;
                }
                else
                {
                    Group group = groupData.group;
                    int? nullable = group != null ? new int?(group.admin_level) : new int?();
                    int num2 = 0;
                    num1 = nullable.GetValueOrDefault() > num2 ? (nullable.HasValue ? 1 : 0) : 0;
                }
                if (num1 != 0 && this._status != ProfileLoadingStatus.Banned && this._status != ProfileLoadingStatus.Deleted)
                    return this._status != ProfileLoadingStatus.Blacklisted;
                return false;
            }
        }

        public GenericCollectionViewModel<WallData, IVirtualizable> WallVM { get; private set; }

        public Visibility StretchingRectVisibility
        {
            get
            {
                if (this.Id <= 0L)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Func<WallData, ListWithCount<IVirtualizable>> ConverterFunc
        {
            get
            {
                return (Func<WallData, ListWithCount<IVirtualizable>>)(wallData =>
                {
                    ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
                    List<IVirtualizable> virtualizableList = WallPostItemsGenerator.Generate(wallData.wall, wallData.profiles, wallData.groups, new Action<WallPostItem>(this.DeletedCallback), 0.0);
                    int totalCount = wallData.TotalCount;
                    listWithCount.TotalCount = totalCount;
                    listWithCount.List.AddRange((IEnumerable<IVirtualizable>)virtualizableList);
                    return listWithCount;
                });
            }
        }

        public bool IsBlacklistedByMe
        {
            get
            {
                if (this._userData != null)
                    return this._userData.user.blacklisted_by_me == 1;
                return false;
            }
            set
            {
                if (this._userData == null)
                    return;
                this._userData.user.blacklisted_by_me = value ? 1 : 0;
                Action barPropertyUpdated = this.AppBarPropertyUpdated;
                if (barPropertyUpdated == null)
                    return;
                barPropertyUpdated();
            }
        }

        public bool IsFavorite
        {
            get
            {
                if (this._profileData != null)
                    return this._profileData.IsFavorite;
                return false;
            }
            set
            {
                if (this._profileData == null)
                    return;
                this._profileData.IsFavorite = value;
                Action barPropertyUpdated = this.AppBarPropertyUpdated;
                if (barPropertyUpdated == null)
                    return;
                barPropertyUpdated();
            }
        }

        public bool IsSubscribed
        {
            get
            {
                if (this._profileData != null)
                    return this._profileData.IsSubscribed;
                return false;
            }
            set
            {
                if (this._profileData == null)
                    return;
                this._profileData.IsSubscribed = value;
                Action barPropertyUpdated = this.AppBarPropertyUpdated;
                if (barPropertyUpdated == null)
                    return;
                barPropertyUpdated();
            }
        }

        public Action AppBarPropertyUpdated { get; set; }

        public bool CanChangePhoto
        {
            get
            {
                if (this._profileData != null)
                    return this._profileData.AdminLevel > 1;
                return false;
            }
        }

        public Group CommunityModel
        {
            get
            {
                return this._groupData.group;
            }
        }

        public ProfileViewModel(long id, string name = "", string source = "")
        {
            this.Id = id;
            this._nameHeader = !string.IsNullOrEmpty(name) ? name.ToUpperInvariant() : "";
            if (this.Id > 0)
            {
                this.GroupHeaderVisibility = Visibility.Collapsed;
                this.HeaderViewModel = (ProfileHeaderViewModelBase)new UserProfileHeaderViewModel(name);
                EventAggregator current = EventAggregator.Current;
                OpenUserEvent openUserEvent = new OpenUserEvent();
                openUserEvent.UserId = id;
                string str = source != "" ? source : null;
                openUserEvent.Source = str;
                current.Publish(openUserEvent);
            }
            else
            {
                this.UserHeaderVisibility = Visibility.Collapsed;
                this.HeaderViewModel = (ProfileHeaderViewModelBase)new GroupProfileHeaderViewModel(name);
                EventAggregator.Current.Publish(new OpenGroupEvent()
                {
                    GroupId = -id,
                    Source = (source != "" ? source : null)
                });
            }
            this.WallVM = new GenericCollectionViewModel<WallData, IVirtualizable>(this)
            {
                LoadCount = 10,
                ReloadCount = 20
            };
            EventAggregator.Current.Subscribe(this);
        }

        public void LoadInfo(bool preloaded = false)
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            if (preloaded)
            {
                base.SetInProgress(true, "");
                this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Reloading);
            }
            else
                this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Loading);
            if (this.Id > 0L)
                UsersService.Instance.GetProfileInfo(this.Id, (Action<BackendResult<UserData, ResultCode>>)(res => this.HandleProfileDataLoaded(res.ResultCode, (IProfileData)res.ResultData)));
            else
                GroupsService.Current.GetGroupInfo(-this.Id, (Action<BackendResult<GroupData, ResultCode>>)(res => this.HandleProfileDataLoaded(res.ResultCode, (IProfileData)res.ResultData)));
        }

        private string ComposeGroupBlacklistedText()
        {
            BanInfo banInfo = this._groupData.group.ban_info;
            if (banInfo == null)
                return CommonResources.Group_Blacklisted;
            string str = "";
            if (banInfo.reason > 0)
            {
                string reasonById = ProfileViewModel.GetReasonById(banInfo.reason);
                if (!string.IsNullOrEmpty(reasonById))
                    str += string.Format("{0}: {1}", CommonResources.Group_BanReason, reasonById);
            }
            if (banInfo.end_date > 0L)
            {
                DateTime dateTime = Extensions.UnixTimeStampToDateTime((double)banInfo.end_date, true);
                if (!string.IsNullOrEmpty(str))
                    str += "\n";
                str += string.Format("{0}: {1}", CommonResources.Group_Blacklisted_BlockedTill, UIStringFormatterHelper.FormateDateForEventUI(dateTime));
            }
            if (!string.IsNullOrEmpty(banInfo.comment))
            {
                if (!string.IsNullOrEmpty(str))
                    str += "\n";
                str += string.Format("{0}: {1}", CommonResources.Group_Blacklisted_Comment, banInfo.comment);
            }
            return str.Insert(0, CommonResources.Group_Blacklisted + (!string.IsNullOrEmpty(str) ? "\n\n" : ""));
        }

        private static string GetReasonById(int reasonId)
        {
            switch (reasonId)
            {
                case 1:
                    return CommonResources.Group_BanReason_Spam;
                case 2:
                    return CommonResources.Group_BanReason_VerbalAbuse;
                case 3:
                    return CommonResources.Group_BanReason_StrongLanguage;
                case 4:
                    return CommonResources.Group_BanReason_IrrelevantMessages;
                default:
                    return "";
            }
        }

        private bool IsTileExist()
        {
            return SecondaryTileManager.Instance.TileExistsFor(this.Id, this.Id < 0L);
        }

        private void UpdateProfileLoadingStatus(ProfileLoadingStatus status)
        {
            this._previousStatus = this._status;
            this._status = status;
            base.NotifyPropertyChanged<string>(() => this.LoadingStatusText);
            base.NotifyPropertyChanged<Visibility>(() => this.User100DescriptionVisibility);
            base.NotifyPropertyChanged<Visibility>(() => this.LoadingStatusVisibility);
            this.NotifyProfileListItemsProperties();
            Action<ProfileLoadingStatus> loadingStatusUpdated = this.LoadingStatusUpdated;
            if (loadingStatusUpdated == null)
                return;
            int status1 = (int)this._status;
            loadingStatusUpdated((ProfileLoadingStatus)status1);
        }

        private void NotifyProfileListItemsProperties()
        {
            base.NotifyPropertyChanged<Visibility>(() => this.MembershipVisibility);
            base.NotifyPropertyChanged<Visibility>(() => this.InfoVisibility);
            base.NotifyPropertyChanged<Visibility>(() => this.AppVisibility);
            base.NotifyPropertyChanged<Visibility>(() => this.MediaVisibility);
            base.NotifyPropertyChanged<Visibility>(() => this.SuggestedPostponedVisibility);
            this.NotifyPostsVisibility();
        }

        private void HandleProfileDataLoaded(ResultCode resultCode, IProfileData profileData)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (resultCode == ResultCode.Succeeded)
                {
                    this._profileData = profileData;
                    this._userData = this._profileData as UserData;
                    this._groupData = this._profileData as GroupData;
                    this.ProcessData();
                    this.ParseLoadingStatus();
                    Action barPropertyUpdated = this.AppBarPropertyUpdated;
                    if (barPropertyUpdated != null)
                        barPropertyUpdated();
                    this._isLoading = false;
                    this.SetInProgress(false, "");
                    this.LoadWall(true, null);
                }
                else
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.LoadingFailed);
                    this._isLoading = false;
                    this.SetInProgress(false, "");
                }
            }));
        }

        private void ParseLoadingStatus()
        {
            if (this._userData != null)
            {
                User user = this._userData.user;
                if (this._serviceUserIds.Contains(user.id))
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Service);
                    return;
                }
                if (!string.IsNullOrEmpty(user.deactivated))
                {
                    string deactivated = user.deactivated;
                    if (!(deactivated == "deleted"))
                    {
                        if (!(deactivated == "banned"))
                            return;
                        this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Banned);
                        return;
                    }
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Deleted);
                    return;
                }
                if (user.blacklisted == 1)
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Blacklisted);
                    return;
                }
            }
            else if (this._groupData != null)
            {
                Group group = this._groupData.group;
                if (!string.IsNullOrEmpty(group.deactivated))
                {
                    string deactivated = group.deactivated;
                    if (!(deactivated == "deleted"))
                    {
                        if (!(deactivated == "banned"))
                            return;
                        this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Banned);
                        return;
                    }
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Deleted);
                    return;
                }
                if (group.ban_info != null)
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Blacklisted);
                    return;
                }
                if (!group.IsMember && (group.Privacy == GroupPrivacy.Private || group.Privacy == GroupPrivacy.Closed && group.GroupType == GroupType.Event) && group.MembershipType != GroupMembershipType.InvitationReceived)
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Private);
                    return;
                }
            }
            this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Loaded);
        }

        private void ProcessData()
        {
            if (this._profileData == null)
                return;
            this.NameHeader = this._profileData.Name.ToUpperInvariant();
            if (this._userData != null)
            {
                this.HeaderViewModel = (ProfileHeaderViewModelBase)new UserProfileHeaderViewModel(this._userData);
                this.MembershipViewModel = (MembershipInfoBase)new UserMembershipInfo(this._userData);
                this.InfoViewModel = (ProfileInfoViewModelBase)new UserProfileInfoViewModel(this._userData);
            }
            else if (this._groupData != null)
            {
                this.HeaderViewModel = (ProfileHeaderViewModelBase)new GroupProfileHeaderViewModel(this._groupData);
                this.MembershipViewModel = (MembershipInfoBase)new GroupMembershipInfo(this._groupData);
                this.InfoViewModel = (ProfileInfoViewModelBase)new GroupProfileInfoViewModel(this._groupData);
                Group group = this._groupData.group;
                AppButton appButton = group != null ? group.app_button : null;
                if (appButton != null)
                {
                    this.AppViewModel = (ProfileAppViewModel)new GroupProfileAppViewModel(group.id, appButton);
                    if (!this._isProfileAppDataLoaded)
                        this._isProfileAppDataLoaded = true;
                }
            }
            this._mediaViewModel.Init(this._profileData);
            // ISSUE: method reference
            this.SuggestedPostponedViewModel = new SuggestedPostponedPostsViewModel(this._profileData)
            {
                UpdatedCallback = delegate
                {
                    Execute.ExecuteOnUIThread(delegate
                    {
                        base.NotifyPropertyChanged<Visibility>(() => this.SuggestedPostponedVisibility);
                    });
                }
            };
            AllProfilePostsToggleViewModel postsToggleViewModel = new AllProfilePostsToggleViewModel(this._profileData, true);
            int num = this._profileData.ShowAllPostsByDefault ? 1 : 0;
            postsToggleViewModel.IsAllPosts = num != 0;
            Action<bool> action = (Action<bool>)(isAllPosts => DelayedExecutorUtil.Execute((() => this.LoadWall(false, null)), 50));
            postsToggleViewModel.StateChangedCallback = action;
            this.PostsToggleViewModel = postsToggleViewModel;
        }

        private void LoadWall(bool deletePrefetchedData, Action callback = null)
        {
            this.LockPostsToggle(true);
            if (deletePrefetchedData)
            {
                this._allWallDataPrefetched = null;
                this._ownerWallDataPrefetched = null;
            }
            this.WallVM.LoadData(true, false, (Action<BackendResult<WallData, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.LockPostsToggle(false);
                this.NotifyPostsVisibility();
                Action action = callback;
                if (action == null)
                    return;
                action();
            }))), deletePrefetchedData);
        }

        public void GetData(GenericCollectionViewModel<WallData, IVirtualizable> caller, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
        {
            if (offset == 0)
            {
                if (this._postsToggleViewModel.IsAllPosts && this._allWallDataPrefetched != null)
                {
                    Action<BackendResult<WallData, ResultCode>> action = callback;
                    BackendResult<WallData, ResultCode> backendResult = new BackendResult<WallData, ResultCode>();
                    backendResult.ResultCode = ResultCode.Succeeded;
                    WallData wallDataPrefetched = this._allWallDataPrefetched;
                    backendResult.ResultData = wallDataPrefetched;
                    action(backendResult);
                    return;
                }
                if (!this._postsToggleViewModel.IsAllPosts && this._ownerWallDataPrefetched != null)
                {
                    Action<BackendResult<WallData, ResultCode>> action = callback;
                    BackendResult<WallData, ResultCode> backendResult = new BackendResult<WallData, ResultCode>();
                    backendResult.ResultCode = ResultCode.Succeeded;
                    WallData wallDataPrefetched = this._ownerWallDataPrefetched;
                    backendResult.ResultData = wallDataPrefetched;
                    action(backendResult);
                    return;
                }
            }
            this.LockPostsToggle(true);
            bool fetchAll = this._postsToggleViewModel.IsAllPosts;
            WallService.Current.GetWall(this.Id, offset, count, (Action<BackendResult<WallData, ResultCode>>)(response =>
            {
                this.LockPostsToggle(false);
                if (offset == 0 && response.ResultCode == ResultCode.Succeeded)
                {
                    if (fetchAll)
                        this._allWallDataPrefetched = response.ResultData;
                    else
                        this._ownerWallDataPrefetched = response.ResultData;
                }
                this.NotifyPostsVisibility();
                callback(response);
                if ((!fetchAll || this._ownerWallDataPrefetched != null) && (fetchAll || this._allWallDataPrefetched != null))
                    return;
                this.LockPostsToggle(true);
                WallService current = WallService.Current;
                long id = this.Id;
                int num1 = offset;
                int num2 = count;
                Action<BackendResult<WallData, ResultCode>> action1 = (Action<BackendResult<WallData, ResultCode>>)(res =>
                {
                    this.LockPostsToggle(false);
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        if (fetchAll)
                            this._ownerWallDataPrefetched = res.ResultData;
                        else
                            this._allWallDataPrefetched = res.ResultData;
                    }
                    this.NotifyPostsVisibility();
                });
                string str = fetchAll ? "owner" : "all";
                current.GetWall(id, num1, num2, action1, str);
            }), fetchAll ? "all" : "owner");
        }

        private void NotifyPostsVisibility()
        {
            base.NotifyPropertyChanged<Visibility>(() => this.PostsVisibility);
            base.NotifyPropertyChanged<Visibility>(() => this.PostsNotLoadingVisibility);
        }

        private void LockPostsToggle(bool isLocked)
        {
            if (this.PostsToggleViewModel == null)
                return;
            this.PostsToggleViewModel.IsLocked = isLocked;
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<WallData, IVirtualizable> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoWallPosts;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneWallPostFrm, CommonResources.TwoWallPostsFrm, CommonResources.FiveWallPostsFrm, true, null, false);
        }

        private void DeletedCallback(WallPostItem obj)
        {
            this.WallVM.Delete((IVirtualizable)obj);
            if (this._allWallDataPrefetched != null)
            {
                WallPost wallPost = (WallPost)Enumerable.FirstOrDefault<WallPost>(this._allWallDataPrefetched.wall, (Func<WallPost, bool>)(w =>
              {
                  if (w.id == obj.WallPost.id)
                      return w.to_id == obj.WallPost.to_id;
                  return false;
              }));
                if (wallPost != null)
                {
                    this._allWallDataPrefetched.wall.Remove(wallPost);
                    --this._allWallDataPrefetched.TotalCount;
                }
            }
            if (this._ownerWallDataPrefetched != null)
            {
                WallPost wallPost = (WallPost)Enumerable.FirstOrDefault<WallPost>(this._ownerWallDataPrefetched.wall, (Func<WallPost, bool>)(w =>
              {
                  if (w.id == obj.WallPost.id)
                      return w.to_id == obj.WallPost.to_id;
                  return false;
              }));
                if (wallPost != null)
                {
                    this._ownerWallDataPrefetched.wall.Remove(wallPost);
                    --this._ownerWallDataPrefetched.TotalCount;
                }
            }
            this.NotifyPostsVisibility();
        }

        public void Handle(WallPostDeleted message)
        {
            WallPostItem wallPostItem = Enumerable.FirstOrDefault<IVirtualizable>(this.WallVM.Collection, (Func<IVirtualizable, bool>)(w =>
          {
              if (w is WallPostItem && ((WallPostItem)w).WallPost.to_id == message.WallPost.to_id)
                  return ((WallPostItem)w).WallPost.id == message.WallPost.id;
              return false;
          })) as WallPostItem;
            if (wallPostItem == null)
                return;
            this.DeletedCallback(wallPostItem);
        }

        public void Handle(WallPostAddedOrEdited message)
        {
            if (message.NewlyAddedWallPost.to_id != this.Id || message.NewlyAddedWallPost.IsPostponed)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                List<WallPost> wallPosts = new List<WallPost>();
                wallPosts.Add(message.NewlyAddedWallPost);
                List<User> users = message.Users;
                List<Group> groups = message.Groups;
                Action<WallPostItem> deletedCallback = new Action<WallPostItem>(this.DeletedCallback);
                double itemsWidth = 0.0;
                IVirtualizable virtualizable1 = ((IEnumerable<IVirtualizable>)WallPostItemsGenerator.Generate(wallPosts, users, groups, deletedCallback, itemsWidth)).First<IVirtualizable>();
                IVirtualizable virtualizable2 = (IVirtualizable)Enumerable.FirstOrDefault<IVirtualizable>(this.WallVM.Collection, (Func<IVirtualizable, bool>)(wp =>
                {
                    if (wp is WallPostItem && ((WallPostItem)wp).WallPost.id == message.NewlyAddedWallPost.id)
                        return ((WallPostItem)wp).WallPost.to_id == message.NewlyAddedWallPost.to_id;
                    return false;
                }));
                if (virtualizable2 == null)
                {
                    int num;
                    if (((IEnumerable<IVirtualizable>)this.WallVM.Collection).Any<IVirtualizable>())
                    {
                        WallPostItem wallPostItem = ((IEnumerable<IVirtualizable>)this.WallVM.Collection).First<IVirtualizable>() as WallPostItem;
                        num = wallPostItem != null ? (wallPostItem.WallPost.is_pinned == 1 ? 1 : 0) : 0;
                    }
                    else
                        num = 0;
                    bool flag = num != 0;
                    this.WallVM.Insert(virtualizable1, flag ? 1 : 0);
                }
                else
                {
                    int ind = ((Collection<IVirtualizable>)this.WallVM.Collection).IndexOf(virtualizable2);
                    this.WallVM.Delete(virtualizable2);
                    this.WallVM.Insert(virtualizable1, ind);
                }
            }));
            this.NotifyPostsVisibility();
        }

        public void Handle(WallPostPinnedUnpinned message)
        {
            if (this.Id != message.OwnerId)
                return;
            this.LoadInfo(true);
        }

        public void Handle(GroupMembershipStatusUpdated message)
        {
            if (this.Id >= 0L || Math.Abs(this.Id) != message.GroupId)
                return;
            this.LoadInfo(true);
        }

        public void Handle(FriendRemoved message)
        {
            if (this.Id != message.UserId && this.Id != AppGlobalStateManager.Current.LoggedInUserId)
                return;
            this.LoadInfo(true);
        }

        public void Handle(FriendRequestAcceptedDeclined message)
        {
            if (this.Id != message.UserId && this.Id != AppGlobalStateManager.Current.LoggedInUserId)
                return;
            this.LoadInfo(true);
        }

        public void Handle(FriendRequestSent message)
        {
            if (this.Id != message.UserId && this.Id != AppGlobalStateManager.Current.LoggedInUserId)
                return;
            this.LoadInfo(true);
        }

        public void Handle(SubscriptionCancelled message)
        {
            if (this.Id != message.UserId && this.Id != AppGlobalStateManager.Current.LoggedInUserId)
                return;
            this.LoadInfo(true);
        }

        public void Handle(BaseDataChangedEvent message)
        {
            if (!message.IsProfileUpdateRequired || this.IsAllDataBlockerStatus)
                return;
            this.LoadInfo(true);
        }

        public void NavigateToNewWallPost()
        {
            if (this._profileData == null)
                return;
            Navigator.Current.NavigateToNewWallPost(Math.Abs(this.Id), this.Id < 0, this._profileData.AdminLevel, this._groupData != null && this._groupData.group.GroupType == GroupType.PublicPage, false, false);
        }

        public void NavigateToSupport()
        {
            Navigator.Current.NavigateToConversation(333L, false, false, "", 0, false);
        }

        public void PinToStart()
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            string photoMax = this._profileData.PhotoMax;
            string name = this._profileData.Name;
            base.SetInProgress(true, "");
            SecondaryTileCreator.CreateTileFor(this.Id, this.Id < 0, name, (Action<bool>)(res =>
            {
                base.SetInProgress(false, "");
                this._isLoading = false;
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    Action barPropertyUpdated = this.AppBarPropertyUpdated;
                    if (barPropertyUpdated == null)
                        return;
                    barPropertyUpdated();
                }));
                if (res)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null)));
            }), photoMax);
        }

        public void BanUnban()
        {
            if (this._userData == null || this.Id < 0L)
                return;
            base.SetInProgress(true, "");
            if (this.IsBlacklistedByMe)
            {
                AccountService instance = AccountService.Instance;
                List<long> longList = new List<long>();
                longList.Add(this.Id);
                Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> action = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    base.SetInProgress(false, "");
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, CommonResources.BannedUsers_UserIsUnbanned, null);
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    this.IsBlacklistedByMe = !this.IsBlacklistedByMe;
                    EventAggregator current = EventAggregator.Current;
                    UserIsBannedOrUnbannedEvent bannedOrUnbannedEvent = new UserIsBannedOrUnbannedEvent();
                    bannedOrUnbannedEvent.IsBanned = false;
                    User user = this._userData.user;
                    bannedOrUnbannedEvent.user = user;
                    current.Publish(bannedOrUnbannedEvent);
                    this.LoadInfo(true);
                })));
                instance.UnbanUsers(longList, action);
            }
            else
                AccountService.Instance.BanUser(this.Id, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    this.SetInProgress(false, "");
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, CommonResources.BannedUsers_UserIsBanned, null);
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    this.IsBlacklistedByMe = !this.IsBlacklistedByMe;
                    EventAggregator current = EventAggregator.Current;
                    UserIsBannedOrUnbannedEvent bannedOrUnbannedEvent = new UserIsBannedOrUnbannedEvent();
                    bannedOrUnbannedEvent.IsBanned = true;
                    User user = this._userData.user;
                    bannedOrUnbannedEvent.user = user;
                    current.Publish(bannedOrUnbannedEvent);
                    this.LoadInfo(true);
                }))));
        }

        public void RemoveFromFriends()
        {
            if (this._userData == null || this.Id < 0L)
                return;
            this.SetInProgress(true, "");
            UsersService.Instance.FriendAddDelete(this.Id, false, (Action<BackendResult<OwnCounters, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                base.SetInProgress(false, "");
                GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.LoadInfo(false);
            }))));
        }

        public void FaveUnfave()
        {
            if (this._userData != null && this.Id > 0L)
            {
                this.FaveUnfaveUser();
            }
            else
            {
                if (this._groupData == null || this.Id >= 0L)
                    return;
                this.FaveUnfaveGroup();
            }
        }

        private void FaveUnfaveUser()
        {
            bool isFavorite = this.IsFavorite;
            base.SetInProgress(true, "");
            FavoritesService.Instance.FaveAddRemoveUser(this.Id, !isFavorite, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.SetInProgress(false, "");
                GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, !isFavorite ? CommonResources.Bookmarks_UserIsAdded : CommonResources.Bookmarks_UserIsRemoved, null);
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.IsFavorite = !this.IsFavorite;
                EventAggregator current = EventAggregator.Current;
                UserIsFavedOrUnfavedEvent favedOrUnfavedEvent = new UserIsFavedOrUnfavedEvent();
                favedOrUnfavedEvent.user = this._userData.user;
                int num = this.IsFavorite ? 1 : 0;
                favedOrUnfavedEvent.IsFaved = num != 0;
                current.Publish(favedOrUnfavedEvent);
            }))));
        }

        private void FaveUnfaveGroup()
        {
            bool isFavorite = this.IsFavorite;
            base.SetInProgress(true, "");
            FavoritesService.Instance.FaveAddRemoveGroup(-this.Id, !isFavorite, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.SetInProgress(false, "");
                GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, !isFavorite ? CommonResources.Bookmarks_CommunityIsAdded : CommonResources.Bookmarks_CommunityIsRemoved, null);
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.IsFavorite = !this.IsFavorite;
                EventAggregator current = EventAggregator.Current;
                GroupFavedUnfavedEvent favedUnfavedEvent = new GroupFavedUnfavedEvent();
                favedUnfavedEvent.group = this._groupData.group;
                int num = this.IsFavorite ? 1 : 0;
                favedUnfavedEvent.IsFaved = num != 0;
                current.Publish(favedUnfavedEvent);
            }))));
        }

        public void SubscribeUnsubscribe()
        {
            if (this._profileData == null)
                return;
            base.SetInProgress(true, "");
            if (this.IsSubscribed)
            {
                WallService current1 = WallService.Current;
                List<long> longList = new List<long>();
                longList.Add(this.Id);
                Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> action = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    this.SetInProgress(false, "");
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, CommonResources.NewsPostsNotificationsAreDisabled, null);
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    this.IsSubscribed = !this.IsSubscribed;
                    EventAggregator current = EventAggregator.Current;
                    ProfileIsSubscribedUnsubscribedToEvent unsubscribedToEvent = new ProfileIsSubscribedUnsubscribedToEvent();
                    unsubscribedToEvent.Id = this.Id;
                    int num = this.IsSubscribed ? 1 : 0;
                    unsubscribedToEvent.IsSubscribed = num != 0;
                    current.Publish(unsubscribedToEvent);
                })));
                current1.WallSubscriptionsUnsubscribe(longList, action);
            }
            else
                WallService.Current.WallSubscriptionsSubscribe(this.Id, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    this.SetInProgress(false, "");
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, CommonResources.NewsPostsNotificationsAreEnabled, null);
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    this.IsSubscribed = !this.IsSubscribed;
                    EventAggregator current = EventAggregator.Current;
                    ProfileIsSubscribedUnsubscribedToEvent unsubscribedToEvent = new ProfileIsSubscribedUnsubscribedToEvent();
                    unsubscribedToEvent.Id = this.Id;
                    int num = this.IsSubscribed ? 1 : 0;
                    unsubscribedToEvent.IsSubscribed = num != 0;
                    current.Publish(unsubscribedToEvent);
                }))));
        }

        public void PickNewPhoto()
        {
            Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
        }

        public void OpenProfilePhotos()
        {
            if (this._status == ProfileLoadingStatus.Banned || this._status == ProfileLoadingStatus.Deleted)
                return;
            Navigator.Current.NavigateToImageViewer("profile", 1, this.Id, this.Id < 0, -1, 0, new List<Photo>(), (Func<int, Image>)(id => null));
        }

        public void DeletePhoto()
        {
            base.SetInProgress(true, "");
            ProfilesService.Instance.DeleteProfilePhoto(this.Id, (Action<BackendResult<string, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.SetInProgress(false, "");
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                if (this._profileData != null)
                {
                    this._profileData.PhotoMax = res.ResultData;
                    if (this._userData != null && AppGlobalStateManager.Current.GlobalState.LoggedInUser != null)
                        AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = res.ResultData;
                    if (this._groupData != null)
                        EventAggregator.Current.Publish(new CommunityPhotoChanged()
                        {
                            CommunityId = this._groupData.Id,
                            PhotoMax = res.ResultData
                        });
                    EventAggregator.Current.Publish(new BaseDataChangedEvent()
                    {
                        IsProfileUpdateRequired = true
                    });
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
            }))));
        }

        public void UploadProfilePhoto(Stream stream, Rect rect)
        {
            if (this._isSaving)
                return;
            this._isSaving = true;
            base.SetInProgress(true, "");
            ImagePreprocessor.PreprocessImage(stream, VKConstants.ResizedImageSize, true, (Action<ImagePreprocessResult>)(preprocessRes =>
            {
                Stream stream1 = preprocessRes.Stream;
                byte[] buffer = (byte[])new byte[stream1.Length];
                stream1.Read(buffer, 0, (int)stream1.Length);
                stream1.Close();
                ProfilesService.Instance.SaveProfilePhoto(this.Id, ImagePreprocessor.GetThumbnailRect((double)preprocessRes.Width, (double)preprocessRes.Height, rect), buffer, (Action<BackendResult<ProfilePhoto, ResultCode>>)(res =>
                {
                    this._isSaving = false;
                    this.SetInProgress(false, "");
                    if (res.ResultCode == ResultCode.Succeeded)
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            if (this._userData != null)
                            {
                                BaseDataManager.Instance.NeedRefreshBaseData = true;
                                BaseDataManager.Instance.RefreshBaseDataIfNeeded();
                            }
                            if (this.Id < 0L)
                                EventAggregator.Current.Publish(new CommunityPhotoChanged()
                                {
                                    CommunityId = -this.Id,
                                    PhotoMax = res.ResultData.photo_200
                                });
                            this.LoadInfo(true);
                        }));
                    else
                        ExtendedMessageBox.ShowSafe(CommonResources.Error);
                }));
            }));
        }

        public void CopyLink()
        {
            string link = this.GetLink();
            if (string.IsNullOrEmpty(link))
                return;
            Clipboard.SetText(link);
        }

        public void OpenInBrowser()
        {
            string link = this.GetLink();
            if (string.IsNullOrEmpty(link))
                return;
            Launcher.LaunchUriAsync(new Uri("http://" + link));
        }

        private string GetLink()
        {
            if (this.Id == 0L || this.Id > 0L && this._userData == null || this.Id < 0L && this._groupData == null)
                return "";
            string str1 = "vk.com/";
            string str2;
            if (this.Id > 0L)
            {
                User user = this._userData.user;
                str2 = str1 + (!string.IsNullOrEmpty(user.domain) ? user.domain : "id" + user.id);
            }
            else
            {
                Group group = this._groupData.group;
                str2 = str1 + (!string.IsNullOrEmpty(group.screen_name) ? group.screen_name : "club" + group.id);
            }
            return str2;
        }

        public void Handle(CommunityLinkAddedOrEdited message)
        {
            if (this._groupData == null || message.CommunityId != this._groupData.Id)
                return;
            if (!message.IsEditing)
            {
                if (this._groupData.group.links == null)
                    this._groupData.group.links = new List<GroupLink>();
                this._groupData.group.links.Insert(0, message.Link);
            }
            else
            {
                if (this._groupData.group.links == null)
                    return;
                GroupLink groupLink = (GroupLink)Enumerable.FirstOrDefault<GroupLink>(this._groupData.group.links, (Func<GroupLink, bool>)(l => l.id == message.Link.id));
                if (groupLink == null)
                    return;
                groupLink.name = message.Link.name;
                groupLink.desc = message.Link.desc;
            }
        }

        public void Handle(CommunityLinkDeleted message)
        {
            if (this._groupData == null || message.CommunityId != this._groupData.Id || this._groupData.group.links == null)
                return;
            GroupLink groupLink = (GroupLink)Enumerable.FirstOrDefault<GroupLink>(this._groupData.group.links, (Func<GroupLink, bool>)(l => l.id == message.LinkId));
            if (groupLink == null)
                return;
            this._groupData.group.links.Remove(groupLink);
        }

        public void Handle(CommunityManagerChanged message)
        {
            if (this._groupData == null || message.CommunityId != this._groupData.Id)
                return;
            switch (message.EditingMode)
            {
                case EditingMode.Adding:
                    if (!message.IsContact)
                        break;
                    if (this._groupData.group.contacts == null)
                        this._groupData.group.contacts = new List<GroupContact>();
                    if (this._groupData.contactsUsers == null)
                        this._groupData.contactsUsers = new List<User>();
                    this._groupData.group.contacts.Add(new GroupContact()
                    {
                        user_id = message.ManagerId,
                        desc = message.Position,
                        email = message.Email,
                        phone = message.Phone
                    });
                    this._groupData.contactsUsers.Add(message.User);
                    break;
                case EditingMode.Editing:
                    if (!message.IsContact)
                    {
                        this.Handle(new CommunityManagerChanged()
                        {
                            CommunityId = message.CommunityId,
                            ManagerId = message.ManagerId,
                            EditingMode = EditingMode.Removing,
                            Role = message.Role
                        });
                        break;
                    }
                    if (this._groupData.group.contacts == null)
                        this._groupData.group.contacts = new List<GroupContact>();
                    if (this._groupData.contactsUsers == null)
                        this._groupData.contactsUsers = new List<User>();
                    GroupContact groupContact1 = (GroupContact)Enumerable.FirstOrDefault<GroupContact>(this._groupData.group.contacts, (Func<GroupContact, bool>)(c => c.user_id == message.ManagerId));
                    if (groupContact1 != null)
                    {
                        groupContact1.desc = message.Position;
                        groupContact1.email = message.Email;
                        groupContact1.phone = message.Phone;
                        break;
                    }
                    this._groupData.group.contacts.Add(new GroupContact()
                    {
                        user_id = message.ManagerId,
                        desc = message.Position,
                        email = message.Email,
                        phone = message.Phone
                    });
                    this._groupData.contactsUsers.Add(message.User);
                    break;
                case EditingMode.Removing:
                    if (message.ManagerId == AppGlobalStateManager.Current.LoggedInUserId)
                    {
                        GroupData groupData = this._groupData;
                        if ((groupData != null ? groupData.group : null) != null)
                        {
                            this._groupData.group.admin_level = (int)message.Role;
                            // ISSUE: method reference
                            base.NotifyPropertyChanged<bool>(() => this.CanManageCommunity);
                            Action barPropertyUpdated = this.AppBarPropertyUpdated;
                            if (barPropertyUpdated != null)
                                barPropertyUpdated();
                        }
                    }
                    if (this._groupData.contactsUsers == null || this._groupData.group.contacts == null)
                        break;
                    GroupContact groupContact2 = (GroupContact)Enumerable.FirstOrDefault<GroupContact>(this._groupData.group.contacts, (Func<GroupContact, bool>)(c => c.user_id == message.ManagerId));
                    if (groupContact2 != null)
                        this._groupData.group.contacts.Remove(groupContact2);
                    User user = (User)Enumerable.FirstOrDefault<User>(this._groupData.contactsUsers, (Func<User, bool>)(c => c.id == message.ManagerId));
                    if (user == null)
                        break;
                    this._groupData.contactsUsers.Remove(user);
                    break;
            }
        }

        public void Handle(CommunityInformationChanged message)
        {
            if (this._groupData == null || message.Id != this._groupData.Id)
                return;
            this.LoadInfo(true);
        }

        public void Handle(CommunityServicesChanged message)
        {
            if (this._groupData == null || message.Id != this._groupData.Id)
                return;
            this.LoadInfo(true);
        }
    }
}
