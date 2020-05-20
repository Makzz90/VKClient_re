using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
			IEnumerable<string> arg_4D_0 = query.TrimStart(new char[]
			{
				'?'
			}).Split(new char[]
			{
				'&'
			});
			Func<string, bool> arg_4D_1 = new Func<string, bool>((x)=>{return x != "";});
			
			return Enumerable.Select<string, KeyValuePair<string, string>>(Enumerable.Where<string>(arg_4D_0, arg_4D_1), delegate(string x)
			{
				string[] array = x.Split(new char[]
				{
					'='
				});
				if (post)
				{
					return new KeyValuePair<string, string>(array[0].UrlDecode(), array[1].UrlDecodeForPost());
				}
				return new KeyValuePair<string, string>(array[0].UrlDecode(), array[1].UrlDecode());
			});
		}

    public static void OpenSetStatusPopup(string status, long groupId, Action<string> callback)
    {
        EditStatusUC editStatusUC = new EditStatusUC();
        editStatusUC.TextBoxText.Text=(status);
        DialogService statusChangePopup = new DialogService
        {
            SetStatusBarBackground = true,
            Child = editStatusUC
        };
        Action updateStatusAction = delegate
        {
            string newStatus = editStatusUC.TextBoxText.Text.Replace("\r\n", "\r").Replace("\r", "\r\n");
            AccountService.Instance.StatusSet(newStatus, "", groupId, delegate(BackendResult<long, ResultCode> res)
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    if (groupId == 0L)
                    {
                        AppGlobalStateManager.Current.UpdateCachedUserStatus(newStatus);
                        EventAggregator.Current.Publish(new BaseDataChangedEvent());
                        bool flag = groupId > 0L;
                        long id = flag ? groupId : AppGlobalStateManager.Current.LoggedInUserId;
                        EventAggregator.Current.Publish(new ProfileStatusChangedEvent(id, flag, newStatus));
                    }
                    if (callback != null)
                    {
                        callback.Invoke(newStatus);
                    }
                }
            });
            statusChangePopup.Hide();
        };
        editStatusUC.TextBoxText.KeyUp+=(delegate(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                updateStatusAction.Invoke();
            }
        });
        editStatusUC.ButtonSave.Click+=(delegate(object s, RoutedEventArgs e)
        {
            updateStatusAction.Invoke();
        });
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
