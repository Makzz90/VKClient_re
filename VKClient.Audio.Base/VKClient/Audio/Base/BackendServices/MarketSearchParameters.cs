using System.Collections.Generic;

namespace VKClient.Audio.Base.BackendServices
{
  public class MarketSearchParameters
  {
    public string Query { get; set; }

    public int PriceFrom { get; set; }

    public int PriceTo { get; set; }

    public List<string> Tags { get; set; }

    public MarketSortType Sort { get; set; }

    public bool IsReversedSort { get; set; }
  }
}
