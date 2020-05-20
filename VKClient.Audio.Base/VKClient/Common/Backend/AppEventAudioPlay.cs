namespace VKClient.Common.Backend
{
  public class AppEventAudioPlay : AppEventBase
  {
    public override string e
    {
      get
      {
        return "audio_play";
      }
    }

    public string audio_id { get; set; }

    public string source { get; set; }
  }
}
