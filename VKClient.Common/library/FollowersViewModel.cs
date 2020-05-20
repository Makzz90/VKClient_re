using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Common.Library
{
  public class FollowersViewModel : ViewModelBase, IHandle<FriendRequestAcceptedDeclined>, IHandle, IHandle<SubscriptionCancelled>, ICollectionDataProvider<List<User>, UserGroupHeader>, ICollectionDataProvider<UsersAndGroups, UserGroupHeader>
  {
    private long _userOrGroupId;
    private bool _isGroup;
    private bool _isLoading;
    private string _name;
    private bool _subscriptions;
    private GenericCollectionViewModel<List<User>, UserGroupHeader> _followersVM;
    private GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> _subscriptionsVM;

    public string Title
    {
      get
      {
        if (!this._subscriptions)
        {
          if (AppGlobalStateManager.Current.LoggedInUserId != this._userOrGroupId && !string.IsNullOrWhiteSpace(this._name))
            return string.Format(CommonResources.FollowersPage_TitleFrm, this._name).ToUpperInvariant();
          return CommonResources.FollowersPage_OwnFollowersTitle.ToUpperInvariant();
        }
        if (AppGlobalStateManager.Current.LoggedInUserId != this._userOrGroupId && !string.IsNullOrWhiteSpace(this._name))
          return string.Format(CommonResources.SubscriptionsPage_TitleFrm, this._name).ToUpperInvariant();
        return CommonResources.SubscriptionsPage_OwnSubscriptionsTitle.ToUpperInvariant();
      }
    }

    public GenericCollectionViewModel<List<User>, UserGroupHeader> FollowersVM
    {
      get
      {
        return this._followersVM;
      }
    }

    public GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> SubscriptionsVM
    {
      get
      {
        return this._subscriptionsVM;
      }
    }

    public Func<List<User>, ListWithCount<UserGroupHeader>> ConverterFunc
    {
      get
      {
        return (Func<List<User>, ListWithCount<UserGroupHeader>>) (l =>
        {
          ListWithCount<UserGroupHeader> listWithCount = new ListWithCount<UserGroupHeader>();
          listWithCount.TotalCount = -1;
          foreach (User user in l)
            listWithCount.List.Add(new UserGroupHeader()
            {
              UserHeader = new FriendHeader(user, false)
            });
          return listWithCount;
        });
      }
    }

    Func<UsersAndGroups, ListWithCount<UserGroupHeader>> ICollectionDataProvider<UsersAndGroups, UserGroupHeader>.ConverterFunc
    {
      get
      {
        return (Func<UsersAndGroups, ListWithCount<UserGroupHeader>>) (ug =>
        {
          ListWithCount<UserGroupHeader> listWithCount = new ListWithCount<UserGroupHeader>();
          listWithCount.TotalCount = ug.users.Count + ug.groups.Count;
          foreach (Group group in ug.groups)
            listWithCount.List.Add(new UserGroupHeader()
            {
              GroupHeader = new GroupHeader(group,  null)
            });
          foreach (User user in ug.users)
            listWithCount.List.Add(new UserGroupHeader()
            {
              UserHeader = new FriendHeader(user, false)
            });
          return listWithCount;
        });
      }
    }

    public FollowersViewModel(long userOrGroupId, bool isGroup, string name, bool subscriptions)
    {
      this._userOrGroupId = userOrGroupId;
      this._isGroup = isGroup;
      this._name = name;
      this._subscriptions = subscriptions;
      EventAggregator.Current.Subscribe(this);
      this._followersVM = new GenericCollectionViewModel<List<User>, UserGroupHeader>((ICollectionDataProvider<List<User>, UserGroupHeader>) this);
      this._subscriptionsVM = new GenericCollectionViewModel<UsersAndGroups, UserGroupHeader>((ICollectionDataProvider<UsersAndGroups, UserGroupHeader>) this);
    }

    public void Handle(FriendRequestAcceptedDeclined message)
    {
      if (this._userOrGroupId != AppGlobalStateManager.Current.LoggedInUserId || this._isGroup || this._subscriptions)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
          UserGroupHeader userGroupHeader = (UserGroupHeader)Enumerable.FirstOrDefault<UserGroupHeader>(this.FollowersVM.Collection, (Func<UserGroupHeader, bool>)(f =>
        {
          if (f.UserHeader != null)
            return f.UserHeader.UserId == message.UserId;
          return false;
        }));
        if (userGroupHeader == null)
          return;
        this.FollowersVM.Delete(userGroupHeader);
      }));
    }

    public void Handle(SubscriptionCancelled message)
    {
      if (this._userOrGroupId != AppGlobalStateManager.Current.LoggedInUserId || this._isGroup || !this._subscriptions)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
          UserGroupHeader userGroupHeader = (UserGroupHeader)Enumerable.FirstOrDefault<UserGroupHeader>(this.FollowersVM.Collection, (Func<UserGroupHeader, bool>)(f =>
        {
          if (f.UserHeader != null)
            return f.UserHeader.UserId == message.UserId;
          return false;
        }));
        if (userGroupHeader == null)
          return;
        this.FollowersVM.Delete(userGroupHeader);
      }));
    }

    public void GetData(GenericCollectionViewModel<List<User>, UserGroupHeader> caller, int offset, int count, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      UsersService.Instance.GetFollowers(this._userOrGroupId, offset, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, UserGroupHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoFollowers;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFollowerFrm, CommonResources.TwoFourFollowersFrm, CommonResources.FiveFollowersFrm, true,  null, false);
    }

    public void GetData(GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> caller, int offset, int count, Action<BackendResult<UsersAndGroups, ResultCode>> callback)
    {
      if (offset == 0)
        UsersService.Instance.GetSubscriptions(this._userOrGroupId, callback);
      else
        callback(new BackendResult<UsersAndGroups, ResultCode>(ResultCode.Succeeded, new UsersAndGroups()));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<UsersAndGroups, UserGroupHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPages;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePageFrm, CommonResources.TwoFourPagesFrm, CommonResources.FivePagesFrm, true,  null, false);
    }
  }
}
