namespace VKClient.Common.Backend
{
  public class AppEventGiftsPurchaseSteps : AppEventBase
  {
    public override string e
    {
      get
      {
        return "gift_purchase_steps";
      }
    }

    public string source { get; set; }

    public string action { get; set; }

    public override string ToString()
    {
      return string.Format("{0}: {1}", this.source, this.action);
    }
  }
}
