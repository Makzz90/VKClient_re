using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.Social;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.BLExtensions
{
  public static class WallPostExtensions
  {
    public static bool CanDelete(this WallPost wallPost, List<Group> knownGroups, bool isCanEditCheck = false)
    {
      if (wallPost == null)
        return false;
      Group group = knownGroups == null ? null : Enumerable.FirstOrDefault<Group>(knownGroups, (Func<Group, bool>)(g => g.id == -wallPost.to_id));
      if (wallPost.to_id == AppGlobalStateManager.Current.LoggedInUserId || wallPost.from_id == AppGlobalStateManager.Current.LoggedInUserId || group != null && group.admin_level > 1)
        return true;
      if (group != null && group.admin_level > 0 && wallPost.from_id != wallPost.owner_id)
        return !isCanEditCheck;
      return false;
    }

    public static bool CanEdit(this WallPost wallPost, List<Group> knownGroups)
    {
      if (wallPost == null || wallPost.to_id == AppGlobalStateManager.Current.LoggedInUserId && wallPost.from_id != AppGlobalStateManager.Current.LoggedInUserId)
        return false;
      bool flag = wallPost.CanDelete(knownGroups, true) && ((DateTime.Now - Extensions.UnixTimeStampToDateTime((double) wallPost.date, true)).TotalHours < 24.0 || wallPost.IsSuggestedPostponed);
      if (wallPost.IsSuggested)
        flag = wallPost.from_id == AppGlobalStateManager.Current.LoggedInUserId;
      return flag;
    }

    public static bool CanPin(this WallPost wallPost, List<Group> knownGroups)
    {
      if (wallPost == null)
        return false;
      long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
      if (wallPost.to_id == loggedInUserId && wallPost.from_id == loggedInUserId && wallPost.is_pinned == 0)
        return true;
      Group group = knownGroups == null ? null : Enumerable.FirstOrDefault<Group>(knownGroups, (Func<Group, bool>)(g => g.id == -wallPost.to_id));
      return wallPost.CanDelete(knownGroups, false) && group != null && (wallPost.is_pinned == 0 && wallPost.from_id == wallPost.to_id);
    }

    public static bool CanUnpin(this WallPost wallPost, List<Group> knownGroups)
    {
      if (wallPost == null)
        return false;
      long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
      if (wallPost.to_id == loggedInUserId && wallPost.from_id == loggedInUserId && wallPost.is_pinned == 1)
        return true;
      Group group = knownGroups == null ? null : Enumerable.FirstOrDefault<Group>(knownGroups, (Func<Group, bool>)(g => g.id == -wallPost.to_id));
      return wallPost.CanDelete(knownGroups, false) && group != null && (wallPost.is_pinned == 1 && wallPost.from_id == wallPost.to_id);
    }

    public static bool CanReport(this WallPost wallPost)
    {
      return wallPost != null && wallPost.from_id != AppGlobalStateManager.Current.LoggedInUserId && !wallPost.IsPostponed;
    }

    public static void NavigateToEditWallPost(this WallPost wallPost, int adminLevel)
    {
      if (wallPost == null)
        return;
      ParametersRepository.SetParameterForId("EditWallPost", wallPost);
      Navigator.Current.NavigateToNewWallPost(Math.Abs(wallPost.to_id), wallPost.to_id < 0, adminLevel, false, false, false);
    }

    public static void NavigateToPublishWallPost(this WallPost wallPost, int adminLevel)
    {
      if (wallPost == null)
        return;
      ParametersRepository.SetParameterForId("PublishWallPost", wallPost);
      Navigator.Current.NavigateToNewWallPost(Math.Abs(wallPost.to_id), wallPost.to_id < 0, adminLevel, false, false, false);
    }

    public static bool AskConfirmationAndDelete(this WallPost wallPost)
    {
      if (wallPost == null || MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.DeleteWallPost, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return false;
      WallService.Current.DeletePost(wallPost.to_id, wallPost.id);
      EventAggregator.Current.Publish(new WallPostDeleted()
      {
        WallPost = wallPost
      });
      SocialDataManager.Instance.MarkFeedAsStale(wallPost.to_id);
      return true;
    }

    public static bool CanRepostToCommunity(this WallPost wallPost)
    {
      if (wallPost == null)
        return false;
      if (wallPost.likes != null && wallPost.likes.can_publish == 1 || wallPost.reposts.user_reposted == 1)
        return true;
      if (wallPost.friends_only == 0 && wallPost.from_id == AppGlobalStateManager.Current.LoggedInUserId)
        return wallPost.to_id == AppGlobalStateManager.Current.LoggedInUserId;
      return false;
    }

    public static void PinUnpin(this WallPost wallPost, Action<bool> callback)
    {
      WallService.Current.PinUnpin(wallPost.is_pinned == 0, wallPost.owner_id, wallPost.id, (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          EventAggregator current = EventAggregator.Current;
          WallPostPinnedUnpinned postPinnedUnpinned = new WallPostPinnedUnpinned();
          postPinnedUnpinned.OwnerId = wallPost.owner_id;
          postPinnedUnpinned.PostId = wallPost.id;
          int num = wallPost.is_pinned == 0 ? 1 : 0;
          postPinnedUnpinned.Pinned = num != 0;
          current.Publish(postPinnedUnpinned);
          wallPost.is_pinned = wallPost.is_pinned != 0 ? 0 : 1;
          SocialDataManager.Instance.MarkFeedAsStale(wallPost.owner_id);
        }
        callback(res.ResultCode == ResultCode.Succeeded);
      }));
    }
  }
}
