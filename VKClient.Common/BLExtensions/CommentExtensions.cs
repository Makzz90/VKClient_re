using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.BLExtensions
{
  public static class CommentExtensions
  {
    public static bool CanEdit(this Comment comment)
    {
      if (comment == null)
        return false;
      Group cachedGroup = GroupsService.Current.GetCachedGroup(-comment.from_id);
      if ((comment.from_id == AppGlobalStateManager.Current.LoggedInUserId || comment.from_id < 0L && cachedGroup != null && cachedGroup.IsEditorOrHigher()) && (DateTime.Now - Extensions.UnixTimeStampToDateTime((double) comment.date, true)).TotalHours < 24.0)
      {
        if (comment.Attachments != null)
        {
          List<Attachment> attachments = comment.Attachments;
          Func<Attachment, bool> func1 = (Func<Attachment, bool>) (a => a.type == "sticker");
          if (Enumerable.Any<Attachment>(attachments, func1))
            goto label_6;
        }
        return true;
      }
label_6:
      return false;
    }

    public static bool CanDelete(this Comment comment, long ownerId)
    {
      if (comment == null)
        return false;
      Group cachedGroup = GroupsService.Current.GetCachedGroup(-ownerId);
      return comment.from_id == AppGlobalStateManager.Current.LoggedInUserId || ownerId == AppGlobalStateManager.Current.LoggedInUserId || ownerId < 0L && cachedGroup != null && (comment.from_id > 0L && cachedGroup.IsModeratorOrHigher() || cachedGroup.IsEditorOrHigher());
    }
  }
}
