namespace VKMessenger.Backend
{
  public class LongPollServerResponse
  {
    public string key { get; set; }

    public string server { get; set; }

    public long ts { get; set; }
  }
}
