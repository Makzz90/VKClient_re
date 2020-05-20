using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftSentResponse
  {
    public int success { get; set; }

    public List<long> user_ids { get; set; }

    public int widthdrawn_votes { get; set; }
  }
}
