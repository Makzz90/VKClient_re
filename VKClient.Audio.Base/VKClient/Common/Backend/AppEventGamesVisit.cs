namespace VKClient.Common.Backend
{
  public class AppEventGamesVisit : AppEventBase
  {
    public override string e
    {
      get
      {
        return "games_visit";
      }
    }

    public string visit_source { get; set; }
  }
}
