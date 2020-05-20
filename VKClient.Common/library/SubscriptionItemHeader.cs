using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Common.Library
{
  public class SubscriptionItemHeader : ISubscriptionItemHeader, IHaveUniqueKey, INotifyPropertyChanged, IHandle<GroupMembershipStatusUpdated>, IHandle
  {
    private readonly bool _isVerified;
    private bool _canSubscribe;
    private bool _isInProgress;
    private readonly User _user;
    private readonly Group _group;

    public SubscriptionItemType SubscriptionItemType
    {
      get
      {
        return SubscriptionItemType.Subscription;
      }
    }

    public long Id { get; private set; }

    public string Title { get; private set; }

    public string Subtitle { get; private set; }

    public string ImageUrl { get; set; }

    public SolidColorBrush PlaceholderFill { get; set; }

    public Visibility VerifiedIconVisibility
    {
      get
      {
        if (!this._isVerified)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SubscribeVisibility
    {
      get
      {
        if (this._canSubscribe && !this._isInProgress)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Visibility SubscribedVisibility
    {
      get
      {
        if (!this._canSubscribe && !this._isInProgress)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Visibility IsInProgressVisibility
    {
      get
      {
        if (!this._isInProgress)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool IsInProgress
    {
      get
      {
        return this._isInProgress;
      }
    }

    public double TitleMaxWidth
    {
      get
      {
        return this._isVerified ? 277.0 : 310.0;
      }
    }

    public string SubscribeButtonIcon
    {
      get
      {
        return this._group == null ? "/Resources/UsersSearch/SearchAddFriend.png" : "/Resources/UsersSearch/SearchAddCommunity.png";
      }
    }

    public Action TapAction { get; private set; }

    public Action SubscribeAction { get; private set; }

    public Action UnsubscribeAction { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private SubscriptionItemHeader()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public SubscriptionItemHeader(User user, bool includeCommonFriendsInfo)
      : this()
    {
      this._user = user;
      if (this._user == null)
        return;
      this.Id = this._user.id;
      this.Title = this._user.Name;
      this.Subtitle = SubscriptionItemHeader.GetSubtitle(this._user, includeCommonFriendsInfo);
      this.ImageUrl = this._user.photo_max;
      this._isVerified = this._user.verified > 0;
      this._canSubscribe = this._user.friend_status == 0 || this._user.friend_status == 2;
      this.TapAction = (Action) (() => Navigator.Current.NavigateToUserProfile(this._user.id, this._user.Name, "", false));
      this.SubscribeAction = (Action) (() =>
      {
        if (this._isInProgress)
          return;
        this.SetIsInProgress(true);
        UsersService.Instance.AddFriend(this._user.id, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
            this.UpdateFriendStatus(1);
          this.SetIsInProgress(false);
        }))));
      });
      this.UnsubscribeAction = (Action) (() =>
      {
        if (this._isInProgress)
          return;
        this.SetIsInProgress(true);
        UsersService.Instance.DeleteFriend(this._user.id, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
            this.UpdateFriendStatus(0);
          this.SetIsInProgress(false);
        }))));
      });
    }

    public SubscriptionItemHeader(Group group)
      : this()
    {
      this._group = group;
      if (this._group == null)
        return;
      this.Id = this._group.id;
      this.Title = this._group.name;
      this.Subtitle = SubscriptionItemHeader.GetSubtitle(this._group);
      this.ImageUrl = this._group.photo_200;
      this._isVerified = this._group.verified > 0;
      this._canSubscribe = this._group.is_member == 0;
      this.TapAction = (Action) (() => Navigator.Current.NavigateToGroup(this._group.id, this._group.name, false));
      this.SubscribeAction = (Action) (() =>
      {
        if (this._isInProgress)
          return;
        this.SetIsInProgress(false);
        GroupsService.Current.Join(this._group.id, false, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this._group.is_member = 1;
            this.UpdateGroupSubscriptionStatus();
          }
          this.SetIsInProgress(false);
        }))),  null);
      });
      this.UnsubscribeAction = (Action) (() =>
      {
        if (this._isInProgress)
          return;
        this.SetIsInProgress(false);
        GroupsService.Current.Leave(this._group.id, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this._group.is_member = 0;
            this.UpdateGroupSubscriptionStatus();
          }
          this.SetIsInProgress(false);
        }))));
      });
    }

    public SubscriptionItemHeader(string title, string subtitle, string imageUrl, SolidColorBrush placeholderFill, Action tapAction)
    {
      this.Title = title;
      this.Subtitle = subtitle;
      this.ImageUrl = imageUrl;
      this.PlaceholderFill = placeholderFill;
      this.TapAction = tapAction;
    }

    private static string GetSubtitle(User user, bool includeCommonFriendsInfo)
    {
      if (user.description != null)
        return user.description;
      if (includeCommonFriendsInfo && user.common_count > 0)
        return UIStringFormatterHelper.FormatNumberOfSomething(user.common_count, CommonResources.OneCommonFriendFrm, CommonResources.TwoFourCommonFriendsFrm, CommonResources.FiveCommonFriendsFrm, true,  null, false);
      Occupation occupation = user.occupation;
      if (occupation != null && !string.IsNullOrEmpty(occupation.name))
        return occupation.name;
      City city = user.city;
      Country country = user.country;
      if (city != null && !string.IsNullOrEmpty(city.name))
      {
        string str = Extensions.ForUI(city.name);
        if (country != null && !string.IsNullOrEmpty(country.name))
          str += string.Format(", {0}", country.name);
        return str;
      }
      if (country != null && !string.IsNullOrEmpty(country.name))
        return country.name;
      return "";
    }

    private static string GetSubtitle(Group group)
    {
      if (!string.IsNullOrWhiteSpace(group.activity) && group.GroupType != GroupType.Event)
        return Extensions.ForUI(group.activity);
      switch (group.GroupType)
      {
        case GroupType.Group:
          return GroupHeader.GetGroupTypeText(group);
        case GroupType.PublicPage:
          return CommonResources.PublicPage;
        default:
          return "";
      }
    }

    public User GetUser()
    {
      return this._user;
    }

    private void SetIsInProgress(bool isInProgress)
    {
      this._isInProgress = isInProgress;
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribeVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribedVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.IsInProgressVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.IsInProgress);
    }

    private void UpdateGroupSubscriptionStatus()
    {
      this._canSubscribe = this._group.is_member == 0;
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribeVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribedVisibility);
    }

    public void UpdateFriendStatus(int friendStatus)
    {
      if (this._user == null)
        return;
      this._user.friend_status = friendStatus;
      this._canSubscribe = this._user.friend_status == 0 || this._user.friend_status == 2;
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribeVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SubscribedVisibility);
    }

    public string GetKey()
    {
      if (this._user != null)
        return "user" + this._user.id;
      if (this._group != null)
        return "group" + this._group.id;
      return "";
    }

    private void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
        return;
      this.RaisePropertyChanged((propertyExpression.Body as MemberExpression).Member.Name);
    }

    private void RaisePropertyChanged(string property)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.PropertyChanged == null)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        // ISSUE: reference to a compiler-generated field
        if (this.PropertyChanged == null)
          return;
        // ISSUE: reference to a compiler-generated field
        this.PropertyChanged(this, new PropertyChangedEventArgs(property));
      }));
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      if (this._group == null || this._group.id != message.GroupId)
        return;
      this._group.is_member = message.Joined ? 1 : 0;
      this.UpdateGroupSubscriptionStatus();
    }
  }
}
