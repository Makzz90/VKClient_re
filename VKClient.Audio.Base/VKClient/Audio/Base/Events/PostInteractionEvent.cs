namespace VKClient.Audio.Base.Events
{
  public class PostInteractionEvent : StatEventBase
  {
    public string PostId { get; set; }

    public PostInteractionAction Action { get; set; }

    public string Link { get; set; }

    public PostInteractionEvent()
    {
      this.ShouldSendImmediately = true;
    }
  }
}
