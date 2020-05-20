using System;
using System.Collections.Generic;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class BaseDataManager
  {
    private DateTime _lastTimeSetOnline = DateTime.MinValue;
    private static BaseDataManager _instance;
    private bool _isLoading;
    private const int BASE_DATA_REFRESH_INTERVAL = 8;

    public static BaseDataManager Instance
    {
      get
      {
        return BaseDataManager._instance ?? (BaseDataManager._instance = new BaseDataManager());
      }
    }

    public bool NeedRefreshBaseData { get; set; }

    public void RefreshBaseDataIfNeeded()
    {
      if (this._isLoading || AppGlobalStateManager.Current.LoggedInUserId == 0L)
        return;
      if (this.NeedRefreshBaseData)
      {
        this._isLoading = true;
        AppGlobalStateData globalState = AppGlobalStateManager.Current.GlobalState;
        AccountService.Instance.GetBaseData(AppInfo.GetPhoneAppInfo(), (Action<BackendResult<AccountBaseData, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
          {
            AccountBaseData resultData = res.ResultData;
            AccountBaseInfo info = resultData.Info;
            BaseDataManager.UpdateOwnCounters(resultData.OwnCounters);
            globalState.LoggedInUser = resultData.User;
            if (info != null)
            {
              globalState.SupportUri = info.support_url;
              List<AccountBaseInfoSettingsEntry> settings = info.settings;
              if (!settings.IsNullOrEmpty())
              {
                foreach (AccountBaseInfoSettingsEntry setting in settings)
                {
                  string name = setting.name;
                  if (!(name == "gif_autoplay"))
                  {
                    if (!(name == "payment_type"))
                    {
                      if (!(name == "money_p2p"))
                      {
                        if (name == "money_clubs_p2p")
                          BaseDataManager.SetMoneyTransfers(setting, true);
                      }
                      else
                        BaseDataManager.SetMoneyTransfers(setting, false);
                    }
                    else
                      BaseDataManager.SetPaymentType(setting);
                  }
                  else
                    BaseDataManager.SetGifAutoplay(setting);
                }
              }
              MoneyTransfersSettings moneyP2pParams = info.money_p2p_params;
              if (moneyP2pParams != null)
              {
                globalState.MoneyTransferMinAmount = moneyP2pParams.min_amount;
                globalState.MoneyTransferMaxAmount = moneyP2pParams.max_amount;
              }
            }
            BaseDataManager.UpdateUserInfo();
            int unixTimestamp = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, false);
            globalState.ServerMinusLocalTimeDelta = resultData.time - unixTimestamp;
            StickersSettings.Instance.UpdateStickersDataAndAutoSuggest((IAccountStickersData) resultData);
            globalState.GamesSectionEnabled = resultData.GamesSectionEnabled == 1;
            globalState.DebugDisabled = resultData.DebugDisabled == 1;
            this.NeedRefreshBaseData = false;
            this._lastTimeSetOnline = DateTime.Now;
            EventAggregator.Current.Publish(new BaseDataChangedEvent());
          }
          this._isLoading = false;
        }), globalState.NeedRefetchStickers);
      }
      else
      {
        if ((DateTime.Now - this._lastTimeSetOnline).TotalMinutes < 8.0)
          return;
        this._isLoading = true;
        AccountService.Instance.GetIntermediateData((Action<BackendResult<AccountIntermediateData, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
          {
            this._lastTimeSetOnline = DateTime.Now;
            StickersSettings.Instance.UpdateStickersDataAndAutoSuggest((IAccountStickersData) res.ResultData);
          }
          this._isLoading = false;
        }));
      }
    }

    private static void UpdateOwnCounters(OwnCounters ownCounters)
    {
      CountersManager.Current.Counters = ownCounters;
    }

    private static void SetGifAutoplay(AccountBaseInfoSettingsEntry setting)
    {
      AppGlobalStateManager.Current.GlobalState.GifAutoplayFeatureAvailable = setting.available;
    }

    private static void SetPaymentType(AccountBaseInfoSettingsEntry setting)
    {
      AccountPaymentType result;
      if (!Enum.TryParse<AccountPaymentType>(setting.value, out result))
        return;
      AppGlobalStateManager.Current.GlobalState.PaymentType = result;
    }

    private static void SetMoneyTransfers(AccountBaseInfoSettingsEntry setting, bool isGroupsSetting)
    {
      AppGlobalStateData globalState = AppGlobalStateManager.Current.GlobalState;
      if (!isGroupsSetting)
      {
        globalState.MoneyTransfersEnabled = setting.available;
        globalState.CanSendMoneyTransfers = setting.value == "can_send";
      }
      else
        globalState.CanSendMoneyTransfersToGroups = setting.available;
    }

    private static void UpdateUserInfo()
    {
      User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
    }
  }
}
