namespace VKClient.Common.Backend
{
  public class AppEventProfileBlockClick : AppEventBase
  {
    public override string e
    {
      get
      {
        return "profile_click";
      }
    }

    public long user_id { get; set; }

    public ProfileBlocksClickData blocks { get; set; }
  }
}
