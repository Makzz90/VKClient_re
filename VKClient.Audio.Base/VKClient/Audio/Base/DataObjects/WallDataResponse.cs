using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class WallDataResponse
  {
    public int count { get; set; }

    public List<WallPost> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }
  }
}
