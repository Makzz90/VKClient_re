namespace VKClient.Audio.Base.Events
{
  public class TransitionFromPostEvent : StatEventBase
  {
    public string post_id { get; set; }

    public string parent_id { get; set; }

    public TransitionFromPostEvent()
    {
      this.ShouldSendImmediately = true;
    }
  }
}
