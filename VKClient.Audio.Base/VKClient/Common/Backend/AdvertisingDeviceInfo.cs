using Microsoft.Phone.Info;
using Newtonsoft.Json;
using System;
using VKClient.Audio.Base;
using Windows.System.UserProfile;

namespace VKClient.Common.Backend
{
  public class AdvertisingDeviceInfo
  {
    private static readonly string ERROR_READING_ADV_ID = "-1";
    private static readonly string ADV_ID_IS_NOT_SUPPORTED = "-2";
    private static readonly string ADV_ID_USE_IS_FORBIDDEN = "-3";
    private PhoneAppInfo _phoneAppInfo;

    public string app_version
    {
      get
      {
        return this._phoneAppInfo.AppVersion;
      }
    }

    public string device_model
    {
      get
      {
        return this._phoneAppInfo.Device;
      }
    }

    public string system_version
    {
      get
      {
        return AppInfo.OSVersion;
      }
    }

    public string system_name
    {
      get
      {
        return "Windows Phone";
      }
    }

    public string manufacturer
    {
      get
      {
        try
        {
          return DeviceStatus.DeviceManufacturer;
        }
        catch (Exception )
        {
          return "";
        }
      }
    }

    public string ads_device_id
    {
      get
      {
        try
        {
          string advertisingId = AdvertisingManager.AdvertisingId;
          if (!string.IsNullOrWhiteSpace(advertisingId))
            return advertisingId;
          return AdvertisingDeviceInfo.ADV_ID_USE_IS_FORBIDDEN;
        }
        catch (Exception )
        {
          return AdvertisingDeviceInfo.ERROR_READING_ADV_ID;
        }
      }
    }

    public int ads_tracking_disabled
    {
      get
      {
        string adsDeviceId = this.ads_device_id;
        return adsDeviceId == AdvertisingDeviceInfo.ADV_ID_USE_IS_FORBIDDEN || adsDeviceId == AdvertisingDeviceInfo.ADV_ID_IS_NOT_SUPPORTED ? 1 : 0;
      }
    }

    public AdvertisingDeviceInfo()
    {
      this._phoneAppInfo = AppInfo.GetPhoneAppInfo();
    }

    public string ToJsonString()
    {
      return JsonConvert.SerializeObject(this);
    }
  }
}
