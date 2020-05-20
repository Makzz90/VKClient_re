using System;
using System.Collections.Generic;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public interface IChatService
  {
    void GetChatInfo(long chatId, Action<BackendResult<ChatInfo, ResultCode>> callback);

    void EditChat(long chatId, string title, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void RemoveChatUsers(long chatId, List<long> usersToBeRemoved, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void AddChatUsers(long chatId, List<long> userIds, Action<BackendResult<ResponseWithId, ResultCode>> callback);
  }
}
