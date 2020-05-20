namespace VKClient.Common.Backend
{
  public class AppEventPostAction : AppEventBase
  {
    public override string e
    {
      get
      {
        return "post_actions";
      }
    }

    public string[] expand { get; set; }

    public string[] photo { get; set; }

    public string[] video { get; set; }

    public string[] audio { get; set; }
  }
}
