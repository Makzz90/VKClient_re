using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKClient.Audio.Base
{
  public class MessagesService
  {
    private static MessagesService _instance;

    public static MessagesService Instance
    {
      get
      {
        if (MessagesService._instance == null)
          MessagesService._instance = new MessagesService();
        return MessagesService._instance;
      }
    }

    public void CreateChat(List<long> userIds, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["user_ids"] = userIds.GetCommaSeparated();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.createChat", parameters, callback, new Func<string, ResponseWithId>(JsonConvert.DeserializeObject<ResponseWithId>), false, true, new CancellationToken?(),  null);
    }

    public void SearchDialogs(string query, Action<BackendResult<List<object>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["q"] = query;
      parameters["limit"] = "50";
      parameters["fields"] = "photo_max,photo_100,photo_200";
      VKRequestsDispatcher.DispatchRequestToVK<List<object>>("messages.searchDialogs", parameters, callback, (Func<string, List<object>>) (jsonStr =>
      {
        VKRequestsDispatcher.GenericRoot<IList<Dictionary<string, object>>> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<IList<Dictionary<string, object>>>>(jsonStr);
        List<object> objectList = new List<object>();
        foreach (Dictionary<string, object> dictionary in (IEnumerable<Dictionary<string, object>>) genericRoot.response)
        {
          if (dictionary.ContainsKey("type"))
          {
            if (dictionary["type"].ToString() == "profile")
            {
              User user1 = new User();
              user1.uid = (long) int.Parse(dictionary["id"].ToString());
              user1.last_name = dictionary["last_name"].ToString();
              user1.first_name = dictionary["first_name"].ToString();
              User user2 = user1;
              object obj = dictionary["photo_max"];
              string str = obj != null ? obj.ToString() :  null;
              user2.photo_max = str;
              objectList.Add(user1);
            }
            if (dictionary["type"].ToString() == "chat")
            {
              Chat chat = new Chat();
              chat.chat_id = long.Parse(dictionary["id"].ToString());
              chat.title = dictionary["title"].ToString();
              if (dictionary.ContainsKey("photo_100"))
                chat.photo_100 = dictionary["photo_100"].ToString();
              if (dictionary.ContainsKey("photo_200"))
                chat.photo_200 = dictionary["photo_200"].ToString();
              string[] strArray = dictionary["users"].ToString().Split(',');
              List<long> longList = new List<long>();
              foreach (string input in strArray)
              {
                string s = Regex.Match(input, "\\d+").Value;
                longList.Add(long.Parse(s));
              }
              chat.users = longList;
              objectList.Add(chat);
            }
            if (dictionary["type"].ToString() == "group")
              objectList.Add(new User()
              {
                id = -long.Parse(dictionary["id"].ToString()),
                first_name = dictionary["name"].ToString(),
                photo_max = dictionary["photo_200"].ToString()
              });
            if (dictionary["type"].ToString() == "email")
              Debugger.Break();
          }
        }
        return objectList;
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetHistory(long userOrChatId, bool isChat, int offset, int count, int? startMessageId, Action<BackendResult<MessageListResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["peer_id"] = this.GetPeerId(userOrChatId, isChat).ToString();
      if (startMessageId.HasValue)
        parameters["start_message_id"] = startMessageId.Value.ToString();
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["fields"] = "first_name_acc,last_name_acc,online,online_mobile,photo_max,sex,friend_status,photo_200,first_name_dat,is_messages_blocked";
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<MessageListResponse>("messages.getHistory", parameters, (Action<BackendResult<MessageListResponse, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          List<Message> messages = res.ResultData.Messages;
          UsersService.Instance.SetCachedUsers((IEnumerable<User>) res.ResultData.Users);
          int num = 1;
          List<long> list = Message.GetAssociatedUserIds(messages, num != 0).Select<long, long>((Func<long, long>) (uid => uid)).Distinct<long>().ToList<long>();
          if (list.Count > res.ResultData.Users.Count)
            UsersService.Instance.GetUsers(list.Except<long>(res.ResultData.Users.Select<User, long>((Func<User, long>) (u => u.uid))).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>) (resUsers =>
            {
              if (resUsers.ResultCode == ResultCode.Succeeded)
              {
                res.ResultData.Users.AddRange((IEnumerable<User>) resUsers.ResultData);
                callback(res);
              }
              else
                callback(new BackendResult<MessageListResponse, ResultCode>(resUsers.ResultCode));
            }));
          else
            callback(res);
        }
        else
          callback(new BackendResult<MessageListResponse, ResultCode>(res.ResultCode));
      }), new Func<string, MessageListResponse>(this.GetMessageHistoryDataForJson), false, true, new CancellationToken?(),  null);
    }

    public long GetPeerId(long userOrChatId, bool isChat)
    {
      if (!isChat)
        return userOrChatId;
      return userOrChatId + 2000000000L;
    }

    public void GetChat(long chatId, Action<BackendResult<ChatExtended, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<ChatExtended>("messages.getChat", new Dictionary<string, string>()
      {
        {
          "chat_id",
          chatId.ToString()
        },
        {
          "fields",
          "photo_200,photo_max,domain"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetDialogs(GetDialogsRequest request, Action<BackendResult<MessageListResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = request.Offset.ToString();
      parameters["count"] = request.Count.ToString();
      parameters["preview_length"] = request.PreviewLength.ToString();
      parameters["fields"] = "first_name_acc,last_name_acc,online,online_mobile,photo_max,sex,friend_status,photo_200,first_name_dat,is_messages_blocked";
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<MessageListResponse>("messages.getDialogs", parameters, (Action<BackendResult<MessageListResponse, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          List<Message> list1 = res.ResultData.DialogHeaders.Select<DialogHeaderInfo, Message>((Func<DialogHeaderInfo, Message>) (d => d.message)).ToList<Message>();
          UsersService.Instance.SetCachedUsers((IEnumerable<User>) res.ResultData.Users);
          int num = 0;
          List<long> list2 = Message.GetAssociatedUserIds(list1, num != 0).Select<long, long>((Func<long, long>) (uid => uid)).Distinct<long>().ToList<long>();
          if (list2.Count > res.ResultData.Users.Count)
            UsersService.Instance.GetUsers(list2.Except<long>(res.ResultData.Users.Select<User, long>((Func<User, long>) (u => u.uid))).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>) (resUsers =>
            {
              if (resUsers.ResultCode == ResultCode.Succeeded)
              {
                res.ResultData.Users.AddRange((IEnumerable<User>) resUsers.ResultData);
                callback(res);
              }
              else
                callback(new BackendResult<MessageListResponse, ResultCode>(resUsers.ResultCode));
            }));
          else
            callback(res);
        }
        else
          callback(new BackendResult<MessageListResponse, ResultCode>(res.ResultCode));
      }), new Func<string, MessageListResponse>(this.GetDialogInfoDataForJson), false, true, new CancellationToken?(),  null);
    }

    public void GetMessages(List<long> messageIds, Action<BackendResult<MessageListResponse, ResultCode>> callback)
    {
      if (messageIds == null || messageIds.Count == 0)
      {
        callback(new BackendResult<MessageListResponse, ResultCode>(ResultCode.Succeeded, new MessageListResponse()
        {
          Messages = new List<Message>(),
          TotalCount = 0
        }));
      }
      else
      {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters["message_ids"] = messageIds.GetCommaSeparated();
        parameters["fields"] = "first_name_acc,last_name_acc,online,online_mobile,photo_max,sex,friend_status,photo_200,is_messages_blocked";
        parameters["extended"] = "1";
        VKRequestsDispatcher.DispatchRequestToVK<MessageListResponse>("messages.getById", parameters, (Action<BackendResult<MessageListResponse, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
          {
            List<Message> messages = res.ResultData.Messages;
            UsersService.Instance.SetCachedUsers((IEnumerable<User>) res.ResultData.Users);
            int num = 1;
            List<long> list = Message.GetAssociatedUserIds(messages, num != 0).Select<long, long>((Func<long, long>) (uid => uid)).Distinct<long>().ToList<long>();
            if (list.Count > res.ResultData.Users.Count)
              UsersService.Instance.GetUsers(list.Except<long>(res.ResultData.Users.Select<User, long>((Func<User, long>) (u => u.uid))).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>) (resUsers =>
              {
                if (resUsers.ResultCode == ResultCode.Succeeded)
                {
                  res.ResultData.Users.AddRange((IEnumerable<User>) resUsers.ResultData);
                  callback(res);
                }
                else
                  callback(new BackendResult<MessageListResponse, ResultCode>(resUsers.ResultCode));
              }));
            else
              callback(res);
          }
          else
            callback(new BackendResult<MessageListResponse, ResultCode>(res.ResultCode));
        }), new Func<string, MessageListResponse>(this.GetMessageHistoryDataForJson), false, true, new CancellationToken?(),  null);
      }
    }

    private MessageListResponse GetDialogInfoDataForJson(string jsonStr)
    {
      DialogWrapper response = JsonConvert.DeserializeObject<GenericRoot<DialogWrapper>>(jsonStr).response;
      List<User> profiles = response.profiles;
      foreach (VKClient.Common.Backend.DataObjects.Group group in response.groups)
      {
        User user = new User();
        user.InitFromGroup(group);
        profiles.Add(user);
      }
      return new MessageListResponse()
      {
        DialogHeaders = response.items,
        TotalCount = response.count,
        Users = profiles
      };
    }

    private MessageListResponse GetMessageHistoryDataForJson(string jsonStr)
    {
      MessageHistoryWrapper response = JsonConvert.DeserializeObject<GenericRoot<MessageHistoryWrapper>>(jsonStr).response;
      List<User> profiles = response.profiles;
      foreach (VKClient.Common.Backend.DataObjects.Group group in response.groups)
      {
        User user = new User();
        user.InitFromGroup(group);
        profiles.Add(user);
      }
      return new MessageListResponse()
      {
        DialogHeaders = new List<DialogHeaderInfo>(response.items.Select<Message, DialogHeaderInfo>((Func<Message, DialogHeaderInfo>) (m => new DialogHeaderInfo()
        {
          message = m
        }))),
        Messages = response.items,
        Skipped = response.skipped,
        Unread = response.unread,
        TotalCount = response.count,
        Users = profiles
      };
    }

    private MessageListResponse GetMessageDataForJson(string jsonStr)
    {
      VKList<Message> response = JsonConvert.DeserializeObject<GenericRoot<VKList<Message>>>(jsonStr).response;
      return new MessageListResponse()
      {
        DialogHeaders = new List<DialogHeaderInfo>(response.items.Select<Message, DialogHeaderInfo>((Func<Message, DialogHeaderInfo>) (m => new DialogHeaderInfo()
        {
          message = m
        }))),
        Messages = response.items,
        TotalCount = response.count
      };
    }

    public void SendMessage(SendMessageRequest request, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index1 = "peer_id";
      string str1 = this.GetPeerId(request.UserOrCharId, request.IsChat).ToString();
      dictionary[index1] = str1;
      string index2 = "random_id";
      string str2 = Convert.ToInt32((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
      dictionary[index2] = str2;
      string index3 = "message";
      string messageBody = request.MessageBody;
      dictionary[index3] = messageBody;
      Dictionary<string, string> parameters = dictionary;
      this.ProcessCommands(request.MessageBody);
      if (request.AttachmentIds != null && request.AttachmentIds.Count > 0)
        parameters["attachment"] = request.AttachmentIds.GetCommaSeparated(",");
      if (request.ForwardedMessagesIds != null && request.ForwardedMessagesIds.Count > 0)
        parameters["forward_messages"] = request.ForwardedMessagesIds.GetCommaSeparated();
      if (request.StickerId != 0)
        parameters["sticker_id"] = request.StickerId.ToString();
      if (!string.IsNullOrEmpty(request.StickerReferrer))
        parameters["sticker_referrer"] = request.StickerReferrer;
      if (request.IsGeoAttached)
      {
        parameters["lat"] = request.Latitude.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        parameters["long"] = request.Longitude.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      }
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.send", parameters, callback, new Func<string, ResponseWithId>(JsonConvert.DeserializeObject<ResponseWithId>), false, true, new CancellationToken?(),  null);
    }

    private void ProcessCommands(string command)
    {
      if (command == "%force_stats_enable%")
        AppGlobalStateManager.Current.GlobalState.ForceStatsSend = true;
      else if (command == "%force_stats_disable%")
        AppGlobalStateManager.Current.GlobalState.ForceStatsSend = false;
      else if (command == "%gif_enable%")
        AppGlobalStateManager.Current.GlobalState.GifAutoplayManualSetting = new bool?(true);
      else if (command == "%gif_disable%")
        AppGlobalStateManager.Current.GlobalState.GifAutoplayManualSetting = new bool?(false);
      else if (command == "%gif_default%")
        AppGlobalStateManager.Current.GlobalState.GifAutoplayManualSetting = new bool?();
      else if (command == "%ads_demo_enable%")
        AppGlobalStateManager.Current.GlobalState.AdsDemoManualSetting = new bool?(true);
      else if (command == "%ads_demo_disable%")
        AppGlobalStateManager.Current.GlobalState.AdsDemoManualSetting = new bool?(false);
      else if (command == "%ads_demo_default%")
        AppGlobalStateManager.Current.GlobalState.AdsDemoManualSetting = new bool?();
      else if (command.StartsWith("%set_domain="))
      {
        string str = command.Substring(12);
        if (!str.EndsWith("%"))
          return;
        AppGlobalStateManager.Current.GlobalState.BaseDomain = str.Substring(0, str.Length - 1);
      }
      else if (command.StartsWith("%set_login_domain="))
      {
        string str = command.Substring(18);
        if (!str.EndsWith("%"))
          return;
        AppGlobalStateManager.Current.GlobalState.BaseLoginDomain = str.Substring(0, str.Length - 1);
      }
      else if (command == "%reset_domain%")
      {
        AppGlobalStateManager.Current.GlobalState.BaseDomain = "";
        AppGlobalStateManager.Current.GlobalState.BaseLoginDomain = "";
      }
      else if (command == "%max_rec_demo%")
      {
        AppGlobalStateManager.Current.GlobalState.AudioRecordingMaxDemo = true;
      }
      else
      {
        if (!(command == "%max_rec_demo_reset%"))
          return;
        AppGlobalStateManager.Current.GlobalState.AudioRecordingMaxDemo = false;
      }
    }

    public void MarkAsRead(List<long> messageIds, long peerId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      if (messageIds.IsNullOrEmpty())
      {
        callback(new BackendResult<ResponseWithId, ResultCode>(ResultCode.Succeeded));
      }
      else
      {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters["peer_id"] = peerId.ToString();
        parameters["start_message_id"] = messageIds.Max().ToString();
        VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.markAsRead", parameters, callback, new Func<string, ResponseWithId>(JsonConvert.DeserializeObject<ResponseWithId>), false, true, new CancellationToken?(),  null);
      }
    }

    public void SetUserIsTyping(long userOrChatId, bool isChatId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["peer_id"] = this.GetPeerId(userOrChatId, isChatId).ToString();
      parameters["type"] = "typing";
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.setActivity", parameters, callback, new Func<string, ResponseWithId>(JsonConvert.DeserializeObject<ResponseWithId>), false, true, new CancellationToken?(),  null);
    }

    public void DeleteMessages(List<int> messageIds, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      this.SetStatusByIds(messageIds, "messages.delete", callback);
    }

    private void SetStatusByIds(List<int> messageIds, string methodName, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("message_ids", messageIds.GetCommaSeparated());
      if (messageIds.Count == 0)
        callback(new BackendResult<ResponseWithId, ResultCode>(ResultCode.Succeeded, new ResponseWithId()));
      else
        VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetPhotoUploadServer(Action<BackendResult<UploadServerAddress, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getMessagesUploadServer", new Dictionary<string, string>(), callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SavePhoto(UploadResponseData uploadData, Action<BackendResult<Photo, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["server"] = uploadData.server;
      dictionary["photo"] = uploadData.photo;
      dictionary["hash"] = uploadData.hash;
      string methodName = "photos.saveMessagesPhoto";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<Photo, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Photo>(methodName, parameters, callback1, (Func<string, Photo>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<List<Photo>>>(jsonStr).response.First<Photo>()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void SearchMessages(string query, int count, int offset, Action<BackendResult<MessageListResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["q"] = query;
      parameters["offset"] = offset.ToString();
      parameters["preview_length"] = "1";
      parameters["count"] = count.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<MessageListResponse>("messages.search", parameters, (Action<BackendResult<MessageListResponse, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded && res.ResultData.Messages.Count > 0)
          this.GetMessages(res.ResultData.Messages.Select<Message, long>((Func<Message, long>) (m => (long) m.id)).ToList<long>(), callback);
        else
          callback(res);
      }), new Func<string, MessageListResponse>(this.GetMessageDataForJson), false, true, new CancellationToken?(),  null);
    }

    public void UploadPhoto(byte[] photoData, Action<BackendResult<Photo, ResultCode>> callback, Action<double> progressCallback = null, Cancellation c = null)
    {
      this.GetPhotoUploadServer((Action<BackendResult<UploadServerAddress, ResultCode>>) (getUplResp =>
      {
        if (getUplResp.ResultCode != ResultCode.Succeeded)
          callback(new BackendResult<Photo, ResultCode>(getUplResp.ResultCode));
        else
          JsonWebRequest.Upload(getUplResp.ResultData.upload_url, (Stream) new MemoryStream(photoData), "photo", "image", (Action<JsonResponseData>) (jsonResult =>
          {
            if (!jsonResult.IsSucceeded)
              callback(new BackendResult<Photo, ResultCode>(ResultCode.UnknownError));
            else
              this.SavePhoto(JsonConvert.DeserializeObject<UploadResponseData>(jsonResult.JsonString), callback);
          }), "MyImage.jpg", progressCallback, c);
      }));
    }

    public void GetVideo(long ownerId, long videoId, Action<BackendResult<VideoData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["videos"] = ownerId.ToString() + "_" + videoId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VideoData>("video.get", parameters, callback, (Func<string, VideoData>) (jsonStr =>
      {
        jsonStr = this.RemoveCountOfRecords(jsonStr);
        try
        {
          return JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<List<VideoData>>>(jsonStr).response.FirstOrDefault<VideoData>();
        }
        catch (Exception )
        {
          return new VideoData();
        }
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetAudio(long ownerId, long audioId, Action<BackendResult<AudioObj, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["audios"] = ownerId.ToString() + "_" + audioId.ToString();
      string methodName = "audio.getById";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<AudioObj, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<AudioObj>(methodName, parameters, callback1, (Func<string, AudioObj>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<List<AudioObj>>>(jsonStr).response.First<AudioObj>()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void DeleteDialog(long userOrChatId, bool isChat, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["peer_id"] = this.GetPeerId(userOrChatId, isChat).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.deleteDialog", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    private string RemoveCountOfRecords(string jsonStr)
    {
      int num = jsonStr.IndexOf(",");
      if (num == -1)
        return jsonStr;
      return jsonStr.Remove(13, num - 13 + 1);
    }

    public void GetChatUploadServer(long chatId, Action<BackendResult<UploadServerAddress, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["chat_id"] = chatId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getChatUploadServer", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetChatPhoto(UploadResponseData uploadData, Action<BackendResult<ChatInfoWithMessageId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["file"] = uploadData.response;
      VKRequestsDispatcher.DispatchRequestToVK<ChatInfoWithMessageId>("messages.setChatPhoto", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void UpdateChatPhoto(long chatId, byte[] photoData, Rect thumbnailRect, Action<BackendResult<ChatInfoWithMessageId, ResultCode>> callback)
    {
      this.GetChatUploadServer(chatId, (Action<BackendResult<UploadServerAddress, ResultCode>>) (getUplResp =>
      {
        if (getUplResp.ResultCode != ResultCode.Succeeded)
        {
          callback(new BackendResult<ChatInfoWithMessageId, ResultCode>(getUplResp.ResultCode));
        }
        else
        {
          string uploadUrl = getUplResp.ResultData.upload_url;
          // ISSUE: explicit reference operation
          if (((Rect) @thumbnailRect).Width != 0.0)
          {
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            string str = string.Format("&_square_crop={0},{1},{2}&_full={0},{1},{2},{2}", (int) ((Rect) @thumbnailRect).X, (int) ((Rect) @thumbnailRect).Y, (int) ((Rect) @thumbnailRect).Width);
            uploadUrl += str;
          }
          MemoryStream memoryStream = new MemoryStream(photoData);
          JsonWebRequest.Upload(uploadUrl, (Stream) memoryStream, "photo", "image", (Action<JsonResponseData>) (jsonResult =>
          {
            if (!jsonResult.IsSucceeded)
              callback(new BackendResult<ChatInfoWithMessageId, ResultCode>(ResultCode.UnknownError));
            else
              this.SetChatPhoto(JsonConvert.DeserializeObject<UploadResponseData>(jsonResult.JsonString), callback);
          }), "MyImage.jpg",  null,  null);
        }
      }));
    }

    public void DeleteChatPhoto(long chatId, Action<BackendResult<ChatInfoWithMessageId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["chat_id"] = chatId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ChatInfoWithMessageId>("messages.deleteChatPhoto", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetConversationMaterials(long peerId, string mediaType, string startFrom, int count, Action<BackendResult<VKList<ConversationMaterial>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "peer_id",
          peerId.ToString()
        },
        {
          "media_type",
          mediaType
        },
        {
          "count",
          count.ToString()
        }
      };
      if (startFrom != null)
        parameters.Add("start_from", startFrom);
      VKRequestsDispatcher.DispatchRequestToVK<VKList<ConversationMaterial>>("messages.getHistoryAttachments", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void AllowDenyMessagesFromGroup(long groupId, bool allow, Action<BackendResult<int, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index = "group_id";
      string str = groupId.ToString();
      dictionary[index] = str;
      Dictionary<string, string> parameters = dictionary;
      VKRequestsDispatcher.DispatchRequestToVK<int>(allow ? "messages.allowMessagesFromGroup" : "messages.denyMessagesFromGroup", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public class RootObject
    {
      public List<Message> response { get; set; }
    }
  }
}
