using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class AppEventMarketItemAction : AppEventBase
  {
    public override string e
    {
      get
      {
        return "open_market_item";
      }
    }

    public List<string> item_ids { get; set; }

    public string source { get; set; }
  }
}
