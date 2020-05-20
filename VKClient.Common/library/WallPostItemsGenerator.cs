using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Library
{
  public class WallPostItemsGenerator
  {
    public static readonly double Width = 480.0;

    public static List<IVirtualizable> Generate(List<WallPost> wallPosts, List<User> users, List<Group> groups, Action<WallPostItem> deletedCallback = null, double itemsWidth = 0.0)
    {
      List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
      double num1 = itemsWidth > 0.0 ? itemsWidth : WallPostItemsGenerator.Width;
      foreach (WallPost wallPost in wallPosts)
      {
          if (wallPost.IsMarkedAsAds == true && AppGlobalStateManager.Current.GlobalState.HideADs == true)
              continue;

        double width = num1;
        NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo = new NewsItemDataWithUsersAndGroupsInfo();
        wallPostWithInfo.WallPost = wallPost;
        wallPostWithInfo.Groups = groups;
        wallPostWithInfo.Profiles = users;

        WallPostItem wallPostItem = new WallPostItem(width, new Thickness(), true, wallPostWithInfo, deletedCallback, false, null, false, false, true, true, null, null);
        virtualizableList.Add(wallPostItem);
      }
      return virtualizableList;
    }
  }
}
