using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
  public class PhotoWithFullInfo
  {
    public Photo Photo { get; set; }

    public int RepostsCount { get; set; }

    public VKList<Comment> comments { get; set; }

    public List<Comment> Comments
    {
      get
      {
        if (this.comments != null)
          return this.comments.items;
        return  null;
      }
      set
      {
        if (this.comments == null)
          this.comments = new VKList<Comment>();
        this.comments.items = value;
      }
    }

    public List<long> LikesAllIds { get; set; }

    public List<User> Users { get; set; }

    public List<Group> Groups { get; set; }

    public List<User> Users2 { get; set; }

    public List<User> Users3 { get; set; }

    public List<PhotoVideoTag> PhotoTags { get; set; }
  }
}
