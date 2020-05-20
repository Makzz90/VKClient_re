using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class StoreBuyProductResult
  {
    public int success { get; set; }

    public StoreProduct product { get; set; }

    public List<long> user_ids { get; set; }

    public int withdrawn_votes { get; set; }
  }
}
