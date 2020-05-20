namespace VKClient.Audio.Base.Events
{
  public class MarketContactEvent : StatEventBase
  {
      public string ItemId { get; private set; }

      public MarketContactAction Action { get; private set; }

    public MarketContactEvent(string itemId, MarketContactAction action)
    {
      this.ItemId = itemId;
      this.Action = action;
    }
  }
}
