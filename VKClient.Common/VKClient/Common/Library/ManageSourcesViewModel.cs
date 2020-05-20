using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Common.Library
{
  public class ManageSourcesViewModel : ViewModelBase, IHandle<UserIsSubcribedUnsubscribedToEvent>, IHandle, IHandle<GroupSubscribedUnsubscribedEvent>, ICollectionDataProvider<ProfilesAndGroups, FriendHeader>, ICollectionDataProvider<ProfilesAndGroups, GroupHeader>
  {
    private GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> _friendsVM;
    private GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> _groupsVM;
    private AsyncHelper<BackendResult<ProfilesAndGroups, ResultCode>> _helperGetBanned;
    private int _selectedCount;
    private ManageSourcesMode _manageSourcesMode;

    public GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> FriendsVM
    {
      get
      {
        return this._friendsVM;
      }
    }

    public string Title
    {
      get
      {
        if (this._manageSourcesMode != ManageSourcesMode.ManageHiddenNewsSources)
          return CommonResources.Settings_Nofications_Sources.ToUpperInvariant();
        return CommonResources.HiddenSources.ToUpperInvariant();
      }
    }

    public GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> GroupsVM
    {
      get
      {
        return this._groupsVM;
      }
    }

    public int SelectedCount
    {
      get
      {
        return this._selectedCount;
      }
      set
      {
        this._selectedCount = value;
        this.NotifyPropertyChanged<int>((Expression<Func<int>>) (() => this.SelectedCount));
      }
    }

    public Func<ProfilesAndGroups, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
        return (Func<ProfilesAndGroups, ListWithCount<FriendHeader>>) (pg =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
          listWithCount.TotalCount = pg.profiles.Count;
          listWithCount.List = pg.profiles.Select<User, FriendHeader>((Func<User, FriendHeader>) (p => new FriendHeader(p, false)
          {
            IsInSelectedState = true
          })).ToList<FriendHeader>();
          foreach (ViewModelBase viewModelBase in listWithCount.List)
            viewModelBase.PropertyChanged += new PropertyChangedEventHandler(this.p_PropertyChanged);
          return listWithCount;
        });
      }
    }

    Func<ProfilesAndGroups, ListWithCount<GroupHeader>> ICollectionDataProvider<ProfilesAndGroups, GroupHeader>.ConverterFunc
    {
      get
      {
        return (Func<ProfilesAndGroups, ListWithCount<GroupHeader>>) (pg =>
        {
          ListWithCount<GroupHeader> listWithCount = new ListWithCount<GroupHeader>();
          listWithCount.TotalCount = pg.groups.Count;
          listWithCount.List = pg.groups.Select<Group, GroupHeader>((Func<Group, GroupHeader>) (g => new GroupHeader(g, (User) null))).ToList<GroupHeader>();
          foreach (ViewModelBase viewModelBase in listWithCount.List)
            viewModelBase.PropertyChanged += new PropertyChangedEventHandler(this.p_PropertyChanged);
          return listWithCount;
        });
      }
    }

    public ManageSourcesViewModel(ManageSourcesMode mode)
    {
      this._manageSourcesMode = mode;
      this._friendsVM = new GenericCollectionViewModel<ProfilesAndGroups, FriendHeader>((ICollectionDataProvider<ProfilesAndGroups, FriendHeader>) this);
      this._groupsVM = new GenericCollectionViewModel<ProfilesAndGroups, GroupHeader>((ICollectionDataProvider<ProfilesAndGroups, GroupHeader>) this);
      if (this._manageSourcesMode == ManageSourcesMode.ManagePushNotificationsSources)
      {
        this._friendsVM.NoItemsDescription = string.Format(CommonResources.NewsNotifications_Desc, (object) (Environment.NewLine + Environment.NewLine));
        this._groupsVM.NoItemsDescription = string.Format(CommonResources.NewsNotifications_Desc, (object) (Environment.NewLine + Environment.NewLine));
      }
      this._helperGetBanned = new AsyncHelper<BackendResult<ProfilesAndGroups, ResultCode>>((Action<Action<BackendResult<ProfilesAndGroups, ResultCode>>>) (a => NewsFeedService.Current.GetBanned(a)));
      EventAggregator.Current.Subscribe((object) this);
    }

    public void DeleteSelected()
    {
      List<FriendHeader> list1 = this._friendsVM.Collection.Where<FriendHeader>((Func<FriendHeader, bool>) (fh => fh.IsSelected)).ToList<FriendHeader>();
      List<GroupHeader> list2 = this._groupsVM.Collection.Where<GroupHeader>((Func<GroupHeader, bool>) (gh => gh.IsSelected)).ToList<GroupHeader>();
      switch (this._manageSourcesMode)
      {
        case ManageSourcesMode.ManageHiddenNewsSources:
          NewsFeedService.Current.DeleteBan(list1.Select<FriendHeader, long>((Func<FriendHeader, long>) (fh => fh.UserId)).ToList<long>(), list2.Select<GroupHeader, long>((Func<GroupHeader, long>) (g => g.Group.id)).ToList<long>(), (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
          {
            if (res.ResultCode != ResultCode.Succeeded)
              return;
            Execute.ExecuteOnUIThread((Action) (() =>
            {
              EventAggregator.Current.Publish((object) new HiddenNewsSourcesCountUpdated()
              {
                UpdatedCount = (this._friendsVM.Collection.Count + this._groupsVM.Collection.Count)
              });
              if (!(NewsViewModel.Instance.NewsSource.Alias == "news"))
                return;
              NewsViewModel.Instance.ReloadNews(true, true, false);
            }));
          }));
          break;
        case ManageSourcesMode.ManagePushNotificationsSources:
          this.DoUnsubscribe(list1.Select<FriendHeader, long>((Func<FriendHeader, long>) (fh => fh.UserId)).Union<long>(list2.Select<GroupHeader, long>((Func<GroupHeader, long>) (g => -g.Group.id))).ToList<long>().Partition<long>(25), 0);
          break;
      }
      foreach (FriendHeader friendHeader in list1)
        this._friendsVM.Delete(friendHeader);
      foreach (GroupHeader groupHeader in list2)
        this._groupsVM.Delete(groupHeader);
      this.SelectedCount = this.SelectedCount - (list1.Count + list2.Count);
    }

    private void DoUnsubscribe(IEnumerable<IEnumerable<long>> listsToBeUnsubscribed, int ind)
    {
      if (ind >= listsToBeUnsubscribed.Count<IEnumerable<long>>())
        return;
      WallService.Current.WallSubscriptionsUnsubscribe(listsToBeUnsubscribed.ToList<IEnumerable<long>>()[ind].ToList<long>(), (Action<BackendResult<ResponseWithId, ResultCode>>) (unsubscribeRes => this.DoUnsubscribe(listsToBeUnsubscribed, ind + 1)));
    }

    public void GetData(GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> caller, int offset, int count, Action<BackendResult<ProfilesAndGroups, ResultCode>> callback)
    {
      switch (this._manageSourcesMode)
      {
        case ManageSourcesMode.ManageHiddenNewsSources:
          this._helperGetBanned.RunAction(callback, false);
          break;
        case ManageSourcesMode.ManagePushNotificationsSources:
          WallService.Current.GetWallSubscriptionsProfiles(offset, count, (Action<BackendResult<VKList<User>, ResultCode>>) (res =>
          {
            ProfilesAndGroups resultData = (ProfilesAndGroups) null;
            if (res.ResultCode == ResultCode.Succeeded)
              resultData = new ProfilesAndGroups()
              {
                profiles = res.ResultData.items
              };
            callback(new BackendResult<ProfilesAndGroups, ResultCode>(res.ResultCode, resultData));
          }));
          break;
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<ProfilesAndGroups, FriendHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPersons;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> caller, int offset, int count, Action<BackendResult<ProfilesAndGroups, ResultCode>> callback)
    {
      switch (this._manageSourcesMode)
      {
        case ManageSourcesMode.ManageHiddenNewsSources:
          this._helperGetBanned.RunAction(callback, false);
          break;
        case ManageSourcesMode.ManagePushNotificationsSources:
          WallService.Current.GetWallSubscriptionsGroups(offset, count, (Action<BackendResult<VKList<Group>, ResultCode>>) (res =>
          {
            ProfilesAndGroups resultData = (ProfilesAndGroups) null;
            if (res.ResultCode == ResultCode.Succeeded)
              resultData = new ProfilesAndGroups()
              {
                groups = res.ResultData.items
              };
            callback(new BackendResult<ProfilesAndGroups, ResultCode>(res.ResultCode, resultData));
          }));
          break;
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<ProfilesAndGroups, GroupHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoCommunites;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCommunityFrm, CommonResources.TwoFourCommunitiesFrm, CommonResources.FiveCommunitiesFrm, true, null, false);
    }

    private void p_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "IsSelected"))
        return;
      bool flag = false;
      if (sender is FriendHeader)
        flag = (sender as FriendHeader).IsSelected;
      if (sender is GroupHeader)
        flag = (sender as GroupHeader).IsSelected;
      if (flag)
        this.SelectedCount = this.SelectedCount + 1;
      else
        this.SelectedCount = this.SelectedCount - 1;
    }

    public void Handle(UserIsSubcribedUnsubscribedToEvent message)
    {
      if (this._manageSourcesMode != ManageSourcesMode.ManagePushNotificationsSources)
        return;
      if (message.IsSubscribed)
      {
        this.FriendsVM.LoadData(true, false, (Action<BackendResult<ProfilesAndGroups, ResultCode>>) null, false);
      }
      else
      {
        FriendHeader friendHeader = this.FriendsVM.Collection.FirstOrDefault<FriendHeader>((Func<FriendHeader, bool>) (fh => fh.UserId == message.user.id));
        if (friendHeader == null)
          return;
        this.FriendsVM.Delete(friendHeader);
      }
    }

    public void Handle(GroupSubscribedUnsubscribedEvent message)
    {
      if (this._manageSourcesMode != ManageSourcesMode.ManagePushNotificationsSources)
        return;
      if (message.IsSubscribed)
      {
        this.GroupsVM.LoadData(true, false, (Action<BackendResult<ProfilesAndGroups, ResultCode>>) null, false);
      }
      else
      {
        GroupHeader groupHeader = this.GroupsVM.Collection.FirstOrDefault<GroupHeader>((Func<GroupHeader, bool>) (fh => fh.Group.id == message.group.id));
        if (groupHeader == null)
          return;
        this.GroupsVM.Delete(groupHeader);
      }
    }
  }
}
