using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class BGTypes
  {
    private static List<BGType> _list;

    public static List<BGType> GetBGTypes()
    {
      if (BGTypes._list == null)
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
          new BGType()
          {
            id = 0,
            name = CommonResources.ThemeChoiceSystemTheme
          }
        };
      return BGTypes._list;
    }
  }
}
