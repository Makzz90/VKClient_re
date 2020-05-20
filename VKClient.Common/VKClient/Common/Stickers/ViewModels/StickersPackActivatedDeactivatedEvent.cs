namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersPackActivatedDeactivatedEvent
  {
      public StockItemHeader StockItemHeader { get; set; }

    public bool IsActive { get; set; }

    public StickersPackActivatedDeactivatedEvent(StockItemHeader stockItemHeader, bool isActive)
    {
      this.StockItemHeader = stockItemHeader;
      this.IsActive = isActive;
    }
  }
}
