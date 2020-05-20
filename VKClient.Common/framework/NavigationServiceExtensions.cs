using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
  public static class NavigationServiceExtensions
  {
    public static void ClearBackStack(this NavigationService navigationService)
    {
      Logger.Instance.Info("NavigationServiceExtensions.ClearBackStack");
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        while (Enumerable.Any<JournalEntry>(navigationService.BackStack))
          navigationService.RemoveBackEntry();
      }));
    }

    public static void GoBackSafe(this NavigationService navigationService)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!Enumerable.Any<JournalEntry>(navigationService.BackStack))
          return;
        Navigator.Current.GoBack();
      }));
    }

    public static void RemoveBackEntrySafe(this NavigationService navigationService)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!Enumerable.Any<JournalEntry>(navigationService.BackStack))
          return;
        Logger.Instance.Info("NavigationServiceExtensions.RemoveBackEntrySafe removed back entry.");
        navigationService.RemoveBackEntry();
      }));
    }
  }
}
