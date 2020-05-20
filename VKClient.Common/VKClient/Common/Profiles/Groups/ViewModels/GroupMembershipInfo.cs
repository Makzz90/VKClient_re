using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Groups.ViewModels
{
  public class GroupMembershipInfo : MembershipInfoBase
  {
    private readonly Group _group;
    private readonly User _invitedByUser;

    public override long Id
    {
      get
      {
        if (this._group == null)
          return 0;
        return this._group.id;
      }
    }

    public override Visibility InvitedByUserVisibility
    {
      get
      {
        return this._invitedByUser == null ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override string InvitationStr
    {
      get
      {
        if (this._invitedByUser == null)
          return "";
        return string.Format("[id{0}|{1}] {2}", (object) this._invitedByUser.id, (object) this._invitedByUser.Name, (object) this.InvitedToCommunityStr);
      }
    }

    public override string InvitedByUserPhoto
    {
      get
      {
        if (this._invitedByUser == null)
          return "";
        return this._invitedByUser.photo_max;
      }
    }

    public string InvitedToCommunityStr
    {
      get
      {
        if (this._group == null || this._invitedByUser == null)
          return "";
        bool isFemale = this._invitedByUser.IsFemale;
        switch (this._group.GroupType)
        {
          case GroupType.Group:
            if (!isFemale)
              return CommonResources.Group_InvitedYouTo_Group_Male;
            return CommonResources.Group_InvitedYouTo_Group_Female;
          case GroupType.Event:
            if (!isFemale)
              return CommonResources.Group_InvitedYouTo_Event_Male;
            return CommonResources.Group_InvitedYouTo_Event_Female;
          default:
            return "";
        }
      }
    }

    public override string TextButtonInvitationReply
    {
      get
      {
        if (this._group == null)
          return "";
        switch (this._group.GroupType)
        {
          case GroupType.Group:
            return CommonResources.Group_Join;
          case GroupType.PublicPage:
            return CommonResources.Group_Follow;
          case GroupType.Event:
            return CommonResources.Group_Reply;
          default:
            return "";
        }
      }
    }

    public override string TextButtonInvitationDecline
    {
      get
      {
        return CommonResources.Group_Decline;
      }
    }

    public override Visibility VisibilityButtonSendMessage
    {
      get
      {
        return this._group == null || this._group.can_message != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override Visibility VisibilityButtonPrimary
    {
      get
      {
        return this._group == null || this._invitedByUser != null || (this._invitedByUser != null || !this._group.CanJoin) || this._group.MembershipType == GroupMembershipType.RequestSent ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override Visibility VisibilityButtonSecondary
    {
      get
      {
        return this._group == null || this._invitedByUser != null || (this._group.GroupType != GroupType.Event || this._group.start_date == 0) || Extensions.UnixTimeStampToDateTime((double) Math.Max(this._group.start_date, this._group.finish_date), true) > DateTime.Now ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override Visibility VisibilityButtonSecondaryAction
    {
      get
      {
        return this._group == null || this._invitedByUser != null || this._group.MembershipType == GroupMembershipType.NotAMember && (this._group.Privacy == GroupPrivacy.Private || this._group.ban_info != null && this._group.ban_info.end_date == 0L) || (this.VisibilityButtonPrimary == Visibility.Visible || this.VisibilityButtonSecondary == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override double SecondaryButtonsMinWidth
    {
      get
      {
        return this._group == null || this.VisibilityButtonSendMessage == Visibility.Visible ? 246.0 : 480.0;
      }
    }

    public override Action SecondaryAction
    {
      get
      {
        if (this._group == null)
          return null;
        if (this._group.IsMember)
          return new Action(((MembershipInfoBase) this).Remove);
        return new Action(((MembershipInfoBase) this).Add);
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
        if (this._group == null || this._group.MembershipType != GroupMembershipType.NotAMember)
          return "";
        if (this._group.Privacy != GroupPrivacy.Public)
          return CommonResources.GroupPage_SendRequest;
        switch (this._group.GroupType)
        {
          case GroupType.Group:
            return CommonResources.Group_Join;
          case GroupType.PublicPage:
            return CommonResources.Group_Follow;
          case GroupType.Event:
            return CommonResources.Group_Join + "...";
          default:
            return "";
        }
      }
    }

    public override string TextButtonSecondary
    {
      get
      {
        if (!this._group.IsMember)
          return CommonResources.Event_RestoreToList;
        return CommonResources.Event_RemoveFromList;
      }
    }

    public override string TextButtonSecondaryAction
    {
      get
      {
        if (this._group == null)
          return "";
        switch (this._group.MembershipType)
        {
          case GroupMembershipType.Member:
            switch (this._group.GroupType)
            {
              case GroupType.Group:
                return CommonResources.Group_Joined;
              case GroupType.PublicPage:
                return CommonResources.Group_Following;
              case GroupType.Event:
                return CommonResources.Group_Attending;
              default:
                return "";
            }
          case GroupMembershipType.NotSure:
            return CommonResources.Group_MayAttend;
          case GroupMembershipType.RequestSent:
            return CommonResources.GroupPage_RequestSent;
          default:
            return "";
        }
      }
    }

    public override IList<MenuItem> MenuItems
    {
      get
      {
        if (this._group == null)
          return (IList<MenuItem>) new List<MenuItem>();
        List<MenuItem> menuItemList = new List<MenuItem>();
        switch (this._group.MembershipType)
        {
          case GroupMembershipType.NotAMember:
            if (this._group.GroupType == GroupType.Event)
            {
              MenuItem menuItem1 = new MenuItem();
              string eventAttend = CommonResources.EventAttend;
              menuItem1.Header = (object) eventAttend;
              MenuItem menuItem2 = menuItem1;
              menuItem2.Click += (RoutedEventHandler) ((sender, args) => this.Add());
              MenuItem menuItem3 = new MenuItem();
              string eventMaybe = CommonResources.EventMaybe;
              menuItem3.Header = (object) eventMaybe;
              MenuItem menuItem4 = menuItem3;
              menuItem4.Click += (RoutedEventHandler) ((sender, args) => this.Add(true));
              MenuItem menuItem5 = new MenuItem();
              string eventNotAttend = CommonResources.EventNotAttend;
              menuItem5.Header = (object) eventNotAttend;
              MenuItem menuItem6 = menuItem5;
              menuItem6.Click += (RoutedEventHandler) ((sender, args) => this.Remove());
              menuItemList.Add(menuItem2);
              menuItemList.Add(menuItem4);
              menuItemList.Add(menuItem6);
              break;
            }
            break;
          case GroupMembershipType.Member:
          case GroupMembershipType.NotSure:
            switch (this._group.GroupType)
            {
              case GroupType.PublicPage:
                MenuItem menuItem7 = new MenuItem();
                string unfollow = CommonResources.Unfollow;
                menuItem7.Header = (object) unfollow;
                MenuItem menuItem8 = menuItem7;
                menuItem8.Click += (RoutedEventHandler) ((sender, args) => this.Remove());
                menuItemList.Add(menuItem8);
                break;
              case GroupType.Event:
                MenuItem menuItem9 = new MenuItem();
                string eventAttend1 = CommonResources.EventAttend;
                menuItem9.Header = (object) eventAttend1;
                MenuItem menuItem10 = menuItem9;
                menuItem10.Click += (RoutedEventHandler) ((sender, args) => this.Add());
                MenuItem menuItem11 = new MenuItem();
                string eventMaybe1 = CommonResources.EventMaybe;
                menuItem11.Header = (object) eventMaybe1;
                MenuItem menuItem12 = menuItem11;
                menuItem12.Click += (RoutedEventHandler) ((sender, args) => this.Add(true));
                MenuItem menuItem13 = new MenuItem();
                string eventNotAttend1 = CommonResources.EventNotAttend;
                menuItem13.Header = (object) eventNotAttend1;
                MenuItem menuItem14 = menuItem13;
                menuItem14.Click += (RoutedEventHandler) ((sender, args) => this.Remove());
                menuItemList.Add(menuItem10);
                menuItemList.Add(menuItem12);
                menuItemList.Add(menuItem14);
                break;
              default:
                MenuItem menuItem15 = new MenuItem();
                string pageLeaveCommunity = CommonResources.GroupPage_LeaveCommunity;
                menuItem15.Header = (object) pageLeaveCommunity;
                MenuItem menuItem16 = menuItem15;
                menuItem16.Click += (RoutedEventHandler) ((sender, args) =>
                {
                  bool flag = true;
                  if (this._group.Privacy != GroupPrivacy.Public)
                    flag = MessageBox.Show(CommonResources.LeavingClosedCommunity, CommonResources.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
                  if (!flag)
                    return;
                  this.Remove();
                });
                menuItemList.Add(menuItem16);
                break;
            }
            break;
          case GroupMembershipType.RequestSent:
            MenuItem menuItem17 = new MenuItem();
            string cancelRequest = CommonResources.CancelRequest;
            menuItem17.Header = (object) cancelRequest;
            MenuItem menuItem18 = menuItem17;
            menuItem18.Click += (RoutedEventHandler) ((sender, args) => this.Remove());
            menuItemList.Add(menuItem18);
            break;
          case GroupMembershipType.InvitationReceived:
            switch (this._group.GroupType)
            {
              case GroupType.Group:
                if (this._invitedByUser == null)
                {
                  MenuItem menuItem1 = new MenuItem();
                  string lowerInvariant = CommonResources.Group_Join.ToLowerInvariant();
                  menuItem1.Header = (object) lowerInvariant;
                  MenuItem menuItem2 = menuItem1;
                  menuItem2.Click += (RoutedEventHandler) ((sender, args) => this.Add());
                  menuItemList.Add(menuItem2);
                  break;
                }
                break;
              case GroupType.Event:
                MenuItem menuItem19 = new MenuItem();
                string eventAttend2 = CommonResources.EventAttend;
                menuItem19.Header = (object) eventAttend2;
                MenuItem menuItem20 = menuItem19;
                menuItem20.Click += (RoutedEventHandler) ((sender, args) => this.Add());
                MenuItem menuItem21 = new MenuItem();
                string eventMaybe2 = CommonResources.EventMaybe;
                menuItem21.Header = (object) eventMaybe2;
                MenuItem menuItem22 = menuItem21;
                menuItem22.Click += (RoutedEventHandler) ((sender, args) => this.Add(true));
                MenuItem menuItem23 = new MenuItem();
                string eventNotAttend2 = CommonResources.EventNotAttend;
                menuItem23.Header = (object) eventNotAttend2;
                MenuItem menuItem24 = menuItem23;
                menuItem24.Click += (RoutedEventHandler) ((sender, args) => this.Remove());
                menuItemList.Add(menuItem20);
                menuItemList.Add(menuItem22);
                menuItemList.Add(menuItem24);
                break;
            }
            break;
        }
        return (IList<MenuItem>) menuItemList;
      }
    }

    public override bool SupportMultipleAddActions
    {
      get
      {
        if (this._group.MembershipType == GroupMembershipType.NotAMember)
          return this._group.GroupType == GroupType.Event;
        return true;
      }
    }

    public GroupMembershipInfo(GroupData groupData)
    {
      if (groupData == null)
        return;
      this._group = groupData.group;
      VKList<Group> invites = groupData.invites;
      if (invites == null || invites.items == null || invites.profiles == null)
        return;
      Group invitationGroup = invites.items.FirstOrDefault<Group>((Func<Group, bool>) (group => group.id == this._group.id));
      if (invitationGroup == null)
        return;
      this._invitedByUser = invites.profiles.FirstOrDefault<User>((Func<User, bool>) (user => user.id == invitationGroup.invited_by));
    }

    public override void Add()
    {
      this.Add(false);
    }

    private void Add(bool maybe)
    {
      GroupsService.Current.Join(this._group.id, maybe, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        CountersManager.Current.Counters = res.ResultData;
        EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, true));
      }));
    }

    public override void Remove()
    {
      GroupsService.Current.Leave(this._group.id, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        CountersManager.Current.Counters = res.ResultData;
        EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, false));
      }));
    }
  }
}
