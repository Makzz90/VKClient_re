using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.Utils
{
  public class BaseFormatterHelper
  {
    public static DateTimeDiff GetDateTimeDiff(double unixTimeStamp)
    {
      return BaseFormatterHelper.GetDateTimeDiff(BaseFormatterHelper.UnixTimeStampToDateTime(unixTimeStamp, true));
    }

    public static DateTimeDiff GetDateTimeDiff(DateTime dateTime)
    {
      DateTime now = DateTime.Now;
      TimeSpan timeSpan = now - dateTime;
      DateTimeDiff dateTimeDiff = new DateTimeDiff();
      if (timeSpan.TotalSeconds < 5.0 || now <= dateTime)
        dateTimeDiff.DiffType = DateTimeDiffType.JustNow;
      else if (timeSpan.TotalSeconds >= 5.0 && timeSpan.TotalSeconds < 60.0)
      {
        dateTimeDiff.DiffType = DateTimeDiffType.Seconds;
        dateTimeDiff.Value = (int) timeSpan.TotalSeconds;
      }
      else if (timeSpan.TotalSeconds >= 60.0 && timeSpan.TotalSeconds < 3600.0)
      {
        dateTimeDiff.DiffType = DateTimeDiffType.Minutes;
        dateTimeDiff.Value = (int) timeSpan.TotalMinutes;
      }
      else if (timeSpan.TotalSeconds >= 3600.0 && timeSpan.TotalSeconds < 86400.0)
      {
        dateTimeDiff.DiffType = DateTimeDiffType.Hours;
        dateTimeDiff.Value = (int) timeSpan.TotalHours;
      }
      else if (timeSpan.TotalSeconds >= 86400.0 && timeSpan.TotalSeconds < 2592000.0 || now.Year == dateTime.Year && now.Month == dateTime.Month)
      {
        dateTimeDiff.DiffType = DateTimeDiffType.Days;
        dateTimeDiff.Value = (int) (timeSpan.TotalSeconds / 86400.0);
      }
      else if (timeSpan.TotalSeconds < 31536000.0)
      {
        dateTimeDiff.DiffType = DateTimeDiffType.Months;
        dateTimeDiff.Value = (int) (timeSpan.TotalSeconds / 2592000.0);
      }
      else
      {
        dateTimeDiff.DiffType = DateTimeDiffType.Years;
        dateTimeDiff.Value = (int) (timeSpan.TotalSeconds / 31536000.0);
      }
      return dateTimeDiff;
    }

    public static List<string> ParsePhoneNumbers(string rawStr)
    {
      if (rawStr == null || rawStr == "")
        return new List<string>();
      rawStr = rawStr.Replace("+", ",+");
      string[] strArray = rawStr.Split(new char[2]
      {
        ',',
        ';'
      });
      List<string> stringList = new List<string>();
      foreach (string str1 in strArray)
      {
        string source1 = str1.Replace('-', ' ').Replace('(', ' ').Replace(')', ' ').Trim();
        int num = source1.Length <= 0 ? 0 : ((int) source1[0] == 43 ? 1 : 0);
        string str2 = new string(source1.Where<char>((Func<char, bool>) (c =>
        {
          if (!char.IsDigit(c))
            return (int) c == 32;
          return true;
        })).ToArray<char>());
        if (num != 0)
          str2 = "+" + str2;
        string source2 = str2;
        Func<char, bool> func = (Func<char, bool>) (c => char.IsDigit(c));
        if (new string(source2.Where<char>(func).ToArray<char>()).Length >= 4)
          stringList.Add(str2);
      }
      return stringList;
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp, bool includeTimeDiff = true)
    {
      DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      if (includeTimeDiff)
      {
        int minusLocalTimeDelta = AppGlobalStateManager.Current.GlobalState.ServerMinusLocalTimeDelta;
        unixTimeStamp -= (double) minusLocalTimeDelta;
      }
      dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
      return dateTime;
    }

    public static long GetLastMidnight(long unixTimeStamp, bool includeTimeDiff = true)
    {
      if (includeTimeDiff)
      {
        int minusLocalTimeDelta = AppGlobalStateManager.Current.GlobalState.ServerMinusLocalTimeDelta;
        unixTimeStamp -= (long) minusLocalTimeDelta;
      }
      return unixTimeStamp - (unixTimeStamp + 14400L) % 86400L;
    }

    public static string FormatNumberOfSomething(int number, string oneSomethingFrm, string twoSomethingFrm, string fiveSomethingFrm, bool includeNumberInResult = true, string additionalFormatParam = null, bool includeZero = false)
    {
      if (number < 0)
        throw new Exception("Invalid number to format.");
      if (number == 0 && !includeZero)
        return "";
      int num = number % 10;
      string format = !(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "en") || !(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "pt") ? (number == 1 ? oneSomethingFrm : twoSomethingFrm) : (num != 1 || number % 100 == 11 ? (num < 2 || num > 4 || number % 100 >= 12 && number % 100 <= 14 ? fiveSomethingFrm : twoSomethingFrm) : oneSomethingFrm);
      if (!includeNumberInResult)
        return string.Format(format, "").Trim();
      NumberFormatInfo numberFormatInfo = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
      numberFormatInfo.NumberGroupSeparator = " ";
      string str = number.ToString("#,#", (IFormatProvider) numberFormatInfo);
      if (additionalFormatParam == null)
        return string.Format(format, str);
      return string.Format(format, str, additionalFormatParam);
    }
  }
}
