using Microsoft.Phone.Info;
using System.IO.IsolatedStorage;
using System.Windows;

namespace VKClient.Common.Utils
{
  public static class ScaleFactor
  {
    public static int _scaleFactor = -1;
    private static bool? _isFullHD;

    public static int GetScaleFactor()
    {
      if (ScaleFactor._scaleFactor == -1)
      {
        ScaleFactor._scaleFactor = !IsolatedStorageSettings.ApplicationSettings.Contains("ScaleFactor") ? 100 : (int) IsolatedStorageSettings.ApplicationSettings["ScaleFactor"];
        if (ScaleFactor._scaleFactor == 225)
          ScaleFactor._scaleFactor = 150;
      }
      return ScaleFactor._scaleFactor;
    }

    public static int GetRealScaleFactor()
    {
      if (!ScaleFactor.IsFullHD())
        return ScaleFactor.GetScaleFactor();
      return 225;
    }

    public static bool IsFullHD()
    {
      if (!ScaleFactor._isFullHD.HasValue)
      {
        Size physicalScreenSize = ScaleFactor.GetPhysicalScreenSize();
        // ISSUE: explicit reference operation
        ScaleFactor._isFullHD = ((Size) @physicalScreenSize).Width <= 1000.0 ? new bool?(false) : new bool?(true);
      }
      return ScaleFactor._isFullHD.Value;
    }

    private static Size GetPhysicalScreenSize()
    {
      object obj;
      if (!DeviceExtendedProperties.TryGetValue("PhysicalScreenResolution", out obj))
        return default(Size);
      return (Size) obj;
    }

    public static void GetScaleFactorLowestFraction(out int divident, out int divisor, bool isRealScaleFactor = false)
    {
      switch (isRealScaleFactor ? ScaleFactor.GetRealScaleFactor() : ScaleFactor.GetScaleFactor())
      {
        case 150:
          divident = 3;
          divisor = 2;
          break;
        case 160:
          divident = 8;
          divisor = 5;
          break;
        case 225:
          divident = 9;
          divisor = 4;
          break;
        default:
          divident = 1;
          divisor = 1;
          break;
      }
    }
  }
}
