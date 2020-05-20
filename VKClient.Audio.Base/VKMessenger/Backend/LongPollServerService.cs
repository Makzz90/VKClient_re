using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKMessenger.Backend
{
  public class LongPollServerService : ILongPollServerService
  {
    private readonly string _getUpdatesURIFrm = "https://{0}?act=a_check&key={1}&ts={2}&wait=25&mode=66&version=1";
    private static LongPollServerService _instance;

    public static LongPollServerService Instance
    {
      get
      {
        if (LongPollServerService._instance == null)
          LongPollServerService._instance = new LongPollServerService();
        return LongPollServerService._instance;
      }
    }

    public void GetLongPollServer(Action<BackendResult<LongPollServerResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["use_ssl"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<LongPollServerResponse>("messages.getLongPollServer", parameters, callback, new Func<string, LongPollServerResponse>(this.GetLongPollResponseForJson), false, false, new CancellationToken?(),  null);
    }

    public bool HaveToken()
    {
      return VKRequestsDispatcher.HaveToken;
    }

    public void GetUpdates(string serverName, string key, long ts, Action<BackendResult<UpdatesResponse, LongPollResultCode>> callback)
    {
      JsonWebRequest.SendHTTPRequestAsync(string.Format(this._getUpdatesURIFrm, serverName, key, ts), (Action<JsonResponseData>) (jsonResp =>
      {
        BackendResult<UpdatesResponse, LongPollResultCode> backendResult = new BackendResult<UpdatesResponse, LongPollResultCode>();
        if (!jsonResp.IsSucceeded || string.IsNullOrWhiteSpace(jsonResp.JsonString))
          backendResult.ResultCode = LongPollResultCode.CommunicationFailed;
        else if (this.ServerReturnedFailure(jsonResp.JsonString))
        {
          backendResult.ResultCode = LongPollResultCode.RequireNewPollServer;
        }
        else
        {
          backendResult.ResultCode = LongPollResultCode.Succeeded;
          LongPollServerService.RootObjectGetUpdates objectGetUpdates = JsonConvert.DeserializeObject<LongPollServerService.RootObjectGetUpdates>(jsonResp.JsonString);
          backendResult.ResultData = new UpdatesResponse();
          backendResult.ResultData.ts = (long) objectGetUpdates.ts;
          backendResult.ResultData.Updates = this.ReadUpdatesResponseFromRaw(objectGetUpdates.updates, new Func<List<object>, LongPollServerUpdateData>(this.GetUpdateDataForNewMessageLongPollData));
        }
        callback(backendResult);
      }),  null);
    }

    private List<LongPollServerUpdateData> ReadUpdatesResponseFromRaw(List<List<object>> rawUpdates, Func<List<object>, LongPollServerUpdateData> getUpdatesForNewMessageFunc)
    {
      List<LongPollServerUpdateData> serverUpdateDataList1 = new List<LongPollServerUpdateData>();
      if (rawUpdates != null)
      {
        foreach (List<object> rawUpdate in rawUpdates)
        {
          if (rawUpdate != null)
          {
            int num1 = int.Parse(rawUpdate.First<object>().ToString());
            if (num1 == 80)
            {
              int result = 0;
              if (rawUpdate.Count > 1 && int.TryParse(rawUpdate[1].ToString(), out result))
                serverUpdateDataList1.Add(new LongPollServerUpdateData()
                {
                  UpdateType = LongPollServerUpdateType.NewCounter,
                  Counter = result
                });
            }
            if (num1 == 9 || num1 == 8)
            {
              int num2 = 7;
              if (rawUpdate.Count > 2)
                num2 = int.Parse(rawUpdate[2].ToString()) % 256;
              long num3 = -long.Parse(rawUpdate[1].ToString());
              serverUpdateDataList1.Add(new LongPollServerUpdateData()
              {
                UpdateType = (LongPollServerUpdateType) num1,
                user_id = num3,
                Platform = num2
              });
            }
            if (num1 == 6)
            {
              long num2 = long.Parse(rawUpdate[1].ToString());
              LongPollServerUpdateData serverUpdateData = new LongPollServerUpdateData()
              {
                UpdateType = LongPollServerUpdateType.IncomingMessagesRead
              };
              if (num2 >= 2000000000L)
                serverUpdateData.chat_id = num2 - 2000000000L;
              else
                serverUpdateData.user_id = num2;
              serverUpdateData.message_id = long.Parse(rawUpdate[2].ToString());
              serverUpdateDataList1.Add(serverUpdateData);
            }
            if (num1 == 4)
            {
              LongPollServerUpdateData serverUpdateData = getUpdatesForNewMessageFunc(rawUpdate);
              if (serverUpdateData != null)
                serverUpdateDataList1.Add(serverUpdateData);
            }
            if (num1 == 3)
            {
              int num2 = int.Parse(rawUpdate[2].ToString());
              int num3 = 1;
              if ((num2 & num3) == 1)
              {
                long num4 = long.Parse(rawUpdate[1].ToString());
                if (rawUpdate.Count >= 4)
                {
                  long num5 = long.Parse(rawUpdate[3].ToString());
                  if (num5 <= 2000000000L && num5 > 0L)
                  {
                    serverUpdateDataList1.Add(new LongPollServerUpdateData()
                    {
                      message_id = num4,
                      user_id = num5,
                      UpdateType = LongPollServerUpdateType.MessageHasBeenRead
                    });
                  }
                  else
                  {
                    List<LongPollServerUpdateData> serverUpdateDataList2 = serverUpdateDataList1;
                    LongPollServerUpdateData serverUpdateData = new LongPollServerUpdateData();
                    serverUpdateData.message_id = num4;
                    long num6 = num5 - 2000000000L;
                    serverUpdateData.chat_id = num6;
                    int num7 = 1;
                    serverUpdateData.isChat = num7 != 0;
                    int num8 = 10;
                    serverUpdateData.UpdateType = (LongPollServerUpdateType) num8;
                    serverUpdateDataList2.Add(serverUpdateData);
                  }
                }
              }
              int num9 = 128;
              if ((num2 & num9) == 128)
              {
                LongPollServerUpdateData serverUpdateData = this.ReadUserOrChatIds(rawUpdate);
                if (serverUpdateData != null)
                {
                  serverUpdateData.UpdateType = LongPollServerUpdateType.MessageHasBeenRestored;
                  serverUpdateDataList1.Add(serverUpdateData);
                }
              }
            }
            if (num1 == 2 && (int.Parse(rawUpdate[2].ToString()) & 128) == 128)
            {
              LongPollServerUpdateData serverUpdateData = this.ReadUserOrChatIds(rawUpdate);
              if (serverUpdateData != null)
              {
                serverUpdateData.UpdateType = LongPollServerUpdateType.MessageHasBeenDeleted;
                serverUpdateDataList1.Add(serverUpdateData);
              }
            }
            if (num1 == 51)
            {
              long num2 = long.Parse(rawUpdate[1].ToString());
              serverUpdateDataList1.Add(new LongPollServerUpdateData()
              {
                UpdateType = LongPollServerUpdateType.ChatParamsWereChanged,
                isChat = true,
                chat_id = num2
              });
            }
            if (num1 == 61)
            {
              long num2 = long.Parse(rawUpdate[1].ToString());
              serverUpdateDataList1.Add(new LongPollServerUpdateData()
              {
                UpdateType = LongPollServerUpdateType.UserIsTyping,
                user_id = num2
              });
            }
            if (num1 == 62)
            {
              long num2 = long.Parse(rawUpdate[1].ToString());
              long num3 = long.Parse(rawUpdate[2].ToString());
              serverUpdateDataList1.Add(new LongPollServerUpdateData()
              {
                UpdateType = LongPollServerUpdateType.UserIsTypingInChat,
                user_id = num2,
                chat_id = num3,
                isChat = true
              });
            }
          }
        }
      }
      return serverUpdateDataList1;
    }

    private LongPollServerUpdateData ReadUserOrChatIds(List<object> updateDataRaw)
    {
      if (updateDataRaw == null || updateDataRaw.Count < 4)
        return  null;
      long num1 = long.Parse(updateDataRaw[1].ToString());
      bool flag = false;
      long num2 = 0;
      long num3 = 0;
      if (updateDataRaw.Count >= 4)
      {
        long num4 = long.Parse(updateDataRaw[3].ToString());
        if (num4 - 2000000000L >= 0L)
        {
          flag = true;
          num3 = num4 - 2000000000L;
        }
        else
        {
          flag = false;
          num2 = num4;
        }
      }
      return new LongPollServerUpdateData()
      {
        user_id = num2,
        chat_id = num3,
        isChat = flag,
        message_id = num1
      };
    }

    private LongPollServerUpdateData GetUpdateDataForNewMessageLongPollData(List<object> updateDataRaw)
    {
      try
      {
        LongPollServerUpdateData serverUpdateData = new LongPollServerUpdateData();
        serverUpdateData.UpdateType = LongPollServerUpdateType.MessageHasBeenAdded;
        long num1 = long.Parse(updateDataRaw[1].ToString());
        bool flag1 = (long.Parse(updateDataRaw[2].ToString()) & 2L) == 2L;
        long num2 = long.Parse(updateDataRaw[3].ToString());
        long num3 = long.Parse(updateDataRaw[4].ToString());
        string str = updateDataRaw[6].ToString();
        bool flag2 = false;
        bool flag3 = false;
        long num4 = 0;
        if (updateDataRaw.Count > 7)
        {
          foreach (KeyValuePair<string, JToken> keyValuePair in updateDataRaw[7] as JObject)
          {
            if (keyValuePair.Key == "from")
            {
              num4 = num2 - 2000000000L;
              num2 = long.Parse(keyValuePair.Value.ToString());
              flag3 = true;
            }
            else
              flag2 = true;
          }
        }
        serverUpdateData.message_id = num1;
        serverUpdateData.@out = flag1;
        serverUpdateData.user_id = num2;
        serverUpdateData.chat_id = num4;
        serverUpdateData.timestamp = num3;
        serverUpdateData.text = str;
        serverUpdateData.isChat = flag3;
        serverUpdateData.hasAttachOrForward = flag2;
        return serverUpdateData;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("GetUpdateDataForNewMessage", ex);
        return  null;
      }
    }

    private LongPollServerResponse GetLongPollResponseForJson(string jsonStr)
    {
      return JsonConvert.DeserializeObject<LongPollServerService.RootObjectGetServer>(jsonStr).response;
    }

    private bool ServerReturnedFailure(string jsonStr)
    {
      return jsonStr.StartsWith("{\"failed");
    }

    public class RootObjectGetServer
    {
      public LongPollServerResponse response { get; set; }
    }

    public class RootObjectGetUpdates
    {
      public int ts { get; set; }

      public List<List<object>> updates { get; set; }
    }
  }
}
