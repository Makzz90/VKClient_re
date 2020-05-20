namespace VKClient.Common.Backend
{
  public class AppEventGamesAction : AppEventBase
  {
    public override string e
    {
      get
      {
        return "games_action";
      }
    }

    public long game_id { get; set; }

    public string request_name { get; set; }

    public string action_type { get; set; }

    public string visit_source { get; set; }

    public string click_source { get; set; }
  }
}
