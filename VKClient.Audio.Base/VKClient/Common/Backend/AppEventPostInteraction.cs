namespace VKClient.Common.Backend
{
  public class AppEventPostInteraction : AppEventBase
  {
    public override string e
    {
      get
      {
        return "post_interaction";
      }
    }

    public string post_id { get; set; }

    public string action { get; set; }

    public string link { get; set; }
  }
}
