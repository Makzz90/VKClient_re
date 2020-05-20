using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.Utils
{
  public static class ExtensionsBase
  {
    public static string GetShorterVersion(this string str)
    {
      if (string.IsNullOrEmpty(str) || str.Length < 32)
        return str;
      return MD5Core.GetHashString(str);
    }

    public static void ParseCoordinates(this string str, out double latitude, out double longitude)
    {
      latitude = 0.0;
      longitude = 0.0;
      string[] strArray = str.Split(' ');
      if (strArray.Length <= 1)
        return;
      double.TryParse(strArray[0], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out latitude);
      double.TryParse(strArray[1], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out longitude);
    }

    public static Uri ConvertToUri(this string uriStr)
    {
      return !string.IsNullOrEmpty(uriStr) ? (uriStr.StartsWith(".") || uriStr.StartsWith("/") ? new Uri(uriStr, UriKind.Relative) : new Uri(uriStr, UriKind.Absolute)) :  null;
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp, bool includeTimeDiff = true)
    {
      return BaseFormatterHelper.UnixTimeStampToDateTime(unixTimeStamp, includeTimeDiff);
    }

    public static int DateTimeToUnixTimestamp(DateTime dt, bool includeTimeDiff = true)
    {
      DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
      int num = (int) ((dt.Ticks - dateTime.Ticks) / 10000000L);
      if (includeTimeDiff)
      {
        int minusLocalTimeDelta = AppGlobalStateManager.Current.GlobalState.ServerMinusLocalTimeDelta;
        num += minusLocalTimeDelta;
      }
      return num;
    }

    public static List<long> ParseCommaSeparated(this string commaSeparatedIds)
    {
      if (string.IsNullOrWhiteSpace(commaSeparatedIds))
        return new List<long>();
      string[] strArray = commaSeparatedIds.Split(new char[1]
      {
        ','
      }, StringSplitOptions.RemoveEmptyEntries);
      List<long> longList = new List<long>();
      foreach (string s in strArray)
        longList.Add(long.Parse(s));
      return longList;
    }

    public static List<string> ParseCommaSeparatedString(this string commaSeparated)
    {
      if (string.IsNullOrWhiteSpace(commaSeparated))
        return new List<string>();
      string[] strArray = commaSeparated.Split(new char[1]
      {
        ','
      }, StringSplitOptions.RemoveEmptyEntries);
      List<string> stringList = new List<string>();
      foreach (string str in strArray)
        stringList.Add(str);
      return stringList;
    }

    public static string ForUI(string backendTextString)
    {
      if (string.IsNullOrEmpty(backendTextString))
        return "";
      return HttpUtility.HtmlDecode(backendTextString.Replace("<br>", Environment.NewLine).Replace("<br/>", Environment.NewLine).Replace("\r\n", "\n").Replace("\n", "\r\n")).Trim();
    }

    public static string ForURL(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return "";
      return HttpUtility.UrlEncode(str);
    }

    public static string ToStringExt(this IDictionary<string, string> dict)
    {
      string str = "";
      if (dict == null)
        return str;
      foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>) dict)
        str = str + keyValuePair.Key + "=" + keyValuePair.Value + ",";
      return str;
    }
  }
}
