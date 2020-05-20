using System;
using System.IO.IsolatedStorage;
using Windows.System;

namespace VKClient.Audio.Base
{
  public class MemoryInfo
  {
    private static bool isLowMemDeviceValue;

    public static bool IsLowMemDevice
    {
      get
      {
        if (IsolatedStorageSettings.ApplicationSettings.Contains("IsLowMemDevice"))
          MemoryInfo.isLowMemDeviceValue = (bool) IsolatedStorageSettings.ApplicationSettings["IsLowMemDevice"];
        return MemoryInfo.isLowMemDeviceValue;
      }
      set
      {
        if (value == MemoryInfo.IsLowMemDevice)
          return;
        IsolatedStorageSettings.ApplicationSettings["IsLowMemDevice"]= value;
      }
    }

    public static void Initialize()
    {
      try
      {
        if ((long) MemoryManager.AppMemoryUsageLimit < 200000000L)
          MemoryInfo.IsLowMemDevice = true;
        else
          MemoryInfo.IsLowMemDevice = false;
      }
      catch (ArgumentOutOfRangeException )
      {
        MemoryInfo.IsLowMemDevice = false;
      }
    }
  }
}
