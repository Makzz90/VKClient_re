namespace VKClient.Audio.Base.Events
{
  public class GiftsPurchaseStepsEvent : StatEventBase
  {
      public GiftPurchaseStepsSource Source { get; private set; }

      public GiftPurchaseStepsAction Action { get; private set; }

    public GiftsPurchaseStepsEvent(GiftPurchaseStepsSource source, GiftPurchaseStepsAction action)
    {
      this.Source = source;
      this.Action = action;
    }
  }
}
