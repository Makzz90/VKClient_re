using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Library
{
  public static class BGTypes
  {
    private static List<BGType> _list;

    public static List<BGType> GetBGTypes()
    {
      if (BGTypes._list == null)
      {
        BGType bgType1 = new BGType()
        {
          id = 0,
          name = CommonResources.ThemeChoiceSystemTheme
        };
        BGType bgType2 = new BGType()
        {
          id = 1,
          name = CommonResources.ThemeChoiceSystemThemeInverted
        };
        BGTypes._list = new List<BGType>()
        {
          new BGType()
          {
            id = 3,
            name = CommonResources.ThemeChoiceLight
          },
          new BGType()
          {
            id = 2,
            name = CommonResources.ThemeChoiceDark
          },
          bgType1
        };
      }
      return BGTypes._list;
    }
  }
}
