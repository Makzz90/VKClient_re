using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class CommentsResponse
  {
    public List<Comment> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public Poll poll { get; set; }

    public int count { get; set; }
  }
}
