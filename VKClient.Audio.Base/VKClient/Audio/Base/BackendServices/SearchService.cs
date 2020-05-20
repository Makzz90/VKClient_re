using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
  public class SearchService
  {
    private static SearchService _instance;

    public static SearchService Instance
    {
      get
      {
        return SearchService._instance ?? (SearchService._instance = new SearchService());
      }
    }

    public void GetSearchHints(string query, Action<BackendResult<List<SearchHint>, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
      string index = "q";
      string str = query;
      dictionary1[index] = str;
      Dictionary<string, string> dictionary2 = dictionary1;
      string methodName = "execute.getSearchHints";
      Dictionary<string, string> parameters = dictionary2;
      Action<BackendResult<List<SearchHint>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.DispatchRequestToVK<List<SearchHint>>(methodName, parameters, callback1, (Func<string, List<SearchHint>>)(s => JsonConvert.DeserializeObject<GenericRoot<List<SearchHint>>>(s).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetTrends(Action<BackendResult<List<Trend>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<List<Trend>>("search.getTrends", new Dictionary<string, string>(), callback, (Func<string, List<Trend>>) (s =>
      {
        GenericRoot<VKList<Trend>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<VKList<Trend>>>(s);
        if (genericRoot == null)
          return  null;
        VKList<Trend> response = genericRoot.response;
        if (response == null)
          return  null;
        return response.items;
      }), false, true, new CancellationToken?(),  null);
    }
  }
}
