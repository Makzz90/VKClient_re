using System.Collections.Generic;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.VideoCatalog;

namespace VKClient.Video.VideoCatalog
{
  public class VideoCatalogItemUCFactory : IVideoCatalogItemUCFactory
  {
    public double Height
    {
      get
      {
        return 128.0;
      }
    }

    public UserControlVirtualizable Create(VKClient.Common.Backend.DataObjects.Video video, List<User> knownUsers, List<Group> knownGroups, StatisticsActionSource actionSource, string context)
    {
      CatalogItemViewModel catalogItemViewModel1 = new CatalogItemViewModel(new VideoCatalogItem(video), knownUsers, knownGroups, false)
      {
        ActionSource = new StatisticsActionSource?(actionSource),
        VideoContext = context
      };
      CatalogItemUC catalogItemUc = new CatalogItemUC();
      CatalogItemViewModel catalogItemViewModel2 = catalogItemViewModel1;
      ((FrameworkElement) catalogItemUc).DataContext = catalogItemViewModel2;
      return (UserControlVirtualizable) catalogItemUc;
    }
  }
}
