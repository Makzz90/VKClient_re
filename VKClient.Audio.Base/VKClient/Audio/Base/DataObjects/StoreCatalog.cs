using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class StoreCatalog : IAccountStickersData
  {
    public List<StoreBanner> banners { get; set; }

    public List<StoreSection> sections { get; set; }

    public int? NewStoreItemsCount { get; set; }

    public bool? HasStickersUpdates { get; set; }

    public VKList<StoreProduct> Products { get; set; }

    public VKList<StockItem> StockItems { get; set; }

    public StoreStickers RecentStickers { get; set; }
  }
}
