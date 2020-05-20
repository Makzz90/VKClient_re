using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class ConversationViewModelCache
  {
    private object _lockObj = new object();
    private Dictionary<string, ConversationViewModel> _inMemoryCachedData = new Dictionary<string, ConversationViewModel>();
    private int _maxNumberOfInMemoryItems = 12;
    private static ConversationViewModelCache _current;

    public static ConversationViewModelCache Current
    {
      get
      {
        if (ConversationViewModelCache._current == null)
          ConversationViewModelCache._current = new ConversationViewModelCache();
        return ConversationViewModelCache._current;
      }
    }

    public void SubscribeToUpdates()
    {
      InstantUpdatesManager.Current.ReceivedUpdates += new InstantUpdatesManager.UpdatesReceivedEventHandler(this.ReceivedUpdates);
    }

    private void ReceivedUpdates(List<LongPollServerUpdateData> updates)
    {
      List<ConversationViewModel> conversationViewModelList = new List<ConversationViewModel>();
      foreach (LongPollServerUpdateData update in updates)
      {
        bool isChat = update.isChat;
        long userOrCharId = isChat ? update.chat_id : update.user_id;
        if (userOrCharId != 0L)
        {
          bool onlyInMemoryCache = update.UpdateType != LongPollServerUpdateType.MessageHasBeenAdded;
          ConversationViewModel vm = this.GetVM(userOrCharId, isChat, onlyInMemoryCache);
          if (vm != null)
            conversationViewModelList.Add(vm);
        }
      }
      using (IEnumerator<ConversationViewModel> enumerator = ((IEnumerable<ConversationViewModel>) Enumerable.Distinct<ConversationViewModel>(conversationViewModelList)).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
          enumerator.Current.ProcessInstantUpdates(updates);
      }
    }

    public void ClearInMemoryCacheImmediately()
    {
      this._inMemoryCachedData.Clear();
    }

    public void FlushToPersistentStorage()
    {
      // ISSUE: unable to decompile the method.
    }

    public ConversationViewModel GetVM(long userOrCharId, bool isChatId, bool onlyInMemoryCache = false)
    {
      object lockObj = this._lockObj;
      bool lockTaken = false;
      try
      {
        Monitor.Enter(lockObj, ref lockTaken);
        string key = ConversationViewModelCache.GetKey(userOrCharId, isChatId);
        if (this._inMemoryCachedData.ContainsKey(key))
          return this._inMemoryCachedData[key];
        if (onlyInMemoryCache)
          return  null;
        ConversationViewModel conversationVM = new ConversationViewModel();
        if (!CacheManager.TryDeserialize((IBinarySerializable) conversationVM, key, CacheManager.DataType.CachedData))
          conversationVM.InitializeWith(userOrCharId, isChatId);
        if (conversationVM.OutboundMessageVM == null || conversationVM.Messages == null)
        {
          conversationVM = new ConversationViewModel();
          conversationVM.InitializeWith(userOrCharId, isChatId);
        }
        this.SetVM(conversationVM, false);
        return conversationVM;
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit(lockObj);
      }
    }

    public void SetVM(ConversationViewModel conversationVM, bool allowFlush)
    {
      if (conversationVM == null)
        return;
      object lockObj = this._lockObj;
      bool lockTaken = false;
      try
      {
        Monitor.Enter(lockObj, ref lockTaken);
        this._inMemoryCachedData[ConversationViewModelCache.GetKey(conversationVM.UserOrCharId, conversationVM.IsChat)] = conversationVM;
        if (!(this._inMemoryCachedData.Count > this._maxNumberOfInMemoryItems & allowFlush))
          return;
        this.FlushToPersistentStorage();
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit(lockObj);
      }
    }

    private static string GetKey(ConversationViewModel cvm)
    {
      return ConversationViewModelCache.GetKey(cvm.UserOrCharId, cvm.IsChat);
    }

    private static string GetKey(long userOrChatId, bool isChatId)
    {
      return string.Concat((string[]) new string[5]{ (string) "MSG", (string) AppGlobalStateManager.Current.LoggedInUserId.ToString(), (string) "_", (string) userOrChatId.ToString(), (string) isChatId.ToString() });
    }
  }
}
