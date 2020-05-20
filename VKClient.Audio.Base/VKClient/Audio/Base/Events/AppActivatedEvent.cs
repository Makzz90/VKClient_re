namespace VKClient.Audio.Base.Events
{
  public class AppActivatedEvent : StatEventBase
  {
    public AppActivationReason Reason { get; set; }

    public string ReasonSubtype { get; set; }
  }
}
