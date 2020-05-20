using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class UsersSearchResultsViewModel : ViewModelBase, ICollectionDataProvider2<VKList<User>, SubscriptionItemHeader>, ISupportSearchParams, IHandle<SearchParamsUpdated>, IHandle, IHandle<FriendRequestSent>, IHandle<SubscriptionCancelled>, IHandle<FriendRequestAcceptedDeclined>, IHandle<FriendRemoved>
  {
    private readonly SearchParamsViewModel _searchParamsViewModel;
    private readonly GenericCollectionViewModel2<VKList<User>, SubscriptionItemHeader> _searchVM;
    private string _searchName;

    public string ParametersSummaryStr
    {
      get
      {
        return UsersSearchParamsViewModel.ToPrettyString(this._searchParamsViewModel.SearchParams);
      }
    }

    public Action OpenParametersPage
    {
      get
      {
        return (Action) (() =>
        {
          ParametersRepository.SetParameterForId("UsersSearchParams", this._searchParamsViewModel.SearchParams.Copy());
          Navigator.Current.NavigateToUsersSearchParams();
        });
      }
    }

    public Action ClearParameters
    {
      get
      {
        return (Action) (() =>
        {
          this._searchParamsViewModel.SearchParams = new SearchParams();
          this.NotifyProperties();
          this.ReloadData();
        });
      }
    }

    public SearchParamsViewModel SearchParamsViewModel
    {
      get
      {
        return this._searchParamsViewModel;
      }
    }

    public GenericCollectionViewModel2<VKList<User>, SubscriptionItemHeader> SearchVM
    {
      get
      {
        return this._searchVM;
      }
    }

    public string UsersFoundCountStr
    {
      get
      {
        if (!this._searchParamsViewModel.SearchParams.IsAnySet)
          return CommonResources.UsersSearch_OnlineNow.ToUpperInvariant();
        return UIStringFormatterHelper.FormatNumberOfSomething(this._searchVM.TotalCount, CommonResources.UsersSearch_OnePersonFoundFrm, CommonResources.UsersSearch_TwoFourPersonsFoundFrm, CommonResources.UsersSearch_FivePersonsFoundFrm, true,  null, false).ToUpperInvariant();
      }
    }

    public Visibility ParametersSetVisibility
    {
      get
      {
        if (!this._searchParamsViewModel.SearchParams.IsAnySet)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Visibility ParametersChangeVisibility
    {
      get
      {
        if (!this._searchParamsViewModel.SearchParams.IsAnySet)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public string SearchName
    {
      get
      {
        return this._searchName;
      }
      set
      {
        int num = !string.IsNullOrWhiteSpace(this._searchName) ? 0 : (string.IsNullOrWhiteSpace(value) ? 1 : 0);
        this._searchName = value;
        this.NotifyPropertyChanged("SearchName");
        if (num != 0)
          return;
        this.ReloadData();
      }
    }

    public Func<VKList<User>, ListWithCount<SubscriptionItemHeader>> ConverterFunc
    {
      get
      {
          return (Func<VKList<User>, ListWithCount<SubscriptionItemHeader>>)(usersList => new ListWithCount<SubscriptionItemHeader>() { TotalCount = usersList.count, List = new List<SubscriptionItemHeader>((IEnumerable<SubscriptionItemHeader>)Enumerable.Select<User, SubscriptionItemHeader>(usersList.items, (Func<User, SubscriptionItemHeader>)(u => new SubscriptionItemHeader(u, false)))) });
      }
    }

    public UsersSearchResultsViewModel(string query)
    {
      this._searchVM = new GenericCollectionViewModel2<VKList<User>, SubscriptionItemHeader>((ICollectionDataProvider2<VKList<User>, SubscriptionItemHeader>) this);
      this._searchName = query;
      this._searchParamsViewModel = new SearchParamsViewModel((ISupportSearchParams) this);
      EventAggregator.Current.Subscribe(this);
    }

    public void ReloadData()
    {
      this._searchVM.LoadData(true, false, true, true,  null, (Action<BackendResult<VKList<User>, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        // ISSUE: type reference
        // ISSUE: method reference
        this.NotifyPropertyChanged<string>(() => this.UsersFoundCountStr);
      }), false);
    }

    public void GetData(GenericCollectionViewModel2<VKList<User>, SubscriptionItemHeader> caller, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      UsersService.Instance.Search(this._searchParamsViewModel.SearchParams, this.SearchName, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<User>, SubscriptionItemHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPersons;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true,  null, false);
    }

    public void Handle(SearchParamsUpdated message)
    {
      if (message.SearchParams == null)
        return;
      this._searchParamsViewModel.SearchParams = message.SearchParams;
      this.NotifyProperties();
      this.ReloadData();
    }

    private void NotifyProperties()
    {
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.ParametersSetVisibility);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.ParametersChangeVisibility);
    }

    public void Handle(FriendRequestSent message)
    {
      this.UpdateUserFriendStatus(message.UserId, 1);
    }

    public void Handle(SubscriptionCancelled message)
    {
      this.UpdateUserFriendStatus(message.UserId, 0);
    }

    public void Handle(FriendRequestAcceptedDeclined message)
    {
      this.UpdateUserFriendStatus(message.UserId, message.Accepted ? 3 : 2);
    }

    public void Handle(FriendRemoved message)
    {
      this.UpdateUserFriendStatus(message.UserId, 2);
    }

    private void UpdateUserFriendStatus(long userId, int friendStatus)
    {
        SubscriptionItemHeader subscriptionItemHeader = (SubscriptionItemHeader)Enumerable.FirstOrDefault<SubscriptionItemHeader>(this._searchVM.Collection, (Func<SubscriptionItemHeader, bool>)(i => i.Id == userId));
      if (subscriptionItemHeader == null)
        return;
      subscriptionItemHeader.UpdateFriendStatus(friendStatus);
    }
  }
}
