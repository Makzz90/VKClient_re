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
            List<string> list = Enumerable.ToList<string>(searchString.Split(new char[] { ' ' }));
            List<string> latinStrings = Enumerable.ToList<string>(Enumerable.Select<string, string>(list, new Func<string, string>(Transliteration.CyrillicToLatin)));
            List<string> cyrillicStrings = Enumerable.ToList<string>(Enumerable.Select<string, string>(list, new Func<string, string>(Transliteration.LatinToCyrillic)));
            return new MatchStrings
            {
                SearchStrings = list,
                LatinStrings = latinStrings,
                CyrillicStrings = cyrillicStrings
            };
        }
    }
}
