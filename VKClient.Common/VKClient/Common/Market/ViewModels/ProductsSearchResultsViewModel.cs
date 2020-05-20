using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.ViewModels
{
  public class ProductsSearchResultsViewModel : ViewModelBase, ICollectionDataProvider2<VKList<Product>, TwoInARowItemViewModel<Product>>, ISupportSearchParams, IHandle<SearchParamsUpdated>, IHandle
  {
    private readonly SearchParamsViewModel _searchParamsViewModel;
    private readonly GenericCollectionViewModel2<VKList<Product>, TwoInARowItemViewModel<Product>> _searchVM;
    private string _query;
    private readonly long _ownerId;
    private readonly long _priceFrom;
    private readonly long _priceTo;
    private readonly int _currencyId;
    private readonly string _currencyName;
    private int _productsCount;

    public string ParametersSummaryStr
    {
      get
      {
        return ProductsSearchParamsViewModel.ToPrettyString(this._searchParamsViewModel.SearchParams, this._currencyName);
      }
    }

    public Action OpenParametersPage
    {
      get
      {
        return (Action) (() =>
        {
          ParametersRepository.SetParameterForId("ProductsSearchParams", (object) this._searchParamsViewModel.SearchParams.Copy());
          Navigator.Current.NavigateToProductsSearchParams(this._priceFrom, this._priceTo, this._currencyId, this._currencyName);
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

    public GenericCollectionViewModel2<VKList<Product>, TwoInARowItemViewModel<Product>> SearchVM
    {
      get
      {
        return this._searchVM;
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

    public string SearchParamsStr
    {
      get
      {
        return UsersSearchParamsViewModel.ToPrettyString(this._searchParamsViewModel.SearchParams);
      }
    }

    public string Query
    {
      get
      {
        return this._query;
      }
      set
      {
        int num = !string.IsNullOrWhiteSpace(this._query) ? 0 : (string.IsNullOrWhiteSpace(value) ? 1 : 0);
        this._query = value;
        this.NotifyPropertyChanged("Query");
        if (num != 0)
          return;
        this.ReloadData();
      }
    }

    public Func<VKList<Product>, ListWithCount<TwoInARowItemViewModel<Product>>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Product>, ListWithCount<TwoInARowItemViewModel<Product>>>) (data =>
        {
          ListWithCount<TwoInARowItemViewModel<Product>> listWithCount = new ListWithCount<TwoInARowItemViewModel<Product>>()
          {
            TotalCount = data.count
          };
          foreach (IEnumerable<Product> source in data.items.Partition<Product>(2))
          {
            List<Product> list = source.ToList<Product>();
            TwoInARowItemViewModel<Product> arowItemViewModel = new TwoInARowItemViewModel<Product>()
            {
              Item1 = list[0]
            };
            this._productsCount = this._productsCount + 1;
            if (list.Count > 1)
            {
              arowItemViewModel.Item2 = list[1];
              this._productsCount = this._productsCount + 1;
            }
            listWithCount.List.Add(arowItemViewModel);
          }
          return listWithCount;
        });
      }
    }

    public ProductsSearchResultsViewModel(long ownerId, long priceFrom, long priceTo, int currencyId, string currencyName, SearchParams searchParams = null)
    {
      this._ownerId = ownerId;
      this._priceFrom = priceFrom;
      this._priceTo = priceTo;
      this._currencyId = currencyId;
      this._currencyName = currencyName;
      this._searchVM = new GenericCollectionViewModel2<VKList<Product>, TwoInARowItemViewModel<Product>>((ICollectionDataProvider2<VKList<Product>, TwoInARowItemViewModel<Product>>) this);
      this._searchParamsViewModel = new SearchParamsViewModel((ISupportSearchParams) this);
      if (searchParams != null)
        this._searchParamsViewModel.SearchParams = searchParams;
      EventAggregator.Current.Subscribe((object) this);
    }

    public void ReloadData()
    {
      this._productsCount = 0;
      this._searchVM.LoadData(true, false, true, true, (Action<List<TwoInARowItemViewModel<Product>>>) null, (Action<BackendResult<VKList<Product>, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel2<VKList<Product>, TwoInARowItemViewModel<Product>> caller, int offset, int count, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      MarketService.Instance.Search(this._ownerId, 0L, this._searchParamsViewModel.SearchParams, this._query, count, this._productsCount, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<Product>, TwoInARowItemViewModel<Product>> caller, int count)
    {
      if (this._productsCount <= 0)
        return CommonResources.NoProducts;
      return UIStringFormatterHelper.FormatNumberOfSomething(this._productsCount, CommonResources.OneProductFrm, CommonResources.TwoFourProductsFrm, CommonResources.FiveProductsFrm, true, null, false);
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
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.SearchParamsStr));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ParametersSetVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ParametersChangeVisibility));
    }
  }
}
