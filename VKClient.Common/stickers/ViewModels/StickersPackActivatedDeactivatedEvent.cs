namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersPackActivatedDeactivatedEvent
  {
      public StockItemHeader StockItemHeader { get; private set; }

      public bool IsActive { get; private set; }

    public StickersPackActivatedDeactivatedEvent(StockItemHeader stockItemHeader, bool isActive)
    {
      this.StockItemHeader = stockItemHeader;
      this.IsActive = isActive;
    }
  }
}
