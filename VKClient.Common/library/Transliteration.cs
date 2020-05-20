using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Common.Library
{
  public static class Transliteration
  {
    private static Dictionary<char, string> mapCyrillic = new Dictionary<char, string>() { { 'А', "A" }, { 'Б', "B" }, { 'В', "V" }, { 'Г', "G" }, { 'Д', "D" }, { 'Е', "E" }, { 'Ж', "Zh" }, { 'З', "Z" }, { 'И', "I" }, { 'Й', "I" }, { 'К', "K" }, { 'Л', "L" }, { 'М', "M" }, { 'Н', "N" }, { 'О', "O" }, { 'П', "P" }, { 'Р', "R" }, { 'С', "S" }, { 'Т', "T" }, { 'У', "U" }, { 'Ф', "F" }, { 'Х', "Kh" }, { 'Ц', "Ts" }, { 'Ч', "Ch" }, { 'Ш', "Sh" }, { 'Щ', "Sch" }, { 'Ъ', "'" }, { 'Ы', "Y" }, { 'Ь', "'" }, { 'Э', "E" }, { 'Ю', "Yu" }, { 'Я', "Ya" }, { 'а', "a" }, { 'б', "b" }, { 'в', "v" }, { 'г', "g" }, { 'д', "d" }, { 'е', "e" }, { 'ж', "zh" }, { 'з', "z" }, { 'и', "i" }, { 'й', "i" }, { 'к', "k" }, { 'л', "l" }, { 'м', "m" }, { 'н', "n" }, { 'о', "o" }, { 'п', "p" }, { 'р', "r" }, { 'с', "s" }, { 'т', "t" }, { 'у', "u" }, { 'ф', "f" }, { 'х', "kh" }, { 'ц', "ts" }, { 'ч', "ch" }, { 'ш', "sh" }, { 'щ', "sch" }, { 'ъ', "'" }, { 'ы', "y" }, { 'ь', "'" }, { 'э', "e" }, { 'ю', "yu" }, { 'я', "ya" } };

    public static string CyrillicToLatin(string inputString)
    {
        return string.Concat((IEnumerable<string>)Enumerable.Select<char, string>(Enumerable.Where<char>(inputString, (Func<char, bool>)(s => Transliteration.mapCyrillic.ContainsKey(s))), (Func<char, string>)(c => Transliteration.mapCyrillic[c])));
    }

    public static bool IsCyrillic(char input)
    {
      return Transliteration.mapCyrillic.ContainsKey(input);
    }

    public static bool IsCyrillic(string input)
    {
        return Enumerable.All<char>(input, (Func<char, bool>)(c => Transliteration.mapCyrillic.ContainsKey(c)));
    }

    public static string LatinToCyrillic(string inputString)
    {
      string str0 = string.Empty;
      string processingValue = inputString;
      while (Enumerable.Count<char>(inputString) > 0 && !string.IsNullOrEmpty(processingValue))
      {
          KeyValuePair<char, string> keyValuePair = (KeyValuePair<char, string>)Enumerable.FirstOrDefault<KeyValuePair<char, string>>(Transliteration.mapCyrillic, (Func<KeyValuePair<char, string>, bool>)(m => string.Compare(m.Value, processingValue) == 0));
        if (keyValuePair.Value == null)
        {
          processingValue = ((string) processingValue).Substring(0, ((string) processingValue).Length - 1);
        }
        else
        {
          inputString = ((string) inputString).Substring(((string) processingValue).Length);
          processingValue = inputString;
          str0 = string.Concat(str0, keyValuePair.Key.ToString());
        }
      }
      return str0;
    }
  }
}
