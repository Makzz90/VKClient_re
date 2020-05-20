namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersTapEvent
  {
      public long StickersProductId { get; set; }

    public int StickerId { get; set; }

    public StickersTapEvent(long stickersProductId, int stickerId = 0)
    {
      this.StickersProductId = stickersProductId;
      this.StickerId = stickerId;
    }
  }
}
