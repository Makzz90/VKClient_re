using System.Collections.Generic;

namespace VKClient.Common.Library.FriendsImport.Twitter
{
  public class TwitterUsersBackendResponse
  {
    public string previous_cursor_str { get; set; }

    public string next_cursor_str { get; set; }

    public List<TwitterUser> users { get; set; }
  }
}
