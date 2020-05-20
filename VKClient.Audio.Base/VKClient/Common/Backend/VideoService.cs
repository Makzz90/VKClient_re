using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class VideoService
  {
    private static readonly Func<string, List<Video>> _deserializeFunc = (Func<string, List<Video>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<Video>>>(jsonStr).response.items);
    private static VideoService _instance;

    public static VideoService Instance
    {
      get
      {
        if (VideoService._instance == null)
          VideoService._instance = new VideoService();
        return VideoService._instance;
      }
    }

    private VideoService()
    {
    }

    public void GetAddToAlbumInfo(long targetId, long ownerId, long videoId, Action<BackendResult<GetAddToAlbumInfoResponse, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<GetAddToAlbumInfoResponse>(string.Format("var target_id = {0};\r\n\r\nvar owner_id = {1};\r\n\r\nvar video_id = {2};\r\n\r\nvar groups = API.groups.get({{extended:1, filter:\"moder\", \"fields\":\"can_upload_video\"}});\r\n\r\nvar albums = API.video.getAlbums({{need_system:1, extended:1, owner_id:target_id}});\r\n\r\nvar albumsByVideo = API.video.getAlbumsByVideo({{target_id:target_id, owner_id:owner_id, video_id:video_id}});\r\n\r\nreturn {{ AlbumsByVideo:albumsByVideo, Albums:albums, Groups:groups}};", targetId, ownerId, videoId), callback,  null, false, true, new CancellationToken?());
    }

    public void AddRemoveToAlbums(long videoId, long ownerId, long targetId, List<long> addToAlbumsIds, List<long> removeFromAlbumsIds, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      string format = "API.video.{0}({{video_id:{1},target_id:{2},album_ids:\"{3}\",owner_id:{4}}});";
      string code = "";
      if (!addToAlbumsIds.IsNullOrEmpty())
        code = code + string.Format(format, "addToAlbum", videoId, targetId, addToAlbumsIds.GetCommaSeparated(), ownerId) + Environment.NewLine;
      if (!removeFromAlbumsIds.IsNullOrEmpty())
        code += string.Format(format, "removeFromAlbum", videoId, targetId, removeFromAlbumsIds.GetCommaSeparated(), ownerId);
      VKRequestsDispatcher.Execute<ResponseWithId>(code, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }

    public void HideCatalogSection(int sectionId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["section_id"] = sectionId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("video.hideCatalogSection", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetVideoCatalog(int count, int itemsCount, string from, Action<BackendResult<GetCatalogResponse, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["extended"] = "1";
      dictionary["count"] = count.ToString();
      dictionary["items_count"] = itemsCount.ToString();
      if (!string.IsNullOrEmpty(from))
        dictionary["from"] = from;
      dictionary["fields"] = "members_count,is_member";
      string methodName = "video.getCatalog";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<GetCatalogResponse, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<GetCatalogResponse>(methodName, parameters, callback1, (Func<string, GetCatalogResponse>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<GetCatalogResponse>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetVideoCatalogSection(string categoryId, string fromStr, Action<BackendResult<GetCatalogSectionResponse, ResultCode>> callback)
    {
      if (string.IsNullOrWhiteSpace(fromStr))
      {
        callback(new BackendResult<GetCatalogSectionResponse, ResultCode>(ResultCode.Succeeded, new GetCatalogSectionResponse()));
      }
      else
      {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary["extended"] = "1";
        dictionary["section_id"] = categoryId;
        dictionary["from"] = fromStr ?? "";
        dictionary["count"] = "16";
        dictionary["fields"] = "members_count";
        string methodName = "video.getCatalogSection";
        Dictionary<string, string> parameters = dictionary;
        Action<BackendResult<GetCatalogSectionResponse, ResultCode>> callback1 = callback;
        int num1 = 0;
        int num2 = 1;
        CancellationToken? cancellationToken = new CancellationToken?();
        // ISSUE: variable of the null type
        
        VKRequestsDispatcher.DispatchRequestToVK<GetCatalogSectionResponse>(methodName, parameters, callback1, (Func<string, GetCatalogSectionResponse>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<GetCatalogSectionResponse>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
      }
    }

    public void GetVideoDataExt(long ownerId, Action<BackendResult<GetVideosDataExtResponse, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<GetVideosDataExtResponse>(string.Format("var oid = {0};\r\nvar added = API.video.get({{\"owner_id\":oid, \"extended\":1}});\r\nvar uploaded = API.video.get({{\"owner_id\":oid, \"album_id\":-1, extended:1}});\r\nvar albums = API.video.getAlbums({{\"owner_id\":oid, extended:1}});\r\nvar group;\r\nvar user;\r\nif (oid >0)\r\n{{\r\n   user = API.users.get ({{user_ids:oid, fields:\"photo_max\"}})[0];\r\n}}\r\nif (oid < 0)\r\n{{\r\n   group = API.groups.getById({{group_id:-oid, fields:\"members_count,can_upload_video\"}})[0];\r\n}}\r\n\r\nreturn  {{\"AddedVideos\":added, \"UploadedVideos\":uploaded, \"Albums\":albums, \"User\":user, \"Group\":group}};", ownerId), callback,  null, false, true, new CancellationToken?());
    }

    public void GetVideoData(long ownerId, Action<BackendResult<VideosData, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<VideosData>(string.Format("var oid = {0};\r\nvar added = API.video.get({{\"owner_id\":oid, \"extended\":1}});\r\nvar uploaded = API.video.get({{\"owner_id\":oid, \"album_id\":-1}}).count;\r\nvar albums = API.video.getAlbums({{\"owner_id\":oid}}).count;\r\n\r\nreturn  {{\"AddedVideos\":added, \"UploadedVideosCount\":uploaded, \"VideoAlbumsCount\":albums}};", ownerId), callback, (Func<string, VideosData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "AddedVideos", true);
        return JsonConvert.DeserializeObject<GenericRoot<VideosData>>(jsonStr).response;
      }), false, true, new CancellationToken?());
    }

    public void SearchVideo(string query, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<Video>, ResultCode>> callback)
    {
      if (parameters == null)
        parameters = new Dictionary<string, string>();
      parameters["q"] = query;
      parameters["search_own"] = "1";
      parameters["extended"] = "1";
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      string methodName = "video.search";
      Dictionary<string, string> parameters1 = parameters;
      Action<BackendResult<VKList<Video>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Video>>(methodName, parameters1, callback1, (Func<string, VKList<Video>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<Video>>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void SearchVideo(string query, int offset, int count, Action<BackendResult<VideoSearchResponse, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<VideoSearchResponse>(string.Format("var q = \"{0}\";\r\n                                var offset = {1};\r\n                                var count = {2};\r\n                                var videos = API.video.search({{\"q\": q, \"search_own\": 1, \"offset\": offset, \"count\": count}});\r\n                                var myVideos = [];\r\n                                var globalVideos = [];\r\n                                var i = 0;\r\n                                var items = videos.items;\r\n                                while (i < items.length) {{\r\n                                    var video = items[i];\r\n                                    if (video.owner_id == {3}) {{\r\n                                        myVideos.push(video);\r\n                                    }} else {{\r\n                                        globalVideos.push(video);\r\n                                    }}\r\n                                    i = i + 1;\r\n                                }}\r\n\r\n                                return  {{\"MyVideos\": myVideos, \"GlobalVideos\": globalVideos, \"TotalCount\": videos.count}};", query, offset, count, AppGlobalStateManager.Current.LoggedInUserId), callback, (Func<string, VideoSearchResponse>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "MyVideos", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "GlobalVideos", true);
        return JsonConvert.DeserializeObject<GenericRoot<VideoSearchResponse>>(jsonStr).response;
      }), false, true, new CancellationToken?());
    }

    public void PlayStarted(long videoId, long ownerId)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["video_id"] = videoId.ToString();
      dictionary["owner_id"] = ownerId.ToString();
      string methodName = "video.playStarted";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<object, ResultCode>> action = (Action<BackendResult<object, ResultCode>>) (res => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<object, ResultCode>> callback = (Action<BackendResult<object, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<object>(methodName, parameters, callback, (Func<string, object>) (jsonStr => new object()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void IncViewCounter(long videoId, long ownerId)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["video_id"] = videoId.ToString();
      dictionary["owner_id"] = ownerId.ToString();
      string methodName = "video.incViewCounter";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<object, ResultCode>> action = (Action<BackendResult<object, ResultCode>>) (res => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<object, ResultCode>> callback = (Action<BackendResult<object, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<object>(methodName, parameters, callback, (Func<string, object>) (jsonStr => new object()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void AddAlbum(string albumName, PrivacyInfo albumPrivacy, Action<BackendResult<VideoAlbum, ResultCode>> callback, long? groupId = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["title"] = albumName;
      if (groupId.HasValue && groupId.Value != 0L)
        parameters["group_id"] = groupId.Value.ToString();
      if (albumPrivacy != null)
        parameters["privacy"] = albumPrivacy.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VideoAlbum>("video.addAlbum", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void EditAlbum(string albumName, long albumId, PrivacyInfo albumPrivacy, Action<BackendResult<object, ResultCode>> callback, long? groupId = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["title"] = albumName;
      parameters["album_id"] = albumId.ToString();
      if (groupId.HasValue && groupId.Value != 0L)
        parameters["group_id"] = groupId.Value.ToString();
      if (albumPrivacy != null)
        parameters["privacy"] = albumPrivacy.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<object>("video.editAlbum", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void DeleteAlbum(long albumId, Action<BackendResult<object, ResultCode>> callback, long? groupId = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["album_id"] = albumId.ToString();
      if (groupId.HasValue && groupId.Value > 0L)
        parameters["group_id"] = groupId.Value.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<object>("video.deleteAlbum", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void MoveVideoToAlbum(List<long> videoIds, long albumId, Action<BackendResult<object, ResultCode>> callback, long? groupId = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["vids"] = videoIds.GetCommaSeparated();
      parameters["album_id"] = albumId.ToString();
      if (groupId.HasValue)
        parameters["group_id"] = groupId.Value.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<object>("video.moveToAlbum", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void DeleteVideo(long videoId, long ownerId, Action<BackendResult<object, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["video_id"] = videoId.ToString();
      parameters["owner_id"] = ownerId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<object>("video.delete", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetAlbums(long userOrGroupId, bool isGroup, bool need_system, int offset, int count, Action<BackendResult<VKList<VideoAlbum>, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      if (isGroup)
        dictionary["group_id"] = userOrGroupId.ToString();
      else
        dictionary["user_id"] = userOrGroupId.ToString();
      if (need_system)
        dictionary["need_system"] = "1";
      dictionary["extended"] = "1";
      dictionary["offset"] = offset.ToString();
      dictionary["count"] = count.ToString();
      string methodName = "video.getAlbums";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<VKList<VideoAlbum>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<VKList<VideoAlbum>>(methodName, parameters, callback1, (Func<string, VKList<VideoAlbum>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<VideoAlbum>>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetVideos(long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<VKList<Video>, ResultCode>> callback, long albumId = 0, bool extended = true)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      if (isGroup)
        dictionary["group_id"] = userOrGroupId.ToString();
      else
        dictionary["user_id"] = userOrGroupId.ToString();
      if (albumId != 0L)
        dictionary["album_id"] = albumId.ToString();
      dictionary["offset"] = offset.ToString();
      dictionary["count"] = count.ToString();
      dictionary["extended"] = extended ? "1" : "0";
      string methodName = "video.get";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<VKList<Video>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Video>>(methodName, parameters, callback1, (Func<string, VKList<Video>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<Video>>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetVideoById(long oid, long vid, string accessKey, Action<BackendResult<List<Video>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["videos"] = oid.ToString() + "_" + vid.ToString();
      parameters["extended"] = "1";
      if (!string.IsNullOrEmpty(accessKey))
        parameters["access_key"] = accessKey;
      parameters["width"] = "320";
      VKRequestsDispatcher.DispatchRequestToVK<List<Video>>("video.get", parameters, callback, VideoService._deserializeFunc, false, true, new CancellationToken?(),  null);
    }

    private string FormatVideosList(List<KeyValuePair<long, long>> videos)
    {
      string empty = string.Empty;
      foreach (KeyValuePair<long, long> video in videos)
      {
        empty += string.Format("{0}_{1}", video.Key, video.Value);
        if (videos.IndexOf(video) != videos.Count - 1)
          empty += ",";
      }
      return empty;
    }

    public void AddRemovedToFromAlbum(bool add, long targetId, long album_id, long owner_id, long video_id, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["target_id"] = targetId.ToString();
      parameters["album_id"] = album_id.ToString();
      parameters["owner_id"] = owner_id.ToString();
      parameters["video_id"] = video_id.ToString();
      string methodName = add ? "video.addToAlbum" : "video.removeFromAlbum";
      if (add && album_id == VideoAlbum.ADDED_ALBUM_ID)
        methodName = "video.add";
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          EventAggregator current = EventAggregator.Current;
          VideoAddedDeleted videoAddedDeleted = new VideoAddedDeleted();
          int num1 = add ? 1 : 0;
          videoAddedDeleted.IsAdded = num1 != 0;
          long num2 = video_id;
          videoAddedDeleted.VideoId = num2;
          long num3 = owner_id;
          videoAddedDeleted.OwnerId = num3;
          long num4 = targetId;
          videoAddedDeleted.TargetId = num4;
          long num5 = album_id;
          videoAddedDeleted.AlbumId = num5;
          current.Publish(videoAddedDeleted);
        }
        callback(res);
      }), (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetComments(long ownerId, long vid, int knownCommentsCount, int offset, int count, StatisticsActionSource actionSource, string context, Action<BackendResult<VideoLikesCommentsData, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<VideoLikesCommentsData>(string.Format("\r\nvar ownerId={0};\r\nvar vid  = {1};\r\nvar countToRead = {4};\r\nvar vidStr= ownerId + \"_\" + vid;\r\n\r\nvar knownCommentsCount = {2};\r\nvar offset = {3};\r\n\r\nvar albums = API.video.getAlbumsByVideo({{\"owner_id\":ownerId, \"video_id\":vid}});\r\n\r\nvar likesAll = API.likes.getList({{ \"type\": \"video\", \"owner_id\":ownerId, \"item_id\":vid, \"count\":10 }});\r\n\r\nvar repostsCount = API.likes.getList({{ \"type\": \"video\", \"owner_id\":ownerId, \"item_id\":vid, \"filter\": \"copies\"}}).count;\r\n\r\nvar comments;\r\n\r\nif (knownCommentsCount == -1)\r\n{{\r\n   comments = API.video.getComments({{\"video_id\":vid, \"owner_id\":ownerId, \"offset\":0, \"count\":countToRead, \"sort\":\"desc\", \"need_likes\":1, \"allow_group_comments\":1 }});\r\n}}\r\n\r\nelse\r\n{{\r\n   var calculatedOffset = knownCommentsCount - offset - countToRead;\r\n   if (calculatedOffset < 0)\r\n{{\r\n    calculatedOffset = 0;\r\n}}\r\n   comments = API.video.getComments({{\"video_id\":vid, \"owner_id\":ownerId, \"offset\":calculatedOffset, \"count\":countToRead, \"sort\":\"asc\", \"need_likes\":1, \"allow_group_comments\":1}});\r\n\r\n}}\r\n\r\nvar users2 = API.getProfiles({{ \"user_ids\":comments.items@.reply_to_user, \"fields\":\"first_name_dat,last_name_dat\"}});\r\n\r\n\r\nvar likesAllIds = likesAll.items;\r\nvar likesAllCount = likesAll.count;\r\n\r\nvar userLiked = API.likes.isLiked({{\"owner_id\":ownerId, \"type\":\"video\", \"item_id\":vid}});\r\n\r\nvar tags = API.video.getTags({{\"owner_id\":ownerId, \"video_id\":vid}});\r\n\r\n\r\nvar userOrGroupIds = likesAllIds;\r\n\r\n\r\n\r\nuserOrGroupIds = userOrGroupIds + comments.items@.from_id;\r\n\r\nvar userIds = [];\r\nvar groupIds = [];\r\n\r\nvar i = 0;\r\n\r\nif (ownerId < 0)\r\n{{\r\n     var negOwner = -ownerId;\r\n     groupIds = groupIds + negOwner;\r\n}}\r\nelse\r\n{{\r\n    userIds = userIds + ownerId;\r\n}}\r\n\r\nvar length = userOrGroupIds.length;\r\n\r\nwhile (i < length)\r\n{{\r\n    var id = parseInt(userOrGroupIds[i]);\r\n    \r\n    if (id > 0)\r\n    {{\r\n       if (userIds.length > 0)\r\n       {{\r\n          userIds = userIds + \",\";\r\n       }}\r\n       userIds = userIds + id;\r\n    }}\r\n    else if (id < 0)\r\n    {{\r\n        id = -id;\r\n        if (groupIds.length > 0)\r\n        {{\r\n            groupIds = groupIds + \",\";\r\n        }}\r\n        groupIds = groupIds + id;\r\n    }}\r\n     \r\n    i = i + 1;\r\n}}\r\n\r\nvar users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile,friend_status\" }});\r\nvar users3 =API.users.get({{\"user_ids\":userIds, \"fields\":\"first_name_dat,last_name_dat,friend_status\" }}); \r\nvar groups = API.groups.getById({{\"group_ids\":groupIds, \"fields\":\"members_count,photo_100\"}});\r\n\r\n\r\nvar videoRecommendations = API.video.getRecommendations({{count: 8, extended: 1, owner_id: ownerId, video_id: vid, source: \"{5}\", context: \"{6}\"}});\r\n\r\n// for test\r\n//var videoRecomList = API.video.get({{count: 8, extended: 1}});\r\n//var videoRecom = {{\r\n//items: videoRecomList.items,\r\n//count: videoRecomList.count,\r\n//context: \"-1324220032\"\r\n//}};\r\n\r\nreturn {{\"comments\":comments, \"LikesAllIds\":likesAllIds,  \"LikesAllCount\":likesAllCount, \"userLiked\":userLiked, \"Users\":users,\"Users2\":users2, \"Users3\": users3, \"Groups\":groups, \"Tags\":tags, \"RepostsCount\":repostsCount, \"Albums\":albums, \"VideoRecommendations\": videoRecommendations}};\r\n", ownerId, vid, knownCommentsCount, offset, count, actionSource, context), callback, (Func<string, VideoLikesCommentsData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users2", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users3", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "comments", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Tags", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "LikesAllIds", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Albums", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "VideoRecommendations", true);
        VideoLikesCommentsData response = JsonConvert.DeserializeObject<GenericRoot<VideoLikesCommentsData>>(jsonStr).response;
        GroupsService.Current.AddCachedGroups((IEnumerable<Group>) response.Groups);
        if (knownCommentsCount < 0)
          response.Comments.Reverse();
        response.Users2.AddRange((IEnumerable<User>) response.Users3);
        return response;
      }), false, true, new CancellationToken?());
    }

    public void CreateComment(long ownerId, long vid, string text, long replyCid, List<string> attachments, Action<BackendResult<Comment, ResultCode>> callback, int stickerId = 0, string stickerReferrer = "")
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("\r\n\r\nvar new_comment_id = API.video.createComment({{\r\n    owner_id: {0},\r\n    video_id: {1},\r\n    message: \"{2}\",\r\n    sticker_id: {3},\r\n    reply_to_comment: {4},\r\n    attachments: \"{5}\",\r\n    sticker_referrer: \"{6}\"\r\n}});\r\n\r\nvar last_comments = API.video.getComments({{\r\n    owner_id: {7},\r\n    video_id: {8},\r\n    need_likes: 1,\r\n    count: 10,\r\n    sort: \"desc\",\r\n    preview_length: 0,\r\n    allow_group_comments: 1\r\n}}).items;\r\n\r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r\n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r\n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", ownerId, vid, text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), stickerId, replyCid, attachments.GetCommaSeparated(","), stickerReferrer, ownerId, vid)
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

    public void DeleteComment(long ownerId, long vid, long cid, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["video_id"] = vid.ToString();
      parameters["comment_id"] = cid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("video.deleteComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void EditComment(long cid, string text, long ownerId, List<string> attachments, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["comment_id"] = cid.ToString();
      parameters["message"] = text;
      parameters["owner_id"] = ownerId.ToString();
      if (!attachments.IsNullOrEmpty())
        parameters["attachments"] = attachments.GetCommaSeparated(",");
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("video.editComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void EditVideo(long videoId, long ownerId, string name, string description, PrivacyInfo privacyView, PrivacyInfo privacyComment, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      parameters["video_id"] = videoId.ToString();
      parameters["name"] = name;
      parameters["desc"] = description;
      parameters["privacy_view"] = privacyView.ToString();
      parameters["privacy_comment"] = privacyComment.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("video.edit", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void Delete(long ownerId, long vid, Action<BackendResult<long, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["video_id"] = vid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<long>("video.delete", parameters, (Action<BackendResult<long, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          EventAggregator current = EventAggregator.Current;
          VideoAddedDeleted videoAddedDeleted = new VideoAddedDeleted();
          videoAddedDeleted.IsAdded = false;
          long num1 = vid;
          videoAddedDeleted.VideoId = num1;
          long num2 = ownerId;
          videoAddedDeleted.OwnerId = num2;
          int num3 = 1;
          videoAddedDeleted.IsDeletedPermanently = num3 != 0;
          current.Publish(videoAddedDeleted);
        }
        callback(res);
      }),  null, false, true, new CancellationToken?(),  null);
    }

    public void Report(long ownerId, long id, ReportReason reportReason, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["video_id"] = id.ToString();
      parameters["reason"] = ((int) reportReason).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("video.report", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void ReportComment(long ownerId, long commentId, ReportReason reportReason, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["comment_id"] = commentId.ToString();
      parameters["reason"] = ((int) reportReason).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("video.reportComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void UploadVideo(Stream stream, bool isPrivate, long albumId, long groupId, string name, string description, Action<BackendResult<SaveVideoResponse, ResultCode>> callback, Action<double> progressCallback = null, Cancellation c = null, PrivacyInfo privacyViewInfo = null, PrivacyInfo privacyCommentInfo = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["is_private"] = isPrivate ? "1" : "0";
      if (groupId != 0L)
        parameters["group_id"] = groupId.ToString();
      if (albumId != 0L)
        parameters["album_id"] = albumId.ToString();
      if (!string.IsNullOrEmpty(name))
        parameters["name"] = name;
      if (!string.IsNullOrEmpty(description))
        parameters["description"] = description;
      if (privacyViewInfo != null && groupId == 0L)
        parameters["privacy_view"] = privacyViewInfo.ToString();
      if (privacyCommentInfo != null && groupId == 0L)
        parameters["privacy_comment"] = privacyCommentInfo.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<SaveVideoResponse>("video.save", parameters, (Action<BackendResult<SaveVideoResponse, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
        {
          callback(new BackendResult<SaveVideoResponse, ResultCode>(res.ResultCode));
        }
        else
        {
          SaveVideoResponse svr = res.ResultData;
          JsonWebRequest.Upload(svr.upload_url, stream, "video_file", "video", (Action<JsonResponseData>) (uploadRes =>
          {
            if (uploadRes.IsSucceeded)
              callback(new BackendResult<SaveVideoResponse, ResultCode>(ResultCode.Succeeded, svr));
            else
              callback(new BackendResult<SaveVideoResponse, ResultCode>(ResultCode.UnknownError));
          }),  null, progressCallback, c);
        }
      }),  null, false, true, new CancellationToken?(),  null);
    }
  }
}
