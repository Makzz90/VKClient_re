namespace VKClient.Common.Market.ViewModels
{
  public abstract class MarketFeedItem
  {
    public MarketFeedItemType Type { get; private set; }

    public object Data { get; set; }

    protected MarketFeedItem(MarketFeedItemType type)
    {
      this.Type = type;
    }
  }
}
