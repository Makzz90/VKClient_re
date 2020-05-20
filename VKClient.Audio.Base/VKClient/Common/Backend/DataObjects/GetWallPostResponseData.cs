using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class GetWallPostResponseData
  {
    public int Count
    {
      get
      {
        if (this.comments != null)
          return this.comments.count;
        return 0;
      }
    }

    public List<Comment> Comments
    {
      get
      {
        if (this.comments != null)
          return this.comments.items;
        return  null;
      }
    }

    public VKList<Comment> comments { get; set; }

    public List<User> Users { get; set; }

    public List<Group> Groups { get; set; }

    public WallPost WallPost { get; set; }

    public List<User> Users2 { get; set; }

    public List<User> LoggedInUser { get; set; }

    public LikesInfo LikesAll { get; set; }

    public Poll Poll { get; set; }
  }
}
