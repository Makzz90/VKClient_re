using System;
using System.Collections.Generic;
using System.Linq;
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
  public class MarketAlbumsViewModel : ViewModelBase, ICollectionDataProvider<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>>
  {
    private readonly long _ownerId;
    private int _albumsCount;
    private readonly GenericCollectionViewModel<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>> _albumsVM;

    public GenericCollectionViewModel<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>> AlbumsVM
    {
      get
      {
        return this._albumsVM;
      }
    }

    public Func<VKList<MarketAlbum>, ListWithCount<TwoInARowItemViewModel<MarketAlbum>>> ConverterFunc
    {
      get
      {
        return (Func<VKList<MarketAlbum>, ListWithCount<TwoInARowItemViewModel<MarketAlbum>>>) (data =>
        {
          ListWithCount<TwoInARowItemViewModel<MarketAlbum>> listWithCount = new ListWithCount<TwoInARowItemViewModel<MarketAlbum>>()
          {
            TotalCount = data.count
          };
          foreach (IEnumerable<MarketAlbum> source in data.items.Partition<MarketAlbum>(2))
          {
            List<MarketAlbum> list = source.ToList<MarketAlbum>();
            TwoInARowItemViewModel<MarketAlbum> arowItemViewModel = new TwoInARowItemViewModel<MarketAlbum>()
            {
              Item1 = list[0]
            };
            this._albumsCount = this._albumsCount + 1;
            if (list.Count > 1)
            {
              arowItemViewModel.Item2 = list[1];
              this._albumsCount = this._albumsCount + 1;
            }
            listWithCount.List.Add(arowItemViewModel);
          }
          return listWithCount;
        });
      }
    }

    public MarketAlbumsViewModel(long ownerId)
    {
      this._ownerId = ownerId;
      this._albumsVM = new GenericCollectionViewModel<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>>((ICollectionDataProvider<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>>) this);
    }

    public void ReloadData()
    {
      this._albumsCount = 0;
      this._albumsVM.LoadData(true, false, (Action<BackendResult<VKList<MarketAlbum>, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>> caller, int offset, int count, Action<BackendResult<VKList<MarketAlbum>, ResultCode>> callback)
    {
      MarketService.Instance.GetAlbums(this._ownerId, count, this._albumsCount, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<MarketAlbum>, TwoInARowItemViewModel<MarketAlbum>> caller, int count)
    {
      if (this._albumsCount == 0)
        return CommonResources.NoMarketAlbums;
      return UIStringFormatterHelper.FormatNumberOfSomething(this._albumsCount, CommonResources.OneMarketAlbumFrm, CommonResources.TwoFourMarketAlbumsFrm, CommonResources.FiveMarketAlbums, true, null, false);
    }
  }
}
