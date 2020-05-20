using System;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
    public sealed class InvitationsViewModel : ViewModelBase, ICollectionDataProvider<VKList<User>, LinkHeader>
    {
        public readonly long CommunityId;

        public GenericCollectionViewModel<VKList<User>, LinkHeader> Invitations { get; set; }

        public Func<VKList<User>, ListWithCount<LinkHeader>> ConverterFunc
        {
            get
            {
                return (Func<VKList<User>, ListWithCount<LinkHeader>>)(list =>
                {
                    ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
                    listWithCount.TotalCount = list.count;
                    foreach (User user in list.items)
                        listWithCount.List.Add(new LinkHeader(user, (Action<FrameworkElement>)(sender =>
                        {
                            LinkHeader dataContext = sender.DataContext as LinkHeader;
                            if (dataContext == null)
                                return;
                            this.RemoveInvitation(dataContext);
                        }), "/Resources/Close32px.png"));
                    return listWithCount;
                });
            }
        }

        public InvitationsViewModel(long communityId)
        {
            this.CommunityId = communityId;
            this.Invitations = new GenericCollectionViewModel<VKList<User>, LinkHeader>((ICollectionDataProvider<VKList<User>, LinkHeader>)this)
            {
                LoadCount = 60,
                ReloadCount = 100
            };
        }

        public void GetData(GenericCollectionViewModel<VKList<User>, LinkHeader> caller, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
        {
            GroupsService.Current.GetInvitations(this.CommunityId, offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<VKList<User>, LinkHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoInvitations;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.Communities_InvitationOneFrm, CommonResources.Communities_InvitationTwoFrm, CommonResources.Communities_InvitationFiveFrm, true, (string)null, false);
        }

        public void RemoveInvitation(LinkHeader item)
        {
            this.SetInProgress(true, "");
            GroupsService.Current.HandleRequest(this.CommunityId, item.Id, false, (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                    this.Invitations.Delete(item);
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                this.SetInProgress(false, "");
            }))));
        }
    }
}
