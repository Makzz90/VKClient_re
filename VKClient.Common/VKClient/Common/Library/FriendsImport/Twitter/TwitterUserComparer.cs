using System.Collections.Generic;

namespace VKClient.Common.Library.FriendsImport.Twitter
{
  public class TwitterUserComparer : IEqualityComparer<TwitterUser>
  {
    public bool Equals(TwitterUser x, TwitterUser y)
    {
      return x.id == y.id;
    }

    public int GetHashCode(TwitterUser obj)
    {
      return (int) obj.id;
    }
  }
}
