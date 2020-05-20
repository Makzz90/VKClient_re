using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class CommunityManagers
  {
    public VKList<User> managers { get; set; }

    public List<GroupContact> contacts { get; set; }
  }
}
