using System;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PushNotificationsManager : IHandle<ChannelUriUpdatedEvent>, IHandle
  {
    private static PushNotificationsManager _instance;
    private bool _initialized;

    public static PushNotificationsManager Instance
    {
      get
      {
        if (PushNotificationsManager._instance == null)
          PushNotificationsManager._instance = new PushNotificationsManager();
        return PushNotificationsManager._instance;
      }
    }

    private AppGlobalStateData GlobalState
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState;
      }
    }

    public void Initialize()
    {
      if (!AppGlobalStateManager.Current.GlobalState.AllowToastNotificationsQuestionAsked || this._initialized || AppGlobalStateManager.Current.LoggedInUserId == 0L)
        return;
      EventAggregator.Current.Subscribe(this);
      PushChannelManager.Instance.OpenChannel();
      this._initialized = true;
    }

    public void Handle(ChannelUriUpdatedEvent message)
    {
      if (!(this.GlobalState.NotificationsUri != message.ChannelUri.OriginalString) && (!this.GlobalState.PushNotificationsEnabled || !string.IsNullOrEmpty(this.GlobalState.RegisteredDeviceId)))
        return;
      this.GlobalState.NotificationsUri = message.ChannelUri.OriginalString;
      this.UpdateDeviceRegistration((Action<bool>) (res => {}));
    }

    public void EnsureTheChannelIsClosed()
    {
      PushChannelManager.Instance.CloseChannel();
      if (AppGlobalStateManager.Current.LoggedInUserId != 0L)
        AccountService.Instance.UnregisterDevice(AppGlobalStateManager.Current.GlobalState.DeviceId, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          this.GlobalState.RegisteredDeviceId = "";
        }));
      this._initialized = false;
    }

    public void UpdateDeviceRegistration(Action<bool> resultCallback)
    {
      if (!string.IsNullOrEmpty(AppGlobalStateManager.Current.GlobalState.RegisteredDeviceId) && !AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled)
        AccountService.Instance.UnregisterDevice(AppGlobalStateManager.Current.GlobalState.DeviceId, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            this.GlobalState.RegisteredDeviceId = "";
          resultCallback(res.ResultCode == ResultCode.Succeeded);
        }));
      else if (this.GlobalState.PushNotificationsEnabled)
      {
        if (string.IsNullOrEmpty(AppGlobalStateManager.Current.GlobalState.NotificationsUri))
          resultCallback(false);
        else
          AccountService.Instance.RegisterDevice(this.GlobalState.DeviceId, this.GlobalState.NotificationsUri, AppInfo.DeviceViewable, AppInfo.OSTypeAndVersion, AppInfo.AppVersionForPushes, string.IsNullOrEmpty(this.GlobalState.RegisteredDeviceId) ? AppGlobalStateManager.Current.GlobalState.PushSettings.ToJsonString() : "", (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res =>
          {
            if (res.ResultCode == ResultCode.Succeeded)
            {
              this.GlobalState.RegisteredDeviceId = AppGlobalStateManager.Current.GlobalState.DeviceId;
              resultCallback(true);
            }
            else
              resultCallback(false);
          }));
      }
      else
        resultCallback(true);
    }
  }
}
