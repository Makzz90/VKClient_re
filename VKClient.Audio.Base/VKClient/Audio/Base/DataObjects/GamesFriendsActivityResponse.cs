using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GamesFriendsActivityResponse
  {
    public int count { get; set; }

    public List<GameActivity> items { get; set; }

    public List<Game> apps { get; set; }

    public List<User> profiles { get; set; }

    public string next_from { get; set; }
  }
}
