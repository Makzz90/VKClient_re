using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class GetTopicsResponse
  {
    public int count { get; set; }

    public List<Topic> items { get; set; }

    public List<User> profiles { get; set; }

    public int can_add_topics { get; set; }

    public List<Group> groups { get; set; }

    public GetTopicsResponse()
    {
      this.items = new List<Topic>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
