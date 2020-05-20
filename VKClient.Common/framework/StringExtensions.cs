using System.Text.RegularExpressions;

namespace VKClient.Common.Framework
{
  public static class StringExtensions
  {
    public static string ReplaceByRegex(this string str, Regex regex, string replace)
    {
      return regex.Replace(str, replace);
    }
  }
}
