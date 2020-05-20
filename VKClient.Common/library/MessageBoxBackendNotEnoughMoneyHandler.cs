using System;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class MessageBoxBackendNotEnoughMoneyHandler : IBackendNotEnoughMoneyHandler
  {
    public void RequestBalanceRefill(Action refilledCallback, Action cancelledCallback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (MessageBox.Show("Пополнено?", "Пополнение баланса", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
          Action action = refilledCallback;
          if (action == null)
            return;
          action();
        }
        else
        {
          Action action = cancelledCallback;
          if (action == null)
            return;
          action();
        }
      }));
    }
  }
}
