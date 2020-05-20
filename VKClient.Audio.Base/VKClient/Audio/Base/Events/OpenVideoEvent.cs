namespace VKClient.Audio.Base.Events
{
  public class OpenVideoEvent : StatEventBase
  {
    public string id { get; set; }

    public StatisticsActionSource Source { get; set; }

    public string context { get; set; }
  }
}
