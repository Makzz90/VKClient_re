using System;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public interface ILoginService
  {
    void GetAccessToken(string userId, string password, Action<BackendResult<AutorizationData, ResultCode>> callbackAction);
  }
}
