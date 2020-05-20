using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Common.Utils
{
  public static class DeviceStatusExtensions
  {
    private static readonly List<string> _largeScreenDeviceNames = new List<string>()
    {
      "rm937",
      "rm938",
      "rm939",
      "rm940",
      "rm994",
      "rm995",
      "rm996"
    };

    private static int DeviceScaleFactor
    {
      get
      {
        return ScaleFactor.GetRealScaleFactor();
      }
    }

    private static string DeviceName
    {
      get
      {
        return DeviceStatus.DeviceName;
      }
    }

    public static bool IsLargeScreen
    {
      get
      {
        if (DeviceStatusExtensions.DeviceScaleFactor == 225)
        {
          string deviceName = DeviceStatusExtensions.DeviceName;
          if (!string.IsNullOrEmpty(deviceName))
          {
            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();
            if (DeviceStatusExtensions._largeScreenDeviceNames.Any<string>((Func<string, bool>) (name => deviceName.StartsWith(name))))
              return true;
          }
        }
        return false;
      }
    }
  }
}
