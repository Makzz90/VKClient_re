namespace VKClient.Common.Backend
{
  public class AppEventMarketContact : AppEventBase
  {
    public override string e
    {
      get
      {
        return "market_contact";
      }
    }

    public string item_id { get; set; }

    public string action { get; set; }
  }
}
