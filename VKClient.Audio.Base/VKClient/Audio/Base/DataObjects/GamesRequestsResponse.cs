using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GamesRequestsResponse
  {
    public int count { get; set; }

    public List<GameRequest> items { get; set; }

    public List<Game> apps { get; set; }

    public List<User> profiles { get; set; }
  }
}
