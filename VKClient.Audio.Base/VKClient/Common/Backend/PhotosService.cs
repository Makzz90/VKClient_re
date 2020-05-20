using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class PhotosService
  {
    private static PhotosService _current;

    public static PhotosService Current
    {
      get
      {
        if (PhotosService._current == null)
          PhotosService._current = new PhotosService();
        return PhotosService._current;
      }
    }

    public void GetAlbums(long userOrGroupId, bool isGroup, long album_id, Action<BackendResult<VKList<Album>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = isGroup ? (-userOrGroupId).ToString() : userOrGroupId.ToString();
      parameters["album_ids"] = album_id.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Album>>("photos.getAlbums", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetUsersAlbums(long userOrGroupid, bool isGroup, int offset, int count, Action<BackendResult<AlbumsData, ResultCode>> callback, bool needGroupSystemAlbums = true)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str;
      if (!isGroup)
        str = string.Format("var allPhotos= API.photos.getAll({{extended:\"1\", owner_id:\"{0}\", count:\"3\", offset:\"0\"}});\r\nvar profilePhotos = API.photos.get({{owner_id:\"{0}\", album_id:\"profile\", offset:\"0\", count:\"3\", extended:\"1\", rev:\"1\"}});\r\nvar userPhotos = API.photos.getUserPhotos({{extended:\"1\", user_id:\"{0}\", count:\"3\", offset:\"0\", sort:\"0\"}});\r\nvar wallPhotos = API.photos.get({{owner_id:\"{0}\", album_id:\"wall\", offset:\"0\", count:\"3\", extended:\"1\", rev:\"1\"}});\r\nvar savedPhotos = API.photos.get({{owner_id:\"{0}\", album_id:\"saved\", offset:\"0\", count:\"3\", extended:\"1\", rev:\"1\"}});\r\nvar albumPhotos = API.photos.getAlbums({{need_covers:\"1\", offset:\"{1}\", count:\"{2}\", owner_id:\"{0}\"}}); \r\n\r\nvar full_gen = API.users.get({{\"user_ids\":\"{0}\", \"name_case\":\"gen\"}})[0];\r\n\r\nvar full_ins = API.users.get({{\"user_ids\":\"{0}\", \"name_case\":\"ins\"}})[0];\r\n\r\nvar owners = albumPhotos.items@.owner_id;\r\nvar thumbIds = albumPhotos.items@.thumb_id;\r\n\r\nvar ownersPlusThumbs = [];\r\nvar i=albumPhotos.items.length-1;\r\n\r\nwhile(i != -1)\r\n{{\r\n  var s = owners[i] + \"_\" + thumbIds[i];\r\n  ownersPlusThumbs.push(s);\r\n  i = i -1;\r\n}};\r\n\r\nvar p = [];\r\nif (albumPhotos.items.length >0)\r\n{{\r\n  p= API.photos.getById({{photos:ownersPlusThumbs}});\r\n}};\r\n\r\nreturn {{\"AllPhotos\":allPhotos, \"ProfilePhotos\":profilePhotos, \"UserPhotos\":userPhotos, \r\n\"WallPhotos\":wallPhotos, \"SavedPhotos\":savedPhotos, \"Albums\":albumPhotos, \"covers\":p,\r\n\"userGen\":full_gen, \"userIns\":full_ins}};", userOrGroupid, offset, count);
      else
        str = string.Format("var allPhotos= API.photos.getAll({{extended:\"1\", owner_id:\"{0}\", count:\"3\", offset:\"0\"}});\r\nvar albumPhotos = API.photos.getAlbums({{need_covers:\"1\", offset:\"{2}\", count:\"{3}\", gid:\"{1}\", need_system:\"{4}\"}}); \r\n\r\nvar owners = albumPhotos.items@.owner_id;\r\nvar thumbIds = albumPhotos.items@.thumb_id;\r\n\r\nvar ownersPlusThumbs = [];\r\nvar i=albumPhotos.items.length-1;\r\n\r\nwhile(i != -1)\r\n{{\r\n  var s = owners[i] + \"_\" + thumbIds[i];\r\n  ownersPlusThumbs.push(s);\r\n  i = i -1;\r\n}};\r\n\r\nvar p = [];\r\nif (albumPhotos.items.length >0)\r\n{{\r\n  p= API.photos.getById({{photos:ownersPlusThumbs}});\r\n}};\r\n\r\nreturn {{\"AllPhotos\":allPhotos,  \"Albums\":albumPhotos, \"covers\":p }};", -userOrGroupid, userOrGroupid, offset, count, (needGroupSystemAlbums ? "1" : "0"));
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<AlbumsData>("execute", parameters, callback, (Func<string, AlbumsData>) (jsonStr =>
      {
        AlbumsData albumsData = new AlbumsData();
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "AllPhotos", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "ProfilePhotos", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "UserPhotos", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "WallPhotos", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "SavedPhotos", true);
        jsonStr = jsonStr.Replace("\"covers\":false", "\"covers\":[]");
        AlbumsData response = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<AlbumsData>>(jsonStr).response;
        this.UpdateThumbSrc(response.albums, response.covers);
        if (offset != 0)
        {
          response.AllPhotos.items.Clear();
          response.ProfilePhotos.items.Clear();
          response.SavedPhotos.items.Clear();
          response.UserPhotos.items.Clear();
          response.WallPhotos.items.Clear();
        }
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    private void UpdateThumbSrc(List<Album> albums, List<Photo> covers)
    {
      if (albums == null || covers == null)
        return;
      foreach (Album album1 in albums)
      {
        Album album = album1;
        Photo photo = covers.FirstOrDefault<Photo>((Func<Photo, bool>) (c => c.pid.ToString() == album.thumb_id));
        if (photo != null)
        {
          album.thumb_src = string.IsNullOrWhiteSpace(photo.photo_807) ? photo.photo_604 : photo.photo_807;
          album.thumb_src_small = photo.photo_130;
        }
      }
    }

    public void GetAllPhotos(long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      this.GetPhotosImpl("photos.getAll", userOrGroupId, isGroup, offset, count, callback);
    }

    public void GetUserPhotos(long userId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      this.GetPhotosImpl("photos.getUserPhotos", userId, false, offset, count, callback);
    }

    public void GetWallPhotos(long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      this.GetAlbumPhotosImpl(userOrGroupId, isGroup, "wall", offset, count, callback);
    }

    public void GetSavedPhotos(long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      this.GetAlbumPhotosImpl(userOrGroupId, isGroup, "saved", offset, count, callback);
    }

    public void GetProfilePhotos(long userId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      this.GetAlbumPhotosImpl(userId, false, "profile", offset, count, callback);
    }

    public Task<BackendResult<PhotosListWithCount, ResultCode>> GetProfilePhotos(long userId, int offset, int count)
    {
      return this.GetAlbumPhotosImpl(userId, false, "profile", offset, count);
    }

    public void GetAlbumPhotos(long userOrGroupId, bool isGroup, string albumId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      this.GetAlbumPhotosImpl(userOrGroupId, isGroup, albumId.ToString(), offset, count, callback);
    }

    private Task<BackendResult<PhotosListWithCount, ResultCode>> GetAlbumPhotosImpl(long userOrGroupId, bool isGroup, string albumId, int offset, int count)
    {
      TaskCompletionSource<BackendResult<PhotosListWithCount, ResultCode>> tcs = new TaskCompletionSource<BackendResult<PhotosListWithCount, ResultCode>>();
      this.GetAlbumPhotosImpl(userOrGroupId, isGroup, albumId, offset, count, (Action<BackendResult<PhotosListWithCount, ResultCode>>) (res => tcs.TrySetResult(res)));
      return tcs.Task;
    }

    private void GetAlbumPhotosImpl(long userOrGroupId, bool isGroup, string albumId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = isGroup ? (-userOrGroupId).ToString() : userOrGroupId.ToString();
      parameters["album_id"] = albumId;
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["extended"] = 1.ToString();
      parameters["rev"] = 1.ToString();
      long result;
      if (long.TryParse(albumId, out result))
        VKRequestsDispatcher.Execute<PhotosListWithCount>(string.Format("var photos = API.photos.get({{\"owner_id\":{0}, \"album_id\":{1}, \"offset\":{2}, \"count\":{3}, \"extended\":1, \"rev\":1}});\r\n                      var albums  = API.photos.getAlbums({{\"owner_id\":{0}, \"album_ids\":{1}}});\r\n                      var thumbId = albums.items@.thumb_id[0];\r\n                      var ownerPlusThumb = {0} + \"_\" + thumbId;\r\n                      var p= API.photos.getById({{\"photos\":ownerPlusThumb}});\r\n                      return {{\"Album\": albums.items[0], \"Photos\":photos, \"Thumb\": p[0]}};", parameters["owner_id"], result, offset, count), callback, (Func<string, PhotosListWithCount>) (jsonStr =>
        {
          PhotosService.AlbumAndPhotosData response = JsonConvert.DeserializeObject<GenericRoot<PhotosService.AlbumAndPhotosData>>(jsonStr).response;
          PhotosListWithCount photosListWithCount = new PhotosListWithCount()
          {
            album = response.Album,
            photosCount = response.Photos.count,
            response = response.Photos.items
          };
          if (photosListWithCount.album != null && response.Thumb != null)
            this.UpdateThumbSrc(new List<Album>()
            {
              photosListWithCount.album
            }, new List<Photo>() { response.Thumb });
          return photosListWithCount;
        }), false, true, new CancellationToken?());
      else
        VKRequestsDispatcher.DispatchRequestToVK<PhotosListWithCount>("photos.get", parameters, callback, (Func<string, PhotosListWithCount>) (jsonStr =>
        {
          VKList<Photo> response = JsonConvert.DeserializeObject<GenericRoot<VKList<Photo>>>(jsonStr).response;
          return new PhotosListWithCount()
          {
            photosCount = response.count,
            response = response.items
          };
        }), false, true, new CancellationToken?(),  null);
    }

    private void GetPhotosImpl(string methodName, long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = isGroup ? (-userOrGroupId).ToString() : userOrGroupId.ToString();
      parameters["user_id"] = parameters["owner_id"];
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["extended"] = 1.ToString();
      parameters["sort"] = 0.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<PhotosListWithCount>(methodName, parameters, callback, (Func<string, PhotosListWithCount>) (jsonStr =>
      {
        VKList<Photo> response = JsonConvert.DeserializeObject<GenericRoot<VKList<Photo>>>(jsonStr).response;
        return new PhotosListWithCount()
        {
          photosCount = response.count,
          response = response.items
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void CreateAlbum(Album album, Action<BackendResult<Album, ResultCode>> callback, long gid = 0)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["title"] = album.title;
      parameters["privacy_view"] = album.PrivacyViewInfo.ToString();
      parameters["description"] = album.description;
      if (gid != 0L)
        parameters["group_id"] = gid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<Album>("photos.createAlbum", parameters, callback, (Func<string, Album>) (jsonStr =>
      {
        GenericRoot<Album> genericRoot = JsonConvert.DeserializeObject<GenericRoot<Album>>(jsonStr);
        genericRoot.response.description = album.description;
        return genericRoot.response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void EditAlbum(Album album, Action<BackendResult<ResponseWithId, ResultCode>> callback, long gid = 0)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["title"] = album.title;
      parameters["privacy_view"] = album.PrivacyViewInfo.ToString();
      parameters["description"] = album.description;
      parameters["album_id"] = album.aid;
      if (gid != 0L)
        parameters["owner_id"] = (-gid).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.editAlbum", parameters, callback, (Func<string, ResponseWithId>) (jsonstr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void DeleteAlbum(string aid, Action<BackendResult<ResponseWithId, ResultCode>> callback, long gid = 0)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["album_id"] = aid;
      if (gid != 0L)
        parameters["group_id"] = gid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.deleteAlbum", parameters, callback, (Func<string, ResponseWithId>) (jsonstr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void UploadPhotoToAlbum(string aid, long optionalGroupId, byte[] photoData, Action<BackendResult<Photo, ResultCode>> callback)
    {
      this.GetPhotoUploadServerAlbum(aid, optionalGroupId, (Action<BackendResult<UploadServerAddress, ResultCode>>) (getUplResp =>
      {
        if (getUplResp.ResultCode != ResultCode.Succeeded)
        {
          callback(new BackendResult<Photo, ResultCode>(getUplResp.ResultCode));
        }
        else
        {
          string uploadUrl = getUplResp.ResultData.upload_url;
          MemoryStream ms = new MemoryStream(photoData);
          MemoryStream memoryStream = ms;
          string paramName = "file1";
          string uploadContentType = "image";
          Action<JsonResponseData> resultCallback = (Action<JsonResponseData>) (jsonResult =>
          {
            ms.Close();
            if (!jsonResult.IsSucceeded)
            {
              callback(new BackendResult<Photo, ResultCode>(ResultCode.UnknownError));
            }
            else
            {
              UploadPhotoResponseData uploadData = JsonConvert.DeserializeObject<UploadPhotoResponseData>(jsonResult.JsonString);
              this.SavePhoto(uploadData.aid, optionalGroupId, uploadData, callback);
            }
          });
          string fileName = "MyImage.jpg";
          // ISSUE: variable of the null type
          // ISSUE: variable of the null type
          JsonWebRequest.Upload(uploadUrl, (Stream)memoryStream, paramName, uploadContentType, resultCallback, fileName, null, null);
        }
      }));
    }

    public void UploadPhotoToWall(long userOrGroupId, bool isGroup, byte[] photoData, Action<BackendResult<Photo, ResultCode>> callback, Action<double> progressCallback = null, Cancellation c = null)
    {
      this.GetPhotoUploadServerWall(userOrGroupId, isGroup, (Action<BackendResult<UploadServerAddress, ResultCode>>) (getUplResp =>
      {
        if (getUplResp.ResultCode != ResultCode.Succeeded)
          callback(new BackendResult<Photo, ResultCode>(getUplResp.ResultCode));
        else
          JsonWebRequest.Upload(getUplResp.ResultData.upload_url, (Stream) new MemoryStream(photoData), "file1", "image", (Action<JsonResponseData>) (jsonResult =>
          {
            if (!jsonResult.IsSucceeded)
              callback(new BackendResult<Photo, ResultCode>(ResultCode.UnknownError));
            else
              this.SaveWallPhoto(userOrGroupId, isGroup, JsonConvert.DeserializeObject<UploadPhotoResponseData>(jsonResult.JsonString), callback);
          }), "MyImage.jpg", progressCallback, c);
      }));
    }

    private void GetPhotoUploadServerWall(long userOrGroupId, bool isGroup, Action<BackendResult<UploadServerAddress, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (userOrGroupId != 0L)
      {
        string index = isGroup ? "group_id" : "user_id";
        parameters[index] = userOrGroupId.ToString();
      }
      VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getWallUploadServer", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    private void GetPhotoUploadServerAlbum(string aid, long optionalGroupId, Action<BackendResult<UploadServerAddress, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["album_id"] = aid;
      if (optionalGroupId != 0L)
        parameters["group_id"] = optionalGroupId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<UploadServerAddress>("photos.getUploadServer", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SavePhoto(string aid, long optionalGroupId, UploadPhotoResponseData uploadData, Action<BackendResult<Photo, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["server"] = uploadData.server;
      dictionary["photos_list"] = uploadData.photos_list;
      dictionary["hash"] = uploadData.hash;
      dictionary["album_id"] = aid;
      if (optionalGroupId != 0L)
        dictionary["group_id"] = optionalGroupId.ToString();
      string methodName = "photos.save";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<Photo, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Photo>(methodName, parameters, callback1, (Func<string, Photo>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<List<Photo>>>(jsonStr).response.First<Photo>()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    private void SaveWallPhoto(long userOrGroupId, bool isGroup, UploadPhotoResponseData uploadData, Action<BackendResult<Photo, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["server"] = uploadData.server;
      dictionary["photo"] = uploadData.photo;
      dictionary["hash"] = uploadData.hash;
      if (userOrGroupId != 0L)
      {
        string index = isGroup ? "group_id" : "user_id";
        dictionary[index] = userOrGroupId.ToString();
      }
      string methodName = "photos.saveWallPhoto";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<Photo, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Photo>(methodName, parameters, callback1, (Func<string, Photo>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<List<Photo>>>(jsonStr).response.First<Photo>()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void DeletePhoto(long pid, long ownerId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["photo_id"] = pid.ToString();
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.delete", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void MakeCover(string aid, long pid, long ownerId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["photo_id"] = pid.ToString();
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      parameters["album_id"] = aid;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.makeCover", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetPhotoWithFullInfo(long ownerId, long pid, string accessKey, int knownCommentsCount, int offset, int commentsCountToRead, Action<BackendResult<PhotoWithFullInfo, ResultCode>> callback)
    {
      string str1 = ownerId.ToString() + "_" + pid;
      if (!string.IsNullOrWhiteSpace(accessKey))
        str1 = str1 + "_" + accessKey;
      string str2 = string.Format("var ownerId = {0};\r\n\r\nvar pid  = {1};\r\n\r\n\r\nvar commentsCount = {2};\r\n\r\nvar offset = {3};\r\n\r\nvar countToRead = {4};\r\n\r\nvar likesAll = API.likes.getList({{ \"type\": \"photo\", \"owner_id\":ownerId, \"item_id\":pid, \"count\":10}}).items;\r\n\r\nvar photo = API.photos.getById({{\"photos\" : \"{6}\", \"extended\":1 }});\r\n\r\nif (commentsCount == -1)\r\n{{\r\n   commentsCount = photo[0].comments.count;   \r\n}}\r\n\r\nvar calculatedOffset = commentsCount - offset - countToRead;\r\n\r\nif (calculatedOffset < 0)\r\n{{\r\n   calculatedOffset = 0;\r\n}}\r\nvar comments = API.photos.getComments({{ \"photo_id\" : pid, \"owner_id\": ownerId, \"offset\":calculatedOffset,  \"count\":countToRead, \"sort\":\"asc\", \"need_likes\":1, \"access_key\":\"{5}\", \"allow_group_comments\":1 }});\r\n\r\n\r\nvar users2 = API.users.get({{ \"user_ids\":comments.items@.reply_to_user, \"fields\":\"first_name_dat,last_name_dat\"}});\r\n\r\n\r\n\r\nvar photoTags = API.photos.getTags({{\"owner_id\":ownerId, \"photo_id\":pid, \"access_key\":\"{5}\"}});\r\n\r\nvar userOrGroupIds = [];\r\n\r\nvar repostsCount = 0;\r\nif(likesAll+\"\"!=\"\")\r\n{{\r\n  repostsCount =  API.likes.getList({{ \"type\": \"photo\", \"owner_id\":ownerId, \"item_id\":pid, \"filter\":\"copies\"}}).count;\r\n  userOrGroupIds = likesAll;\r\n}}\r\n\r\nif (commentsCount>0)\r\n{{\r\n    userOrGroupIds = userOrGroupIds + comments.items@.from_id;\r\n}}\r\n\r\nvar userIds = [];\r\nvar groupIds = [];\r\n\r\nvar i = 0;\r\n\r\nif (ownerId < 0)\r\n{{\r\n     var negOwner = -ownerId;\r\n     groupIds = groupIds + negOwner;\r\n\r\n     if (photo[0].user_id != 0 && photo[0].user_id != 100)\r\n     {{\r\n         userIds = userIds + photo[0].user_id;\r\n     }}\r\n}}\r\nelse\r\n{{\r\n    userIds = userIds + ownerId;\r\n}}\r\n\r\nvar length = userOrGroupIds.length;\r\n\r\nwhile (i < length)\r\n{{\r\n    var id = parseInt(userOrGroupIds[i]);\r\n    \r\n    if (id > 0)\r\n    {{\r\n       if (userIds.length > 0)\r\n       {{\r\n          userIds = userIds + \",\";\r\n       }}\r\n       userIds = userIds + id;\r\n    }}\r\n    else if (id < 0)\r\n    {{\r\n        id = -id;\r\n        if (groupIds.length > 0)\r\n        {{\r\n            groupIds = groupIds + \",\";\r\n        }}\r\n        groupIds = groupIds + id;\r\n    }}\r\n     \r\n    i = i + 1;\r\n}}\r\n\r\nvar users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile\" }});\r\nvar users3 =API.users.get({{\"user_ids\":userIds, \"fields\":\"first_name_dat,last_name_dat\" }}); \r\nvar groups = API.groups.getById({{\"group_ids\":groupIds}});\r\n\r\n\r\n\r\nreturn {{\"Photo\":photo[0], \"comments\": comments, \"LikesAllIds\":likesAll, \"Users\":users, \r\n\"Groups\":groups, \"Users2\":users2, \"Users3\": users3, \"PhotoTags\": photoTags, \"RepostsCount\":repostsCount}};", ownerId, pid, knownCommentsCount, offset, commentsCountToRead, accessKey, str1);
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str2;
      VKRequestsDispatcher.DispatchRequestToVK<PhotoWithFullInfo>("execute", parameters, callback, (Func<string, PhotoWithFullInfo>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "comments", true);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users2", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Users3", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "Groups", false);
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "PhotoTags", false);
        PhotoWithFullInfo response = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<PhotoWithFullInfo>>(jsonStr).response;
        response.Users2.AddRange((IEnumerable<User>) response.Users3);
        GroupsService.Current.AddCachedGroups((IEnumerable<Group>) response.Groups);
        foreach (Comment comment in response.Comments)
        {
          string message = comment.message;
          comment.text = message;
        }
        if (response.LikesAllIds == null)
          response.LikesAllIds = new List<long>();
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void CreateComment(long ownerId, long pid, long replyCid, string message, bool fromGroup, List<string> attachmentIds, Action<BackendResult<Comment, ResultCode>> callback, string accessKey = "", int sticker_id = 0, string stickerReferrer = "")
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("\r\n\r\nvar new_comment_id = API.photos.createComment({{\r\n    owner_id: {0},\r\n    photo_id: {1},\r\n    message: \"{2}\",\r\n    from_group: {3},\r\n    sticker_id: {4},\r\n    reply_to_comment: {5},\r\n    attachments: \"{6}\",\r\n    access_key: \"{7}\",\r\n    sticker_referrer: \"{8}\"\r\n}});\r\n\r\nvar last_comments = API.photos.getComments({{\r\n    owner_id: {9},\r\n    photo_id: {10},\r\n    need_likes: 1,\r\n    count: 10,\r\n    sort: \"desc\",\r\n    preview_length: 0,\r\n    access_key: \"{11}\",\r\n    allow_group_comments: 1\r\n}}).items;\r\n\r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r\n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r\n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", ownerId, pid, message.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), sticker_id, replyCid, attachmentIds.GetCommaSeparated(","), accessKey, stickerReferrer, ownerId, pid, accessKey)
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

    public void DeleteComment(long ownerId, long pid, long cid, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["photo_id"] = pid.ToString();
      parameters["comment_id"] = cid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.deleteComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void EditComment(long cid, string text, long ownerId, List<string> attachmentIds, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["comment_id"] = cid.ToString();
      parameters["message"] = text;
      parameters["owner_id"] = ownerId.ToString();
      if (!attachmentIds.IsNullOrEmpty())
        parameters["attachments"] = attachmentIds.GetCommaSeparated(",");
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.editComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void DeletePhotos(long ownerId, List<long> pids)
    {
      string format = "API.photos.delete({{ \"owner_id\": {0}, \"photo_id\":{1} }});" + Environment.NewLine;
      string str = "";
      foreach (long pid in pids)
        str += string.Format(format, ownerId.ToString(), pid);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["code"] = str;
      string methodName = "execute";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ResponseWithId, ResultCode>> action = (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void MovePhotos(long ownerId, string aid, List<long> pids, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      string format = "API.photos.move({{\"photo_id\":{0}, \"target_aid\":\"{1}\", \"owner_id\":{2}}});" + Environment.NewLine;
      string str = "";
      foreach (long pid in pids)
        str += string.Format(format, pid, aid, ownerId);
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("execute", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void ReorderPhotos(long ownerId, long pid, long beforePid, long afterPid, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      if (beforePid != 0L)
        parameters["before"] = beforePid.ToString();
      if (afterPid != 0L)
        parameters["after"] = afterPid.ToString();
      parameters["photo_id"] = pid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.reorderPhotos", parameters, callback, (Func<string, ResponseWithId>) (json => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void DeleteAlbums(List<string> aids, long gid = 0)
    {
      string empty = string.Empty;
      string format = gid != 0L ? "API.photos.deleteAlbum({{\"album_id\":\"{0}\", \"group_id\":{1}}});" + Environment.NewLine : "API.photos.deleteAlbum({{\"album_id\":\"{0}\"}});" + Environment.NewLine;
      string str = "";
      foreach (string aid in aids)
        str = gid != 0L ? str + string.Format(format, aid, gid) : str + string.Format(format, aid);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["code"] = str;
      string methodName = "execute";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ResponseWithId, ResultCode>> action = (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {});
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type

      Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { });
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void ReorderAlbums(string aid, string before, string after, long ownerId, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      if (ownerId != 0L)
        parameters["owner_id"] = ownerId.ToString();
      if (!string.IsNullOrEmpty(before))
        parameters["before"] = before.ToString();
      if (!string.IsNullOrEmpty(after))
        parameters["after"] = after.ToString();
      parameters["album_id"] = aid;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.reorderAlbums", parameters, callback, (Func<string, ResponseWithId>) (json => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void GetPhotosByIds(long userOrGroupId, bool isGroup, List<long> pids, Action<BackendResult<List<Photo>, ResultCode>> callback)
    {
      List<string> ids = new List<string>();
      foreach (long pid in pids)
      {
        string str = (isGroup ? "-" : "") + userOrGroupId.ToString() + "_" + pid.ToString();
        ids.Add(str);
      }
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["photos"] = ids.GetCommaSeparated(",");
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<List<Photo>>("photos.getById", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetPhotoById(string ownerPlusPhotoId, Action<BackendResult<List<Photo>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["photos"] = ownerPlusPhotoId;
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<List<Photo>>("photos.getById", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetPhotos(long userOrGroupId, bool isGroup, string aid, List<long> pids, long feed, string feedType, Action<BackendResult<List<Photo>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = isGroup ? (-userOrGroupId).ToString() : userOrGroupId.ToString();
      parameters["album_id"] = aid;
      if (!pids.IsNullOrEmpty())
        parameters["photo_ids"] = pids.GetCommaSeparated();
      parameters["extended"] = "1";
      if (feed != 0L)
        parameters["feed"] = feed.ToString();
      if (!string.IsNullOrEmpty(feedType))
        parameters["feed_type"] = feedType;
      VKRequestsDispatcher.DispatchRequestToVK<List<Photo>>("photos.get", parameters, callback, (Func<string, List<Photo>>) (jsonStr =>
      {
        try
        {
          return JsonConvert.DeserializeObject<GenericRoot<VKList<Photo>>>(jsonStr).response.items;
        }
        catch (Exception )
        {
          return new List<Photo>();
        }
      }), false, true, new CancellationToken?(),  null);
    }

    public void CopyPhotos(long ownerId, long photoId, string accessKey, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["photo_id"] = photoId.ToString();
      parameters["access_key"] = accessKey.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.copy", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void Report(long ownerId, long id, ReportReason reportReason, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["photo_id"] = id.ToString();
      parameters["reason"] = ((int) reportReason).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.report", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void ReportComment(long ownerId, long commentId, ReportReason reportReason, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["owner_id"] = ownerId.ToString();
      parameters["comment_id"] = commentId.ToString();
      parameters["reason"] = ((int) reportReason).ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("photos.reportComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public class AlbumAndPhotosData
    {
      public Album Album { get; set; }

      public VKList<Photo> Photos { get; set; }

      public Photo Thumb { get; set; }
    }
  }
}
