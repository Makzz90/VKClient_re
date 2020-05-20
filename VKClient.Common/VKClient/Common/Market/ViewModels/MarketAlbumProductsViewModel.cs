using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.ViewModels
{
  public class MarketAlbumProductsViewModel : ViewModelBase, ICollectionDataProvider<MarketAlbum, TwoInARowItemViewModel<Product>>
  {
    private string _header = "";
    private readonly long _ownerId;
    private readonly long _albumId;
    private int _productsCount;
    private readonly GenericCollectionViewModel<MarketAlbum, TwoInARowItemViewModel<Product>> _productsVM;

    public GenericCollectionViewModel<MarketAlbum, TwoInARowItemViewModel<Product>> ProductsVM
    {
      get
      {
        return this._productsVM;
      }
    }

    public string Header
    {
      get
      {
        return this._header;
      }
      set
      {
        this._header = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Header));
      }
    }

    public Func<MarketAlbum, ListWithCount<TwoInARowItemViewModel<Product>>> ConverterFunc
    {
      get
      {
        return (Func<MarketAlbum, ListWithCount<TwoInARowItemViewModel<Product>>>) (data =>
        {
          ListWithCount<TwoInARowItemViewModel<Product>> listWithCount = new ListWithCount<TwoInARowItemViewModel<Product>>()
          {
            TotalCount = data.count
          };
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
            listWithCount.List.Add(arowItemViewModel);
          }
          return listWithCount;
        });
      }
    }

    public MarketAlbumProductsViewModel(long ownerId, long albumId)
    {
      this._ownerId = ownerId;
      this._albumId = albumId;
      this._productsVM = new GenericCollectionViewModel<MarketAlbum, TwoInARowItemViewModel<Product>>((ICollectionDataProvider<MarketAlbum, TwoInARowItemViewModel<Product>>) this);
    }

    public void ReloadData()
    {
      this._productsCount = 0;
      this._productsVM.LoadData(true, false, (Action<BackendResult<MarketAlbum, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel<MarketAlbum, TwoInARowItemViewModel<Product>> caller, int offset, int count, Action<BackendResult<MarketAlbum, ResultCode>> callback)
    {
      MarketService.Instance.GetAlbumTitleWithProducts(this._ownerId, this._albumId, count, this._productsCount, (Action<BackendResult<MarketAlbum, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
          this.Header = result.ResultData.Title.ToUpperInvariant();
        callback(result);
      }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<MarketAlbum, TwoInARowItemViewModel<Product>> caller, int count)
    {
      if (this._productsCount == 0)
        return CommonResources.NoProducts;
      return UIStringFormatterHelper.FormatNumberOfSomething(this._productsCount, CommonResources.OneProductFrm, CommonResources.TwoFourProductsFrm, CommonResources.FiveProductsFrm, true, null, false);
    }
  }
}
