using System;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
    public sealed class BlacklistViewModel : ViewModelBase, ICollectionDataProvider<BlockedUsers, LinkHeader>, IHandle<CommunityBlockChanged>, IHandle
    {
        public readonly long CommunityId;

        public GenericCollectionViewModel<BlockedUsers, LinkHeader> Users { get; set; }

        public Func<BlockedUsers, ListWithCount<LinkHeader>> ConverterFunc
        {
            get
            {
                return (Func<BlockedUsers, ListWithCount<LinkHeader>>)(list =>
                {
                    ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
                    listWithCount.TotalCount = list.blocked_users.count;
                    foreach (User user1 in list.blocked_users.items)
                    {
                        User user = user1;
                        LinkHeader linkHeader = new LinkHeader(user);
                        listWithCount.List.Add(linkHeader);
                        User user2 = list.managers.First<User>((Func<User, bool>)(m => m.id == user.ban_info.admin_id));
                        linkHeader.Description = string.Format("{0} {1}", user2.sex != 1 ? (object)CommonResources.Photo_AddedMale : (object)CommonResources.Photo_AddedFemale, (object)user2.Name);
                        linkHeader.User.ban_info.manager = user2;
                    }
                    return listWithCount;
                });
            }
        }

        public BlacklistViewModel(long communityId)
        {
            this.CommunityId = communityId;
            this.Users = new GenericCollectionViewModel<BlockedUsers, LinkHeader>((ICollectionDataProvider<BlockedUsers, LinkHeader>)this)
            {
                LoadCount = 60,
                ReloadCount = 100
            };
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<BlockedUsers, LinkHeader> caller, int offset, int count, Action<BackendResult<BlockedUsers, ResultCode>> callback)
        {
            GroupsService.Current.GetBlacklist(this.CommunityId, offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<BlockedUsers, LinkHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoPersons;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, (string)null, false);
        }

        public void UnblockUser(LinkHeader item)
        {
            this.SetInProgress(true, "");
            GroupsService.Current.UnblockUser(this.CommunityId, item.Id, (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                    this.Users.Delete(item);
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                this.SetInProgress(false, "");
            }))));
        }

        public void Handle(CommunityBlockChanged message)
        {
            if (message.CommunityId != this.CommunityId)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
                LinkHeader linkHeader = this.Users.Collection.FirstOrDefault<LinkHeader>((Func<LinkHeader, bool>)(u => u.Id == message.User.id));
                if (!message.IsEditing && linkHeader == null)
                {
                    this.Users.Insert(new LinkHeader(message.User)
                    {
                        Description = string.Format("{0} {1}", loggedInUser.sex != 1 ? (object)CommonResources.Photo_AddedMale : (object)CommonResources.Photo_AddedFemale, (object)loggedInUser.Name)
                    }, 0);
                }
                else
                {
                    if (linkHeader == null)
                        return;
                    linkHeader.Description = string.Format("{0} {1}", loggedInUser.sex != 1 ? (object)CommonResources.Photo_AddedMale : (object)CommonResources.Photo_AddedFemale, (object)loggedInUser.Name);
                    if (message.IsEditing)
                        return;
                    linkHeader.User = message.User;
                    this.Users.Delete(linkHeader);
                    this.Users.Insert(linkHeader, 0);
                }
            }));
        }
    }
}
