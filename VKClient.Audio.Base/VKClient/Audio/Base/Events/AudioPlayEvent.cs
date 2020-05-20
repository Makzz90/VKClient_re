namespace VKClient.Audio.Base.Events
{
  public class AudioPlayEvent : StatEventBase
  {
    public string OwnerAndAudioId { get; set; }

    public StatisticsActionSource Source { get; set; }
  }
}
