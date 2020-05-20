using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GameDashboardResponse
  {
    public GamesRequestsResponse requests { get; set; }

    public GamesFriendsActivityResponse activity { get; set; }

    public GameLeaderboardResponse leaderboard { get; set; }

    public List<User> friends { get; set; }
  }
}
