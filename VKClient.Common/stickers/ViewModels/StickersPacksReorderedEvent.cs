using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersPacksReorderedEvent
  {
      public StockItem StockItem { get; private set; }

      public int NewIndex { get; private set; }

    public StickersPacksReorderedEvent(StockItem stockItem, int newIndex)
    {
      this.NewIndex = newIndex;
      this.StockItem = stockItem;
    }
  }
}
