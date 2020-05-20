namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersPackPurchasedEvent
  {
      public StockItemHeader StockItemHeader { get; private set; }

    public bool IsGift { get; set; }

    public StickersPackPurchasedEvent(StockItemHeader stockItemHeader)
    {
      this.StockItemHeader = stockItemHeader;
    }
  }
}
