using System;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Shared
{
  public class OwnerHeaderWithSubscribeViewModel : ViewModelBase, IHandle<GroupMembershipStatusUpdated>, IHandle
  {
    private Group _group;
    private bool _isInProgress;

    public string Title
    {
      get
      {
        return this._group.name;
      }
    }

    public string ImageUri
    {
      get
      {
        return this._group.photo_100;
      }
    }

    public Visibility FollowButtonVisibility
    {
      get
      {
        return (!this._group.IsMember).ToVisiblity();
      }
    }

    public Visibility UnfollowButtonVisibility
    {
      get
      {
        return (this._group.IsMember && this._group.admin_level == 0).ToVisiblity();
      }
    }

    public OwnerHeaderWithSubscribeViewModel(Group group)
    {
      this._group = group;
      EventAggregator.Current.Subscribe((object) this);
    }

    public void NavigateToOwner()
    {
      Navigator.Current.NavigateToGroup(this._group.id, this._group.name, false);
    }

    public void SubscribeUnsubscribe(Action<bool> callback = null)
    {
      if (this._isInProgress)
        return;
      this._isInProgress = true;
      this.SetInProgressMain(true, "");
      if (this._group.IsMember)
        GroupsService.Current.Leave(this._group.id, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, false));
          this._isInProgress = false;
          this.SetInProgressMain(false, "");
          if (callback == null)
            return;
          Execute.ExecuteOnUIThread((Action) (() => callback(res.ResultCode == ResultCode.Succeeded)));
        }));
      else
        GroupsService.Current.Join(this._group.id, false, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, true));
          this._isInProgress = false;
          this.SetInProgressMain(false, "");
          if (callback == null)
            return;
          Execute.ExecuteOnUIThread((Action) (() => callback(res.ResultCode == ResultCode.Succeeded)));
        }));
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      if (message.GroupId != this._group.id)
        return;
      this._group.IsMember = message.Joined;
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FollowButtonVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.UnfollowButtonVisibility));
    }
  }
}
