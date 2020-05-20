using System;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Balance.ViewModels;
using VKClient.Common.Balance.Views;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class BackendNotEnoughMoneyHandler : IBackendNotEnoughMoneyHandler, IHandle<BalanceUpdatedEvent>, IHandle
  {
    private readonly FullscreenLoader _loader;

    public BackendNotEnoughMoneyHandler()
    {
      this._loader = new FullscreenLoader()
      {
        HideOnBackKeyPress = true
      };
      EventAggregator.Current.Subscribe(this);
    }

    public void RequestBalanceRefill(Action refilledCallback, Action cancelledCallback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this._loader.HiddenCallback = (Action<FullscreenLoaderHiddenEventArgs>) (args =>
        {
          if (args.ByBackKey)
          {
            Action action = cancelledCallback;
            if (action == null)
              return;
            action();
          }
          else
          {
            Action action = refilledCallback;
            if (action == null)
              return;
            action();
          }
        });
        BalanceRefillPopupUC balanceRefillPopupUc = new BalanceRefillPopupUC();
        BalanceRefillPopupViewModel refillPopupViewModel = new BalanceRefillPopupViewModel();
        ((FrameworkElement) balanceRefillPopupUc).DataContext = refillPopupViewModel;
        refillPopupViewModel.Reload(true);
        this._loader.Show((FrameworkElement) balanceRefillPopupUc, true);
      }));
    }

    public void Handle(BalanceUpdatedEvent message)
    {
      this._loader.Hide(false);
    }
  }
}
