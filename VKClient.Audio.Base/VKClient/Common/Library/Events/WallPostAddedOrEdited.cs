using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class WallPostAddedOrEdited
  {
    public WallPost NewlyAddedWallPost { get; set; }

    public List<User> Users { get; set; }

    public List<Group> Groups { get; set; }

    public bool Edited { get; set; }
  }
}
