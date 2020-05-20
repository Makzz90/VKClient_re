using System.Collections.Generic;

namespace VKMessenger.Backend
{
  public class UpdatesResponse
  {
    public long ts { get; set; }

    public List<LongPollServerUpdateData> Updates { get; set; }
  }
}
