namespace VKClient.Audio.Base.DataObjects
{
  public class GamesDashboardResponse
  {
    public GamesRequestsResponse requests { get; set; }

    public VKList<Game> myGames { get; set; }

    public GamesFriendsActivityResponse activity { get; set; }

    public VKList<Game> banners { get; set; }

    public VKList<Game> catalog { get; set; }
  }
}
