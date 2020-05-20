namespace VKClient.Common.Backend
{
  public class AppEventGifPlay : AppEventBase
  {
    public override string e
    {
      get
      {
        return "gif_play";
      }
    }

    public string gif_id { get; set; }

    public string start_type { get; set; }

    public string source { get; set; }

    public string context { get; set; }
  }
}
