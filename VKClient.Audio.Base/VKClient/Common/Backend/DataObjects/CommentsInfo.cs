using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class CommentsInfo
  {
    public List<Comment> comments { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public Poll poll { get; set; }

    public int TotalCommentsCount { get; set; }
  }
}
