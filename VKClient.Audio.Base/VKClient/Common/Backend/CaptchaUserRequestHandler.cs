using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend
{
  public class CaptchaUserRequestHandler
  {
    public static Action<CaptchaUserRequest, Action<CaptchaUserResponse>> CaptchaRequest { private get; set; }

    public static Action<ValidationUserRequest, Action<ValidationUserResponse>> ValidationRequest { private get; set; }

    public static Action<Validation2FAUserRequest, Action<ValidationUserResponse>> Validation2FARequest { private get; set; }

    public static void InvokeValidationRequest(ValidationUserRequest request, Action<ValidationUserResponse> callback)
    {
      if (CaptchaUserRequestHandler.ValidationRequest == null)
        callback(new ValidationUserResponse()
        {
          IsSucceeded = false
        });
      else
        Execute.ExecuteOnUIThread((Action) (() => CaptchaUserRequestHandler.ValidationRequest(request, callback)));
    }

    public static void InvokeValidation2FARequest(Validation2FAUserRequest request, Action<ValidationUserResponse> callback)
    {
      if (CaptchaUserRequestHandler.Validation2FARequest == null)
        callback(new ValidationUserResponse()
        {
          IsSucceeded = false
        });
      else
        Execute.ExecuteOnUIThread((Action) (() => CaptchaUserRequestHandler.Validation2FARequest(request, callback)));
    }

    public static void InvokeCaptchaRequest(CaptchaUserRequest request, Action<CaptchaUserResponse> callback)
    {
      if (CaptchaUserRequestHandler.CaptchaRequest == null)
      {
        Action<CaptchaUserResponse> action = callback;
        CaptchaUserResponse captchaUserResponse = new CaptchaUserResponse();
        captchaUserResponse.Request = request;
        captchaUserResponse.EnteredString = string.Empty;
        int num = 1;
        captchaUserResponse.IsCancelled = num != 0;
        action(captchaUserResponse);
      }
      else
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (CaptchaUserRequestHandler.CaptchaRequest == null)
          {
            Action<CaptchaUserResponse> action = callback;
            CaptchaUserResponse captchaUserResponse = new CaptchaUserResponse();
            captchaUserResponse.Request = request;
            captchaUserResponse.EnteredString = string.Empty;
            int num = 1;
            captchaUserResponse.IsCancelled = num != 0;
            action(captchaUserResponse);
          }
          else
            CaptchaUserRequestHandler.CaptchaRequest(request, callback);
        }));
    }
  }
}
