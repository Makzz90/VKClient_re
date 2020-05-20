namespace VKClient.Common.Backend
{
  public class AppEventOpenVideo : AppEventBase
  {
    public override string e
    {
      get
      {
        return "open_video";
      }
    }

    public string video_id { get; set; }

    public string source { get; set; }

    public string context { get; set; }
  }
}
