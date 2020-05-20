using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class LoginService : ILoginService
  {
    private static LoginService _instance;

    public static LoginService Instance
    {
      get
      {
        if (LoginService._instance == null)
          LoginService._instance = new LoginService();
        return LoginService._instance;
      }
    }

    public void GetAccessToken(string userId, string password, Action<BackendResult<AutorizationData, ResultCode>> callbackAction)
    {
      VKRequestsDispatcher.DispatchLoginRequest(userId, password, callbackAction, new CancellationToken?());
    }

    public void GetAccessToken(string userId, string password, string code, Action<BackendResult<AutorizationData, ResultCode>> callbackAction)
    {
      VKRequestsDispatcher.DispatchLoginRequest(userId, password, code, false, callbackAction, new CancellationToken?());
    }

    [Obsolete("Method is deprecated. Use validationSid", false)]
    public void SendSMS(string userId, string password, Action<BackendResult<AutorizationData, ResultCode>> callbackAction)
    {
      VKRequestsDispatcher.DispatchLoginRequest(userId, password, "", true, callbackAction, new CancellationToken?());
    }

    public void SendSMS(string validationSid, Action<BackendResult<int, ResultCode>> callbackAction)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("auth.validatePhone", new Dictionary<string, string>()
      {
        {
          "sid",
          validationSid
        }
      }, callbackAction,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
