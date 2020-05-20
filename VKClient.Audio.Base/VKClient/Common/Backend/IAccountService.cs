using System;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public interface IAccountService
  {
    void GetCounters(Action<BackendResult<OwnCounters, ResultCode>> callback);

    void RegisterDeviceForPushNotifications(string urlToken, string systemVersion, bool sendText, string subscribe, Action<BackendResult<object, ResultCode>> callback);

    void SetSilenceMode(string urlToken, int nrOfSeconds, Action<BackendResult<object, ResultCode>> callback, long chatId = 0, long uid = 0);

    void UnregisterDeviceForPushNotifications(string urlToken, Action<BackendResult<object, ResultCode>> callback);

    void GetPushNotificationsSettings(string urlToken, Action<BackendResult<PushNotificationSettings, ResultCode>> callback);

    void SetUserOnline(Action<BackendResult<object, ResultCode>> callback);

    void GetBaseData(PhoneAppInfo phoneAppInfo, Action<BackendResult<AccountBaseData, ResultCode>> callback);

    void ResolveScreenName(string name, Action<BackendResult<ResolvedObject, ResultCode>> callback);
  }
}
