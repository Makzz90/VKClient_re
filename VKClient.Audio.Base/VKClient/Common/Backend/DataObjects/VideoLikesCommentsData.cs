using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class VideoLikesCommentsData
  {
    public List<User> Users { get; set; }

    public int? RepostsCount { get; set; }

    public List<User> Users2 { get; set; }

    public List<User> Users3 { get; set; }

    public List<long> Albums { get; set; }

    public VKList<Comment> comments { get; set; }

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

    public List<Group> Groups { get; set; }

    public List<PhotoVideoTag> Tags { get; set; }

    public List<long> LikesAllIds { get; set; }

    public int? LikesAllCount { get; set; }

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

    public UserLikedInfo userLiked { get; set; }

    public VKList<Video> VideoRecommendations { get; set; }
  }
}
