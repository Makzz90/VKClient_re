using Microsoft.Phone.Net.NetworkInformation;
using System;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
  public class CountersManager
  {
    private static CountersManager _instance = new CountersManager();
    private OwnCounters _counters = new OwnCounters();
    private bool _isLoading;

    public static CountersManager Current
    {
      get
      {
        return CountersManager._instance;
      }
    }

    public OwnCounters Counters
    {
      get
      {
        return this._counters;
      }
      set
      {
        if (value == null)
          return;
        this._counters = value;
        EventAggregator.Current.Publish((object) new CountersChanged(this._counters));
      }
    }

    public void RefreshCounters()
    {
      if (this._isLoading)
        return;
      this._isLoading = true;
      AccountService.Instance.GetCounters((Action<BackendResult<OwnCounters, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded && res.ResultData != null && !res.ResultData.IsEqual(this.Counters))
        {
          this._counters = res.ResultData;
          EventAggregator.Current.Publish((object) new CountersChanged(this.Counters));
        }
        this._isLoading = false;
      }));
    }

    public void Save()
    {
      CacheManager.TrySerialize((IBinarySerializable) this._counters, "Counters", false, CacheManager.DataType.CachedData);
    }

    public void Restore()
    {
      OwnCounters ownCounters = new OwnCounters();
      if (CacheManager.TryDeserialize((IBinarySerializable) ownCounters, "Counters", CacheManager.DataType.CachedData))
        this.Counters = ownCounters;
      DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(this.DeviceNetworkInformation_NetworkAvailabilityChanged);
    }

    private void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
    {
      if (e.NotificationType != NetworkNotificationType.InterfaceConnected)
        return;
      this.RefreshCounters();
    }

    public void SetUnreadMessages(int c)
    {
      this.Counters.messages = c;
      EventAggregator.Current.Publish((object) new CountersChanged(this.Counters));
    }

    public void ResetFeedback()
    {
      this.Counters.notifications = 0;
      EventAggregator.Current.Publish((object) new CountersChanged(this.Counters));
    }
  }
}
