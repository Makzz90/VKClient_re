namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersUpdatedEvent
  {
      public StockItemHeader StockItemHeader { get; private set; }

    public StickersUpdatedEvent(StockItemHeader stockItemHeader)
    {
      this.StockItemHeader = stockItemHeader;
    }
  }
}
