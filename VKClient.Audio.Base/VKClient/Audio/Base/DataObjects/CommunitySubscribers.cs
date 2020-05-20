using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class CommunitySubscribers
  {
    public VKList<User> subscribers { get; set; }

    public List<User> managers { get; set; }

    public List<GroupContact> contacts { get; set; }
  }
}
