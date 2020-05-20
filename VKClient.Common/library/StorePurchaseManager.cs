using System;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public static class StorePurchaseManager
  {
    public static void BuyStickersPack(StockItemHeader stockItemHeader, string referrer = "", Action successCallback = null, Action errorCallback = null)
    {
      StoreBuyProductParams buyParams = new StoreBuyProductParams(StoreProductType.stickers, stockItemHeader.ProductId) { StickerReferrer = referrer };
      FullscreenLoader loader = new FullscreenLoader();
      loader.Show( null, true);
      StoreService.Instance.BuyProduct(buyParams, (Action<BackendResult<StoreBuyProductResult, ResultCode>>) (result =>
      {
        loader.Hide(false);
        Action action = successCallback;
        if (action != null)
          action();
        stockItemHeader.SetPurchasedState();
        EventAggregator.Current.Publish(new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.success));
        EventAggregator.Current.Publish(new StickersPackPurchasedEvent(stockItemHeader));
      }), (Action<BackendResult<StorePurchaseResult, ResultCode>>) (result =>
      {
        loader.HiddenCallback = (Action<FullscreenLoaderHiddenEventArgs>) (args => StorePurchaseManager.HandleStorePurchaseError(result, errorCallback));
        loader.Hide(false);
      }));
    }

    public static void BuyVotesPack(VotesPack votesPack, BalanceTopupSource source, Action successCallback = null, Action errorCallback = null)
    {
      FullscreenLoader loader = new FullscreenLoader();
      loader.Show( null, true);
      EventAggregator.Current.Publish(new BalanceTopupEvent(source, BalanceTopupAction.purchase_click));
      StoreService.Instance.PurchaseVotesPack(votesPack, (Action) (() =>
      {
        EventAggregator.Current.Publish(new BalanceTopupEvent(source, BalanceTopupAction.success));
        StorePurchaseManager.UpdateBalanceData((Action) (() =>
        {
          loader.Hide(false);
          Action action = successCallback;
          if (action == null)
            return;
          action();
        }));
      }), (Action<BackendResult<StorePurchaseResult, ResultCode>>) (result =>
      {
        loader.HiddenCallback = (Action<FullscreenLoaderHiddenEventArgs>) (args => StorePurchaseManager.HandleStorePurchaseError(result, errorCallback));
        loader.Hide(false);
      }), (Action) (() => loader.Hide(false)));
    }

    public static void RestorePurchases(string productId = null)
    {
      FullscreenLoader loader = new FullscreenLoader();
      loader.Show( null, true);
      StoreService.Instance.RestorePurchases(productId, (Action) (() => StorePurchaseManager.UpdateBalanceData((Action) (() => loader.Hide(false)))), (Action<BackendResult<StorePurchaseResult, ResultCode>>) (result =>
      {
        loader.HiddenCallback = (Action<FullscreenLoaderHiddenEventArgs>) (args => StorePurchaseManager.HandleStorePurchaseError(result,  null));
        loader.Hide(false);
      }));
    }

    private static void UpdateBalanceData(Action callback)
    {
      StoreService.Instance.GetBalanceData((Action<BackendResult<BalanceData, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded && result.ResultData != null)
          EventAggregator.Current.Publish(new BalanceUpdatedEvent()
          {
            BalanceData = result.ResultData
          });
        Action action = callback;
        if (action == null)
          return;
        action();
      }));
    }

    private static void HandleStorePurchaseError(BackendResult<StorePurchaseResult, ResultCode> result, Action callback = null)
    {
      ResultCode resultCode = result.ResultCode;
      if (resultCode != ResultCode.Succeeded)
        GenericInfoUC.ShowBasedOnResult(resultCode, "", (VKRequestsDispatcher.Error) null);
      else if (result.ResultData != null && result.ResultData.state < 0)
        Execute.ExecuteOnUIThread((Action) (() => new GenericInfoUC(2000).ShowAndHideLater(result.ResultData.error_message ?? CommonResources.UnableToCompletePurchase,  null)));
      if (callback == null)
        return;
      callback();
    }

    public static void ActivateStickersPack(StockItemHeader stockItemHeader, Action<bool> callback = null)
    {
      StoreService.Instance.ActivateProduct(stockItemHeader.ProductId, (Action<BackendResult<bool, ResultCode>>) (result =>
      {
        bool flag = result.ResultCode == ResultCode.Succeeded;
        if (flag)
        {
          stockItemHeader.IsActive = true;
          EventAggregator.Current.Publish(new StickersPackActivatedDeactivatedEvent(stockItemHeader, true));
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        Action<bool> action = callback;
        if (action == null)
          return;
        int num = flag ? 1 : 0;
        action(num != 0);
      }));
    }

    public static void DeactivateStickersPack(StockItemHeader stockItemHeader, Action<bool> callback = null)
    {
      StoreService.Instance.DeactivateProduct(stockItemHeader.ProductId, (Action<BackendResult<bool, ResultCode>>) (result =>
      {
        bool flag = result.ResultCode == ResultCode.Succeeded;
        if (flag)
        {
          stockItemHeader.IsActive = false;
          EventAggregator.Current.Publish(new StickersPackActivatedDeactivatedEvent(stockItemHeader, false));
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        Action<bool> action = callback;
        if (action == null)
          return;
        int num = flag ? 1 : 0;
        action(num != 0);
      }));
    }
  }
}
