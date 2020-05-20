using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BackendServices
{
  public class ProfilesService
  {
    private static ProfilesService _instance;

    public static ProfilesService Instance
    {
      get
      {
        return ProfilesService._instance ?? (ProfilesService._instance = new ProfilesService());
      }
    }

    private ProfilesService()
    {
    }

    public void GetAllPhotos(long ownerId, int offset, Action<BackendResult<VKList<Photo>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Photo>>("photos.getAll", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          "28"
        },
        {
          "skip_hidden",
          "1"
        },
        {
          "offset",
          offset.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetPhotos(long ownerId, int offset, long albumId, Action<BackendResult<VKList<Photo>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Photo>>("photos.get", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        },
        {
          "count",
          "28"
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "album_id",
          albumId.ToString()
        },
        {
          "rev",
          "1"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void DeleteProfilePhoto(long ownerId, Action<BackendResult<string, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<string>("execute.deleteProfilePhoto", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SaveProfilePhoto(long ownerId, Rect thumbnailRect, byte[] photoData, Action<BackendResult<ProfilePhoto, ResultCode>> callback)
    {
      ProfilesService.GetPhotoUploadServer(ownerId, (Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>>) (res =>
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
              ProfilesService.SaveProfilePhoto(JsonConvert.DeserializeObject<UploadPhotoResponseData>(jsonResult.JsonString), ownerId, callback);
          }), "MyImage.jpg",  null,  null);
        }
      }));
    }

    private static void GetPhotoUploadServer(long ownerId, Action<BackendResult<VKClient.Audio.Base.UploadServerAddress, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKClient.Audio.Base.UploadServerAddress>("photos.getOwnerPhotoUploadServer", new Dictionary<string, string>()
      {
        {
          "owner_id",
          ownerId.ToString()
        }
      }, callback, (Func<string, VKClient.Audio.Base.UploadServerAddress>) null, false, true, new CancellationToken?(),  null);
    }

    private static void SaveProfilePhoto(UploadPhotoResponseData responseData, long ownerId, Action<BackendResult<ProfilePhoto, ResultCode>> callback)
    {
      Action<BackendResult<ProfilePhoto, ResultCode>> callback1 = (Action<BackendResult<ProfilePhoto, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded && ownerId < 0L)
        {
          BackendResult<ProfilePhoto, ResultCode> savePhotoResult = result;
          Action<BackendResult<Group[], ResultCode>> callback2 = (Action<BackendResult<Group[], ResultCode>>) (getGroupResult =>
          {
            if (getGroupResult.ResultCode == ResultCode.Succeeded)
            {
              savePhotoResult.ResultData.photo_200 = ((IEnumerable<Group>) getGroupResult.ResultData).First<Group>().photo_200;
              callback(savePhotoResult);
            }
            else
            {
              result.ResultCode = getGroupResult.ResultCode;
              callback(result);
            }
          });
          VKRequestsDispatcher.DispatchRequestToVK<Group[]>("groups.getById", new Dictionary<string, string>()
          {
            {
              "group_id",
              (-ownerId).ToString()
            }
          }, callback2, (Func<string, Group[]>) null, false, true, new CancellationToken?(),  null);
        }
        else
          callback(result);
      });
      VKRequestsDispatcher.DispatchRequestToVK<ProfilePhoto>("photos.saveOwnerPhoto", new Dictionary<string, string>()
      {
        {
          "server",
          responseData.server
        },
        {
          "photo",
          responseData.photo
        },
        {
          "hash",
          responseData.hash
        }
      }, callback1,  null, false, true, new CancellationToken?(),  null);
    }
  }
}
