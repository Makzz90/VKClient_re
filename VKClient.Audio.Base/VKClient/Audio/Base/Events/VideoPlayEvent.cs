namespace VKClient.Audio.Base.Events
{
  public class VideoPlayEvent : StatEventBase
  {
    public string id { get; set; }

    public int quality { get; set; }

    public StatisticsActionSource Source { get; set; }

    public string Context { get; set; }

    public StatisticsVideoPosition Position { get; set; }

    public VideoPlayEvent()
    {
      this.ShouldSendImmediately = true;
    }
  }
}
