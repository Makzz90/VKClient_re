namespace VKClient.Audio.Base.Events
{
  public class GifPlayEvent : StatEventBase
  {
      public string GifId { get; private set; }

      public GifPlayStartType StartType { get; private set; }

      public StatisticsActionSource Source { get; private set; }

    public GifPlayEvent(string gifId, GifPlayStartType startType, StatisticsActionSource source)
    {
      this.GifId = gifId;
      this.StartType = startType;
      this.Source = source;
    }
  }
}
