using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public static class LanguagesList
  {
    private static List<BGType> _list;

    public static List<BGType> GetLanguages()
    {
      if (LanguagesList._list == null)
      {
        BGType bgType1 = new BGType()
        {
          id = 0,
          name = CommonResources.AccentChoiceSystem
        };
        BGType bgType2 = new BGType()
        {
          id = 1,
          name = "English"
        };
        BGType bgType3 = new BGType()
        {
          id = 2,
          name = "русский"
        };
        BGType bgType4 = new BGType()
        {
          id = 3,
          name = "українська"
        };
        BGType bgType5 = new BGType()
        {
          id = 4,
          name = "беларуская"
        };
        BGType bgType6 = new BGType()
        {
          id = 5,
          name = "português brasileiro"
        };
        BGType bgType7 = new BGType()
        {
          id = 6,
          name = "қазақша"
        };
        LanguagesList._list = new List<BGType>()
        {
          bgType1,
          bgType2,
          bgType3,
          bgType4,
          bgType5,
          bgType7,
          bgType6
        };
      }
      return LanguagesList._list;
    }
  }
}
