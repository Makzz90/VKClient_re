using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class WallData
  {
    public int TotalCount
    {
      get
      {
        return this.count;
      }
      set
      {
        this.count = value;
      }
    }

    public List<WallPost> wall
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

    public int count { get; set; }

    public List<WallPost> items { get; set; }

    public WallData()
    {
      this.wall = new List<WallPost>();
      this.profiles = new List<User>();
      this.groups = new List<Group>();
    }
  }
}
