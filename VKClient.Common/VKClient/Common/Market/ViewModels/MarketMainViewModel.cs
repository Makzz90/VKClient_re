using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Market.Views;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.ViewModels
{
  public class MarketMainViewModel : ViewModelBase, ICollectionDataProvider2<MarketFeedResponse, MarketFeedItem>
  {
    private readonly long _ownerId;
    private long _priceFrom;
    private long _priceTo;
    private int _currencyId;
    private string _currencyName;
    private bool _isLoaded;
    private int _productsCount;

    public GenericCollectionViewModel2<MarketFeedResponse, MarketFeedItem> FeedVM { get; set; }

    public Func<MarketFeedResponse, ListWithCount<MarketFeedItem>> ConverterFunc
    {
      get
      {
        return (Func<MarketFeedResponse, ListWithCount<MarketFeedItem>>) (data =>
        {
          ListWithCount<MarketFeedItem> listWithCount = new ListWithCount<MarketFeedItem>();
          if (data.products == null || data.products.items.IsNullOrEmpty())
          {
            listWithCount.TotalCount = 0;
            return listWithCount;
          }
          listWithCount.TotalCount = data.products.count;
          if (!this._isLoaded)
          {
            this._priceFrom = data.priceFrom;
            this._priceTo = data.priceTo;
            this._currencyId = data.currencyId;
            this._currencyName = data.currencyName;
            if (data.albums != null && !data.albums.items.IsNullOrEmpty())
            {
              MarketFeedAlbumsItem marketFeedAlbumsItem = new MarketFeedAlbumsItem()
              {
                Data = (object) data.albums
              };
              listWithCount.List.Add((MarketFeedItem) marketFeedAlbumsItem);
            }
            MarketFeedProductsHeaderItem productsHeaderItem = new MarketFeedProductsHeaderItem()
            {
              Data = (object) data.products.count
            };
            listWithCount.List.Add((MarketFeedItem) productsHeaderItem);
            this._isLoaded = true;
          }
          foreach (IEnumerable<Product> source in data.products.items.Partition<Product>(2))
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
            MarketFeedProductItem marketFeedProductItem = new MarketFeedProductItem()
            {
              Data = (object) arowItemViewModel
            };
            listWithCount.List.Add((MarketFeedItem) marketFeedProductItem);
          }
          return listWithCount;
        });
      }
    }

    public MarketMainViewModel(long ownerId)
    {
      this._ownerId = ownerId;
      this.FeedVM = new GenericCollectionViewModel2<MarketFeedResponse, MarketFeedItem>((ICollectionDataProvider2<MarketFeedResponse, MarketFeedItem>) this);
    }

    public void ReloadData()
    {
      this._isLoaded = false;
      this._productsCount = 0;
      this.FeedVM.LoadData(true, false, false, false, (Action<List<MarketFeedItem>>) null, (Action<BackendResult<MarketFeedResponse, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel2<MarketFeedResponse, MarketFeedItem> caller, int offset, int count, Action<BackendResult<MarketFeedResponse, ResultCode>> callback)
    {
      MarketService.Instance.GetFeed(this._ownerId, count, this._productsCount, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<MarketFeedResponse, MarketFeedItem> caller, int count)
    {
      if (this._productsCount == 0)
        return CommonResources.NoProducts;
      return UIStringFormatterHelper.FormatNumberOfSomething(this._productsCount, CommonResources.OneProductFrm, CommonResources.TwoFourProductsFrm, CommonResources.FiveProductsFrm, true, null, false);
    }

    public void ShowSearch()
    {
      if (!this._isLoaded)
        return;
      ProductsSearchResultsUC.Show(this._ownerId, this._priceFrom, this._priceTo, this._currencyId, this._currencyName, (SearchParams) null);
    }

    public void OpenSearchParams()
    {
      if (!this._isLoaded)
        return;
      Navigator.Current.NavigateToProductsSearchParams(this._priceFrom, this._priceTo, this._currencyId, this._currencyName);
    }
  }
}
