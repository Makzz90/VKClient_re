namespace VKClient.Common.Backend
{
  public class AppEventAudioMessagePlay : AppEventBase
  {
    public override string e
    {
      get
      {
        return "audio_message_play";
      }
    }

    public string audio_message_id { get; set; }
  }
}
