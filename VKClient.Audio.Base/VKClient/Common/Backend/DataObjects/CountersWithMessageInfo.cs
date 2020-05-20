using VKMessenger.Backend;

namespace VKClient.Common.Backend.DataObjects
{
  public class CountersWithMessageInfo
  {
    public Message LastMessage { get; set; }

    public User User { get; set; }

    public OwnCounters Counters { get; set; }
  }
}
