namespace VKClient.Audio.Base.Events
{
  public class OpenUserEvent : StatEventBase
  {
    public long UserId { get; set; }

    public string Source { get; set; }
  }
}
