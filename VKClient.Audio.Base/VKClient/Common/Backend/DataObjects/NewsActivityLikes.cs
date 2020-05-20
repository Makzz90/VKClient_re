using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class NewsActivityLikes
  {
    public List<long> user_ids { get; set; }

    public string text { get; set; }
  }
}
