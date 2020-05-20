using System.Collections.Generic;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.VideoCatalog
{
  public interface IVideoCatalogItemUCFactory
  {
    double Height { get; }

    UserControlVirtualizable Create(VKClient.Common.Backend.DataObjects.Video video, List<User> knownUsers, List<Group> knownGroups, StatisticsActionSource actionSource, string context);
  }
}
