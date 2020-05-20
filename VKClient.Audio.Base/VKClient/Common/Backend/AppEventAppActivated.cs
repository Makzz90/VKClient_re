namespace VKClient.Common.Backend
{
  public class AppEventAppActivated : AppEventBase
  {
    public override string e
    {
      get
      {
        return "open_app";
      }
    }

    public string @ref { get; set; }

    public string type { get; set; }
  }
}
