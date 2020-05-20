using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GameLeaderboardResponse
  {
    public int count { get; set; }

    public List<GameLeaderboardItem> items { get; set; }

    public List<User> profiles { get; set; }
  }
}
