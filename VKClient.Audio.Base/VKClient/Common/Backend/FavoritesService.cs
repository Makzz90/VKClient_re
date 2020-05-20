using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class FavoritesService
  {
    private static FavoritesService _instance;

    public static FavoritesService Instance
    {
      get
      {
        if (FavoritesService._instance == null)
          FavoritesService._instance = new FavoritesService();
        return FavoritesService._instance;
      }
    }

    public void GetFavePhotos(int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<PhotosListWithCount>("fave.getPhotos", parameters, callback, (Func<string, PhotosListWithCount>) (jsonStr =>
      {
        GenericRoot<VKList<Photo>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<VKList<Photo>>>(jsonStr);
        return new PhotosListWithCount()
        {
          response = genericRoot.response.items,
          photosCount = genericRoot.response.count
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetFaveVideos(int offset, int count, Action<BackendResult<VKList<Video>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Video>>("fave.getVideos", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetFavePosts(int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<WallData>("fave.getPosts", parameters, callback, (Func<string, WallData>) (jsonStr =>
      {
        GenericRoot<WallDataResponse> genericRoot = JsonConvert.DeserializeObject<GenericRoot<WallDataResponse>>(jsonStr);
        return new WallData()
        {
          groups = genericRoot.response.groups,
          profiles = genericRoot.response.profiles,
          wall = genericRoot.response.items,
          TotalCount = genericRoot.response.count
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetFaveUsers(int offset, int count, Action<BackendResult<UsersListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str = string.Format("  var us = API.fave.getUsers({{\"offset\":{0}, \"count\":{1}}});\r\nvar users = API.users.get({{user_ids: us.items@.id, \"fields\": \"online, online_mobile, photo_max\"}});\r\nif (users)\r\n{{\r\n\r\nreturn users;\r\n}}\r\nreturn [];", offset, count);
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<UsersListWithCount>("execute", parameters, callback, (Func<string, UsersListWithCount>) (jsonStr =>
      {
        GenericRoot<List<User>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<List<User>>>(jsonStr);
        return new UsersListWithCount()
        {
          users = genericRoot.response
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetFaveLinks(int offset, int count, Action<BackendResult<VKList<Link>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Link>>("fave.getLinks", new Dictionary<string, string>()
      {
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetFaveProducts(int offset, int count, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Product>>("fave.getMarketItems", new Dictionary<string, string>()
      {
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "extended",
          "1"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void FaveAddRemoveUser(long userId, bool add, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["user_id"] = userId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(add ? "fave.addUser" : "fave.removeUser", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void FaveAddRemoveGroup(long groupId, bool add, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["group_id"] = groupId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(add ? "fave.addGroup" : "fave.removeGroup", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }
  }
}
