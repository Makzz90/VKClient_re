using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class AccountIntermediateData : IAccountStickersData
  {
    public int? NewStoreItemsCount { get; set; }

    public bool? HasStickersUpdates { get; set; }

    public VKList<StoreProduct> Products { get; set; }

    public VKList<StockItem> StockItems { get; set; }

    public StoreStickers RecentStickers { get; set; }
  }
}
