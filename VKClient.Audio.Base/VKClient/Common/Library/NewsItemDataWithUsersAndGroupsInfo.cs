using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public class NewsItemDataWithUsersAndGroupsInfo
  {
    public NewsItem NewsItem { get; set; }

    public WallPost WallPost { get; set; }

    public List<User> Profiles { get; set; }

    public List<Group> Groups { get; set; }
  }
}
