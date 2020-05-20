using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base
{
  public class CountersService : ICountersService
  {
    private static CountersService _instance;

    public static CountersService Instance
    {
      get
      {
        if (CountersService._instance == null)
          CountersService._instance = new CountersService();
        return CountersService._instance;
      }
    }

    public void GetCounters(Action<BackendResult<OwnCounters, ResultCode>> callback)
    {
      string methodName = "getCounters";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      Action<BackendResult<OwnCounters, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>(methodName, parameters, callback1, (Func<string, OwnCounters>) (jsonStr => CountersDeserializerHelper.Deserialize(jsonStr)), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetCountersWithLastMessage(Action<BackendResult<CountersWithMessageInfo, ResultCode>> callback)
    {
      string str = "var lastMessage= API.messages.get({\"count\":1, \"filters\":0}).items[0];\r\nvar user = API.users.get({\"user_id\":lastMessage.user_id})[0];\r\nvar counters= API.getCounters();\r\nreturn {\"LastMessage\":lastMessage, \"User\":user, \"Counters\":counters};";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<CountersWithMessageInfo>("execute", parameters, callback, (Func<string, CountersWithMessageInfo>) (jsonStr =>
      {
        jsonStr = jsonStr.Replace("\"Counters\":[]", "\"Counters\":{}");
        return JsonConvert.DeserializeObject<GenericRoot<CountersWithMessageInfo>>(jsonStr).response;
      }), false, true, new CancellationToken?(),  null);
    }
  }
}
