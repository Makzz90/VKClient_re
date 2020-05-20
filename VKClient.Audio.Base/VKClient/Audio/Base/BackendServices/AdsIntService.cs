using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
  public class AdsIntService
  {
    public static void HideAd(string adData, string adObjectType, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["ad_data"] = adData;
      parameters["object_type"] = adObjectType;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("adsint.hideAd", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public static void ReportAd(string adData, ReportAdReason reportAdReason, Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["ad_data"] = adData;
      parameters["reason"] = reportAdReason.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.ResponseWithId>("adsint.reportAd", parameters, callback, (Func<string, VKClient.Audio.Base.ResponseWithId>) (jsonStr => new VKClient.Audio.Base.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }
  }
}
