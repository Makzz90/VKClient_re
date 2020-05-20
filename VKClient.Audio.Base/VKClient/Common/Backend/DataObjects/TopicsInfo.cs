using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class TopicsInfo
  {
    public int TotalCount { get; set; }

    public int can_add_topics { get; set; }

    public List<Topic> topics { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public TopicsInfo()
    {
      this.topics = new List<Topic>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
