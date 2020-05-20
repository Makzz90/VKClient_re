using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public class MessageHistoryWrapper
  {
    public int count { get; set; }

    public int unread { get; set; }

    public int skipped { get; set; }

    public List<Message> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public MessageHistoryWrapper()
    {
      this.items = new List<Message>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
