using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersPacksReorderedEvent
  {
      public StockItem StockItem { get; set; }

      public int NewIndex { get; set; }

    public StickersPacksReorderedEvent(StockItem stockItem, int newIndex)
    {
      this.NewIndex = newIndex;
      this.StockItem = stockItem;
    }
  }
}
