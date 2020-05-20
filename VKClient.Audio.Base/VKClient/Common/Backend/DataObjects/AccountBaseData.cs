using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class AccountBaseData : IAccountStickersData
  {
    public User User { get; set; }

    public OwnCounters OwnCounters { get; set; }

    public NewsFeedData NewsData { get; set; }

    public int time { get; set; }

    public AccountBaseInfo Info { get; set; }

    public int GamesSectionEnabled { get; set; }

    public int DebugDisabled { get; set; }

    public int? NewStoreItemsCount { get; set; }

    public bool? HasStickersUpdates { get; set; }

    public VKList<StoreProduct> Products { get; set; }

    public VKList<StockItem> StockItems { get; set; }

    public StoreStickers RecentStickers { get; set; }
  }
}
