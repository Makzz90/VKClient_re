using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;

namespace VKClient.Groups.Library
{
    public class CommunitySubscribersViewModel : ViewModelBase, ICollectionDataProvider<CommunitySubscribers, LinkHeader>, IHandle<CommunityManagerChanged>, IHandle, IHandle<CommunityBlockChanged>
    {
        private List<GroupContact> _contacts = new List<GroupContact>();
        public List<User> Managers = new List<User>();
        public readonly long CommunityId;
        public readonly GroupType CommunityType;
        private readonly bool _isManagement;
        public GenericCollectionViewModel2<VKList<User>, LinkHeader> SearchViewModel;

        public GenericCollectionViewModel<CommunitySubscribers, LinkHeader> All { get; set; }

        public GenericCollectionViewModel<CommunitySubscribers, LinkHeader> Unsure { get; set; }

        public GenericCollectionViewModel<CommunitySubscribers, LinkHeader> Friends { get; set; }

        public string Title
        {
            get
            {
                if (this.CommunityType != GroupType.PublicPage)
                    return CommonResources.PARTICIPANTS;
                return CommonResources.SUBSCRIBERS;
            }
        }

        public Func<CommunitySubscribers, ListWithCount<LinkHeader>> ConverterFunc
        {
            get
            {
                return (Func<CommunitySubscribers, ListWithCount<LinkHeader>>)(list =>
                {
                    ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
                    listWithCount.TotalCount = list.subscribers.count;
                    if (list.managers != null && list.managers.Any<User>())
                        this.Managers = list.managers;
                    if (list.contacts != null && list.contacts.Any<GroupContact>())
                        this._contacts = list.contacts;
                    CommunityManagementRole currentUserRole = CommunityManagementRole.Unknown;
                    User user1 = this.Managers.FirstOrDefault<User>((Func<User, bool>)(m => m.id == AppGlobalStateManager.Current.LoggedInUserId));
                    if (user1 != null)
                        currentUserRole = user1.Role;
                    foreach (User user2 in list.subscribers.items)
                    {
                        User user = user2;
                        User user3 = this.Managers.FirstOrDefault<User>((Func<User, bool>)(m => m.id == user.id));
                        if (user3 != null)
                            user.Role = user3.Role;
                        LinkHeader linkHeader = new LinkHeader(user, currentUserRole, this._isManagement);
                        listWithCount.List.Add(linkHeader);
                    }
                    return listWithCount;
                });
            }
        }

        public CommunitySubscribersViewModel(long communityId, GroupType communityType, bool isManagement)
        {
            this.CommunityId = communityId;
            this.CommunityType = communityType;
            this._isManagement = isManagement;
            this.All = new GenericCollectionViewModel<CommunitySubscribers, LinkHeader>((ICollectionDataProvider<CommunitySubscribers, LinkHeader>)this);
            this.Unsure = new GenericCollectionViewModel<CommunitySubscribers, LinkHeader>((ICollectionDataProvider<CommunitySubscribers, LinkHeader>)this);
            this.Friends = new GenericCollectionViewModel<CommunitySubscribers, LinkHeader>((ICollectionDataProvider<CommunitySubscribers, LinkHeader>)this);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<CommunitySubscribers, LinkHeader> caller, int offset, int count, Action<BackendResult<CommunitySubscribers, ResultCode>> callback)
        {
            string filter = "all";
            if (caller == this.Unsure)
                filter = "unsure";
            if (caller == this.Friends)
                filter = "friends";
            GroupsService.Current.GetSubscribers(this.CommunityId, offset, count, filter, offset == 0, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<CommunitySubscribers, LinkHeader> caller, int count)
        {
            if (this.CommunityType == GroupType.PublicPage)
            {
                if (count <= 0)
                    return GroupResources.NoSubscribersYet;
                return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneSubscriberFrm, GroupResources.TwoFourSubscribersFrm, GroupResources.FiveSubscribersFrm, true, (string)null, false);
            }
            if (count <= 0)
                return GroupResources.NoParticipantsYet;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneMemberFrm, GroupResources.TwoFourMembersFrm, GroupResources.FiveMembersFrm, true, (string)null, false);
        }

        public void RemoveFromCommunity(LinkHeader user)
        {
            this.SetInProgress(true, "");
            Func<LinkHeader, bool> func1;
            Func<LinkHeader, bool> func2;
            Func<LinkHeader, bool> func3;
            Func<LinkHeader, bool> func4;
            GroupsService.Current.HandleRequest(this.CommunityId, user.Id, false, (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    LinkHeader linkHeader1 = this.All.Collection.FirstOrDefault<LinkHeader>( (func1 = (Func<LinkHeader, bool>)(i => i.Id == user.Id)));
                    if (linkHeader1 != null)
                        this.All.Delete(linkHeader1);
                    LinkHeader linkHeader2 = this.Unsure.Collection.FirstOrDefault<LinkHeader>( (func2 = (Func<LinkHeader, bool>)(i => i.Id == user.Id)));
                    if (linkHeader2 != null)
                        this.Unsure.Delete(linkHeader2);
                    LinkHeader linkHeader3 = this.Friends.Collection.FirstOrDefault<LinkHeader>( (func3 = (Func<LinkHeader, bool>)(i => i.Id == user.Id)));
                    if (linkHeader3 != null)
                        this.Friends.Delete(linkHeader3);
                    GenericCollectionViewModel2<VKList<User>, LinkHeader> searchViewModel = this.SearchViewModel;
                    if ((searchViewModel != null ? searchViewModel.GroupedCollection : (ObservableCollection<Group<LinkHeader>>)null) != null && this.SearchViewModel.GroupedCollection.Count > 1)
                    {
                        LinkHeader linkHeader4 = this.SearchViewModel.GroupedCollection[1].FirstOrDefault<LinkHeader>( (func4 = (Func<LinkHeader, bool>)(i => i.Id == user.Id)));
                        if (linkHeader4 != null)
                            this.SearchViewModel.DeleteGrouped(linkHeader4);
                    }
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                this.SetInProgress(false, "");
            }))));
        }

        private void HandleUserChanged(CommunityManagerChanged message, LinkHeader user)
        {
            GroupContact groupContact = this._contacts.FirstOrDefault<GroupContact>((Func<GroupContact, bool>)(c => c.user_id == message.ManagerId));
            if (message.Role != CommunityManagementRole.Unknown)
            {
                user.User.Role = message.Role;
                user.AddToManagersVisibility = Visibility.Collapsed;
                user.EditVisibility = Visibility.Visible;
                user.BlockVisibility = Visibility.Collapsed;
                if (groupContact != null)
                {
                    if (message.IsContact)
                    {
                        groupContact.desc = message.Position;
                        groupContact.email = message.Email;
                        groupContact.phone = message.Phone;
                    }
                    else
                        this._contacts.Remove(groupContact);
                }
                else if (message.IsContact)
                    this._contacts.Add(new GroupContact()
                    {
                        user_id = user.Id,
                        desc = message.Position,
                        email = message.Email,
                        phone = message.Phone
                    });
            }
            else
            {
                user.User.Role = message.Role;
                user.AddToManagersVisibility = Visibility.Visible;
                user.EditVisibility = Visibility.Collapsed;
                user.BlockVisibility = Visibility.Visible;
                if (groupContact != null)
                    this._contacts.Remove(groupContact);
            }
            user.UpdateRole(message.Role);
        }

        public void NavigateToManagerEditing(LinkHeader item)
        {
            bool isContact = false;
            string position = "";
            string email = "";
            string phone = "";
            GroupContact groupContact = this._contacts.FirstOrDefault<GroupContact>((Func<GroupContact, bool>)(c => c.user_id == item.Id));
            if (groupContact != null)
            {
                isContact = true;
                position = groupContact.desc;
                email = groupContact.email;
                phone = groupContact.phone;
            }
            Navigator.Current.NavigateToCommunityManagementManagerEditing(this.CommunityId, this.CommunityType, item.User, isContact, position, email, phone);
        }

        public void Handle(CommunityManagerChanged message)
        {
            if (message.CommunityId != this.CommunityId)
                return;
            LinkHeader user1 = this.All.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.ManagerId));
            if (user1 != null)
                this.HandleUserChanged(message, user1);
            LinkHeader user2 = this.Unsure.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.ManagerId));
            if (user2 != null)
                this.HandleUserChanged(message, user2);
            LinkHeader user3 = this.Friends.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.ManagerId));
            if (user3 != null)
                this.HandleUserChanged(message, user3);
            GenericCollectionViewModel2<VKList<User>, LinkHeader> searchViewModel = this.SearchViewModel;
            if ((searchViewModel != null ? searchViewModel.GroupedCollection : (ObservableCollection<Group<LinkHeader>>)null) == null || this.SearchViewModel.GroupedCollection.Count <= 1)
                return;
            LinkHeader user4 = this.SearchViewModel.GroupedCollection[1].FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.ManagerId));
            if (user4 == null)
                return;
            this.HandleUserChanged(message, user4);
        }

        public void Handle(CommunityBlockChanged message)
        {
            if (message.CommunityId != this.CommunityId || message.User.ban_info.end_date != 0)
                return;
            LinkHeader linkHeader1 = this.All.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.User.id));
            if (linkHeader1 != null)
                this.All.Delete(linkHeader1);
            LinkHeader linkHeader2 = this.Unsure.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.User.id));
            if (linkHeader2 != null)
                this.Unsure.Delete(linkHeader2);
            LinkHeader linkHeader3 = this.Friends.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.User.id));
            if (linkHeader3 != null)
                this.Friends.Delete(linkHeader3);
            GenericCollectionViewModel2<VKList<User>, LinkHeader> searchViewModel = this.SearchViewModel;
            if ((searchViewModel != null ? searchViewModel.GroupedCollection : (ObservableCollection<Group<LinkHeader>>)null) == null || this.SearchViewModel.GroupedCollection.Count <= 1)
                return;
            LinkHeader linkHeader4 = this.SearchViewModel.GroupedCollection[1].FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.User.id));
            if (linkHeader4 == null)
                return;
            this.SearchViewModel.GroupedCollection[1].Remove(linkHeader4);
        }
    }
}
