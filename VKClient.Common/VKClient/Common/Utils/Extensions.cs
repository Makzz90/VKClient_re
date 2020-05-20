using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.Utils
{
  public static class Extensions
  {
    public static bool IsDigit(this Key key)
    {
      if (key != Key.D1 && key != Key.D2 && (key != Key.D3 && key != Key.D4) && (key != Key.D5 && key != Key.D6 && (key != Key.D7 && key != Key.D8)) && key != Key.D9)
        return key == Key.D0;
      return true;
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
      return !string.IsNullOrEmpty(uriStr) ? (uriStr.StartsWith(".") || uriStr.StartsWith("/") || !uriStr.ToLowerInvariant().StartsWith("http") ? new Uri(uriStr, UriKind.Relative) : new Uri(uriStr, UriKind.Absolute)) : (Uri) null;
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
      string[] strArray = commaSeparatedIds.Split(new char[1]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
      List<long> longList = new List<long>();
      foreach (string s in strArray)
        longList.Add(long.Parse(s));
      return longList;
    }

    public static List<string> ParseCommaSeparatedString(this string commaSeparated)
    {
      if (string.IsNullOrWhiteSpace(commaSeparated))
        return new List<string>();
      string[] strArray = commaSeparated.Split(new char[1]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
      List<string> stringList = new List<string>();
      foreach (string str in strArray)
        stringList.Add(str);
      return stringList;
    }

    public static Color ToColor(this string colorHex)// NEW: 4.8.0
    {
        return Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
    }

    public static SolidColorBrush GetColor(this string colorHex)
    {
        return new SolidColorBrush(colorHex.ToColor());
    }

    public static Color GetColorFromString(string colorHex)
    {
      return Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
    }

    public static Color AlphaBlend(Color background, Color foreground)
    {
      double num1 = (double) background.R;
      double num2 = (double) background.G;
      double num3 = (double) background.B;
      double num4 = (double) foreground.A;
      double num5 = (double) foreground.R;
      double num6 = (double) foreground.G;
      double num7 = (double) foreground.B;
      double num8 = (double) byte.MaxValue;
      double num9 = num4 / num8;
      double num10 = num9 * num5 + (1.0 - num9) * num1;
      double num11 = num9 * num6 + (1.0 - num9) * num2;
      double num12 = num9 * num7 + (1.0 - num9) * num3;
      return new Color()
      {
        A = byte.MaxValue,
        R = (byte) num10,
        G = (byte) num11,
        B = (byte) num12
      };
    }

    public static string GetUserStatusString(this UserStatus userStatus, bool isMale)
    {
      if (userStatus == null || userStatus.time == 0L)
        return "";
      if (userStatus.online == 1L)
        return CommonResources.Conversation_Online;
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) userStatus.time, true);
      string str1 = string.Empty;
      DateTime now = DateTime.Now;
      int int32 = Convert.ToInt32(Math.Floor((now - dateTime).TotalMinutes));
      string str2;
      if (int32 > 0 && int32 < 60)
      {
        if (int32 < 2)
        {
          str2 = !isMale ? CommonResources.Conversation_LastSeenAMomentAgoFemale : CommonResources.Conversation_LastSeenAMomentAgoMale;
        }
        else
        {
          int num = int32 % 10;
          str2 = !isMale ? (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoFemaleFrm, (object) int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoFemaleFrm, (object) int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoFemaleFrm, (object) int32)) : (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoMaleFrm, (object) int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoMaleFrm, (object) int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoMaleFrm, (object) int32));
        }
      }
      else
        str2 = !(now.Date == dateTime.Date) ? (!(now.AddDays(-1.0).Date == dateTime.Date) ? (now.Year != dateTime.Year ? (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, (object) dateTime.ToString("dd MMMM yyyy"), (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, (object) dateTime.ToString("dd MMMM yyyy"), (object) dateTime.ToString("HH:mm"))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, (object) dateTime.ToString("dd MMMM"), (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, (object) dateTime.ToString("dd MMMM"), (object) dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenYesterdayFemaleFrm, (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenYesterdayMaleFrm, (object) dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenTodayFemaleFrm, (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenTodayMaleFrm, (object) dateTime.ToString("HH:mm")));
      return str2;
    }

    public static string ForUI(this string backendTextString)
    {
      return ExtensionsBase.ForUI(backendTextString);
    }

    public static string ForURL(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return "";
      return HttpUtility.UrlEncode(str);
    }

    public static string FromURL(this string str)
    {
      if (string.IsNullOrEmpty(str))
        return "";
      return HttpUtility.UrlDecode(str);
    }

    public static void ScrollToBottomWithAnimation(this ScrollViewer scroll)
    {
      scroll.ScrollToOffsetWithAnimation(scroll.ScrollableHeight, 0.25, true);
    }

    public static void ScrollToTopWithAnimation(this ScrollViewer scroll)
    {
      scroll.ScrollToOffsetWithAnimation(0.0, 0.35, true);
    }

    public static void ScrollToOffsetWithAnimation(this ScrollViewer scroll, double offset, double durationSec, bool prepareVirtPanel = false)
    {
      if (scroll.VerticalOffset == offset)
        return;
      ScrollViewerOffsetMediator viewerOffsetMediator = new ScrollViewerOffsetMediator();
      MyVirtualizingPanel virtualizingPanel = scroll.Descendants<MyVirtualizingPanel>().FirstOrDefault<DependencyObject>() as MyVirtualizingPanel;
      if (virtualizingPanel != null & prepareVirtPanel)
        virtualizingPanel.PrepareForScrollToBottom(offset > 0.0);
      double verticalOffset = scroll.VerticalOffset;
      viewerOffsetMediator.ScrollViewer = scroll;
      viewerOffsetMediator.VerticalOffset = verticalOffset;
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.From = new double?(verticalOffset);
      doubleAnimation.To = new double?(offset);
      doubleAnimation.Duration = (Duration) TimeSpan.FromSeconds(durationSec);
      doubleAnimation.AutoReverse = false;
      doubleAnimation.EasingFunction = (IEasingFunction) new CubicEase();
      Storyboard.SetTarget((Timeline) doubleAnimation, (DependencyObject) viewerOffsetMediator);
      Storyboard.SetTargetProperty((Timeline) doubleAnimation, new PropertyPath((object) ScrollViewerOffsetMediator.VerticalOffsetProperty));
      Storyboard storyboard = new Storyboard();
      storyboard.Children.Add((Timeline) doubleAnimation);
      storyboard.Begin();
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
