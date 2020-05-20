using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class VKList<T>
  {
    public int count { get; set; }

    public int skipped { get; set; }

    public int unread { get; set; }

    public List<T> items { get; set; }

    public bool more { get; set; }

    public string next_from { get; set; }

    public List<T> users
    {
      get
      {
        return this.items;
      }
      set
      {
        this.items = value;
      }
    }

    public List<User> profiles { get; set; }

    public List<Group> groups { get; set; }

    public string context { get; set; }

    public VKList()
    {
      this.items = new List<T>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
