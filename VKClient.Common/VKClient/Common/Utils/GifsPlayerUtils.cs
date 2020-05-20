using VKClient.Audio.Base;
using VKClient.Common.Library;

namespace VKClient.Common.Utils
{
  public static class GifsPlayerUtils
  {
    public static bool ShouldShowSize()
    {
      return !GifsPlayerUtils.AllowAutoplay();
    }

    public static bool AllowAutoplay()
    {
      if (!AppGlobalStateManager.Current.GlobalState.GifAutoplayFeatureAvailable)
        return false;
      GifAutoplayMode gifAutoplayType = AppGlobalStateManager.Current.GlobalState.GifAutoplayType;
      NetworkStatus networkStatus = NetworkStatusInfo.Instance.NetworkStatus;
      if (gifAutoplayType == GifAutoplayMode.Always)
        return true;
      if (gifAutoplayType == GifAutoplayMode.WifiOnly)
        return networkStatus == NetworkStatus.WiFi;
      return false;
    }
  }
}
