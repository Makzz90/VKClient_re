using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend
{
  public class LogoutRequestHandler
  {
    private static object _lockObj = new object();

    public static Action LogoutRequest { private get; set; }

    public static void InvokeLogoutRequest()
    {
      lock (LogoutRequestHandler._lockObj)
      {
        if (LogoutRequestHandler.LogoutRequest == null)
          return;
        Execute.ExecuteOnUIThread(LogoutRequestHandler.LogoutRequest);
        LogoutRequestHandler.LogoutRequest =  null;
      }
    }
  }
}
