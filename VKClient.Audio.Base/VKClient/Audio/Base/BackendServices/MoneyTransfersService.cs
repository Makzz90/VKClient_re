using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
  public static class MoneyTransfersService
  {
    public static void SendTransfer(long receiverId, int amount, string message, Action<BackendResult<MoneyTransferSendResponse, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index1 = "receiver_id";
      string str1 = receiverId.ToString();
      dictionary[index1] = str1;
      string index2 = "amount";
      string str2 = amount.ToString();
      dictionary[index2] = str2;
      Dictionary<string, string> parameters = dictionary;
      if (!string.IsNullOrWhiteSpace(message))
        parameters["message"] = message;
      VKRequestsDispatcher.DispatchRequestToVK<MoneyTransferSendResponse>("money.sendTransfer", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public static void GetTransfer(long id, long fromId, long toId, Action<BackendResult<MoneyTransferResponse, ResultCode>> callback, CancellationToken? cancellationToken = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "id";
      string str1 = id.ToString();
      parameters[index1] = str1;
      string index2 = "from_id";
      string str2 = fromId.ToString();
      parameters[index2] = str2;
      string index3 = "to_id";
      string str3 = toId.ToString();
      parameters[index3] = str3;
      string index4 = "func_v";
      string str4 = "2";
      parameters[index4] = str4;
      VKRequestsDispatcher.DispatchRequestToVK<MoneyTransferResponse>("execute.checkMoneyTransfer", parameters, callback,  null, false, true, cancellationToken,  null);
    }

    public static void CheckTransfer(long id, long fromId, long toId, Action<BackendResult<MoneyTransfer, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "id";
      string str1 = id.ToString();
      parameters[index1] = str1;
      string index2 = "from_id";
      string str2 = fromId.ToString();
      parameters[index2] = str2;
      string index3 = "to_id";
      string str3 = toId.ToString();
      parameters[index3] = str3;
      VKRequestsDispatcher.DispatchRequestToVK<MoneyTransfer>("money.checkTransfer", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public static void DeclineTransfer(long id, Action<BackendResult<int, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index = "id";
      string str = id.ToString();
      parameters[index] = str;
      VKRequestsDispatcher.DispatchRequestToVK<int>("money.declineTransfer", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public static void GetTransfersList(int type, int offset, int count, Action<BackendResult<VKList<MoneyTransfer>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "type";
      string str1 = type.ToString();
      parameters[index1] = str1;
      string index2 = "offset";
      string str2 = offset.ToString();
      parameters[index2] = str2;
      string index3 = "count";
      string str3 = count.ToString();
      parameters[index3] = str3;
      string index4 = "extended";
      string str4 = "1";
      parameters[index4] = str4;
      string index5 = "fields";
      string str5 = "photo_max,first_name_dat,last_name_dat,first_name_gen,last_name_gen";
      parameters[index5] = str5;
      VKRequestsDispatcher.DispatchRequestToVK<VKList<MoneyTransfer>>("money.getTransferList", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
