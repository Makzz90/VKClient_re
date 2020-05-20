using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.UC;

namespace VKClient.Common.Utils
{
  internal static class Utility
  {
    private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long ToUnixTime(this DateTime target)
    {
      return (long) (target - Utility.unixEpoch).TotalSeconds;
    }

    public static string UrlEncode(this string stringToEscape)
    {
      return Uri.EscapeDataString(stringToEscape).Replace("!", "%21").Replace("*", "%2A").Replace("'", "%27").Replace("(", "%28").Replace(")", "%29");
    }

    public static string UrlDecode(this string stringToUnescape)
    {
      return stringToUnescape.UrlDecodeForPost().Replace("%21", "!").Replace("%2A", "*").Replace("%27", "'").Replace("%28", "(").Replace("%29", ")");
    }

    public static string UrlDecodeForPost(this string stringToUnescape)
    {
      stringToUnescape = stringToUnescape.Replace("+", " ");
      return Uri.UnescapeDataString(stringToUnescape);
    }

    public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string query, bool post = false)
    {
      return ((IEnumerable<string>) query.TrimStart('?').Split('&')).Where<string>((Func<string, bool>) (x => x != "")).Select<string, KeyValuePair<string, string>>((Func<string, KeyValuePair<string, string>>) (x =>
      {
        string[] strArray = x.Split('=');
        if (post)
          return new KeyValuePair<string, string>(strArray[0].UrlDecode(), strArray[1].UrlDecodeForPost());
        return new KeyValuePair<string, string>(strArray[0].UrlDecode(), strArray[1].UrlDecode());
      }));
    }

    public static void OpenSetStatusPopup(string status, long groupId, Action<string> callback)
    {
      EditStatusUC editStatusUC = new EditStatusUC();
      editStatusUC.TextBoxText.Text = status;
      DialogService dialogService = new DialogService();
      dialogService.SetStatusBarBackground = true;
      EditStatusUC editStatusUc = editStatusUC;
      dialogService.Child = (FrameworkElement) editStatusUc;
      DialogService statusChangePopup = dialogService;
      Action updateStatusAction = (Action) (() =>
      {
        string newStatus = editStatusUC.TextBoxText.Text.Replace("\r\n", "\r").Replace("\r", "\r\n");
        AccountService.Instance.StatusSet(newStatus, "", groupId, (Action<BackendResult<long, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          if (groupId == 0L)
          {
            AppGlobalStateManager.Current.UpdateCachedUserStatus(newStatus);
            EventAggregator.Current.Publish((object) new BaseDataChangedEvent());
            bool isGroup = groupId > 0L;
            EventAggregator.Current.Publish((object) new ProfileStatusChangedEvent(isGroup ? groupId : AppGlobalStateManager.Current.LoggedInUserId, isGroup, newStatus));
          }
          if (callback == null)
            return;
          callback(newStatus);
        }));
        statusChangePopup.Hide();
      });
      editStatusUC.TextBoxText.KeyUp += (KeyEventHandler) ((sender, args) =>
      {
        if (args.Key != Key.Enter)
          return;
        updateStatusAction();
      });
      editStatusUC.ButtonSave.Click += (RoutedEventHandler) ((s, e) => updateStatusAction());
      statusChangePopup.Show(null);
    }

    public static string Wrap(this string input, string wrapper)
    {
      return wrapper + input + wrapper;
    }

    public static string ToString<T>(this IEnumerable<T> source, string separator)
    {
      return string.Join<T>(separator, source);
    }
  }
}
