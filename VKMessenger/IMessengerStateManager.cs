using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using VKMessenger.Library;

namespace VKMessenger
{
  public interface IMessengerStateManager
  {
    List<string> ConversationSearchStrings { get; }

    List<string> FriendsSearchStrings { get; }

    DateTime AppStartedTime { get; set; }

    ConversationSearchViewModel ConversationSearchVM { get; }

    PhoneApplicationFrame RootFrame { get; }

    long HandleInAppNotification(string title, string message, long lUid, string isChat, string imageSrc);

    void EnsureOnlineStatusIsSet(bool p);

    void IncreaseNumberOfUnreadMessagesBy(int number);
  }
}
