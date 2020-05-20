namespace VKClient.Common.Backend
{
  public class AppEventVideoPlay : AppEventBase
  {
    public override string e
    {
      get
      {
        return "video_play";
      }
    }

    public string video_id { get; set; }

    public string position { get; set; }

    public string source { get; set; }

    public int quality { get; set; }

    public string context { get; set; }
  }
}
