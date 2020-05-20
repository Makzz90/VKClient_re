using System;
using System.Collections.Generic;
using System.Linq;
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
        if (BaseDataManager._instance == null)
          BaseDataManager._instance = new BaseDataManager();
        return BaseDataManager._instance;
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
            this.UpdateOwnCounters(res.ResultData.OwnCounters);
            globalState.LoggedInUser = res.ResultData.User;
            if (res.ResultData.Info != null)
            {
              globalState.SupportUri = res.ResultData.Info.support_url;
              List<AccountBaseInfoSettingsEntry> settings = res.ResultData.Info.settings;
              if (!settings.IsNullOrEmpty())
              {
                AccountBaseInfoSettingsEntry infoSettingsEntry1 = settings.FirstOrDefault<AccountBaseInfoSettingsEntry>((Func<AccountBaseInfoSettingsEntry, bool>) (s => s.name == AccountBaseInfoSettingsEntry.GIF_AUTOPLAY_KEY));
                if (infoSettingsEntry1 != null)
                  globalState.GifAutoplayFeatureAvailable = infoSettingsEntry1.available;
                AccountBaseInfoSettingsEntry infoSettingsEntry2 = settings.FirstOrDefault<AccountBaseInfoSettingsEntry>((Func<AccountBaseInfoSettingsEntry, bool>) (s => s.name == AccountBaseInfoSettingsEntry.PAYMENT_TYPE_KEY));
                AccountPaymentType result;
                if (infoSettingsEntry2 != null && Enum.TryParse<AccountPaymentType>(infoSettingsEntry2.value, out result))
                  globalState.PaymentType = result;
              }
            }
            this.UpdateUserInfo();
            int unixTimestamp = Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, false);
            globalState.ServerMinusLocalTimeDelta = res.ResultData.time - unixTimestamp;
            StickersSettings.Instance.UpdateStickersDataAndAutoSuggest((IAccountStickersData) res.ResultData);
            EventAggregator.Current.Publish((object) new BaseDataChangedEvent());
            globalState.GamesSectionEnabled = res.ResultData.GamesSectionEnabled == 1;
            this.NeedRefreshBaseData = false;
            this._lastTimeSetOnline = DateTime.Now;
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

    private void UpdateUserInfo()
    {
      User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
    }

    private void UpdateOwnCounters(OwnCounters ownCounters)
    {
      CountersManager.Current.Counters = ownCounters;
    }
  }
}
