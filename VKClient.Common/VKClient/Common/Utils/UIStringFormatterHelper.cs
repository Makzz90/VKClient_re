using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Localization;

namespace VKClient.Common.Utils
{
  public class UIStringFormatterHelper
  {
    private static readonly Regex _userOrGroupRegEx = new Regex("\\[(id|club)\\d+.*?\\|.*?\\]");

    private static string YesterdayStr
    {
      get
      {
        return CommonResources.ConversationsList_Yesterday;
      }
    }

    public static string SubstituteMentionsWithNames(string text)
    {
      int startIndex = 0;
      MatchCollection matchCollection = UIStringFormatterHelper._userOrGroupRegEx.Matches(text);
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Match match in matchCollection)
      {
        if (match.Index != startIndex)
          stringBuilder = stringBuilder.Append(text.Substring(startIndex, match.Index - startIndex));
        int num = match.Value.IndexOf("|");
        string str = match.Value.Substring(num + 1, match.Value.Length - num - 2);
        stringBuilder = stringBuilder.Append(str);
        startIndex = match.Index + match.Length;
      }
      if (startIndex < text.Length)
        stringBuilder = stringBuilder.Append(text.Substring(startIndex));
      return stringBuilder.ToString();
    }

    public static string CorrectNewLineCharacters(string str)
    {
      if (str == null)
        return null;
      str = str.Replace("\r\n", "\r");
      return str.Replace("\r", "\r\n");
    }

    public static string FormatNumberOfSomething(int number, string oneSomethingFrm, string twoSomethingFrm, string fiveSomethingFrm, bool includeNumberInResult = true, string additionalFormatParam = null, bool includeZero = false)
    {
      return BaseFormatterHelper.FormatNumberOfSomething(number, oneSomethingFrm, twoSomethingFrm, fiveSomethingFrm, includeNumberInResult, additionalFormatParam, includeZero);
    }

    public static string FormatDateTimeForUI(int unixTime)
    {
      return UIStringFormatterHelper.FormatDateTimeForUI(Extensions.UnixTimeStampToDateTime((double) unixTime, true));
    }

    public static string FormatDateTimeForUI(DateTime dateTime)
    {
      DateTime now = DateTime.Now;
      int int32 = Convert.ToInt32(Math.Floor((now - dateTime).TotalMinutes));
      string @string = dateTime.ToString("HH:mm");
      if (int32 >= -1 && int32 < 60)
      {
        if (int32 < 1)
          return CommonResources.MomentAgo.ToLowerInvariant();
        return UIStringFormatterHelper.FormatNumberOfSomething(int32, CommonResources.OneMinuteFrm, CommonResources.TwoFourMinutes, CommonResources.FiveMinutes, true, null, false);
      }
      if (now.Date == dateTime.Date)
      {
        if (string.IsNullOrEmpty(UIStringFormatterHelper.AtStr(dateTime)))
          return string.Format("{0} {1}", (object) CommonResources.Today, (object) @string);
        return string.Format("{0} {1} {2}", (object) CommonResources.Today, (object) UIStringFormatterHelper.AtStr(dateTime), (object) @string);
      }
      if (now.AddDays(-1.0).Date == dateTime.Date)
      {
        if (string.IsNullOrEmpty(UIStringFormatterHelper.AtStr(dateTime)))
          return string.Format("{0} {1}", (object) CommonResources.Yesterday, (object) @string);
        return string.Format("{0} {1} {2}", (object) CommonResources.Yesterday, (object) UIStringFormatterHelper.AtStr(dateTime), (object) @string);
      }
      if (now.Year == dateTime.Year)
      {
        int day = dateTime.Day;
        string ofMonthStr = UIStringFormatterHelper.GetOfMonthStr(dateTime.Month);
        if (string.IsNullOrEmpty(UIStringFormatterHelper.AtStr(dateTime)))
          return string.Format("{0} {1} {2}", (object) day, (object) ofMonthStr, (object) @string);
        return string.Format("{0} {1} {2} {3}", (object) day, (object) ofMonthStr, (object) UIStringFormatterHelper.AtStr(dateTime), (object) @string);
      }
      int day1 = dateTime.Day;
      string ofMonthStr1 = UIStringFormatterHelper.GetOfMonthStr(dateTime.Month);
      int year = dateTime.Year;
      string prepositionBeforeYear = CommonResources.PrepositionBeforeYear;
      string str = UIStringFormatterHelper.AtStr(dateTime);
      if (string.IsNullOrEmpty(str))
        return string.Format("{0} {1} {2}", (object) day1, (object) ofMonthStr1, (object) @string);
      if (string.IsNullOrEmpty(prepositionBeforeYear))
      {
        if (string.IsNullOrEmpty(str))
          return string.Format("{0} {1} {2} {3}", (object) day1, (object) ofMonthStr1, (object) year, (object) @string);
        return string.Format("{0} {1} {2} {3} {4}", (object) day1, (object) ofMonthStr1, (object) year, (object) UIStringFormatterHelper.AtStr(dateTime), (object) @string);
      }
      if (string.IsNullOrEmpty(str))
        return string.Format("{0} {1} {2} {3} {4}", (object) day1, (object) ofMonthStr1, (object) prepositionBeforeYear, (object) year, (object) @string);
      return string.Format("{0} {1} {2} {3} {4} {5}", (object) day1, (object) ofMonthStr1, (object) prepositionBeforeYear, (object) year, (object) UIStringFormatterHelper.AtStr(dateTime), (object) @string);
    }

    public static string FormatShortDate(long timestamp)
    {
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) timestamp, true);
      DateTime now = DateTime.Now;
      if (now.Year - dateTime.Year > 0)
        return string.Format(CommonResources.ShortDateFrm_Years, (object) dateTime.Year);
      int num1 = now.Month - dateTime.Month;
      if (num1 > 0)
        return string.Format(CommonResources.ShortDateFrm_Months, (object) num1);
      TimeSpan timeSpan = now - dateTime;
      int num2 = (int) timeSpan.TotalDays;
      if (num2 > 0)
        return string.Format(CommonResources.ShortDateFrm_Days, (object) num2);
      int num3 = (int) timeSpan.TotalHours;
      if (num3 > 0)
        return string.Format(CommonResources.ShortDateFm_Hours, (object) num3);
      int num4 = (int) timeSpan.TotalMinutes;
      if (num4 > 0)
        return string.Format(CommonResources.ShortDateFrm_Minutes, (object) num4);
      int num5 = (int) timeSpan.TotalSeconds;
      if (num5 > 0)
        return string.Format(CommonResources.ShortDateFrm_Seconds, (object) num5);
      return "";
    }

    public static string FormatDateTimeForTimerAttachment(DateTime dateTime)
    {
      return string.Format("{0} {1} {2}", (object) dateTime.ToString("dd MMM"), (object) UIStringFormatterHelper.AtStr(dateTime), (object) dateTime.ToString("HH:mm"));
    }

    public static string AtStr(DateTime dt)
    {
      if (dt.Hour == 1)
        return CommonResources.AtOneInTheNight;
      return CommonResources.At;
    }

    public static string FormateDateForEventUI(DateTime dateTime)
    {
      string str1 = string.Empty;
      string str2;
      if (dateTime.Date != DateTime.Today)
      {
        DateTime date1 = dateTime.Date;
        DateTime dateTime1 = DateTime.Today.AddDays(-1.0);
        DateTime date2 = dateTime1.Date;
        if (date1 == date2)
        {
          str2 = str1 + CommonResources.ConversationsList_Yesterday;
        }
        else
        {
          DateTime date3 = dateTime.Date;
          dateTime1 = DateTime.Today;
          dateTime1 = dateTime1.AddDays(1.0);
          DateTime date4 = dateTime1.Date;
          if (date3 == date4)
          {
            str2 = str1 + CommonResources.ConversationsList_Tomorrow;
          }
          else
          {
            str2 = str1 + (object) dateTime.Day + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
            int year1 = dateTime.Year;
            dateTime1 = DateTime.Now;
            int year2 = dateTime1.Year;
            if (year1 != year2)
              str2 = str2 + " " + (object) dateTime.Year;
          }
        }
      }
      else
        str2 = CommonResources.Today;
      return str2 + " " + UIStringFormatterHelper.AtStr(dateTime) + " " + dateTime.ToShortTimeString();
    }

    public static string FormatDateForMessageUI(DateTime dateTime)
    {
      string str = string.Empty;
      if (dateTime.Date != DateTime.Today)
      {
        DateTime date1 = dateTime.Date;
        DateTime dateTime1 = DateTime.Today.AddDays(-1.0);
        DateTime date2 = dateTime1.Date;
        if (date1 == date2)
        {
          str = str + CommonResources.ConversationsList_Yesterday + ", ";
        }
        else
        {
          int year1 = dateTime.Year;
          dateTime1 = DateTime.Now;
          int year2 = dateTime1.Year;
          str = year1 == year2 ? str + dateTime.ToString("m") + ", " : str + dateTime.ToString("d") + ", ";
        }
      }
      return str + dateTime.ToShortTimeString();
    }

    public static string FormatDateForUIShort(DateTime dateTime)
    {
      DateTime now = DateTime.Now;
      DateTime dateTime1 = new DateTime(now.Year, now.Month, now.Day);
      DateTime dateTime2 = new DateTime(now.Year, 1, 1);
      if (dateTime.Year == now.Year && dateTime.Month == now.Month && dateTime.Day == now.Day)
        return dateTime.ToString("HH:mm");
      DateTime dateTime3 = dateTime.AddDays(-1.0);
      DateTime dateTime4 = now.AddDays(-1.0);
      if (dateTime3.Year == dateTime4.Year && dateTime3.Month == dateTime4.Month && dateTime3.Day == dateTime4.Day)
        return UIStringFormatterHelper.YesterdayStr;
      if (dateTime.Year == now.Year)
        return dateTime.ToString("dd.MM");
      return dateTime.ToString("dd.MM.yy");
    }

    public static string FormatForUIShort(long value)
    {
      if (value < 10000L)
        return UIStringFormatterHelper.FormatForUI(value);
      return (value / 1000L).ToString() + "K";
    }

    public static string FormatForUIVeryShort(long value)
    {
      if (value < 1000L)
        return UIStringFormatterHelper.FormatForUI(value);
      return (value / 1000L).ToString() + "K";
    }

    public static string FormatForUI(long value)
    {
      NumberFormatInfo numberFormatInfo = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
      numberFormatInfo.NumberGroupSeparator = " ";
      return value.ToString("#,#", (IFormatProvider) numberFormatInfo);
    }

    public static string FormatDuration(int durationSeconds)
    {
      if (durationSeconds < 3600)
        return TimeSpan.FromSeconds((double) durationSeconds).ToString("m\\:ss");
      return TimeSpan.FromSeconds((double) durationSeconds).ToString("h\\:mm\\:ss");
    }

    public static string GetOfMonthStr(int month)
    {
      switch (month)
      {
        case 1:
          return CommonResources.OfJanuary;
        case 2:
          return CommonResources.OfFebruary;
        case 3:
          return CommonResources.OfMarch;
        case 4:
          return CommonResources.OfApril;
        case 5:
          return CommonResources.OfMay;
        case 6:
          return CommonResources.OfJune;
        case 7:
          return CommonResources.OfJuly;
        case 8:
          return CommonResources.OfAugust;
        case 9:
          return CommonResources.OfSeptember;
        case 10:
          return CommonResources.OfOctober;
        case 11:
          return CommonResources.OfNovember;
        case 12:
          return CommonResources.OfDecember;
        default:
          return "";
      }
    }
  }
}
