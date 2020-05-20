using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Common.Profiles.Users.ViewModels
{
    public class SubscriptionsViewModel : ViewModelBase, ICollectionDataProvider<UsersAndGroups, UserGroupHeader>, ICollectionDataProvider<List<Group>, GroupHeader>
    {
        private readonly long _userId;
        private readonly AsyncHelper<BackendResult<UsersAndGroups, ResultCode>> _asyncHelper;
        private bool _eventsFetchCalledAtLeastOnce;

        public GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> PagesVM { get; private set; }

        public GenericCollectionViewModel<List<Group>, GroupHeader> GroupsVM { get; private set; }

        Func<UsersAndGroups, ListWithCount<UserGroupHeader>> ICollectionDataProvider<UsersAndGroups, UserGroupHeader>.ConverterFunc
        {
            get
            {
                return (Func<UsersAndGroups, ListWithCount<UserGroupHeader>>)(data =>
                {
                    ListWithCount<UserGroupHeader> listWithCount = new ListWithCount<UserGroupHeader>() { TotalCount = 0 };
                    if (data.pages != null)
                    {
                        listWithCount.TotalCount += data.pages.Count;
                        listWithCount.List.AddRange((IEnumerable<UserGroupHeader>)Enumerable.Select<Group, UserGroupHeader>(data.pages, (Func<Group, UserGroupHeader>)(page => new UserGroupHeader()
                        {
                            GroupHeader = new GroupHeader(page, null)
                        })));
                    }
                    if (data.users != null)
                    {
                        listWithCount.TotalCount += data.users.Count;
                        listWithCount.List.AddRange((IEnumerable<UserGroupHeader>)Enumerable.Select<User, UserGroupHeader>(data.users, (Func<User, UserGroupHeader>)(user => new UserGroupHeader()
                        {
                            UserHeader = new FriendHeader(user, false)
                        })));
                    }
                    return listWithCount;
                });
            }
        }

        Func<List<Group>, ListWithCount<GroupHeader>> ICollectionDataProvider<List<Group>, GroupHeader>.ConverterFunc
        {
            get
            {
                return (Func<List<Group>, ListWithCount<GroupHeader>>)(data =>
                {
                    ListWithCount<GroupHeader> listWithCount = new ListWithCount<GroupHeader>() { TotalCount = 0 };
                    if (data != null)
                    {
                        listWithCount.TotalCount += data.Count;
                        listWithCount.List.AddRange((IEnumerable<GroupHeader>)Enumerable.Select<Group, GroupHeader>(data, (Func<Group, GroupHeader>)(group => new GroupHeader(group, null))));
                    }
                    return listWithCount;
                });
            }
        }

        public SubscriptionsViewModel(long userId)
        {
            this._userId = userId;
            this.PagesVM = new GenericCollectionViewModel<UsersAndGroups, UserGroupHeader>((ICollectionDataProvider<UsersAndGroups, UserGroupHeader>)this);
            this.GroupsVM = new GenericCollectionViewModel<List<Group>, GroupHeader>((ICollectionDataProvider<List<Group>, GroupHeader>)this);
            this._asyncHelper = new AsyncHelper<BackendResult<UsersAndGroups, ResultCode>>((Action<Action<BackendResult<UsersAndGroups, ResultCode>>>)(callback => UsersService.Instance.GetSubscriptions(this._userId, callback)));
        }

        public void LoadData(bool refresh = false, Action<bool> callback = null)
        {
            this.PagesVM.LoadData(refresh, false, (Action<BackendResult<UsersAndGroups, ResultCode>>)(result =>
            {
                Action<bool> action = callback;
                if (action == null)
                    return;
                int num = result.ResultCode == ResultCode.Succeeded ? 1 : 0;
                action(num != 0);
            }), false);
            this.GroupsVM.LoadData(refresh, false, null, false);
        }

        public void GetData(GenericCollectionViewModel<List<Group>, GroupHeader> caller, int offset, int count, Action<BackendResult<List<Group>, ResultCode>> callback)
        {
            if (offset > 0)
                callback(new BackendResult<List<Group>, ResultCode>(ResultCode.Succeeded, new List<Group>()));
            this._asyncHelper.RunAction((Action<BackendResult<UsersAndGroups, ResultCode>>)(response =>
            {
                if (response.ResultCode != ResultCode.Succeeded)
                    return;
                callback(new BackendResult<List<Group>, ResultCode>(ResultCode.Succeeded)
                {
                    ResultData = response.ResultData.groups
                });
            }), this._eventsFetchCalledAtLeastOnce);
            this._eventsFetchCalledAtLeastOnce = true;
        }

        public void GetData(GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> caller, int offset, int count, Action<BackendResult<UsersAndGroups, ResultCode>> callback)
        {
            if (offset > 0)
                callback(new BackendResult<UsersAndGroups, ResultCode>(ResultCode.Succeeded, new UsersAndGroups()));
            this._asyncHelper.RunAction((Action<BackendResult<UsersAndGroups, ResultCode>>)(response =>
            {
                if (response.ResultCode != ResultCode.Succeeded)
                    return;
                callback(new BackendResult<UsersAndGroups, ResultCode>(ResultCode.Succeeded)
                {
                    ResultData = response.ResultData
                });
            }), this._eventsFetchCalledAtLeastOnce);
            this._eventsFetchCalledAtLeastOnce = true;
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> caller, int count)
        {
            if (count == 0)
                return CommonResources.NoPages;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePageFrm, CommonResources.TwoFourPagesFrm, CommonResources.FivePagesFrm, true, null, false);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<Group>, GroupHeader> caller, int count)
        {
            if (count == 0)
                return CommonResources.NoCommunites;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCommunityFrm, CommonResources.TwoFourCommunitiesFrm, CommonResources.FiveCommunitiesFrm, true, null, false);
        }

        //
        internal double px_per_tick = 96.0 / 10.0 / 2.0;

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
