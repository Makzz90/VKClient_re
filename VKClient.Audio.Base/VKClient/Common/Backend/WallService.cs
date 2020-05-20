using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class WallService
  {
    private static WallService _current = new WallService();

    public static WallService Current
    {
      get
      {
        return WallService._current;
      }
    }

    public void GetWallSubscriptionsProfiles(int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<VKList<User>>(string.Format("var subscriptions = API.wall.getSubscriptions({{\"offset\":{0}, \"count\":{1}, \"type\":1}});\r\nvar users = [];\r\nif (subscriptions.items.length > 0)\r\n{{\r\n     users = API.users.get({{\"user_ids\":subscriptions.items, \"fields\":\"photo_max, online, online_mobile\"}});\r\n}}\r\nreturn {{\"items\": users, \"count\":subscriptions.count}};", offset, count), callback,  null, false, true, new CancellationToken?());
    }

    public void GetWallSubscriptionsGroups(int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<VKList<Group>>(string.Format("var subscriptions = API.wall.getSubscriptions({{\"offset\":{0}, \"count\":{1}, \"type\":2}});\r\nvar groups = [];\r\nif (subscriptions.items.length > 0)\r\n{{\r\n   var groupIds = [];\r\n   var i= 0;\r\n\r\n   while (i < subscriptions.items.length)\r\n   {{\r\n       groupIds.push(-subscriptions.items[i]);\r\n       i = i + 1;\r\n   }}\r\n\r\n   groups = API.groups.getById({{\"group_ids\":groupIds}});\r\n}}\r\nreturn {{\"items\": groups, \"count\":subscriptions.count}};", offset, count), callback,  null, false, true, new CancellationToken?());
    }

    public void WallSubscriptionsUnsubscribe(List<long> ownerIds, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      string format = "API.wall.unsubscribe({{\"owner_id\":{0} }});";
      string code = "";
      foreach (long ownerId in ownerIds)
        code = code + string.Format(format, ownerId) + Environment.NewLine;
      VKRequestsDispatcher.Execute<ResponseWithId>(code, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }

    public void WallSubscriptionsSubscribe(long ownerId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("wall.subscribe", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void PinUnpin(bool pin, long ownerId, long postId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["post_id"] = postId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(pin ? "wall.pin" : "wall.unpin", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void Edit(WallPostRequestData postData, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      long num;
      if (postData.owner_id != 0L)
      {
        Dictionary<string, string> dictionary = parameters;
        string index = "owner_id";
        num = postData.owner_id;
        string str = num.ToString();
        dictionary[index] = str;
      }
      Dictionary<string, string> dictionary1 = parameters;
      string index1 = "post_id";
      num = postData.post_id;
      string str1 = num.ToString();
      dictionary1[index1] = str1;
      if (!string.IsNullOrEmpty(postData.message))
        parameters["message"] = postData.message ?? "";
      parameters["signed"] = postData.Sign ? "1" : "0";
      if (!postData.AttachmentIds.IsNullOrEmpty())
        parameters["attachments"] = postData.AttachmentIds.GetCommaSeparated(",");
      double? nullable;
      if (postData.latitude.HasValue)
      {
        Dictionary<string, string> dictionary2 = parameters;
        string index2 = "lat";
        nullable = postData.latitude;
        string str2 = nullable.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        dictionary2[index2] = str2;
      }
      nullable = postData.longitude;
      if (nullable.HasValue)
      {
        Dictionary<string, string> dictionary2 = parameters;
        string index2 = "long";
        nullable = postData.longitude;
        string str2 = nullable.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        dictionary2[index2] = str2;
      }
      if (postData.publish_date.HasValue)
      {
        Dictionary<string, string> dictionary2 = parameters;
        string index2 = "publish_date";
        num = postData.publish_date.Value;
        string str2 = num.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        dictionary2[index2] = str2;
      }
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("wall.edit", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()
      {
        response = 1L
      }), false, true, new CancellationToken?(),  null);
    }

    public void Post(WallPostRequestData postData, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      long num;
      if (postData.owner_id != 0L)
      {
        Dictionary<string, string> dictionary = parameters;
        string index = "owner_id";
        num = postData.owner_id;
        string str = num.ToString();
        dictionary[index] = str;
      }
      if (!string.IsNullOrEmpty(postData.message))
        parameters["message"] = postData.message ?? "";
      if (!postData.AttachmentIds.IsNullOrEmpty())
        parameters["attachments"] = postData.AttachmentIds.GetCommaSeparated(",");
      double? nullable;
      if (postData.latitude.HasValue)
      {
        Dictionary<string, string> dictionary = parameters;
        string index = "lat";
        nullable = postData.latitude;
        string str = nullable.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        dictionary[index] = str;
      }
      nullable = postData.longitude;
      if (nullable.HasValue)
      {
        Dictionary<string, string> dictionary = parameters;
        string index = "long";
        nullable = postData.longitude;
        string str = nullable.Value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        dictionary[index] = str;
      }
      if (postData.publish_date.HasValue)
      {
        Dictionary<string, string> dictionary = parameters;
        string index = "publish_date";
        num = postData.publish_date.Value;
        string str = num.ToString((IFormatProvider) CultureInfo.InvariantCulture);
        dictionary[index] = str;
      }
      if (postData.PublishOnTwitter)
        parameters["services"] = "twitter";
      if (postData.PublishOnFacebook)
        parameters["services"] = !postData.PublishOnTwitter ? "facebook" : "twitter,facebook";
      if (postData.post_id != 0L)
      {
        Dictionary<string, string> dictionary = parameters;
        string index = "post_id";
        num = postData.post_id;
        string str = num.ToString();
        dictionary[index] = str;
      }
      parameters["from_group"] = postData.OnBehalfOfGroup || postData.Sign ? "1" : "0";
      parameters["signed"] = postData.Sign ? "1" : "0";
      if (postData.FriendsOnly)
        parameters["friends_only"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("wall.post", parameters, callback, (Func<string, ResponseWithId>) (jsonStr =>
      {
        WallService.ResponseWithPostId responseWithPostId = JsonConvert.DeserializeObject<WallService.ResponseWithPostId>(jsonStr);
        return new ResponseWithId()
        {
          response = responseWithPostId.response.post_id
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void Report(long ownerId, long id, ReportReason reportReason, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["post_id"] = id.ToString();
      parameters["reason"] = ((int) reportReason).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("wall.reportPost", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void ReportComment(long ownerId, long commentId, ReportReason reportReason, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["comment_id"] = commentId.ToString();
      parameters["reason"] = ((int) reportReason).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("wall.reportComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetWallPostByIdWithComments(long postId, long ownerId, int offset, int countToRead, int knownTotalCount, bool needWallPost, Action<BackendResult<GetWallPostResponseData, ResultCode>> callback, long pollId = 0, long pollOwnerId = 0, LikeObjectType likeObjType = LikeObjectType.post)
    {
      long userId = VKRequestsDispatcher.AuthData.user_id;
      if (knownTotalCount != -1 && offset + countToRead > knownTotalCount)
        countToRead = knownTotalCount - offset;
      string str1 = (!needWallPost ? "var likesAll = API.likes.getList({{\"item_id\":{0}, \"owner_id\":{1}, \"count\":20, type:\"{8}\"}});" : " var wallPost = API.wall.getById({{\"posts\":\"{1}_{0}\"}});\r\nvar likesAll = API.likes.getList({{\"item_id\":{0}, \"owner_id\":{1}, \"count\":20, type:wallPost[0].post_type}});" + Environment.NewLine) + "\r\n\r\n\r\n\r\nvar offset = {2};\r\n\r\n\r\nvar comments = API.wall.getComments({{\"post_id\":\"{0}\", \"owner_id\":\"{1}\", \"offset\":offset, \"count\":\"{3}\", \"need_likes\":\"1\", \"sort\":\"desc\", \"preview_length\":\"0\", \"allow_group_comments\":1}});\r\n\r\nvar datUsersNames = comments.items@.reply_to_user + comments.items@.from_id;\r\nvar users2 = API.users.get({{\"user_ids\":datUsersNames, \"fields\":\"first_name_dat,last_name_dat\"}});\r\n\r\n\r\n\r\nvar userOrGroupIds = likesAll.items;\r\n";
      if (needWallPost)
        str1 += "userOrGroupIds = userOrGroupIds + wallPost@.from_id + wallPost@.to_id + wallPost@.signer_id + wallPost[0].copy_history@.owner_id + wallPost[0].copy_history@.from_id;\r\n";
      string str2 = str1 + "userOrGroupIds = userOrGroupIds + comments.items@.from_id;\r\n\r\n\r\nvar userIds = [];\r\nvar groupIds = [];\r\n\r\nvar i = 0;\r\n\r\nvar length = userOrGroupIds.length;\r\n\r\nwhile (i < length)\r\n{{\r\n    var id = parseInt(userOrGroupIds[i]);\r\n    \r\n    if (id > 0)\r\n    {{\r\n       if (userIds.length > 0)\r\n       {{\r\n          userIds = userIds + \",\";\r\n       }}\r\n       userIds = userIds + id;\r\n    }}\r\n    else if (id < 0)\r\n    {{\r\n        id = -id;\r\n        if (groupIds.length > 0)\r\n        {{\r\n            groupIds = groupIds + \",\";\r\n        }}\r\n        groupIds = groupIds + id;\r\n    }}\r\n     \r\n    i = i + 1;\r\n}}\r\n\r\nif ({1} < 0)\r\n{{\r\n    if (groupIds.length > 0) groupIds = groupIds + \",\";\r\n    groupIds = groupIds + ({1} * -1);\r\n}}\r\n\r\nvar users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile\" }});\r\nvar groups = API.groups.getById({{\"group_ids\":groupIds}});";
      string str3 = string.Format(pollId == 0L ? (!needWallPost ? str2 + "return {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"comments\": comments, \"Users2\": users2 }};" : str2 + "return {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"WallPost\":wallPost[0], \"comments\": comments, \"Users2\": users2 }};") : (!needWallPost ? str2 + "\r\nvar poll= API.polls.getById({{\"owner_id\":{7}, \"poll_id\":{6}}});   \r\nreturn {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"comments\": comments, \"Users2\": users2, \"Poll\":poll }};" : str2 + "\r\nvar poll= API.polls.getById({{\"owner_id\":{7}, \"poll_id\":{6}}});   \r\nreturn {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"WallPost\":wallPost[0], \"comments\": comments, \"Users2\": users2, \"Poll\":poll }};"), postId, ownerId, offset, countToRead, knownTotalCount, userId, pollId, pollOwnerId, likeObjType);
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str3;
      VKRequestsDispatcher.DispatchRequestToVK<GetWallPostResponseData>("execute", parameters, callback, (Func<string, GetWallPostResponseData>) (jsonStr =>
      {
        VKRequestsDispatcher.GenericRoot<GetWallPostResponseData> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GetWallPostResponseData>>(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(jsonStr, "Users", false), "Users2", false), "Groups", false), "Poll", true), "LikesAll", true), "comments", true));
        if (genericRoot.response.LikesAll.items != null)
          genericRoot.response.LikesAll.users = new List<UserLike>(genericRoot.response.LikesAll.items.Select<long, UserLike>((Func<long, UserLike>) (it => new UserLike()
          {
            uid = it
          })));
        genericRoot.response.Users.Add(AppGlobalStateManager.Current.GlobalState.LoggedInUser);
        genericRoot.response.Comments.Reverse();
        GroupsService.Current.AddCachedGroups((IEnumerable<Group>) genericRoot.response.Groups);
        if (countToRead == 0)
          genericRoot.response.Comments.Clear();
        if (genericRoot.response.WallPost == null)
          genericRoot.response.WallPost = new WallPost();
        return genericRoot.response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void AddComment(long postId, long ownerId, string text, long replyToCid, bool fromGroup, List<string> attachmentIds, Action<BackendResult<Comment, ResultCode>> callback, int stickerId = 0, string stickerReferrer = "")
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("\r\n\r\nvar new_comment_id = API.wall.addComment({{\r\n    owner_id: {0},\r\n    post_id: {1},\r\n    text: \"{2}\",\r\n    from_group: {3},\r\n    sticker_id: {4},\r\n    reply_to_comment: {5},\r\n    attachments: \"{6}\",\r\n    sticker_referrer: \"{7}\"\r\n}}).comment_id;\r\n\r\nvar last_comments = API.wall.getComments({{\r\n    owner_id: {8},\r\n    post_id: {9},\r\n    need_likes: 1,\r\n    count: 10,\r\n    sort: \"desc\",\r\n    preview_length: 0,\r\n    allow_group_comments: 1\r\n}}).items;\r\n\r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r\n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r\n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", ownerId, postId, text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), stickerId, replyToCid, attachmentIds.GetCommaSeparated(","), stickerReferrer, ownerId, postId)
        }
      };
      string methodName = "execute";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<Comment, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Comment>(methodName, parameters, callback1, (Func<string, Comment>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<Comment>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void DeleteComment(long ownerId, long cid, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["comment_id"] = cid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("wall.deleteComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetWallById(long ownerId, long postId, Action<BackendResult<WallData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["posts"] = ownerId.ToString() + "_" + postId.ToString();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<WallData>("wall.getById", parameters, callback, (Func<string, WallData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "items", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "profiles", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
        return JsonConvert.DeserializeObject<GenericRoot<WallData>>(jsonStr).response;
      }), false, true, new CancellationToken?(),  null);
    }

    public Task<BackendResult<WallData, ResultCode>> GetWall(long ownerId, int offset, int count, string filter = "all")
    {
      TaskCompletionSource<BackendResult<WallData, ResultCode>> tcs = new TaskCompletionSource<BackendResult<WallData, ResultCode>>();
      this.GetWall(ownerId, offset, count, (Action<BackendResult<WallData, ResultCode>>) (res => tcs.TrySetResult(res)), filter);
      return tcs.Task;
    }

    public Task<BackendResult<List<WallData>, ResultCode>> GetWallForManyUsers(List<WallService.WallRequestData> requestData)
    {
      TaskCompletionSource<BackendResult<List<WallData>, ResultCode>> tcs = new TaskCompletionSource<BackendResult<List<WallData>, ResultCode>>();
      this.GetWallForManyUsers(requestData, (Action<BackendResult<List<WallData>, ResultCode>>) (res => tcs.TrySetResult(res)));
      return tcs.Task;
    }

    public void GetWallForManyUsers(List<WallService.WallRequestData> requestData, Action<BackendResult<List<WallData>, ResultCode>> callback)
    {
      StringBuilder stringBuilder1 = new StringBuilder().AppendFormat("var users =  API.users.get({{\"user_ids\":\"{0}\", \"fields\":\"wall_default\"}});", new object[1]
      {
        requestData.Select<WallService.WallRequestData, long>((Func<WallService.WallRequestData, long>) (r => r.UserId)).ToList<long>().GetCommaSeparated()
      }).Append(Environment.NewLine);
      for (int index = 0; index < requestData.Count; ++index)
      {
        WallService.WallRequestData wallRequestData = requestData[index];
        stringBuilder1 = stringBuilder1.AppendFormat("var wall{3} = API.wall.get({{\"owner_id\":{0}, \"offset\":{1}, \"count\":{2}, \"extended\":1, \"filter\":users[{3}].wall_default}});", wallRequestData.UserId, wallRequestData.Offset, wallRequestData.Count, index).AppendFormat(Environment.NewLine);
      }
      StringBuilder stringBuilder2 = stringBuilder1.Append("return {");
      for (int index = 0; index < requestData.Count; ++index)
      {
        if (index > 0)
          stringBuilder2 = stringBuilder2.Append(", ");
        stringBuilder2 = stringBuilder2.AppendFormat("\"Wall{0}\":wall{0} ", new object[1]
        {
          index
        });
      }
      VKRequestsDispatcher.Execute<List<WallData>>(stringBuilder2.Append("};").ToString(), callback, (Func<string, List<WallData>>) (jsonStr =>
      {
        List<WallData> wallDataList1 = new List<WallData>();
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "wall", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "profiles", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
        for (int index = 0; index < requestData.Count; ++index)
          jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Wall" + index, true);
        List<int> resultCounts;
        jsonStr = VKRequestsDispatcher.GetArrayCountsAndRemove(jsonStr, "wall", out resultCounts);
        GenericRoot<Dictionary<string, WallData>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<Dictionary<string, WallData>>>(jsonStr);
        List<WallData> wallDataList2 = new List<WallData>();
        foreach (KeyValuePair<string, WallData> keyValuePair in genericRoot.response)
          wallDataList2.Add(keyValuePair.Value);
        return wallDataList2;
      }), false, true, new CancellationToken?());
    }

    public void GetWall(long ownerId, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback, string filter = "all")
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["owner_id"] = ownerId.ToString();
      dictionary["offset"] = offset.ToString();
      dictionary["count"] = count.ToString();
      dictionary["extended"] = "1";
      dictionary["filter"] = filter;
      string methodName = "wall.get";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<WallData, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<WallData>(methodName, parameters, callback1, (Func<string, WallData>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<WallData>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void Repost(long ownerId, long obj_id, string message, RepostObject obj, long gid, Action<BackendResult<RepostResult, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str = obj.ToString() + ownerId.ToString() + "_" + obj_id.ToString();
      parameters["object"] = str;
      if (!string.IsNullOrEmpty(message))
        parameters["message"] = message;
      if (gid != 0L)
        parameters["group_id"] = gid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<RepostResult>("wall.repost", parameters, (Action<BackendResult<RepostResult, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          EventAggregator.Current.Publish(new RepostedObjectEvent()
          {
            owner_id = ownerId,
            obj_id = obj_id,
            rObj = obj,
            groupId = gid,
            RepostResult = res.ResultData
          });
        callback(res);
      }),  null, false, true, new CancellationToken?(),  null);
    }

    public void DeletePost(long ownerId, long post_id)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["owner_id"] = ownerId.ToString();
      dictionary["post_id"] = post_id.ToString();
      string methodName = "wall.delete";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<object, ResultCode>> action = (Action<BackendResult<object, ResultCode>>) (res => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<object, ResultCode>> callback = (Action<BackendResult<object, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<object>(methodName, parameters, callback, (Func<string, object>) (jsonstr => new object()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void EditComment(WallPostRequestData data, Action<BackendResult<long, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = data.owner_id.ToString();
      parameters["comment_id"] = data.comment_id.ToString();
      parameters["message"] = data.message;
      if (!data.AttachmentIds.IsNullOrEmpty())
        parameters["attachments"] = data.AttachmentIds.GetCommaSeparated(",");
      VKRequestsDispatcher.DispatchRequestToVK<long>("wall.editComment", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void Search(long ownerId, string domain, string query, int count, int offset, Action<BackendResult<WallData, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index1 = "query";
      string str1 = query;
      dictionary[index1] = str1;
      string index2 = "count";
      string str2 = count.ToString();
      dictionary[index2] = str2;
      string index3 = "offset";
      string str3 = offset.ToString();
      dictionary[index3] = str3;
      string index4 = "extended";
      string str4 = "1";
      dictionary[index4] = str4;
      Dictionary<string, string> parameters = dictionary;
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      else if (!string.IsNullOrEmpty(domain))
        parameters["domain"] = domain;
      VKRequestsDispatcher.DispatchRequestToVK<WallData>("wall.search", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SendStats(List<WallPost> viewedWallPosts)
    {
      List<string> ids1 = new List<string>();
      List<string> ids2 = new List<string>();
      foreach (WallPost viewedWallPost in viewedWallPosts)
      {
        List<string> stringList1 = ids1;
        long num = viewedWallPost.to_id;
        string str1 = num.ToString();
        string str2 = "_";
        num = viewedWallPost.id;
        string str3 = num.ToString();
        string str4 = str1 + str2 + str3;
        stringList1.Add(str4);
        if (!viewedWallPost.copy_history.IsNullOrEmpty())
        {
          foreach (WallPost wallPost in viewedWallPost.copy_history)
          {
            List<string> stringList2 = ids2;
            // ISSUE: variable of a boxed type
            long ownerId = wallPost.owner_id;
            string str5 = "_";
            num = wallPost.id;
            string str6 = num.ToString();
            string str7 = ownerId.ToString() + str5 + str6;
            stringList2.Add(str7);
          }
        }
      }
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["post_ids"] = ids1.GetCommaSeparated(",");
      dictionary["repost_ids"] = ids2.GetCommaSeparated(",");
      string methodName = "stats.viewPosts";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ResponseWithId, ResultCode>> action = (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public class ResponseWithPostId
    {
      public WallService.ResponseWithPostId.Post response { get; set; }

      public class Post
      {
        public long post_id { get; set; }
      }
    }

    public class CommentResponse
    {
      public long comment_id { get; set; }
    }

    public class WallRequestData
    {
      public long UserId { get; set; }

      public int Offset { get; set; }

      public int Count { get; set; }
    }
  }
}
