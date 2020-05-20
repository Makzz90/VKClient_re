using System;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class MessageBoxBackendConfirmationHandler : IBackendConfirmationHandler
  {
    public void Confirm(string confirmationText, Action<bool> callback)
    {
      Execute.ExecuteOnUIThread((Action) (() => callback(MessageBox.Show(confirmationText, CommonResources.Confirmation, MessageBoxButton.OKCancel) == MessageBoxResult.OK)));
    }
  }
}
