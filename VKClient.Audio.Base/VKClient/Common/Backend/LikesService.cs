using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend
{
  public class LikesService
  {
    private static LikesService _current = new LikesService();

    public static LikesService Current
    {
      get
      {
        return LikesService._current;
      }
    }

    public void AddRemoveLike(bool add, long owner_id, long item_id, LikeObjectType objectType, Action<BackendResult<ResponseWithId, ResultCode>> callback, string accessKey = "")
    {
      string methodName = add ? "likes.add" : "likes.delete";
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["owner_id"] = owner_id.ToString();
      dictionary["item_id"] = item_id.ToString();
      dictionary["type"] = objectType.ToString();
      if (!string.IsNullOrEmpty(accessKey))
        dictionary["access_key"] = accessKey;
      EventAggregator.Current.Publish(new ObjectLikedUnlikedEvent()
      {
        Liked = add,
        ItemId = item_id,
        OwnerId = owner_id,
        LikeObjType = objectType
      });
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ResponseWithId, ResultCode>> callback1 = (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
      {
        int resultCode = (int) res.ResultCode;
        callback(res);
      });
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback1, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetLikesList(LikeObjectType type, long owner_id, long item_id, int count, int offset, bool onlyCopies, bool onlyFriends, Action<BackendResult<LikesList, ResultCode>> callback)
    {
      string str = string.Format("var count = {0};\r\nvar offset= {1};\r\nvar owner_id={2};\r\nvar item_id= {3};\r\nvar type = \"{4}\";\r\n\r\n\r\nvar likesAll = API.likes.getList({{\"type\":type,\r\n\"owner_id\": owner_id, \"item_id\":item_id, \"count\":count, \"offset\":offset, \"filter\":\"{5}\", \"friends_only\":{6} }});\r\n\r\nvar userOrGroupIds = likesAll.items;\r\n\r\nvar userIds = [];\r\nvar groupIds = [];\r\n\r\nvar i = 0;\r\n\r\nvar length = userOrGroupIds.length;\r\n\r\nwhile (i < length)\r\n{{\r\n    var id = parseInt(userOrGroupIds[i]);\r\n    \r\n    if (id > 0)\r\n    {{\r\n       if (userIds.length > 0)\r\n       {{\r\n          userIds = userIds + \",\";\r\n       }}\r\n       userIds = userIds + id;\r\n    }}\r\n    else if (id < 0)\r\n    {{\r\n        id = -id;\r\n        if (groupIds.length > 0)\r\n        {{\r\n            groupIds = groupIds + \",\";\r\n        }}\r\n        groupIds = groupIds + id;\r\n    }}\r\n     \r\n    i = i + 1;\r\n}}\r\n\r\nvar users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile\" }});\r\nvar groups = API.groups.getById({{\"group_ids\":groupIds}});\r\n\r\nreturn {{\"AllCount\": likesAll.count, \"All\":users, \"AllGroups\":groups, \"AllIds\" : userOrGroupIds}};", count, offset, owner_id, item_id, type.ToString(), (onlyCopies ? "copies" : "likes"), (onlyFriends ? 1 : 0));
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<LikesList>("execute", parameters, callback, (Func<string, LikesList>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "All", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "AllGroups", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "AllIds", false);
        return JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<LikesList>>(jsonStr).response;
      }), false, true, new CancellationToken?(),  null);
    }
  }
}
