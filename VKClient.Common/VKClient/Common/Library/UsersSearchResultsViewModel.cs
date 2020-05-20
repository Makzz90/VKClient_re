using System;
using System.Collections.Generic;
using System.Linq;
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
          ParametersRepository.SetParameterForId("UsersSearchParams", (object) this._searchParamsViewModel.SearchParams.Copy());
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
        return UIStringFormatterHelper.FormatNumberOfSomething(this._searchVM.TotalCount, CommonResources.UsersSearch_OnePersonFoundFrm, CommonResources.UsersSearch_TwoFourPersonsFoundFrm, CommonResources.UsersSearch_FivePersonsFoundFrm, true, null, false).ToUpperInvariant();
      }
    }

    public Visibility ParametersSetVisibility
    {
      get
      {
        return !this._searchParamsViewModel.SearchParams.IsAnySet ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility ParametersChangeVisibility
    {
      get
      {
        return !this._searchParamsViewModel.SearchParams.IsAnySet ? Visibility.Collapsed : Visibility.Visible;
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
        return (Func<VKList<User>, ListWithCount<SubscriptionItemHeader>>) (usersList => new ListWithCount<SubscriptionItemHeader>()
        {
          TotalCount = usersList.count,
          List = new List<SubscriptionItemHeader>(usersList.items.Select<User, SubscriptionItemHeader>((Func<User, SubscriptionItemHeader>) (u => new SubscriptionItemHeader(u, false))))
        });
      }
    }

    public UsersSearchResultsViewModel(string query)
    {
      this._searchVM = new GenericCollectionViewModel2<VKList<User>, SubscriptionItemHeader>((ICollectionDataProvider2<VKList<User>, SubscriptionItemHeader>) this);
      this._searchName = query;
      this._searchParamsViewModel = new SearchParamsViewModel((ISupportSearchParams) this);
      EventAggregator.Current.Subscribe((object) this);
    }

    public void ReloadData()
    {
      this._searchVM.LoadData(true, false, true, true, (Action<List<SubscriptionItemHeader>>) null, (Action<BackendResult<VKList<User>, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UsersFoundCountStr));
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
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
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
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ParametersSetVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ParametersChangeVisibility));
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
      SubscriptionItemHeader subscriptionItemHeader = this._searchVM.Collection.FirstOrDefault<SubscriptionItemHeader>((Func<SubscriptionItemHeader, bool>) (i => i.Id == userId));
      if (subscriptionItemHeader == null)
        return;
      subscriptionItemHeader.UpdateFriendStatus(friendStatus);
    }
  }
}
