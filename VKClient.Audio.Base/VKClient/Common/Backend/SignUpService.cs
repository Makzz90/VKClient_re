using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class SignUpService
  {
    //private readonly string BaseUrl = "https://api.vk.com/method/";
    //private readonly string CheckPhoneUrl = "auth.checkPhone";
    //private readonly string ConfirmUrl = "auth.confirm";
    //private readonly string SignUpUrl = "auth.signup";
    private static SignUpService _instance;
    private static readonly bool TestMode;

    public static SignUpService Instance
    {
      get
      {
        if (SignUpService._instance == null)
          SignUpService._instance = new SignUpService();
        return SignUpService._instance;
      }
    }

    public void CheckPhone(string phone, Action<BackendResult<string, ResultCode>> callbackAction)
    {
      VKRequestsDispatcher.DispatchRequestToVK<string>("auth.checkPhone", new Dictionary<string, string>()
      {
        {
          "phone",
          phone
        },
        {
          "client_id",
          VKConstants.ApplicationID
        },
        {
          "client_secret",
          VKConstants.ApplicationSecretKey
        }
      }, callbackAction,  null, false, true, new CancellationToken?(),  null);
    }

    public void SignUp(string phone, string firstName, string lastName, bool voice, bool isMale, string sid, Action<BackendResult<string, ResultCode>> callbackAction)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary.Add("phone", phone);
      dictionary.Add("first_name", firstName);
      dictionary.Add("last_name", lastName);
      dictionary.Add("client_id", VKConstants.ApplicationID);
      dictionary.Add("client_secret", VKConstants.ApplicationSecretKey);
      dictionary["voice"] = voice ? "1" : "0";
      dictionary["sex"] = isMale ? "2" : "1";
      if (SignUpService.TestMode)
        dictionary["test_mode"] = "1";
      if (!string.IsNullOrEmpty(sid))
        dictionary.Add("sid", sid);
      string methodName = "auth.signup";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<string, ResultCode>> callback = callbackAction;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<string>(methodName, parameters, callback, (Func<string, string>) (jsonStr => JsonConvert.DeserializeObject<KeyValuePair<string, string>>(jsonStr).Value), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void ConfirmSignUp(string phone, string code, string password, Action<BackendResult<SignupConfirmation, ResultCode>> callbackAction)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("phone", phone);
      parameters.Add("code", code);
      parameters.Add("client_id", VKConstants.ApplicationID);
      parameters.Add("client_secret", VKConstants.ApplicationSecretKey);
      if (!string.IsNullOrEmpty(password))
        parameters.Add("password", password);
      if (SignUpService.TestMode)
        parameters["test_mode"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<SignupConfirmation>("auth.confirm", parameters, callbackAction,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
