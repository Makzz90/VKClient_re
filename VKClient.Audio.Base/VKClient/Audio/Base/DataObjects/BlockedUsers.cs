using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class BlockedUsers
  {
    public VKList<User> blocked_users { get; set; }

    public List<User> managers { get; set; }
  }
}
