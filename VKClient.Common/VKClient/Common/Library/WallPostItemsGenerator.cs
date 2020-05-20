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
        double width = num1;
        Thickness margin = new Thickness();
        int num2 = 1;
        NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo = new NewsItemDataWithUsersAndGroupsInfo();
        wallPostWithInfo.WallPost = wallPost;
        wallPostWithInfo.Groups = groups;
        wallPostWithInfo.Profiles = users;
        Action<WallPostItem> deletedItemCallback = deletedCallback;
        int num3 = 0;
        object local1 = null;
        int num4 = 0;
        int num5 = 0;
        int num6 = 1;
        int num7 = 1;
        object local2 = null;
        object local3 = null;
        WallPostItem wallPostItem = new WallPostItem(width, margin, num2 != 0, wallPostWithInfo, deletedItemCallback, num3 != 0, (Action<long, User, Group>) local1, num4 != 0, num5 != 0, num6 != 0, num7 != 0, (NewsFeedAdsItem) local2, (Func<List<MenuItem>>) local3);
        virtualizableList.Add((IVirtualizable) wallPostItem);
      }
      return virtualizableList;
    }
  }
}
