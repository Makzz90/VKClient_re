using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public class DialogWrapper
  {
    public int count { get; set; }

    public List<DialogHeaderInfo> items { get; set; }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public DialogWrapper()
    {
      this.items = new List<DialogHeaderInfo>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
