using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public interface IAccountStickersData
  {
    int? NewStoreItemsCount { get; set; }

    bool? HasStickersUpdates { get; set; }

    VKList<StoreProduct> Products { get; set; }

    VKList<StockItem> StockItems { get; set; }

    StoreStickers RecentStickers { get; set; }
  }
}
