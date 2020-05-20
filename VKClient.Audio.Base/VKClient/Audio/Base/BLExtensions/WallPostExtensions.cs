using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BLExtensions
{
  public static class WallPostExtensions
  {
    public static bool IsFromGroup(this WallPost wallPost)
    {
      return wallPost.from_id < 0L;
    }

    public static bool IsRepost(this WallPost wallPost)
    {
      if (wallPost.copy_history != null)
        return wallPost.copy_history.Count > 0;
      return false;
    }

    public static bool CanGoToOriginal(this WallPost wallPost)
    {
      if (!wallPost.IsRepost())
        return false;
      if (!(wallPost.copy_history[0].post_type == "post"))
        return wallPost.copy_history[0].post_type == "reply";
      return true;
    }

    public static string GetAuthorDisplayName(this WallPost wallPost, List<Group> groups, List<User> users)
    {
      return WallPostExtensions.GetUserOrGroupName(wallPost.from_id, groups, users);
    }

    public static string GetFromToString(this WallPost wallPost, List<Group> groups, List<User> users)
    {
      if (wallPost.from_id == wallPost.to_id)
        return "";
      return string.Format("{0} ▶ {1}", WallPostExtensions.GetUserOrGroupName(wallPost.from_id, groups, users), WallPostExtensions.GetUserOrGroupName(wallPost.to_id, groups, users));
    }

    public static bool GetIsMale(this WallPost wallPost, List<User> users)
    {
      if (wallPost.from_id < 0L)
        return true;
      User user = users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == wallPost.from_id));
      if (user != null)
        return user.sex != 1;
      return true;
    }

    public static long GetChildItemFromId(this WallPost wallPost)
    {
      if (wallPost.copy_history == null || wallPost.copy_history.Count <= 0)
        return 0;
      return wallPost.copy_history[0].from_id;
    }

    public static string GetChildAuthorDisplayName(this WallPost wallPost, List<Group> groups, List<User> users)
    {
      return WallPostExtensions.GetUserOrGroupName(wallPost.GetChildItemFromId(), groups, users);
    }

    public static string GetUserOrGroupName(long userOrGroupId, List<Group> groups, List<User> users)
    {
      string str = "";
      if (userOrGroupId < 0L)
      {
        Group group = groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -userOrGroupId));
        if (group != null)
          str = group.name ?? "";
      }
      else
      {
        User user = users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == userOrGroupId));
        if (user != null)
          str = user.Name ?? "";
      }
      return str;
    }

    public static bool IsProfilePhotoUpdatePost(this WallPost wallPost, List<User> users, out bool isMale)
    {
      List<Attachment> attachments = wallPost.attachments;
      isMale = true;
      if (attachments != null)
      {
        Attachment attachment = attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>) (a => a.type == "photo"));
        if (attachment != null && attachment.photo != null && (attachment.photo.aid == -6L && wallPost.post_source != null) && wallPost.post_source.data == "profile_photo")
        {
          User user = !wallPost.IsRepost() ? users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == wallPost.to_id)) : users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == wallPost.copy_history[0].owner_id));
          if (user != null)
          {
            if (user.sex == 1)
              isMale = false;
            return true;
          }
        }
      }
      return false;
    }

    public static LikeObjectType GetLikeObjectType(this WallPost wallPost)
    {
      return wallPost == null || !(wallPost.post_type == "post_ads") ? LikeObjectType.post : LikeObjectType.post_ads;
    }

    public static RepostObject GetRepostObjectType(this WallPost wallPost)
    {
      return wallPost == null || !(wallPost.post_type == "post_ads") ? RepostObject.wall : RepostObject.wall_ads;
    }
  }
}
