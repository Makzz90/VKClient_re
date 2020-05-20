using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
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
      EventAggregator.Current.Subscribe(this);
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
          this.StartShowningNotifications((List<User>)Enumerable.ToList<User>(Enumerable.Where<User>(savedUsers, (Func<User, bool>)(u => u.IsBirthdayToday()))), 0);
    }

    private void StartShowningNotifications(List<User> bdays, int ind)
    {
        if (ind < bdays.Count)
        {
            User user = bdays[ind];
            AppNotificationUC.Instance.ShowAndHideLater(MultiResolutionHelper.Instance.AppendResolutionSuffix("/Resources/New/BirthdayToast.png", true, ""), user.Name, CommonResources.HasABirthdayToday, delegate
            {
                EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.push, GiftPurchaseStepsAction.profile));
                Navigator.Current.NavigateToUserProfile(user.id, user.Name, "", false);
            }, delegate
            {
                BirthdaysNotificationManager arg_1D_0 = this;
                List<User> arg_1D_1 = bdays;
                int ind2 = ind + 1;
                ind = ind2;
                arg_1D_0.StartShowningNotifications(arg_1D_1, ind2);
            });
        }
    }

    public void Handle(FriendsCacheUpdated message)
    {
      this.ShowNotificationsIfNeeded();
    }
  }
}
