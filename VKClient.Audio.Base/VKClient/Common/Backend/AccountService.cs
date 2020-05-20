using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class AccountService
  {
    private static AccountService _instance;

    public static AccountService Instance
    {
      get
      {
        if (AccountService._instance == null)
          AccountService._instance = new AccountService();
        return AccountService._instance;
      }
    }

    public void DeleteProfilePhoto(Action<BackendResult<User, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<User>("var ph = API.photos.get({album_id:-6, rev:1, count:1}).items[0];\r\nAPI.photos.delete({owner_id:ph.owner_id, photo_id:ph.id});\r\nreturn API.users.get({fields:\"photo_max\"})[0];", callback,  null, false, true, new CancellationToken?());
    }

    public void RegisterDevice(string deviceId, string token, string deviceModelViewable, string systemVersion, string appVersion, string settings, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["token"] = token;
      parameters["device_id"] = deviceId;
      parameters["device_model"] = deviceModelViewable;
      parameters["system_version"] = systemVersion;
      parameters["app_version"] = appVersion;
      if (!string.IsNullOrEmpty(settings))
        parameters["settings"] = settings;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.registerDevice", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void UnregisterDevice(string deviceId, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["device_id"] = deviceId;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.unregisterDevice", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void SetPushSettings(string deviceId, string key, string value, string key2, string value2, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      string format = "API.account.setPushSettings({{\"device_id\":\"{0}\", \"key\":\"{1}\", \"value\":\"{2}\"}});";
      string code = string.Format(format, deviceId, key, value);
      if (!string.IsNullOrWhiteSpace(key2) && !string.IsNullOrWhiteSpace(value2))
        code = code + Environment.NewLine + string.Format(format, deviceId, key2, value2);
      VKRequestsDispatcher.Execute<VKClient.Common.Backend.DataObjects.ResponseWithId>(code, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?());
    }

    public void CheckShortName(string shortName, Action<BackendResult<CheckNameResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["screen_name"] = shortName;
      parameters["suggestions"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<CheckNameResponse>("utils.checkScreenName", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetShortName(string shortName, Action<BackendResult<SaveProfileResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["screen_name"] = shortName;
      VKRequestsDispatcher.DispatchRequestToVK<SaveProfileResponse>("account.saveProfileInfo", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetInfo(string name, string val, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["name"] = name;
      parameters["value"] = val;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.setInfo", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void SetInfo(bool? showOwnPostsDefault, bool? noWallReplies, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (showOwnPostsDefault.HasValue)
        parameters["own_posts_default"] = showOwnPostsDefault.Value ? "1" : "0";
      if (noWallReplies.HasValue)
        parameters["no_wall_replies"] = noWallReplies.Value ? "1" : "0";
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.setInfo", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void ChangePassword(string oldPassword, string newPassword, Action<BackendResult<ChangePasswordResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["old_password"] = oldPassword;
      parameters["new_password"] = newPassword;
      VKRequestsDispatcher.DispatchRequestToVK<ChangePasswordResponse>("account.changePassword", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetSettingsAccountInfo(Action<BackendResult<SettingsAccountInfo, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<SettingsAccountInfo>("var account = API.account.getInfo();\r\n\r\nvar newsBanned = API.newsfeed.getBanned();\r\n\r\nvar pi = API.account.getProfileInfo();\r\n\r\nreturn {\"Account\": account, \"NewsBanned\":newsBanned, \"ProfileInfo\":pi};", callback,  null, false, true, new CancellationToken?());
    }

    public void SaveSettingsAccountInfo(Dictionary<string, string> parameters, Action<BackendResult<SaveProfileResponse, ResultCode>> callback, UploadPhotoResponseData newPhotoData = null)
    {
      if (parameters.Count == 0)
      {
        if (newPhotoData == null)
          return;
        UsersService.Instance.SaveProfilePhoto(newPhotoData, (Action<BackendResult<ProfilePhoto, ResultCode>>) (pres => callback(new BackendResult<SaveProfileResponse, ResultCode>(pres.ResultCode, new SaveProfileResponse()))));
      }
      else
        VKRequestsDispatcher.DispatchRequestToVK<SaveProfileResponse>("account.saveProfileInfo", parameters, (Action<BackendResult<SaveProfileResponse, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded && newPhotoData != null)
            UsersService.Instance.SaveProfilePhoto(newPhotoData, (Action<BackendResult<ProfilePhoto, ResultCode>>) (pres => callback(res)));
          else
            callback(res);
        }),  null, false, true, new CancellationToken?(),  null);
    }

    public void GetSettingsProfileInfo(Action<BackendResult<ProfileInfo, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<ProfileInfo>("var pi = API.account.getProfileInfo();\r\nvar u = API.users.get({\"fields\":\"photo_max\"});\r\nvar partner;\r\nif (pi.relation_partner + \"\"!=\"\")\r\n{\r\n    partner = API.users.get({\"user_ids\":pi.relation_partner.id, \"fields\":\"photo_max,first_name_gen,last_name_gen,sex\"});\r\n    return {\"User\":u[0], \"ProfileInfo\":pi, \"Partner\":partner[0]};\r\n}\r\nvar relation_requests;\r\nif (pi.relation_requests + \"\"!=\"\")\r\n{\r\n    relation_requests = API.users.get({\"user_ids\":pi.relation_requests@.id, \"fields\":\"sex\"});\r\n}\r\n\r\nreturn {\"User\":u[0], \"ProfileInfo\":pi, \"RelationRequests\":relation_requests};", callback, (Func<string, ProfileInfo>) (jsonStr =>
      {
        GetProfileInfoResponse response = JsonConvert.DeserializeObject<GenericRoot<GetProfileInfoResponse>>(jsonStr).response;
        response.ProfileInfo.photo_max = response.User.photo_max;
        if (response.partner != null && response.ProfileInfo.relation_partner != null)
        {
          response.ProfileInfo.relation_partner.photo_max = response.partner.photo_max;
          response.ProfileInfo.relation_partner.first_name_gen = response.partner.first_name_gen;
          response.ProfileInfo.relation_partner.last_name_gen = response.partner.last_name_gen;
          response.ProfileInfo.relation_partner.sex = response.partner.sex;
        }
        if (response.RelationRequests != null)
          response.ProfileInfo.relation_requests = response.RelationRequests;
        return response.ProfileInfo;
      }), false, true, new CancellationToken?());
    }

    public void GetPrivacySettings(Action<BackendResult<PrivacySettingsInfo, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<PrivacySettingsInfo>("account.getPrivacySettings", new Dictionary<string, string>(), callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetPrivacy(string key, string value, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["key"] = key;
      parameters["value"] = value;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.setPrivacy", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void TestValidation(Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.testValidation", new Dictionary<string, string>(), callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonResp => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetBannedUsers(int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["fields"] = "photo_max";
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("account.getBanned", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void BanUser(long user_id, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["user_id"] = user_id.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("account.banUser", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void UnbanUsers(List<long> user_ids, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      string format = "API.account.unbanUser({{\"user_id\":{0}}});";
      string code = "";
      foreach (long userId in user_ids)
        code = code + string.Format(format, userId) + Environment.NewLine;
      VKRequestsDispatcher.Execute<VKClient.Common.Backend.DataObjects.ResponseWithId>(code, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?());
    }

    public void StatsTrackEvents(List<AppEventBase> appEvents, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      List<AppEventBase> appEventBaseList1 = new List<AppEventBase>();
      List<AppEventBase> appEventBaseList2 = new List<AppEventBase>();
      foreach (AppEventBase appEvent in appEvents)
      {
        if (appEvent is AppEventAdImpression)
          appEventBaseList2.Add(appEvent);
        else
          appEventBaseList1.Add(appEvent);
      }
      string format1 = "API.stats.trackEvents({{events:\"{0}\"}});";
      string format2 = "API.adsint.registerAdEvents({{events:\"{0}\"}});";
      string code = "";
      if (appEventBaseList2.Count > 0)
        code += string.Format(format2, JsonConvert.SerializeObject(appEventBaseList2).Replace("\"", "\\\""));
      if (appEventBaseList1.Count > 0)
        code += string.Format(format1, JsonConvert.SerializeObject(appEventBaseList1).Replace("\"", "\\\""));
      VKRequestsDispatcher.Execute<VKClient.Common.Backend.DataObjects.ResponseWithId>(code, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?());
    }

    public void InternalGetNotifications(string device, string os, string appVersion, string locale, Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["device"] = device;
      parameters["os"] = os;
      parameters["app_version"] = appVersion;
      parameters["locale"] = locale;
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Common.Backend.DataObjects.ResponseWithId>("internal.getNotifications", parameters, callback, (Func<string, VKClient.Common.Backend.DataObjects.ResponseWithId>) (jsonStr => new VKClient.Common.Backend.DataObjects.ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void SetUserOnline(Action<BackendResult<object, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<object>("account.setOnline", new Dictionary<string, string>(), (Action<BackendResult<object, ResultCode>>) (res => callback(res)),  null, true, true, new CancellationToken?(),  null);
    }

    public void SetSilenceMode(string device_id, int nrOfSeconds, Action<BackendResult<object, ResultCode>> callback, long chatId = 0, long uid = 0)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str = nrOfSeconds.ToString();
      parameters.Add("device_id", device_id);
      parameters.Add("time", str);
      if (chatId != 0L)
        parameters["peer_id"] = MessagesService.Instance.GetPeerId(chatId, true).ToString();
      if (uid != 0L)
        parameters["peer_id"] = MessagesService.Instance.GetPeerId(uid, false).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<object>("account.setSilenceMode", parameters, callback,  null, false, true, new CancellationToken?(),  null);
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

    public void GetBaseData(PhoneAppInfo phoneAppInfo, Action<BackendResult<AccountBaseData, ResultCode>> callback, bool loadStickersInfo = false)
    {
      ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
      Color color = ((SolidColorBrush) Application.Current.Resources["PhoneBackgroundBrush"]).Color;
      // ISSUE: explicit reference operation
      string str1 = (int) ((Color) @color).R == 0 ? "dark" : "light";
      string str2 = "";
      switch (themeSettings.BackgroundSettings)
      {
        case 0:
          str2 = "system";
          break;
        case 2:
          str2 = "dark";
          break;
        case 3:
          str2 = "light";
          break;
      }
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "userId",
          AppGlobalStateManager.Current.LoggedInUserId.ToString()
        },
        {
          "device",
          phoneAppInfo.Device
        },
        {
          "os",
          phoneAppInfo.OS
        },
        {
          "version",
          phoneAppInfo.AppVersion
        },
        {
          "locale",
          phoneAppInfo.Locale
        },
        {
          "themeBackgroundMode",
          str2
        },
        {
          "themeActiveBackground",
          str1
        },
        {
          "func_v",
          "5"
        }
      };
      if (loadStickersInfo)
        parameters["loadStickers"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<AccountBaseData>("execute.getBaseData", parameters, callback, (Func<string, AccountBaseData>) (jsonStr =>
      {
        jsonStr = jsonStr.Replace("\"OwnCounters\":[]", "\"OwnCounters\":{}");
        jsonStr = jsonStr.Replace("\"exports\":[]", "\"exports\":{}");
        return JsonConvert.DeserializeObject<GenericRoot<AccountBaseData>>(jsonStr).response;
      }), true, true, new CancellationToken?(),  null);
    }

    public void GetIntermediateData(Action<BackendResult<AccountIntermediateData, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "func_v",
          "2"
        }
      };
      string methodName = "execute.getIntermediateData";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<AccountIntermediateData, ResultCode>> callback1 = callback;
      int num1 = 1;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<AccountIntermediateData>(methodName, parameters, callback1, (Func<string, AccountIntermediateData>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<AccountIntermediateData>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void ResolveScreenName(string name, Action<BackendResult<ResolvedData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<ResolvedData>("execute.resolveScreenName", new Dictionary<string, string>()
      {
        {
          "name",
          name
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void StatusSet(string text, string audio, long groupId, Action<BackendResult<long, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (!string.IsNullOrEmpty(audio))
        parameters["audio"] = audio;
      else
        parameters["text"] = text;
      if (groupId > 0L)
        parameters["group_id"] = groupId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<long>("status.set", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void LookupContacts(string service, string myContact, List<string> contacts, Action<BackendResult<LookupContactsResponse, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["service"] = service;
      parameters["fields"] = "photo_max,verified,occupation,city,country,friend_status,common_count";
      if (!string.IsNullOrEmpty(myContact))
        parameters["mycontact"] = myContact;
      if (!contacts.IsNullOrEmpty())
        parameters["contacts"] = string.Join(",", (IEnumerable<string>) contacts);
      VKRequestsDispatcher.DispatchRequestToVK<LookupContactsResponse>("account.lookupContacts", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
