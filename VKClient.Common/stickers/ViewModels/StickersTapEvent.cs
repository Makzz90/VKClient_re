namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersTapEvent
  {
      public long StickersProductId { get; private set; }

      public int StickerId { get; private set; }

    public StickersTapEvent(long stickersProductId, int stickerId = 0)
    {
      this.StickersProductId = stickersProductId;
      this.StickerId = stickerId;
    }
  }
}
