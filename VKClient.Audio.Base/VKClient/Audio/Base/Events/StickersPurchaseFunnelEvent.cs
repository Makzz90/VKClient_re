using VKClient.Audio.Base.Library;

namespace VKClient.Audio.Base.Events
{
  public class StickersPurchaseFunnelEvent : StatEventBase
  {
      public StickersPurchaseFunnelSource Source { get; private set; }

      public StickersPurchaseFunnelAction Action { get; private set; }

    public StickersPurchaseFunnelEvent(StickersPurchaseFunnelSource source, StickersPurchaseFunnelAction action)
    {
      this.Source = source;
      this.Action = action;
    }

    public StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction action)
      : this(CurrentStickersPurchaseFunnelSource.Source, action)
    {
    }
  }
}
