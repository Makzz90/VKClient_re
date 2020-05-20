using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class LikesInfo
  {
    private List<long> _items = new List<long>();

    public int count { get; set; }

    public List<UserLike> users { get; set; }

    public List<long> items
    {
      get
      {
        return this._items;
      }
      set
      {
        this._items = value;
      }
    }

    public int repostsCount { get; set; }

    public LikesInfo()
    {
      this.users = new List<UserLike>();
    }
  }
}
