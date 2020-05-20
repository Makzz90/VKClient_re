using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VKClient.Common.Utils
{
  public static class UriExtensions
  {
    private static readonly Regex QueryStringRegex = new Regex("[\\?&](?<name>[^&=]+)=(?<value>[^&=]+)");

    public static Dictionary<string, string> ParseQueryString(this Uri uri)
    {
      if (uri ==  null)
        throw new ArgumentException("uri");
      return uri.OriginalString.ParseQueryString();
    }

    public static Dictionary<string, string> ParseQueryString(this string uriString)
    {
      if (uriString == null)
        throw new ArgumentException("uri");
      MatchCollection matchCollection = UriExtensions.QueryStringRegex.Matches(uriString);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      for (int index = 0; index < matchCollection.Count; ++index)
      {
        Match match = matchCollection[index];
        dictionary[match.Groups["name"].Value] = match.Groups["value"].Value;
      }
      return dictionary;
    }

    public static bool IsExternal(this string uriString)
    {
      if (string.IsNullOrWhiteSpace(uriString))
        return false;
      if (!uriString.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
        return uriString.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);
      return true;
    }
  }
}
