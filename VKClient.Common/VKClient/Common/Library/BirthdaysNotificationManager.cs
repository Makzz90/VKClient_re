using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class BirthdaysNotificationManager : IHandle<FriendsCacheUpdated>, IHandle
  {
    public static BirthdaysNotificationManager _instance;

    public static BirthdaysNotificationManager Instance
    {
      get
      {
        if (BirthdaysNotificationManager._instance == null)
          BirthdaysNotificationManager._instance = new BirthdaysNotificationManager();
        return BirthdaysNotificationManager._instance;
      }
    }

    public void Initialize()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    public async void ShowNotificationsIfNeeded()
    {
      if (!AppGlobalStateManager.Current.GlobalState.ShowBirthdaysNotifications)
        return;
      DateTime dateTime = AppGlobalStateManager.Current.GlobalState.LastTimeShownBSNotification;
      int dayOfYear1 = dateTime.DayOfYear;
      dateTime = DateTime.Now;
      int dayOfYear2 = dateTime.DayOfYear;
      if (dayOfYear1 == dayOfYear2 || AppGlobalStateManager.Current.GlobalState.LoggedInUserId == 0L)
        return;
      DateTime oldLastTime = AppGlobalStateManager.Current.GlobalState.LastTimeShownBSNotification;
      AppGlobalStateManager.Current.GlobalState.LastTimeShownBSNotification = DateTime.Now;
      List<User> savedUsers = (await FriendsCache.Instance.GetFriends()).SavedUsers;
      if (savedUsers.Count == 0)
        AppGlobalStateManager.Current.GlobalState.LastTimeShownBSNotification = oldLastTime;
      else
        this.StartShowningNotifications(savedUsers.Where<User>((Func<User, bool>) (u => u.IsBirthdayToday())).ToList<User>(), 0);
    }

    private void StartShowningNotifications(List<User> bdays, int ind)
    {
      if (ind >= bdays.Count)
        return;
      User user = bdays[ind];
      AppNotificationUC.Instance.ShowAndHideLater(MultiResolutionHelper.Instance.AppendResolutionSuffix("/Resources/New/BirthdayToast.png", true, ""), user.Name, CommonResources.HasABirthdayToday, (Action) (() => Navigator.Current.NavigateToUserProfile(user.id, user.Name, "", false)), (Action) (() =>
      {
        List<User> bdays1 = bdays;
        int num = ind + 1;
        ind = num;
        int ind1 = num;
        this.StartShowningNotifications(bdays1, ind1);
      }));
    }

    public void Handle(FriendsCacheUpdated message)
    {
      this.ShowNotificationsIfNeeded();
    }
  }
}
