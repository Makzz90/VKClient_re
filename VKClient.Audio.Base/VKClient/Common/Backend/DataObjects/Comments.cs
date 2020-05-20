using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class Comments
  {
    public int count { get; set; }

    public int can_post { get; set; }

    public List<Comment> list { get; set; }
  }
}
