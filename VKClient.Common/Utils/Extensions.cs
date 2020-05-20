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
      return !string.IsNullOrEmpty(uriStr) ? (uriStr.StartsWith(".") || uriStr.StartsWith("/") || !uriStr.ToLowerInvariant().StartsWith("http") ? new Uri(uriStr, UriKind.Relative) : new Uri(uriStr, UriKind.Absolute)) :  null;
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

    public static Color ToColor(this string colorHex)
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
      // ISSUE: explicit reference operation
      double r1 = (double) ((Color) @background).R;
      // ISSUE: explicit reference operation
      double g1 = (double) ((Color) @background).G;
      // ISSUE: explicit reference operation
      double b1 = (double) ((Color) @background).B;
      // ISSUE: explicit reference operation
      double a = (double) ((Color) @foreground).A;
      // ISSUE: explicit reference operation
      double r2 = (double) ((Color) @foreground).R;
      // ISSUE: explicit reference operation
      double g2 = (double) ((Color) @foreground).G;
      // ISSUE: explicit reference operation
      double b2 = (double) ((Color) @foreground).B;
      double maxValue = (double) byte.MaxValue;
      double num1 = a / maxValue;
      double num2 = num1 * r2 + (1.0 - num1) * r1;
      double num3 = num1 * g2 + (1.0 - num1) * g1;
      double num4 = num1 * b2 + (1.0 - num1) * b1;
      Color color = new Color();
      // ISSUE: explicit reference operation
      color.A = byte.MaxValue;
      // ISSUE: explicit reference operation
      color.R=((byte)num2);
      // ISSUE: explicit reference operation
      color.G=((byte)num3);
      // ISSUE: explicit reference operation
      color.B=((byte)num4);
      return color;
    }

    public static string GetUserStatusString(this UserStatus userStatus, bool isMale)
    {
      if (userStatus == null || userStatus.time == 0L)
        return "";
      if (userStatus.online == 1L)
        return CommonResources.Conversation_Online;
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) userStatus.time, true);
      string empty = string.Empty;
      DateTime now = DateTime.Now;
      int int32 = Convert.ToInt32(Math.Floor((now - dateTime).TotalMinutes));
      string str;
      if (int32 > 0 && int32 < 60)
      {
        if (int32 < 2)
        {
          str = !isMale ? CommonResources.Conversation_LastSeenAMomentAgoFemale : CommonResources.Conversation_LastSeenAMomentAgoMale;
        }
        else
        {
          int num = int32 % 10;
          str = !isMale ? (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoFemaleFrm, int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoFemaleFrm, int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoFemaleFrm, int32)) : (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoMaleFrm, int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoMaleFrm, int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoMaleFrm, int32));
        }
      }
      else
        str = !(now.Date == dateTime.Date) ? (!(now.AddDays(-1.0).Date == dateTime.Date) ? (now.Year != dateTime.Year ? (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, dateTime.ToString("dd MMMM yyyy"), dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, dateTime.ToString("dd MMMM yyyy"), dateTime.ToString("HH:mm"))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, dateTime.ToString("dd MMMM"), dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, dateTime.ToString("dd MMMM"), dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenYesterdayFemaleFrm, dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenYesterdayMaleFrm, dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenTodayFemaleFrm, dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenTodayMaleFrm, dateTime.ToString("HH:mm")));
      return str;
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
      MyVirtualizingPanel virtualizingPanel = Enumerable.FirstOrDefault<DependencyObject>(((DependencyObject) scroll).Descendants<MyVirtualizingPanel>()) as MyVirtualizingPanel;
      if (virtualizingPanel != null & prepareVirtPanel)
        virtualizingPanel.PrepareForScrollToBottom(offset > 0.0);
      double verticalOffset = scroll.VerticalOffset;
      viewerOffsetMediator.ScrollViewer = scroll;
      viewerOffsetMediator.VerticalOffset = verticalOffset;
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.From=(new double?(verticalOffset));
      doubleAnimation.To=(new double?(offset));
      ((Timeline) doubleAnimation).Duration=((TimeSpan.FromSeconds(durationSec)));
      ((Timeline) doubleAnimation).AutoReverse = false;
      doubleAnimation.EasingFunction=((IEasingFunction) new CubicEase());
      Storyboard.SetTarget((Timeline) doubleAnimation, (DependencyObject) viewerOffsetMediator);
      Storyboard.SetTargetProperty((Timeline) doubleAnimation, new PropertyPath(ScrollViewerOffsetMediator.VerticalOffsetProperty));
      Storyboard storyboard = new Storyboard();
      ((PresentationFrameworkCollection<Timeline>) storyboard.Children).Add((Timeline) doubleAnimation);
      storyboard.Begin();
    }

    public static string ToStringExt(this IDictionary<string, string> dict)
    {
      string str = "";
      if (dict == null)
        return str;
      IEnumerator<KeyValuePair<string, string>> enumerator = ((IEnumerable<KeyValuePair<string, string>>) dict).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<string, string> current = enumerator.Current;
          str = str + current.Key + "=" + current.Value + ",";
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
      return str;
    }
  }
}
