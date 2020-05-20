using System;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class MessageBoxBackendConfirmationHandler : IBackendConfirmationHandler
  {
    public void Confim(string confirmationText, Action<bool> callback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        MessageBoxResult messageBoxResult = MessageBox.Show(confirmationText, CommonResources.Confirmation, MessageBoxButton.OKCancel);
        EventAggregator.Current.Publish((object) new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.purchase_window));
        callback(messageBoxResult == MessageBoxResult.OK);
      }));
    }
  }
}
