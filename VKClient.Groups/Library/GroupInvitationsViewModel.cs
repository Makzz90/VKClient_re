using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;
using VKClient.Groups.UC;

namespace VKClient.Groups.Library
{
  public class GroupInvitationsViewModel : ViewModelBase, IHandle<GroupMembershipStatusUpdated>, IHandle, ICollectionDataProvider<CommunityInvitationsList, CommunityInvitation>
  {
    public CommunityInvitationsUC ParentCommunityInvitationsUC;
    private readonly GenericCollectionViewModel<CommunityInvitationsList, CommunityInvitation> _invitationsVM;

    public GenericCollectionViewModel<CommunityInvitationsList, CommunityInvitation> InvitationsVM
    {
      get
      {
        return this._invitationsVM;
      }
    }

    public Func<CommunityInvitationsList, ListWithCount<CommunityInvitation>> ConverterFunc
    {
        get
        {
            return (Func<CommunityInvitationsList, ListWithCount<CommunityInvitation>>)(response =>
            {
                ListWithCount<CommunityInvitation> listWithCount = new ListWithCount<CommunityInvitation>();
                foreach (Group invitation1 in response.invitations)
                {
                    Group invitation = invitation1;
                    CommunityInvitation data = new CommunityInvitation()
                    {
                        community = invitation,
                        inviter = ((IEnumerable<User>)response.inviters).First<User>((Func<User, bool>)(u => u.id == invitation.invited_by))
                    };
                    data.InvitationHandledAction = (Action<CommunityInvitations>)(i =>
                    {
                        this._invitationsVM.Delete(data);
                        if (this.ParentCommunityInvitationsUC == null)
                            return;
                        ((GroupsListViewModel)(this.ParentCommunityInvitationsUC).DataContext).InvitationsViewModel = i;
                    });
                    listWithCount.List.Add(data);
                }
                listWithCount.TotalCount = response.count;
                CountersManager.Current.Counters.groups = response.count;
                EventAggregator.Current.Publish((object)new CountersChanged(CountersManager.Current.Counters));
                if (this.ParentCommunityInvitationsUC != null)
                    this.ParentCommunityInvitationsUC.UpdateDataView(response);
                return listWithCount;
            });
        }
    }

    public GroupInvitationsViewModel()
    {
      EventAggregator.Current.Subscribe(this);
      this._invitationsVM = new GenericCollectionViewModel<CommunityInvitationsList, CommunityInvitation>((ICollectionDataProvider<CommunityInvitationsList, CommunityInvitation>) this)
      {
        NeedCollectionCountBeforeFullyLoading = true
      };
    }

    public void LoadInvitations()
    {
      this._invitationsVM.LoadData(false, false,  null, false);
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            CommunityInvitation communityInvitation = this._invitationsVM.Collection.FirstOrDefault<CommunityInvitation>((Func<CommunityInvitation, bool>)(gh => gh.community.id == message.GroupId));
            if (communityInvitation == null)
                return;
            this.InvitationsVM.Delete(communityInvitation);
        }));
    }

    public void GetData(GenericCollectionViewModel<CommunityInvitationsList, CommunityInvitation> caller, int offset, int count, Action<BackendResult<CommunityInvitationsList, ResultCode>> callback)
    {
      GroupsService.Current.GetCommunityInvitations(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<CommunityInvitationsList, CommunityInvitation> caller, int count)
    {
      if (count > 0)
        return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.Communities_InvitationOneFrm, CommonResources.Communities_InvitationTwoFrm, CommonResources.Communities_InvitationFiveFrm, true,  null, false);
      return GroupResources.NoInvitations;
    }
  }
}
