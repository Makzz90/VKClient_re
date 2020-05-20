using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Shared
{
  public class OwnerFullHeaderWithSubscribeViewModel : ViewModelBase, IHandle<GroupMembershipStatusUpdated>, IHandle, IHandle<FriendRequestAcceptedDeclined>, IHandle<FriendRemoved>, IHandle<FriendRequestSent>, IHandle<SubscriptionCancelled>
  {
    private readonly Group _group;
    private readonly User _user;
    private bool _isInProgress;

    public string Title
    {
      get
      {
        if (this._group != null)
          return this._group.name;
        if (this._user != null)
          return this._user.Name;
        return "";
      }
    }

    public string SubscribersCountStr
    {
      get
      {
        if (this._group != null)
          return UIStringFormatterHelper.FormatNumberOfSomething(this._group.members_count, CommonResources.OneSubscriberFrm, CommonResources.TwoFourSubscribersFrm, CommonResources.FiveSubscribersFrm, true,  null, false);
        return "";
      }
    }

    public Visibility SubscribersCountVisibility
    {
      get
      {
        if (string.IsNullOrEmpty(this.SubscribersCountStr))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public string ImageUri
    {
      get
      {
        if (this._group != null)
          return this._group.photo_100;
        if (this._user != null)
          return this._user.photo_max;
        return "";
      }
    }

    private bool CanSubscribe
    {
      get
      {
        if (this._group != null)
        {
          if (!this._group.IsMember && this._group.Privacy != GroupPrivacy.Closed && this._group.Privacy != GroupPrivacy.Private)
            return this._group.admin_level == 0;
          return false;
        }
        if (this._user != null && this._user.id != AppGlobalStateManager.Current.LoggedInUserId && this._user.friend_status != 1)
          return this._user.friend_status != 3;
        return false;
      }
    }

    private bool CanUnsubscribe
    {
      get
      {
        if (this._group != null)
        {
          if (this._group.IsMember && this._group.Privacy != GroupPrivacy.Closed && this._group.Privacy != GroupPrivacy.Private)
            return this._group.admin_level == 0;
          return false;
        }
        if (this._user != null && this._user.id != AppGlobalStateManager.Current.LoggedInUserId && this._user.friend_status != 0)
          return this._user.friend_status != 2;
        return false;
      }
    }

    public Visibility SubscribeVisibility
    {
      get
      {
        if (!this.CanSubscribe)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SubscribedVisibility
    {
      get
      {
        if (!this.CanUnsubscribe)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    private bool IsSubscribed
    {
      get
      {
        if (this._group != null)
          return this._group.IsMember;
        if (this._user == null)
          return false;
        if (this._user.friend_status != 1)
          return this._user.friend_status == 3;
        return true;
      }
    }

    public OwnerFullHeaderWithSubscribeViewModel(Group group)
      : this()
    {
      this._group = group;
    }

    public OwnerFullHeaderWithSubscribeViewModel(User user)
      : this()
    {
      this._user = user;
    }

    public OwnerFullHeaderWithSubscribeViewModel()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public void NavigateToOwner()
    {
      if (this._group != null)
        Navigator.Current.NavigateToGroup(this._group.id, this._group.name, false);
      if (this._user == null)
        return;
      Navigator.Current.NavigateToUserProfile(this._user.id, this._user.Name, "", false);
    }

    public void SubscribeUnsubscribe()
    {
      if (this._group == null && this._user == null || this._isInProgress)
        return;
      this._isInProgress = true;
      this.SetInProgressMain(true, "");
      if (this._group != null)
        this.SubscribeUnsubscribeGroup();
      if (this._user == null)
        return;
      this.SubscribeUnsubscribeUser();
    }

    private void SubscribeUnsubscribeGroup()
    {
      if (this.IsSubscribed)
        GroupsService.Current.Leave(this._group.id, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            EventAggregator.Current.Publish(new GroupMembershipStatusUpdated(this._group.id, false));
          this._isInProgress = false;
          this.SetInProgressMain(false, "");
        }));
      else
        GroupsService.Current.Join(this._group.id, false, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            EventAggregator.Current.Publish(new GroupMembershipStatusUpdated(this._group.id, true));
          this._isInProgress = false;
          this.SetInProgressMain(false, "");
        }),  null);
    }

    private void SubscribeUnsubscribeUser()
    {
      long userId = this._user.id;
      UsersService.Instance.FriendAddDelete(userId, !this.IsSubscribed, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          switch (this._user.friend_status)
          {
            case 0:
              EventAggregator.Current.Publish(new FriendRequestSent()
              {
                UserId = userId
              });
              break;
            case 1:
              EventAggregator.Current.Publish(new SubscriptionCancelled(userId));
              break;
            case 2:
              EventAggregator.Current.Publish(new FriendRequestAcceptedDeclined(true, userId));
              break;
            case 3:
              EventAggregator.Current.Publish(new FriendRemoved(userId));
              break;
          }
          CountersManager.Current.Counters = res.ResultData;
        }
        this._isInProgress = false;
        this.SetInProgressMain(false, "");
      }));
    }

    private void NotifySubscribeProperties()
    {
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribeVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribedVisibility);
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      if (this._group == null || message.GroupId != this._group.id)
        return;
      this._group.IsMember = message.Joined;
      this.NotifySubscribeProperties();
    }

    public void Handle(FriendRequestAcceptedDeclined message)
    {
      if (this._user == null || message.UserId != this._user.id)
        return;
      this._user.friend_status = message.Accepted ? 3 : 2;
      this.NotifySubscribeProperties();
    }

    public void Handle(FriendRemoved message)
    {
      if (this._user == null || message.UserId != this._user.id)
        return;
      this._user.friend_status = 2;
      this.NotifySubscribeProperties();
    }

    public void Handle(FriendRequestSent message)
    {
      if (this._user == null || message.UserId != this._user.id)
        return;
      this._user.friend_status = 1;
      this.NotifySubscribeProperties();
    }

    public void Handle(SubscriptionCancelled message)
    {
      if (this._user == null || message.UserId != this._user.id)
        return;
      this._user.friend_status = 0;
      this.NotifySubscribeProperties();
    }
  }
}
