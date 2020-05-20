using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Library
{
  public static class LanguagesList
  {
    private static List<BGType> _list;

    public static List<BGType> GetLanguages()
    {
      if (LanguagesList._list == null)
        LanguagesList._list = new List<BGType>()
        {
          new BGType()
          {
            id = 0,
            name = CommonResources.AccentChoiceSystem
          },
          new BGType() { id = 1, name = "English" },
          new BGType() { id = 2, name = "русский" },
          new BGType() { id = 3, name = "українська" },
          new BGType() { id = 4, name = "беларуская" },
          new BGType() { id = 5, name = "português brasileiro" }
        };
      return LanguagesList._list;
    }
  }
}
