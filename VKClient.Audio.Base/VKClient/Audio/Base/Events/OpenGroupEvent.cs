namespace VKClient.Audio.Base.Events
{
  public class OpenGroupEvent : StatEventBase
  {
    public long GroupId { get; set; }

    public string Source { get; set; }
  }
}
