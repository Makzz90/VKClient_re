using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
  public static class TransliterationHelper
  {
    public static MatchStrings GetMatchStrings(string searchString)
    {
      List<string> list1 = ((IEnumerable<string>) searchString.Split(' ')).ToList<string>();
      List<string> list2 = list1.Select<string, string>(new Func<string, string>(Transliteration.CyrillicToLatin)).ToList<string>();
      List<string> list3 = list1.Select<string, string>(new Func<string, string>(Transliteration.LatinToCyrillic)).ToList<string>();
      return new MatchStrings()
      {
        SearchStrings = list1,
        LatinStrings = list2,
        CyrillicStrings = list3
      };
    }
  }
}
