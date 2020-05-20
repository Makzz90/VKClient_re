namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersPackPurchasedEvent
  {
      public StockItemHeader StockItemHeader { get; set; }

    public bool IsGift { get; set; }

    public StickersPackPurchasedEvent(StockItemHeader stockItemHeader)
    {
      this.StockItemHeader = stockItemHeader;
    }
  }
}
