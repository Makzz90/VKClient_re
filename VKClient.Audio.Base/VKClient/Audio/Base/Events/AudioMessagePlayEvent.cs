namespace VKClient.Audio.Base.Events
{
  public class AudioMessagePlayEvent : StatEventBase
  {
      public string AudioMessageId { get; private set; }

    public AudioMessagePlayEvent(string audioMessageId)
    {
      this.AudioMessageId = audioMessageId;
    }
  }
}
