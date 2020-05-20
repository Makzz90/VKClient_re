using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKMessenger;
using VKMessenger.Framework;
using VKMessenger.Library;
using VKMessenger.Views;

namespace VKClient.Library
{
  public class MessengerStateManager : IMessengerStateManager
  {
    private DateTime _lastTimeUserOnlineIsSet = DateTime.MinValue;
    private ConversationSearchViewModel _conSearchVM = new ConversationSearchViewModel();

    public List<string> ConversationSearchStrings
    {
      get
      {
        return this._conSearchVM.AllSearchStrings;
      }
    }

    public List<string> FriendsSearchStrings
    {
      get
      {
        return new List<string>();
      }
    }

    public DateTime AppStartedTime { get; set; }

    public ConversationSearchViewModel ConversationSearchVM
    {
      get
      {
        return this._conSearchVM;
      }
    }

    public PhoneApplicationFrame RootFrame
    {
      get
      {
        return ((App) Application.Current).RootFrame;
      }
    }

    public long HandleInAppNotification(string title, string message, long lUid, string isChat, string imageSrc)
{
	Action _9__1=null;
	Execute.ExecuteOnUIThread(delegate
	{
		ConversationPage conversationPage = ((App)Application.Current).RootFrame.Content as ConversationPage;
		long num = 0L;
		if (conversationPage != null)
		{
			num = conversationPage.ConversationVM.UserOrCharId;
		}
		if (num != lUid)
		{
			if (AppGlobalStateManager.Current.GlobalState.NotificationsEnabled)
			{
				string arg_77_0 = title;
				string arg_77_1 = message;
				Action arg_77_2;
				if ((arg_77_2 = _9__1) == null)
				{
                    arg_77_2 = (_9__1 = delegate
					{
						Navigator.Current.NavigateToConversation(lUid, isChat == bool.TrueString, false, "", 0, false);
					});
				}
				InAppToastNotification.Show(arg_77_0, arg_77_1, arg_77_2, imageSrc);
			}
			if (AppGlobalStateManager.Current.GlobalState.VibrationsEnabled)
			{
				DeviceManager.Vibrate();
			}
			if (AppGlobalStateManager.Current.GlobalState.SoundEnabled)
			{
				DeviceManager.PlaySound();
			}
		}
	});
	return lUid;
}

    public void EnsureOnlineStatusIsSet(bool force)
    {
      if (!((DateTime.Now - this._lastTimeUserOnlineIsSet).TotalMinutes > 10.0 | force))
        return;
      this._lastTimeUserOnlineIsSet = DateTime.Now;
      AccountService.Instance.SetUserOnline((Action<BackendResult<object, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          return;
        Logger.Instance.Error("Failed to set user online");
      }));
    }

    public void IncreaseNumberOfUnreadMessagesBy(int number)
    {
    }
  }
}
