using System;

namespace VKClient.Audio.Base.Extensions
{
  public static class StringExtensions
  {
    public static string Capitalize(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return str;
      if (str.Length > 1)
        return str[0].ToString().ToUpperInvariant() + str.Substring(1);
      return str[0].ToString().ToUpperInvariant();
    }

    public static T ParseToEnum<T>(this string value) where T : struct, IConvertible
    {
      if (!typeof (T).IsEnum)
        throw new ArgumentException("T must be an enumerated type");
      T result;
      if (string.IsNullOrEmpty(value) || !Enum.TryParse<T>(value, out result))
        return default (T);
      return result;
    }
  }
}
