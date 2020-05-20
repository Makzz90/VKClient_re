using System;
using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class AudioHelper
  {
    public static void ShowContentRestrictedMessage(int contentRestricted)
    {
      if (contentRestricted <= 0)
        return;
      string error = contentRestricted == 1 ? CommonResources.AudioRestrictedCopyright : (contentRestricted == 2 ? CommonResources.AudioRestrictedRegion : CommonResources.AudioUnavailable);
      Execute.ExecuteOnUIThread((Action) (() => MessageBox.Show(error, CommonResources.Error, (MessageBoxButton) 0)));
    }
  }
}
