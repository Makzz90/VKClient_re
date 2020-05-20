using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class ProductLikesCommentsData
  {
    public VKList<Comment> comments { get; set; }

    public List<long> likesAllIds { get; set; }

    public int? likesAllCount { get; set; }

    public UserLikedInfo userLiked { get; set; }

    public List<User> users { get; set; }

    public List<User> users2 { get; set; }

    public List<User> users3 { get; set; }

    public List<Group> groups { get; set; }

    public List<PhotoVideoTag> tags { get; set; }

    public int repostsCount { get; set; }

    public List<Comment> Comments
    {
      get
      {
        if (this.comments != null)
          return this.comments.items;
        return new List<Comment>();
      }
      set
      {
        if (this.comments == null)
          this.comments = new VKList<Comment>();
        this.comments.items = value;
      }
    }

    public int TotalCommentsCount
    {
      get
      {
        if (this.comments != null)
          return this.comments.count;
        return 0;
      }
    }

    public int UserLiked
    {
      get
      {
        if (this.userLiked != null)
          return this.userLiked.liked;
        return 0;
      }
      set
      {
        if (this.userLiked == null)
          this.userLiked = new UserLikedInfo();
        this.userLiked.liked = value;
      }
    }
  }
}
