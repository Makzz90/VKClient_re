using System;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public interface ILongPollServerService
  {
    bool HaveToken();

    void GetLongPollServer(Action<BackendResult<LongPollServerResponse, ResultCode>> callback);

    void GetUpdates(string serverName, string key, long ts, Action<BackendResult<UpdatesResponse, LongPollResultCode>> callback);
  }
}
