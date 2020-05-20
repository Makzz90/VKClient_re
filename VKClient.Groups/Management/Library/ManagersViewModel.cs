using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
    public sealed class ManagersViewModel : ViewModelBase, ICollectionDataProvider<CommunityManagers, LinkHeader>, IHandle<CommunityManagerChanged>, IHandle
    {
        private List<GroupContact> _contacts = new List<GroupContact>();
        public readonly long CommunityId;
        private readonly GroupType _communityType;

        public GenericCollectionViewModel<CommunityManagers, LinkHeader> Managers { get; set; }

        public Func<CommunityManagers, ListWithCount<LinkHeader>> ConverterFunc
        {
            get
            {
                return (Func<CommunityManagers, ListWithCount<LinkHeader>>)(list =>
                {
                    ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
                    listWithCount.TotalCount = list.managers.count;
                    if (!this._contacts.Any<GroupContact>() && list.contacts != null)
                        this._contacts = list.contacts;
                    foreach (User user1 in list.managers.items)
                    {
                        User user = user1;
                        LinkHeader linkHeader = new LinkHeader(user, (Action<FrameworkElement>)(e =>
                        {
                            GroupContact groupContact = this._contacts.FirstOrDefault<GroupContact>((Func<GroupContact, bool>)(c => c.user_id == user.id));
                            Navigator.Current.NavigateToCommunityManagementManagerEditing(this.CommunityId, this._communityType, user, groupContact != null, groupContact != null ? groupContact.desc : (string)null, groupContact != null ? groupContact.email : (string)null, groupContact != null ? groupContact.phone : (string)null);
                        }), "/Resources/Edit32px.png");
                        listWithCount.List.Add(linkHeader);
                    }
                    return listWithCount;
                });
            }
        }

        public long[] ManagerIds
        {
            get
            {
                return this.Managers.Collection.Select<LinkHeader, long>((Func<LinkHeader, long>)(m => m.Id)).ToArray<long>();
            }
        }

        public ManagersViewModel(long communityId, GroupType communityType)
        {
            this.CommunityId = communityId;
            this.Managers = new GenericCollectionViewModel<CommunityManagers, LinkHeader>((ICollectionDataProvider<CommunityManagers, LinkHeader>)this);
            this._communityType = communityType;
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<CommunityManagers, LinkHeader> caller, int offset, int count, Action<BackendResult<CommunityManagers, ResultCode>> callback)
        {
            GroupsService.Current.GetManagers(this.CommunityId, offset, count, offset == 0, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<CommunityManagers, LinkHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoPersons;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, (string)null, false);
        }

        public void Handle(CommunityManagerChanged message)
        {
            if (message.CommunityId != this.CommunityId)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (message.EditingMode == EditingMode.Adding)
                {
                    User user = message.User;
                    user.Role = message.Role;
                    LinkHeader linkHeader = new LinkHeader(user, (Action<FrameworkElement>)(e =>
                    {
                        GroupContact groupContact = this._contacts.FirstOrDefault<GroupContact>((Func<GroupContact, bool>)(c => c.user_id == user.id));
                        Navigator.Current.NavigateToCommunityManagementManagerEditing(this.CommunityId, this._communityType, user, groupContact != null, groupContact != null ? groupContact.desc : (string)null, groupContact != null ? groupContact.email : (string)null, groupContact != null ? groupContact.phone : (string)null);
                    }), "/Resources/Edit32px.png");
                    if (message.IsContact)
                        this._contacts.Add(new GroupContact()
                        {
                            user_id = message.ManagerId,
                            desc = message.Position,
                            email = message.Email,
                            phone = message.Phone
                        });
                    this.Managers.Insert(linkHeader, 0);
                }
                else
                {
                    LinkHeader linkHeader = this.Managers.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(m => m.Id == message.ManagerId));
                    if (linkHeader == null)
                        return;
                    if (message.Role == CommunityManagementRole.Unknown)
                    {
                        this.Managers.Delete(linkHeader);
                    }
                    else
                    {
                        switch (message.Role)
                        {
                            case CommunityManagementRole.Moderator:
                                linkHeader.Description = CommonResources.CommunityManager_Moderator;
                                break;
                            case CommunityManagementRole.Editor:
                                linkHeader.Description = CommonResources.CommunityManager_Editor;
                                break;
                            case CommunityManagementRole.Administrator:
                                linkHeader.Description = CommonResources.CommunityManager_Administrator;
                                break;
                        }
                        linkHeader.User.Role = message.Role;
                        GroupContact groupContact = this._contacts.FirstOrDefault<GroupContact>((Func<GroupContact, bool>)(c => c.user_id == message.ManagerId));
                        if (!message.IsContact)
                        {
                            if (groupContact == null)
                                return;
                            this._contacts.Remove(groupContact);
                        }
                        else if (groupContact != null)
                        {
                            groupContact.desc = message.Position;
                            groupContact.email = message.Email;
                            groupContact.phone = message.Phone;
                        }
                        else
                            this._contacts.Add(new GroupContact()
                            {
                                user_id = message.ManagerId,
                                desc = message.Position,
                                email = message.Email,
                                phone = message.Phone
                            });
                    }
                }
            }));
        }
    }
}
