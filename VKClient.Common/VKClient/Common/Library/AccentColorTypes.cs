using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class AccentColorTypes
  {
    private static List<BGType> _list;

    public static List<BGType> GetAccentTypes()
    {
      if (AccentColorTypes._list == null)
        AccentColorTypes._list = new List<BGType>()
        {
          new BGType()
          {
            id = 1,
            name = CommonResources.AccentChoiceVK
          },
          new BGType()
          {
            id = 0,
            name = CommonResources.AccentChoiceSystem
          }
        };
      return AccentColorTypes._list;
    }
  }
}
