using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class NewsFeedService
  {
    private static NewsFeedService _current = new NewsFeedService();

    public static NewsFeedService Current
    {
      get
      {
        return NewsFeedService._current;
      }
    }

    public Task<BackendResult<NewsFeedData, ResultCode>> GetNewsFeed(NewsFeedGetParams parameters)
    {
      TaskCompletionSource<BackendResult<NewsFeedData, ResultCode>> tcs = new TaskCompletionSource<BackendResult<NewsFeedData, ResultCode>>();
      this.GetNewsFeed(parameters, (Action<BackendResult<NewsFeedData, ResultCode>>) (res => tcs.TrySetResult(res)));
      return tcs.Task;
    }

    public void GetNewsFeed(NewsFeedGetParams parameters, Action<BackendResult<NewsFeedData, ResultCode>> callback)
    {
      string methodName = "execute.getNewsfeed";
      Dictionary<string, string> paramDict = new Dictionary<string, string>()
      {
        {
          "count",
          parameters.count.ToString()
        },
        {
          "device_info",
          new AdvertisingDeviceInfo().ToJsonString()
        }
      };
      paramDict["fields"] = "sex,online,photo_50,photo_100,photo_200,is_friend";
      if (!string.IsNullOrEmpty(parameters.from))
        paramDict["start_from"] = parameters.from;
      bool? nullable;
      if (parameters.NewsListId == -100L)
      {
        paramDict["filters"] = "video";
        paramDict["grouping"] = "100";
        paramDict["extended"] = "1";
      }
      else if (parameters.photoFeed || parameters.NewsListId == -101L)
      {
        paramDict["filters"] = "photo,photo_tag,wall_photo";
        paramDict["max_photos"] = "10";
        if (parameters.source_ids.NotNullAndHasAtLeastOneNonNullElement())
        {
          paramDict["source_ids"] = parameters.source_ids.GetCommaSeparated();
        }
        else
        {
          paramDict["source_ids"] = "friends,following";
          paramDict["return_banned"] = "1";
        }
      }
      else
      {
        paramDict["filters"] = "post,photo,photo_tag";
        long num1 = parameters.NewsListId - -10L;
        long num2 = 3;
        if ((ulong) num1 <= (ulong) num2)
        {
          switch ((uint) num1)
          {
            case 0:
              paramDict["is_newsfeed"] = "1";
              Dictionary<string, string> dictionary1 = paramDict;
              dictionary1["filters"] = dictionary1["filters"] + ",friends_recomm,ads_post";
              nullable = AppGlobalStateManager.Current.GlobalState.AdsDemoManualSetting;
              bool flag = true;
              if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
              {
                Dictionary<string, string> dictionary2 = paramDict;
                dictionary2["filters"] = dictionary2["filters"] + ",ads_demo";
                goto label_16;
              }
              else
                goto label_16;
            case 1:
              paramDict["recommended"] = "1";
              goto label_16;
            case 2:
              paramDict["source_ids"] = "friends,following";
              goto label_16;
            case 3:
              paramDict["source_ids"] = "groups,pages";
              goto label_16;
          }
        }
        paramDict["source_ids"] = "list" + parameters.NewsListId;
      }
label_16:
      NewsFeedType? feedType = parameters.FeedType;
      if (feedType.HasValue)
      {
        paramDict["feed_type"] = feedType.Value.ToString();
        if (parameters.UpdateFeedType)
          paramDict["set_feed_type"] = "1";
      }
      if (parameters.SyncNotifications)
        paramDict["sync_notifications"] = "1";
      nullable = parameters.TopFeedPromoAnswer;
      if (nullable.HasValue)
      {
        Dictionary<string, string> dictionary = paramDict;
        string index = "top_feed_promo_accepted";
        nullable = parameters.TopFeedPromoAnswer;
        string str = nullable.Value ? "1" : "0";
        dictionary[index] = str;
        paramDict["top_feed_promo_id"] = parameters.TopFeedPromoId.ToString();
      }
      ConnectionType connectionType = NetworkStatusInfo.Instance.RetrieveNetworkConnectionType();
      if (connectionType != null)
      {
        paramDict["connection_type"] = connectionType.Type;
        paramDict["connection_subtype"] = connectionType.Subtype;
      }
      if (parameters.UpdateAwayTime > 0)
        paramDict["update_away_time"] = parameters.UpdateAwayTime.ToString();
      if (parameters.UpdatePosition > -1)
        paramDict["update_position"] = parameters.UpdatePosition.ToString();
      if (!string.IsNullOrEmpty(parameters.UpdatePost))
        paramDict["update_post"] = parameters.UpdatePost;
      NewsFeedService.DoLoadNewsFeed(methodName, paramDict, callback);
    }

    private static void DoLoadNewsFeed(string methodName, Dictionary<string, string> paramDict, Action<BackendResult<NewsFeedData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<NewsFeedData>(methodName, paramDict, callback, (Func<string, NewsFeedData>) (jsonStr =>
      {
        List<int> resultCounts1;
        jsonStr = VKRequestsDispatcher.GetArrayCountsAndRemove(jsonStr, "photos", out resultCounts1);
        List<int> resultCounts2;
        jsonStr = VKRequestsDispatcher.GetArrayCountsAndRemove(jsonStr, "photo_tags", out resultCounts2);
        NewsFeedData response = JsonConvert.DeserializeObject<GenericRoot<NewsFeedData>>(jsonStr).response;
        NewsFeedType result1;
        if (Enum.TryParse<NewsFeedType>(response.feed_type, out result1))
          response.FeedType = new NewsFeedType?(result1);
        VKList<UserNotification> notifications = response.notifications;
        if ((notifications != null ? notifications.items :  null) != null)
        {
          foreach (UserNotification userNotification in response.notifications.items)
          {
            UserNotificationType result2;
            if (Enum.TryParse<UserNotificationType>(userNotification.type, out result2))
              userNotification.Type = result2;
          }
        }
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetNewsComments(int startTime, int endTime, int count, string fromStr, Action<BackendResult<NewsFeedData, ResultCode>> callback)
    {
      string str = string.Format("    \r\n                    var comments = API.newsfeed.getComments({{ last_comments_count: 3, allow_group_comments: 1{0}{1}{2}{3} }});\r\n\r\n                    var response = \r\n                    {{\r\n                        items: [],\r\n                        profiles: comments.profiles,\r\n                        groups: comments.groups,\r\n                        next_from: comments.next_from\r\n                    }};\r\n\r\n                    var i = 0;\r\n                    while (i < comments.items.length)\r\n                    {{\r\n                        var item = comments.items[i];\r\n    \r\n                        if (item.type == \"video\")\r\n                            item.views = {{ count: item.views }};\r\n\r\n                        response.items.push(item);\r\n                        i = i + 1;\r\n                    }}\r\n\r\n                    return response;\r\n                ", (startTime > 0 ? string.Format(", start_time: {0}", startTime) : ""), (endTime > 0 ? string.Format(", end_time: {0}", endTime) : ""), (count > 0 ? string.Format(", count: {0}", count) : ""), (!string.IsNullOrWhiteSpace(fromStr) ? string.Format(", start_from: {0}", fromStr) : ""));
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str);
      Action<BackendResult<NewsFeedData, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<NewsFeedData>(methodName, parameters, callback1, (Func<string, NewsFeedData>) (jsonString =>
      {
        jsonString = VKRequestsDispatcher.FixFalseArray(jsonString, "profiles", false);
        jsonString = VKRequestsDispatcher.FixFalseArray(jsonString, "groups", false);
        return JsonConvert.DeserializeObject<GenericRoot<NewsFeedData>>(jsonString).response;
      }), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetNotifications(int startTime, int endTime, int offset, string fromStr, int count, Action<BackendResult<NotificationData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (startTime > 0)
        parameters["start_time"] = startTime.ToString();
      if (endTime > 0)
        parameters["end_time"] = endTime.ToString();
      if (count > 0)
        parameters["count"] = count.ToString();
      if (offset > 0)
        parameters["offset"] = offset.ToString();
      if (!string.IsNullOrWhiteSpace(fromStr))
        parameters["start_from"] = fromStr;
      parameters["fields"] = "sex,photo_50,photo_100,online,screen_name,first_name_dat,last_name_dat,first_name_gen,last_name_gen";
      Dictionary<string, string> dictionary = parameters;
      dictionary["fields"] = dictionary["fields"] + ",is_closed,type,is_admin,is_member,photo_200";
      VKRequestsDispatcher.DispatchRequestToVK<NotificationData>("notifications.get", parameters, callback, (Func<string, NotificationData>) (jsonStr =>
      {
        int resultCount = 0;
        jsonStr = VKRequestsDispatcher.GetArrayCountAndRemove(jsonStr, "items", out resultCount);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "profiles", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
        NotificationData response = JsonConvert.DeserializeObject<GenericRoot<NotificationData>>(jsonStr).response;
        response.TotalCount = resultCount;
        List<Notification> notificationList = new List<Notification>();
        foreach (Notification notification in response.items)
        {
          notification.UpdateNotificationType();
          object parsedFeedback = notification.ParsedFeedback;
          object parsedParent = notification.ParsedParent;
          if (notification.NotType == NotificationType.unknown)
            notificationList.Add(notification);
        }
        foreach (Notification notification in notificationList)
          response.items.Remove(notification);
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void Search(string searchStr, int count, int startTime, int endTime, string startFrom, Action<BackendResult<NewsFeedData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["q"] = searchStr;
      parameters["count"] = count.ToString();
      if (startTime > 0)
        parameters["start_time"] = startTime.ToString();
      if (endTime > 0)
        parameters["end_time"] = endTime.ToString();
      if (!string.IsNullOrWhiteSpace("startFrom"))
        parameters["start_from"] = startFrom.ToString();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<NewsFeedData>("newsfeed.search", parameters, callback, (Func<string, NewsFeedData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "items", false);
        NewsFeedData response = JsonConvert.DeserializeObject<GenericRoot<NewsFeedData>>(jsonStr).response;
        foreach (NewsItem newsItem in response.items)
        {
          if (newsItem.user != null)
            response.profiles.Add(newsItem.user);
          if (newsItem.group != null)
            response.groups.Add(newsItem.group);
        }
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void MarkAsViewed()
    {
      string methodName = "notifications.markAsViewed";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      Action<BackendResult<ResponseWithId, ResultCode>> action = (Action<BackendResult<ResponseWithId, ResultCode>>) (r => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback, (Func<string, ResponseWithId>) (j => new ResponseWithId()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetBanned(Action<BackendResult<ProfilesAndGroups, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["extended"] = "1";
      parameters["fields"] = "online, online_mobile, photo_max";
      VKRequestsDispatcher.DispatchRequestToVK<ProfilesAndGroups>("newsfeed.getBanned", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void DeleteBan(List<long> uids, List<long> gids, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      NewsFeedService.AddDeleteBan(false, uids, gids, callback);
    }

    public void AddBan(List<long> uids, List<long> gids, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      NewsFeedService.AddDeleteBan(true, uids, gids, callback);
    }

    private static void AddDeleteBan(bool addBan, List<long> uids, List<long> gids, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (uids != null && uids.Count > 0)
        parameters["user_ids"] = uids.GetCommaSeparated();
      if (gids != null && gids.Count > 0)
        parameters["group_ids"] = gids.GetCommaSeparated();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(addBan ? "newsfeed.addBan" : "newsfeed.deleteBan", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetSuggestedSources(int offset, int count, bool shuffle, Action<BackendResult<VKList<UserOrGroupSource>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "offset";
      string str1 = offset.ToString();
      parameters[index1] = str1;
      string index2 = "count";
      string str2 = count.ToString();
      parameters[index2] = str2;
      string index3 = "shuffle";
      string str3 = shuffle ? "1" : "0";
      parameters[index3] = str3;
      string index4 = "fields";
      string str4 = "is_member,activity,is_closed,photo_200,photo_max,verified,friends_status,occupation,city,country";
      parameters[index4] = str4;
      VKRequestsDispatcher.DispatchRequestToVK<VKList<UserOrGroupSource>>("newsfeed.getSuggestedSources", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void Unsubscribe(string type, long ownerId, long itemId, Action<BackendResult<bool, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string index1 = "type";
      string str1 = type;
      parameters[index1] = str1;
      string index2 = "owner_id";
      string str2 = ownerId.ToString();
      parameters[index2] = str2;
      string index3 = "item_id";
      string str3 = itemId.ToString();
      parameters[index3] = str3;
      VKRequestsDispatcher.DispatchRequestToVK<bool>("newsfeed.unsubscribe", parameters, callback, (Func<string, bool>) (jsonStr =>
      {
        VKRequestsDispatcher.GenericRoot<int> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(jsonStr);
        if (genericRoot == null)
          return false;
        return genericRoot.response == 1;
      }), false, true, new CancellationToken?(),  null);
    }

    public void IgnoreItem(string type, long ownerId, long itemId, Action<BackendResult<bool, ResultCode>> callback)
    {
      this.IgnoreUnignoreItem(true, type, ownerId, itemId, callback);
    }

    public void UnignoreItem(string type, long ownerId, long itemId, Action<BackendResult<bool, ResultCode>> callback)
    {
      this.IgnoreUnignoreItem(false, type, ownerId, itemId, callback);
    }

    private void IgnoreUnignoreItem(bool ignore, string type, long ownerId, long itemId, Action<BackendResult<bool, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index1 = "type";
      string str1 = type;
      dictionary[index1] = str1;
      string index2 = "owner_id";
      string str2 = ownerId.ToString();
      dictionary[index2] = str2;
      string index3 = "item_id";
      string str3 = itemId.ToString();
      dictionary[index3] = str3;
      Dictionary<string, string> parameters = dictionary;
      VKRequestsDispatcher.DispatchRequestToVK<bool>(ignore ? "newsfeed.ignoreItem" : "newsfeed.unignoreItem", parameters, callback, (Func<string, bool>) (jsonStr =>
      {
        VKRequestsDispatcher.GenericRoot<int> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(jsonStr);
        if (genericRoot == null)
          return false;
        return genericRoot.response == 1;
      }), false, true, new CancellationToken?(),  null);
    }
  }
}
