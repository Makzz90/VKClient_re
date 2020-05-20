using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class UsersService
  {
    private static readonly Func<string, List<User>> _deserializeUsersFunc = (Func<string, List<User>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<User>>>(jsonStr).response.items);
    private Dictionary<long, User> _usersCache = new Dictionary<long, User>();
    private readonly object _lockObj = new object();
    private int numberOfMinutesUserIsValidInCache = 1440;
    private Dictionary<string, List<Action<BackendResult<List<User>, ResultCode>>>> _callbacksToExecute = new Dictionary<string, List<Action<BackendResult<List<User>, ResultCode>>>>();
    private static UsersService _instance;
    private static ILookup<long, User> _friends;

    public static UsersService Instance
    {
      get
      {
        return UsersService._instance ?? (UsersService._instance = new UsersService());
      }
    }

    private UsersService()
    {
    }

    public User GetCachedUser(long userId)
    {
      lock (this._lockObj)
      {
        if (this._usersCache.ContainsKey(userId))
        {
          User user = this._usersCache[userId];
          if ((DateTime.Now - user.CachedDateTime).TotalMinutes <= (double) this.numberOfMinutesUserIsValidInCache)
            return user;
        }
      }
      return  null;
    }

    public void SetCachedUser(User user)
    {
      user.CachedDateTime = DateTime.Now;
      lock (this._lockObj)
        this._usersCache[user.uid] = user;
    }

    public void SetCachedUsers(IEnumerable<User> users)
    {
      lock (this._lockObj)
      {
        foreach (User user in users)
        {
          user.CachedDateTime = DateTime.Now;
          this._usersCache[user.uid] = user;
        }
      }
    }

    public void GetUsersOnlineStatus(List<int> userIds, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<List<User>>("users.get", new Dictionary<string, string>()
      {
        {
          "user_ids",
          userIds.GetCommaSeparated()
        },
        {
          "fields",
          "uid, online,online_mobile"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetStatusText(string text, Action<BackendResult<object, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<object>("status.set", new Dictionary<string, string>()
      {
        {
          "text",
          text
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetUsers(List<long> userIds, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      if (userIds == null || userIds.Count == 0)
      {
        callback(new BackendResult<List<User>, ResultCode>()
        {
          ResultCode = ResultCode.Succeeded,
          ResultData = new List<User>()
        });
      }
      else
      {
        List<User> resultData = new List<User>();
        bool flag = true;
        foreach (long userId in userIds)
        {
          DateTime now = DateTime.Now;
          if (this.GetCachedUser(userId) != null)
          {
            resultData.Add(this._usersCache[userId]);
          }
          else
          {
            flag = false;
            break;
          }
        }
        if (flag)
        {
          Logger.Instance.Info("Returning {0} users from cache", resultData.Count);
          callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded, resultData));
        }
        else
        {
          string allUserIds = userIds.GetCommaSeparated();
          if (this._callbacksToExecute.ContainsKey(allUserIds))
          {
            this._callbacksToExecute[allUserIds].Add(callback);
          }
          else
          {
            this._callbacksToExecute.Add(allUserIds, new List<Action<BackendResult<List<User>, ResultCode>>>()
            {
              callback
            });
            Dictionary<string, string> dictionary = new Dictionary<string, string>()
            {
              {
                "user_ids",
                allUserIds
              },
              {
                "fields",
                "uid,first_name,last_name,online,online_mobile,photo_max,sex,photo_medium,friend_status"
              }
            };
            VKRequestsDispatcher.Execute<UsersAndGroups>(string.Format("\r\nvar userIds = \"{0}\";\r\nvar groupIds = \"{1}\";\r\n\r\nvar users = [];\r\nvar groups = [];\r\n\r\nif (userIds.length > 0)\r\n{{\r\n   users = API.users.get({{\"user_ids\":userIds, \"fields\":\"uid,first_name,last_name,online,online_mobile,photo_max,sex,photo_medium,friend_status,first_name_dat\" }});\r\n}}\r\n\r\nif (groupIds.length > 0)\r\n{{\r\n   groups = API.groups.getById({{\"group_ids\":groupIds, \"fields\":\"photo_200,is_messages_blocked\"}});\r\n}}\r\n\r\nreturn {{\"users\":users, \"groups\":groups}};", userIds.Where<long>((Func<long, bool>) (uid => uid > 0L)).ToList<long>().GetCommaSeparated(), userIds.Where<long>((Func<long, bool>) (uid =>
            {
              if (uid < 0L)
                return uid > -2000000000L;
              return false;
            })).Select<long, long>((Func<long, long>) (uid => -uid)).ToList<long>().GetCommaSeparated()), (Action<BackendResult<UsersAndGroups, ResultCode>>) (res =>
            {
              if (res.ResultCode == ResultCode.Succeeded)
              {
                foreach (Group group in res.ResultData.groups)
                {
                  User user = new User();
                  user.InitFromGroup(group);
                  res.ResultData.users.Add(user);
                }
                foreach (User user in res.ResultData.users)
                  this.SetCachedUser(user);
              }
              BackendResult<List<User>, ResultCode> backendResult = new BackendResult<List<User>, ResultCode>();
              backendResult.ResultCode = res.ResultCode;
              if (res.ResultData != null)
                backendResult.ResultData = res.ResultData.users;
              foreach (Action<BackendResult<List<User>, ResultCode>> action in this._callbacksToExecute[allUserIds])
                action(backendResult);
              this._callbacksToExecute.Remove(allUserIds);
            }),  null, false, true, new CancellationToken?());
          }
        }
      }
    }

    public void GetUsersNoCache(List<int> userIds, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<List<User>>("users.get", new Dictionary<string, string>()
      {
        {
          "user_ids",
          userIds.GetCommaSeparated()
        },
        {
          "fields",
          "uid,first_name,last_name,online,online_mobile,photo_max,sex,photo_medium,friend_status"
        }
      }, (Action<BackendResult<List<User>, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          foreach (User user in res.ResultData)
          {
            user.CachedDateTime = DateTime.Now;
            this._usersCache[user.uid] = user;
          }
        }
        callback(res);
      }),  null, false, true, new CancellationToken?(),  null);
    }

    public void GetFriendsForCurrentUser(Action<BackendResult<List<User>, ResultCode>> callback)
    {
      Random random = new Random(DateTime.Now.Millisecond);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary.Add("random", random.Next().ToString());
      dictionary.Add("fields", "uid,first_name,last_name,online,online_mobile,photo_max,site,contacts,bdate,photo_max,bdate,sex,first_name_dat");
      dictionary.Add("order", "hints");
      string methodName = "friends.get";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<List<User>, ResultCode>> callback1 = (Action<BackendResult<List<User>, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          UsersService._friends = res.ResultData.ToLookup<User, long>((Func<User, long>) (u => u.uid));
        callback(res);
      });
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<List<User>>(methodName, parameters, callback1, (Func<string, List<User>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<User>>>(jsonStr).response.items), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetFriendsWithRequests(Action<BackendResult<AllFriendsList, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "requests_count",
          "1"
        },
        {
          "requests_offset",
          "0"
        },
        {
          "without_friends",
          "0"
        },
        {
          "requests_only",
          "0"
        },
        {
          "suggested_only",
          "0"
        }
      };
      string methodName = "execute.getFriendsWithRequests";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<AllFriendsList, ResultCode>> callback1 = (Action<BackendResult<AllFriendsList, ResultCode>>) (result => callback(result));
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<AllFriendsList>(methodName, parameters, callback1, (Func<string, AllFriendsList>) (jsonString => JsonConvert.DeserializeObject<GenericRoot<AllFriendsList>>(jsonString).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetFriends(long uid, long lid, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("fields", "uid,first_name,last_name,online, online_mobile,photo_max");
      parameters.Add("order", "hints");
      if (uid != 0L)
        parameters["user_id"] = uid.ToString();
      if (lid != 0L)
        parameters["list_id"] = lid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<List<User>>("friends.get", parameters, callback, UsersService._deserializeUsersFunc, false, true, new CancellationToken?(),  null);
    }

    public void GetStatus(long userId, Action<BackendResult<UserStatus, ResultCode>> callback)
    {
      if (userId < 0L)
        callback(new BackendResult<UserStatus, ResultCode>(ResultCode.Succeeded, new UserStatus()));
      else
        VKRequestsDispatcher.DispatchRequestToVK<UserStatus>("messages.getLastActivity", new Dictionary<string, string>()
        {
          {
            "user_id",
            userId.ToString()
          }
        }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void AddFriend(long userId, Action<BackendResult<OwnCounters, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var uid = {0};\r\n\r\nAPI.friends.add({{\"user_id\": uid}});\r\n\r\nreturn API.getCounters();", userId)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(),  null);
    }

    public void DeleteFriend(long userId, Action<BackendResult<OwnCounters, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var uid = {0};\r\n\r\nAPI.friends.delete({{\"user_id\": uid}});\r\n\r\nreturn API.getCounters();", userId)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(),  null);
    }

    public void SearchUsers(string searchString, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("users.search", new Dictionary<string, string>()
      {
        {
          "q",
          searchString
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "fields",
          "uid, first_name, last_name, online, online_mobile, photo_max, phone, photo_medium"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SaveProfilePhoto(Rect thumbnailRect, byte[] photoData, Action<BackendResult<ProfilePhoto, ResultCode>> callback)
    {
      this.GetPhotoUploadServer((Action<BackendResult<UploadServerAddress, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
        {
          callback(new BackendResult<ProfilePhoto, ResultCode>(res.ResultCode));
        }
        else
        {
          string uploadUrl = res.ResultData.upload_url;
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
              callback(new BackendResult<ProfilePhoto, ResultCode>(ResultCode.UnknownError));
            else
              this.SaveProfilePhoto(JsonConvert.DeserializeObject<UploadPhotoResponseData>(jsonResult.JsonString), callback);
          }), "MyImage.jpg",  null,  null);
        }
      }));
    }

    public void UploadProfilePhoto(byte[] photoData, Action<BackendResult<UploadPhotoResponseData, ResultCode>> callback)
    {
      this.GetPhotoUploadServer((Action<BackendResult<UploadServerAddress, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
          callback(new BackendResult<UploadPhotoResponseData, ResultCode>(res.ResultCode));
        else
          JsonWebRequest.Upload(res.ResultData.upload_url, (Stream) new MemoryStream(photoData), "photo", "image", (Action<JsonResponseData>) (jsonResult =>
          {
            if (!jsonResult.IsSucceeded)
              callback(new BackendResult<UploadPhotoResponseData, ResultCode>(ResultCode.UnknownError));
            else
              callback(new BackendResult<UploadPhotoResponseData, ResultCode>(ResultCode.Succeeded, JsonConvert.DeserializeObject<UploadPhotoResponseData>(jsonResult.JsonString)));
          }), "MyImage.jpg",  null,  null);
      }));
    }

    public void SaveProfilePhoto(UploadPhotoResponseData responseData, Action<BackendResult<ProfilePhoto, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["server"] = responseData.server;
      dictionary["photo"] = responseData.photo;
      dictionary["hash"] = responseData.hash;
      string methodName = "photos.saveOwnerPhoto";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ProfilePhoto, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<ProfilePhoto>(methodName, parameters, callback1, (Func<string, ProfilePhoto>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<ProfilePhoto>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetPhotoUploadServer(Action<BackendResult<UploadServerAddress, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getOwnerPhotoUploadServer", new Dictionary<string, string>(), callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetProfileInfo(long userId, Action<BackendResult<UserData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "userId";
      string str1 = userId.ToString();
      parameters[index1] = str1;
      string index2 = "func_v";
      string str2 = "3";
      parameters[index2] = str2;
      VKRequestsDispatcher.DispatchRequestToVK<UserData>("execute.getProfileInfo", parameters, callback, (Func<string, UserData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "relatives", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "wallData", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups", false);
        jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "personal");
        jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "occupation");
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "photos", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "subscriptions", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "gifts", true);
        int resultCount;
        jsonStr = VKRequestsDispatcher.GetArrayCountAndRemove(jsonStr, "wall", out resultCount);
        VKRequestsDispatcher.GenericRoot<UserData> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<UserData>>(jsonStr);
        if (genericRoot.response.user.counters.docs == 0)
          genericRoot.response.user.counters.docs = genericRoot.response.docsCount;
        return genericRoot.response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void FriendAddDelete(long userId, bool add, Action<BackendResult<OwnCounters, ResultCode>> callback)
    {
      string str = add ? "friends.add" : "friends.delete";
      VKRequestsDispatcher.DispatchRequestToVK<OwnCounters>("execute", new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("var uid = {0};\r\n\r\nAPI.{1}({{\"user_id\": uid}});\r\n\r\nreturn API.getCounters();", userId, str)
        }
      }, callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?(),  null);
    }

    public void GetFriendsAndMutual(long userId, Action<BackendResult<FriendsAndMutualFriends, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str = string.Format("var uid= {0};\r\nvar friends=API.friends.get({{\"user_id\":uid, \"order\":\"hints\", \"fields\":\"uid,first_name,last_name,online, online_mobile,photo_max\"}}).items;\r\nvar mutualFriends = API.friends.getMutual({{\"target_uid\": uid}});\r\n\r\nreturn {{\"friends\" : friends, \"mutualFriends\": mutualFriends}};", userId);
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<FriendsAndMutualFriends>("execute", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetFriendsAndLists(Action<BackendResult<FriendsAndLists, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str = "var friends=API.friends.get({ \"order\":\"hints\", \"fields\":\"uid,first_name,last_name,online, online_mobile,photo_max, first_name_gen, last_name_gen\"}).items;\r\nvar friendLists = API.friends.getLists({\"return_system\":1}).items;\r\n\r\nreturn {\"friends\":friends, \"friendLists\":friendLists};";
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<FriendsAndLists>("execute", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void AddList(string name, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["name"] = name;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("friends.addList", parameters, callback, (Func<string, ResponseWithId>) (jsonStr =>
      {
        VKRequestsDispatcher.GenericRoot<FriendsList> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<FriendsList>>(jsonStr);
        return new ResponseWithId()
        {
          response = genericRoot.response.lid
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void RemoveList(long lid, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["list_id"] = lid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("friends.deleteList", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void EditList(long lid, string name, List<long> uids, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["name"] = name;
      parameters["list_id"] = lid.ToString();
      if (!uids.IsNullOrEmpty())
        parameters["user_ids"] = uids.GetCommaSeparated();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("friends.editList", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetFriendsForList(long lid, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["list_id"] = lid.ToString();
      parameters["fields"] = "uid,first_name,last_name,online, online_mobile,photo_max";
      VKRequestsDispatcher.DispatchRequestToVK<List<User>>("friends.get", parameters, callback, UsersService._deserializeUsersFunc, false, true, new CancellationToken?(),  null);
    }

    public void EditListName(long lid, string updatedName, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      this.GetFriendsForList(lid, (Action<BackendResult<List<User>, ResultCode>>) (resFriends =>
      {
        if (resFriends.ResultCode != ResultCode.Succeeded)
          callback(new BackendResult<ResponseWithId, ResultCode>(resFriends.ResultCode));
        else
          this.EditList(lid, updatedName, resFriends.ResultData.Select<User, long>((Func<User, long>) (u => u.uid)).ToList<long>(), callback);
      }));
    }

    public void GetFollowers(long uid, int offset, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["user_id"] = uid.ToString();
      dictionary["offset"] = offset.ToString();
      dictionary["fields"] = "online, online_mobile,photo_max";
      string methodName = "users.getFollowers";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<List<User>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<List<User>>(methodName, parameters, callback1, (Func<string, List<User>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<User>>>(jsonStr).response.items), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetFriendRequests(bool areSuggestedFriends, int offset, int count, Action<BackendResult<FriendRequests, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "requests_count",
          count.ToString()
        },
        {
          "requests_offset",
          offset.ToString()
        },
        {
          "without_friends",
          "1"
        },
        {
          "requests_only",
          !areSuggestedFriends ? "1" : "0"
        },
        {
          "suggested_only",
          areSuggestedFriends ? "1" : "0"
        }
      };
      string methodName = "execute.getFriendsWithRequests";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<FriendRequests, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<FriendRequests>(methodName, parameters, callback1, (Func<string, FriendRequests>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<FriendRequests>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetLists(Action<BackendResult<List<FriendsList>, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["return_system"] = "1";
      string methodName = "friends.getLists";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<List<FriendsList>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<List<FriendsList>>(methodName, parameters, callback1, (Func<string, List<FriendsList>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<FriendsList>>>(jsonStr).response.items), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetProfileBasicInfo(long userId, Action<BackendResult<UserData, ResultCode>> callback)
    {
      string code = string.Format("var u = {0};\r\nvar full = API.users.get({{\"user_ids\":u, \"fields\":\"uid,first_name,last_name,photo_max,online, online_mobile,can_write_private_message,last_seen, can_post,activity,counters,wall_comments,sex\"}});\r\nvar friend_state = API.friends.areFriends({{\"user_ids\":u}})[0];\r\nreturn {{\"User\": full[0], \"friend\": friend_state}};", userId);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      Action<BackendResult<UserData, ResultCode>> callback1 = callback;
      // ISSUE: variable of the null type
      
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.Execute<UserData>(code, callback1, null, num1 != 0, num2 != 0, cancellationToken);
    }

    public void GetSubscriptions(long uid, Action<BackendResult<UsersAndGroups, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<UsersAndGroups>("execute.getSubscriptions", new Dictionary<string, string>()
      {
        {
          "user_id",
          uid.ToString()
        }
      }, callback, (Func<string, UsersAndGroups>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "users", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "pages", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
        return JsonConvert.DeserializeObject<GenericRoot<UsersAndGroups>>(jsonStr).response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetUsersForTile(long uid, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      if (uid < 0L)
      {
        List<long> userIds = new List<long>();
        userIds.Add(uid);
        Action<BackendResult<List<User>, ResultCode>> callback1 = callback;
        this.GetUsers(userIds, callback1);
      }
      else
      {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters["user_ids"] = uid.ToString();
        parameters["fields"] = "photo_max";
        string name = Thread.CurrentThread.CurrentUICulture.Name;
        parameters["name_case"] = "ins";
        VKRequestsDispatcher.DispatchRequestToVK<List<User>>("users.get", parameters, callback,  null, false, true, new CancellationToken?(),  null);
      }
    }

    public void Search(SearchParams searchParams, string query, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      if (searchParams == null)
        return;
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        }
      };
      if (!string.IsNullOrWhiteSpace(query))
        parameters.Add("q", query);
      Country country = searchParams.GetValue<Country>("country");
      long id;
      if (country != null && country.id > 0L)
      {
        Dictionary<string, string> dictionary = parameters;
        string key = "country";
        id = country.id;
        string str = id.ToString();
        dictionary.Add(key, str);
      }
      City city = searchParams.GetValue<City>("city");
      if (city != null && city.id > 0L)
      {
        Dictionary<string, string> dictionary = parameters;
        string key = "city";
        id = city.id;
        string str = id.ToString();
        dictionary.Add(key, str);
      }
      if (searchParams.GetValue<int>("age_from") > 0)
      {
        int num1 = searchParams.GetValue<int>("age_from");
        int num2 = searchParams.GetValue<int>("age_to");
        if (num1 > 0)
          parameters.Add("age_from", num1.ToString());
        if (num2 > 0)
          parameters.Add("age_to", num2.ToString());
      }
      int num3 = searchParams.GetValue<int>("sex");
      if (num3 > 0)
        parameters.Add("sex", num3.ToString());
      int num4 = searchParams.GetValue<int>("status");
      if (num4 > 0)
        parameters.Add("status", num4.ToString());
      if (searchParams.GetValue<bool>("has_photo"))
        parameters.Add("has_photo", "1");
      if (searchParams.GetValue<bool>("online"))
        parameters.Add("online", "1");
      parameters.Add("fields", "photo_max,verified,occupation,city,country,friend_status");
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("users.search", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetNearby(double lat, double lon, uint? accuracy, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "latitude",
          lat.ToString((IFormatProvider) CultureInfo.InvariantCulture)
        },
        {
          "longitude",
          lon.ToString((IFormatProvider) CultureInfo.InvariantCulture)
        },
        {
          "radius",
          "2"
        },
        {
          "timeout",
          "300"
        },
        {
          "fields",
          "photo_max,verified,occupation,city,country,friend_status,common_count"
        }
      };
      if (accuracy.HasValue)
        parameters["accuracy"] = accuracy.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("users.getNearby", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetFriendsSuggestions(string nextFrom, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("friends.getRecommendations", new Dictionary<string, string>()
      {
        {
          "count",
          count.ToString()
        },
        {
          "start_from",
          nextFrom
        },
        {
          "fields",
          "photo_max,friend_status"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetFeatureUsers(int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("friends.getFeatureUsers", new Dictionary<string, string>()
      {
        {
          "count",
          count.ToString()
        },
        {
          "fields",
          "photo_max"
        },
        {
          "feature",
          "contacts_import"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
