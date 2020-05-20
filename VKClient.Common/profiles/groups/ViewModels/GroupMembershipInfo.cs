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
                if (this._invitedByUser == null)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public override string InvitationStr
        {
            get
            {
                if (this._invitedByUser == null)
                    return "";
                return string.Format("[id{0}|{1}] {2}", this._invitedByUser.id, this._invitedByUser.Name, this.InvitedToCommunityStr);
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
                if (this._group == null)
                    return Visibility.Collapsed;
                if (this._group.can_message != 1)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public override Visibility VisibilityButtonPrimary
        {
            get
            {
                if (this._group == null || this._invitedByUser != null || this._invitedByUser != null)
                    return Visibility.Collapsed;
                if (!this._group.CanJoin || this._group.MembershipType == GroupMembershipType.RequestSent)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public override Visibility VisibilityButtonSecondary
        {
            get
            {
                if (this._group == null || this._invitedByUser != null || (this._group.GroupType != GroupType.Event || this._group.start_date == 0))
                    return Visibility.Collapsed;
                if (!(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double)Math.Max(this._group.start_date, this._group.finish_date), true) > DateTime.Now))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public override Visibility VisibilityButtonSecondaryAction
        {
            get
            {
                if (this._group == null || this._invitedByUser != null)
                    return Visibility.Collapsed;
                if (this._group.MembershipType == GroupMembershipType.NotAMember && (this._group.Privacy == GroupPrivacy.Private || this._group.ban_info != null && this._group.ban_info.end_date == 0L))
                    return Visibility.Collapsed;
                if (this.VisibilityButtonPrimary != Visibility.Visible && this.VisibilityButtonSecondary != Visibility.Visible)
                    return Visibility.Visible;
                return Visibility.Collapsed;
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
                    return new Action(((MembershipInfoBase)this).Remove);
                return new Action(((MembershipInfoBase)this).Add);
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
                {
                    return new List<MenuItem>();
                }
                List<MenuItem> list = new List<MenuItem>();
                switch (this._group.MembershipType)
                {
                    case GroupMembershipType.NotAMember:
                        {
                            GroupType groupType = this._group.GroupType;
                            if (groupType == GroupType.Event)
                            {
                                MenuItem menuItem = new MenuItem
                                {
                                    Header = CommonResources.EventAttend
                                };
                                menuItem.Click += delegate(object sender, RoutedEventArgs args)
                                {
                                    this.Add();
                                };
                                MenuItem menuItem2 = new MenuItem
                                {
                                    Header = CommonResources.EventMaybe
                                };
                                menuItem2.Click += delegate(object sender, RoutedEventArgs args)
                                {
                                    this.Add(true);
                                };
                                MenuItem menuItem3 = new MenuItem
                                {
                                    Header = CommonResources.EventNotAttend
                                };
                                menuItem3.Click += delegate(object sender, RoutedEventArgs args)
                                {
                                    this.Remove();
                                };
                                list.Add(menuItem);
                                list.Add(menuItem2);
                                list.Add(menuItem3);
                            }
                            break;
                        }
                    case GroupMembershipType.Member:
                    case GroupMembershipType.NotSure:
                        {
                            GroupType groupType = this._group.GroupType;
                            if (groupType != GroupType.PublicPage)
                            {
                                if (groupType != GroupType.Event)
                                {
                                    MenuItem menuItem4 = new MenuItem
                                    {
                                        Header = CommonResources.GroupPage_LeaveCommunity
                                    };
                                    menuItem4.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        bool flag = true;
                                        if (this._group.Privacy != GroupPrivacy.Public)
                                        {
                                            flag = (MessageBox.Show(CommonResources.LeavingClosedCommunity, CommonResources.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK);
                                        }
                                        if (flag)
                                        {
                                            this.Remove();
                                        }
                                    };
                                    list.Add(menuItem4);
                                }
                                else
                                {
                                    MenuItem menuItem5 = new MenuItem
                                    {
                                        Header = CommonResources.EventAttend
                                    };
                                    menuItem5.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        this.Add();
                                    };
                                    MenuItem menuItem6 = new MenuItem
                                    {
                                        Header = CommonResources.EventMaybe
                                    };
                                    menuItem6.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        this.Add(true);
                                    };
                                    MenuItem menuItem7 = new MenuItem
                                    {
                                        Header = CommonResources.EventNotAttend
                                    };
                                    menuItem7.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        this.Remove();
                                    };
                                    list.Add(menuItem5);
                                    list.Add(menuItem6);
                                    list.Add(menuItem7);
                                }
                            }
                            else
                            {
                                MenuItem menuItem8 = new MenuItem
                                {
                                    Header = CommonResources.Unfollow
                                };
                                menuItem8.Click += delegate(object sender, RoutedEventArgs args)
                                {
                                    this.Remove();
                                };
                                list.Add(menuItem8);
                            }
                            break;
                        }
                    case GroupMembershipType.RequestSent:
                        {
                            MenuItem menuItem9 = new MenuItem
                            {
                                Header = CommonResources.CancelRequest
                            };
                            menuItem9.Click += delegate(object sender, RoutedEventArgs args)
                            {
                                this.Remove();
                            };
                            list.Add(menuItem9);
                            break;
                        }
                    case GroupMembershipType.InvitationReceived:
                        {
                            GroupType groupType = this._group.GroupType;
                            if (groupType != GroupType.Group)
                            {
                                if (groupType == GroupType.Event)
                                {
                                    MenuItem menuItem10 = new MenuItem
                                    {
                                        Header = CommonResources.EventAttend
                                    };
                                    menuItem10.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        this.Add();
                                    };
                                    MenuItem menuItem11 = new MenuItem
                                    {
                                        Header = CommonResources.EventMaybe
                                    };
                                    menuItem11.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        this.Add(true);
                                    };
                                    MenuItem menuItem12 = new MenuItem
                                    {
                                        Header = CommonResources.EventNotAttend
                                    };
                                    menuItem12.Click += delegate(object sender, RoutedEventArgs args)
                                    {
                                        this.Remove();
                                    };
                                    list.Add(menuItem10);
                                    list.Add(menuItem11);
                                    list.Add(menuItem12);
                                }
                            }
                            else if (this._invitedByUser == null)
                            {
                                MenuItem menuItem13 = new MenuItem
                                {
                                    Header = CommonResources.Group_Join.ToLowerInvariant()
                                };
                                menuItem13.Click += delegate(object sender, RoutedEventArgs args)
                                {
                                    this.Add();
                                };
                                list.Add(menuItem13);
                            }
                            break;
                        }
                }
                return list;
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
            Group invitationGroup = (Group)Enumerable.FirstOrDefault<Group>(invites.items, (Func<Group, bool>)(group => group.id == this._group.id));
            if (invitationGroup == null)
                return;
            this._invitedByUser = (User)Enumerable.FirstOrDefault<User>(invites.profiles, (Func<User, bool>)(user => user.id == invitationGroup.invited_by));
        }

        public override void Add()
        {
            this.Add(false);
        }

        private void Add(bool maybe)
        {
            GroupsService.Current.Join(this._group.id, maybe, (Action<BackendResult<OwnCounters, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                CountersManager.Current.Counters = res.ResultData;
                EventAggregator.Current.Publish(new GroupMembershipStatusUpdated(this._group.id, true));
            }), null);
        }

        public override void Remove()
        {
            GroupsService.Current.Leave(this._group.id, (Action<BackendResult<OwnCounters, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                CountersManager.Current.Counters = res.ResultData;
                EventAggregator.Current.Publish(new GroupMembershipStatusUpdated(this._group.id, false));
            }));
        }
    }
}
