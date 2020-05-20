using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class BGAutoplayTypes
  {
    private static List<BGType> _list;

    public static List<BGType> GetBGTypes()
    {
      if (BGAutoplayTypes._list == null)
        BGAutoplayTypes._list = new List<BGType>()
        {
          new BGType()
          {
            id = 1,
            name = CommonResources.Settings_General_AutoplayGifAlways
          },
          new BGType()
          {
            id = 0,
            name = CommonResources.Settings_General_AutoplayGifWiFiOnly
          },
          new BGType()
          {
            id = 2,
            name = CommonResources.Settings_General_AutoplayGifNever
          }
        };
      return BGAutoplayTypes._list;
    }
  }
}
