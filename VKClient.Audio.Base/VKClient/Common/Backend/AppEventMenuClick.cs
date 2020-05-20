namespace VKClient.Common.Backend
{
  public class AppEventMenuClick : AppEventBase
  {
    public override string e
    {
      get
      {
        return "menu_click";
      }
    }

    public string item { get; set; }
  }
}
