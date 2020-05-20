using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Profiles.Users.ViewModels
{
  public class UserMembershipInfo : MembershipInfoBase
  {
    private readonly UserData _userData;
    private bool _isLoading;

    public override long Id
    {
      get
      {
        if (this._userData == null)
          return 0;
        return this._userData.Id;
      }
    }

    public override Visibility InvitedByUserVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public override string InvitationStr
    {
      get
      {
        return "";
      }
    }

    public override string InvitedByUserPhoto
    {
      get
      {
        return "";
      }
    }

    public override string TextButtonInvitationReply
    {
      get
      {
        return "";
      }
    }

    public override string TextButtonInvitationDecline
    {
      get
      {
        return "";
      }
    }

    public override Visibility VisibilityButtonSendMessage
    {
      get
      {
        if (this._userData != null && this._userData.Id != AppGlobalStateManager.Current.LoggedInUserId && this._userData.user.can_write_private_message != 0)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public override Visibility VisibilityButtonPrimary
    {
      get
      {
        if (this._userData == null || this._userData.Id == AppGlobalStateManager.Current.LoggedInUserId)
          return Visibility.Collapsed;
        if (this.VisibilityButtonSecondary != Visibility.Visible)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public override Visibility VisibilityButtonSecondary
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public override Visibility VisibilityButtonSecondaryAction
    {
      get
      {
        if (this._userData == null || this._userData.Id == AppGlobalStateManager.Current.LoggedInUserId)
          return Visibility.Collapsed;
        switch (this._userData.friend.friend_status)
        {
          case FriendshipStatus.Friends:
          case FriendshipStatus.RequestSent:
            return Visibility.Visible;
          default:
            return Visibility.Collapsed;
        }
      }
    }

    public override double SecondaryButtonsMinWidth
    {
      get
      {
          return this._userData == null || this.VisibilityButtonSendMessage == Visibility.Visible ? 246.0 : 480.0;
      }
    }

    public override Action SecondaryAction
    {
      get
      {
        return  null;
      }
    }

    public override string TextButtonSendMessage
    {
      get
      {
        return CommonResources.Group_SendAMessage;
      }
    }

    public override string TextButtonPrimary
    {
      get
      {
        if (this._userData == null)
          return "";
        switch (this._userData.friend.friend_status)
        {
          case FriendshipStatus.No:
            if (this._userData.user.can_send_friend_request == 1)
              return CommonResources.Profile_AddToFriends;
            return CommonResources.Follow;
          case FriendshipStatus.RequestReceived:
            if (this._userData.friend.read_state == 0)
              return CommonResources.Profile_ReplyToRequest;
            if (!this._userData.user.IsFemale)
              return CommonResources.Profile_FollowingYou_Male;
            return CommonResources.Profile_FollowingYou_Female;
          default:
            return "";
        }
      }
    }

    public override string TextButtonSecondary
    {
      get
      {
        return "";
      }
    }

    public override string TextButtonSecondaryAction
    {
      get
      {
        if (this._userData == null)
          return "";
        switch (this._userData.friend.friend_status)
        {
          case FriendshipStatus.Friends:
            return CommonResources.Profile_YouAreFriends;
          case FriendshipStatus.RequestSent:
            return CommonResources.YouAreFollowing;
          default:
            return "";
        }
      }
    }

    public override IList<MenuItem> MenuItems
    {
      get
      {
        if (this._userData == null)
          return (IList<MenuItem>) new List<MenuItem>();
        List<MenuItem> menuItemList = new List<MenuItem>();
        switch (this._userData.friend.friend_status)
        {
          case FriendshipStatus.RequestSent:
            MenuItem menuItem1 = new MenuItem();
            string str = this._userData.user.can_send_friend_request == 1 ? CommonResources.CancelRequest : CommonResources.Unfollow;
            menuItem1.Header = str;
            menuItem1.Click += delegate(object sender, RoutedEventArgs args)
			{
				this.Remove();
			};
			menuItemList.Add(menuItem1);
            break;
          case FriendshipStatus.RequestReceived:
            if (this._userData.friend.read_state == 1)
            {
              if (this._userData.Id != AppGlobalStateManager.Current.LoggedInUserId)
              {
                MenuItem menuItem3 = new MenuItem();
                string lowerInvariant = CommonResources.Profile_AddToFriends.ToLowerInvariant();
                menuItem3.Header = lowerInvariant;
                MenuItem menuItem4 = menuItem3;
                menuItem4.Click += delegate(object sender, RoutedEventArgs args)
					{
						this.Add();
					};
                menuItemList.Add(menuItem4);
                break;
              }
              break;
            }
            MenuItem menuItem5 = new MenuItem();
            string profileAcceptRequest = CommonResources.Profile_AcceptRequest;
            menuItem5.Header = profileAcceptRequest;
            MenuItem menuItem6 = menuItem5;
            menuItem6.Click += delegate(object sender, RoutedEventArgs args)
				{
					this.Add();
				};
            MenuItem menuItem7 = new MenuItem();
            string profileKeepAsFollower = CommonResources.Profile_KeepAsFollower;
            menuItem7.Header = profileKeepAsFollower;
            MenuItem menuItem8 = menuItem7;
            menuItem8.Click += delegate(object sender, RoutedEventArgs args)
				{
					this.Remove();
				};
            menuItemList.Add(menuItem6);
            menuItemList.Add(menuItem8);
            break;
          case FriendshipStatus.Friends:
            MenuItem menuItem9 = new MenuItem();
            string lowerInvariant1 = CommonResources.Profile_RemoveFromFriends.ToLowerInvariant();
            menuItem9.Header = lowerInvariant1;
            MenuItem menuItem10 = menuItem9;
            menuItem10.Click += delegate(object sender, RoutedEventArgs args)
            {
                this.Remove();
            };
            menuItemList.Add(menuItem10);
            break;
        }
        return menuItemList;
      }
    }

    public override bool SupportMultipleAddActions
    {
      get
      {
        if (this._userData != null)
          return this._userData.friend.friend_status == FriendshipStatus.RequestReceived;
        return false;
      }
    }

    private bool IsBlacklistedByMe
    {
      get
      {
        if (this._userData != null)
          return this._userData.user.blacklisted_by_me == 1;
        return false;
      }
    }

    public UserMembershipInfo(UserData userData)
    {
      this._userData = userData;
    }

    public override void Add()
    {
      this.AddDeleteFriend(false);
    }

    public override void Remove()
    {
      this.AddDeleteFriend(true);
    }

    private void AddDeleteFriend(bool delete = false)
    {
      if (this._userData == null || this._isLoading)
        return;
      if (this.IsBlacklistedByMe && !delete)
      {
        new GenericInfoUC().ShowAndHideLater(CommonResources.BannedUsers_UserIsInBlackList,  null);
      }
      else
      {
        this._isLoading = true;
        bool add = this._userData.friend.friend_status == FriendshipStatus.No || this._userData.friend.friend_status == FriendshipStatus.RequestReceived && !delete;
        long userId = this._userData.Id;
        UsersService.Instance.FriendAddDelete(userId, add, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
        {
          this._isLoading = false;
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          switch (this._userData.friend.friend_status)
          {
            case FriendshipStatus.No:
              EventAggregator.Current.Publish(new FriendRequestSent()
              {
                UserId = userId
              });
              break;
            case FriendshipStatus.RequestSent:
              EventAggregator.Current.Publish(new SubscriptionCancelled(userId));
              break;
            case FriendshipStatus.RequestReceived:
              EventAggregator.Current.Publish(new FriendRequestAcceptedDeclined(true, userId));
              break;
            case FriendshipStatus.Friends:
              EventAggregator.Current.Publish(new FriendRemoved(userId));
              break;
          }
          CountersManager.Current.Counters = res.ResultData;
        }));
      }
    }
  }
}
