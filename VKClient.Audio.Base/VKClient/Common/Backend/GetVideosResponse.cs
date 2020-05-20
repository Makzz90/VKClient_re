using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class GetVideosResponse
  {
    public int count { get; set; }

    public List<Video> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }
  }
}
