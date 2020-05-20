using System.Collections.Generic;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public static class ConversationsViewModelUpdatesListener
  {
    public static void Listen()
    {
      InstantUpdatesManager.Current.ReceivedUpdates += new InstantUpdatesManager.UpdatesReceivedEventHandler(ConversationsViewModelUpdatesListener.ProcessInstantUpdates);
    }

    private static void ProcessInstantUpdates(List<LongPollServerUpdateData> updates)
    {
      ConversationsViewModel.Instance.ProcessInstantUpdates(updates);
    }
  }
}
