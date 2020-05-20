using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;

namespace VKClient.Groups.Library
{
  public class GroupsListViewModel : ViewModelBase, ICollectionDataProvider<VKList<Group>, GroupHeader>, ICollectionDataProvider<VKList<Group>, Group<GroupHeader>>, IHandle<GroupMembershipStatusUpdated>, IHandle, IHandle<CountersChanged>
  {
    private long _userId;
    private long _excludedId;
    private bool _pickManaged;
    private string _userName;
    private CommunityInvitations _invitationsViewModel;

    public GenericCollectionViewModel<VKList<Group>, GroupHeader> AllVM { get;private set; }

    public GenericCollectionViewModel<VKList<Group>, Group<GroupHeader>> EventsVM { get; private set; }

    public GenericCollectionViewModel<VKList<Group>, GroupHeader> ManagedVM { get; private set; }

    public Visibility EventsCountVisibility
    {
      get
      {
        if (!this.EventsVM.IsLoaded || ((IEnumerable<Group<GroupHeader>>) this.EventsVM.Collection).Any<Group<GroupHeader>>())
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility InvitationsBlockVisibility
    {
      get
      {
        if (this._userId == AppGlobalStateManager.Current.LoggedInUserId)
        {
          CommunityInvitations invitationsViewModel = this.InvitationsViewModel;
          if ((invitationsViewModel != null ? invitationsViewModel.count : 0) > 0)
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
      }
    }

    public CommunityInvitations InvitationsViewModel
    {
      get
      {
        return this._invitationsViewModel;
      }
      set
      {
        this._invitationsViewModel = value;
        this.NotifyPropertyChanged<CommunityInvitations>((Expression<Func<CommunityInvitations>>) (() => this.InvitationsViewModel));
        this.NotifyPropertyChanged<Visibility>((() => this.InvitationsBlockVisibility));
      }
    }

    public SolidColorBrush AllListBackground
    {
      get
      {
        if (((Collection<GroupHeader>) this.AllVM.Collection).Count > 0 || this.InvitationsBlockVisibility == Visibility.Visible)
          return (SolidColorBrush) Application.Current.Resources["PhoneRequestOrInvitationBackgroundBrush"];
        return new SolidColorBrush(Colors.Transparent);
      }
    }

    public SolidColorBrush ManageListBackground
    {
      get
      {
        if (((Collection<GroupHeader>) this.ManagedVM.Collection).Count > 0)
          return (SolidColorBrush) Application.Current.Resources["PhoneRequestOrInvitationBackgroundBrush"];
        return new SolidColorBrush(Colors.Transparent);
      }
    }

    public string Title
    {
      get
      {
        if (this._pickManaged)
          return GroupResources.SELECTAGROUP;
        if (string.IsNullOrEmpty(this._userName))
          return GroupResources.GroupsListPage_Title;
        return string.Format(GroupResources.GroupsListPage_TitleFrm, this._userName).ToUpperInvariant();
      }
    }

    public Func<VKList<Group>, ListWithCount<GroupHeader>> ConverterFunc
{
	get
	{
		return delegate(VKList<Group> communities)
		{
			ListWithCount<GroupHeader> list = new ListWithCount<GroupHeader>();
			list.TotalCount = communities.count;
			ListWithCount<GroupHeader> arg_6F_0 = list;
			IEnumerable<Group> arg_65_0 = Enumerable.Where<Group>(communities.items, delegate(Group g)
			{
				if (g.id == this._excludedId)
				{
					ListWithCount<GroupHeader> expr_19 = list;
					int totalCount = expr_19.TotalCount;
					expr_19.TotalCount = totalCount - 1;
					return false;
				}
				return true;
			});
            Func<Group, GroupHeader> arg_65_1 = new Func<Group, GroupHeader>((g) => { return new GroupHeader(g, null); });
			
			arg_6F_0.List = new List<GroupHeader>(Enumerable.Select<Group, GroupHeader>(arg_65_0, arg_65_1));
			return list;
		};
	}
}

    Func<VKList<Group>, ListWithCount<Group<GroupHeader>>> ICollectionDataProvider<VKList<Group>, Group<GroupHeader>>.ConverterFunc
    {
        get
        {
            return (Func<VKList<Group>, ListWithCount<Group<GroupHeader>>>)(events =>
            {
                List<GroupHeader> pastEvents = new List<GroupHeader>();
                List<GroupHeader> futureEvents = new List<GroupHeader>();
                events.items.ForEach((Action<Group>)(g =>
                {
                    GroupHeader groupHeader = new GroupHeader(g, (User)null);
                    if (groupHeader.PastEvent)
                        pastEvents.Add(groupHeader);
                    else
                        futureEvents.Add(groupHeader);
                }));
                pastEvents = pastEvents.OrderBy<GroupHeader, int>((Func<GroupHeader, int>)(g => -g.Group.start_date)).ToList<GroupHeader>();
                futureEvents = futureEvents.OrderBy<GroupHeader, int>((Func<GroupHeader, int>)(g => g.Group.start_date)).ToList<GroupHeader>();
                ListWithCount<Group<GroupHeader>> listWithCount = new ListWithCount<Group<GroupHeader>>();
                listWithCount.TotalCount = events.count;
                if (futureEvents.Any<GroupHeader>())
                {
                    Group<GroupHeader> group = new Group<GroupHeader>(GroupResources.GroupsListPage_FutureEvents, (IEnumerable<GroupHeader>)futureEvents, false);
                    listWithCount.List.Add(group);
                }
                if (pastEvents.Any<GroupHeader>())
                {
                    Group<GroupHeader> group = new Group<GroupHeader>(GroupResources.GroupsListPage_PastEvents, (IEnumerable<GroupHeader>)pastEvents, false);
                    listWithCount.List.Add(group);
                }
                return listWithCount;
            });
        }
    }

    public GroupsListViewModel(long userId, string userName = "", bool pickManaged = false, long excludedId = 0)
    {
      this._userId = userId;
      this._userName = userName;
      this._pickManaged = pickManaged;
      this._excludedId = excludedId;
      EventAggregator.Current.Subscribe(this);
      this.AllVM = new GenericCollectionViewModel<VKList<Group>, GroupHeader>((ICollectionDataProvider<VKList<Group>, GroupHeader>) this)
      {
        NoContentImage = "../Resources/NoContentImages/Communities.png",
        NoContentText = CommonResources.NoContent_Communities,
        NeedCollectionCountBeforeFullyLoading = true
      };
      this.EventsVM = new GenericCollectionViewModel<VKList<Group>, Group<GroupHeader>>((ICollectionDataProvider<VKList<Group>, Group<GroupHeader>>) this)
      {
        NeedCollectionCountBeforeFullyLoading = true
      };
      this.ManagedVM = new GenericCollectionViewModel<VKList<Group>, GroupHeader>((ICollectionDataProvider<VKList<Group>, GroupHeader>) this)
      {
        NeedCollectionCountBeforeFullyLoading = true
      };
    }

    public void GetData(GenericCollectionViewModel<VKList<Group>, GroupHeader> caller, int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      if (caller == this.AllVM)
      {
        GroupsService.Current.GetUserGroups(this._userId, offset, count, "", (Action<BackendResult<GroupsLists, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            callback(new BackendResult<VKList<Group>, ResultCode>(ResultCode.Succeeded)
            {
              ResultData = result.ResultData.Communities
            });
            this.InvitationsViewModel = result.ResultData.Invitations;
            CountersManager.Current.Counters.groups = this.InvitationsViewModel.count;
            EventAggregator.Current.Publish(new CountersChanged(CountersManager.Current.Counters));
          }
          else
            callback(new BackendResult<VKList<Group>, ResultCode>(result.ResultCode));
          this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.AllListBackground));
        }));
      }
      else
      {
        if (caller != this.ManagedVM)
          return;
        GroupsService.Current.GetUserGroups(this._userId, offset, count, "moder", (Action<BackendResult<GroupsLists, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
            callback(new BackendResult<VKList<Group>, ResultCode>(ResultCode.Succeeded)
            {
              ResultData = result.ResultData.Communities
            });
          else
            callback(new BackendResult<VKList<Group>, ResultCode>(result.ResultCode));
          this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.ManageListBackground));
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Group>, GroupHeader> caller, int count)
    {
      if (count <= 0)
        return GroupResources.NoCommunites;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneCommunityFrm, GroupResources.TwoFourCommunitiesFrm, GroupResources.FiveCommunitiesFrm, true,  null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<Group>, Group<GroupHeader>> caller, int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      GroupsService.Current.GetUserGroups(this._userId, offset, count, "events", (Action<BackendResult<GroupsLists, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
          callback(new BackendResult<VKList<Group>, ResultCode>(ResultCode.Succeeded)
          {
            ResultData = result.ResultData.Communities
          });
        else
          callback(new BackendResult<VKList<Group>, ResultCode>(result.ResultCode));
      }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Group>, Group<GroupHeader>> caller, int count)
    {
      if (count <= 0)
        return GroupResources.NoEvents;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneEventFrm, GroupResources.TwoFourEventsFrm, GroupResources.FiveEventsFrm, true,  null, false);
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      if (this._userId != AppGlobalStateManager.Current.LoggedInUserId)
        return;
      this.AllVM.LoadData(true, true,  null, false);
      this.EventsVM.LoadData(true, true,  null, false);
      this.ManagedVM.LoadData(true, true,  null, false);
      CountersManager.Current.RefreshCounters();
    }

    public void Handle(CountersChanged message)
    {
      this.NotifyPropertyChanged<CommunityInvitations>((Expression<Func<CommunityInvitations>>) (() => this.InvitationsViewModel));
      this.NotifyPropertyChanged<Visibility>((() => this.InvitationsBlockVisibility));
      this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.AllListBackground));
      this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>) (() => this.ManageListBackground));
      this.NotifyPropertyChanged<Visibility>((() => this.EventsCountVisibility));
    }
  }
}
