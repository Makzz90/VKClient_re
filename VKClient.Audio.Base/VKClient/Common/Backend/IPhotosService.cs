using System;
using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public interface IPhotosService
  {
    void GetUsersAlbums(long uid, bool isGroup, Action<BackendResult<AlbumsData, ResultCode>> callback);

    void GetAllPhotos(long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback);

    void GetUserPhotos(long userId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback);

    void GetWallPhotos(long userOrGroupId, bool isGroup, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback);

    void GetProfilePhotos(long userId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback);

    void GetAlbumPhotos(long userOrGroupId, bool isGroup, string albumId, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback);

    void CreateAlbum(Album album, Action<BackendResult<Album, ResultCode>> callback, long gid = 0);

    void EditAlbum(Album album, Action<BackendResult<ResponseWithId, ResultCode>> callback, long gid = 0);

    void DeleteAlbum(string aid, Action<BackendResult<ResponseWithId, ResultCode>> callback, long gid);

    void DeleteAlbums(List<string> aids, long gid = 0);

    void ReorderAlbums(string aid, string before, string after, long ownerId, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void UploadPhotoToAlbum(string aid, long optionalGroupId, byte[] photoData, Action<BackendResult<Photo, ResultCode>> callback);

    void UploadPhotoToWall(long userOrGroupId, bool isGroup, byte[] photoData, Action<BackendResult<Photo, ResultCode>> callback);

    void DeletePhoto(long pid, long ownerId, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void GetPhotoWithFullInfo(long ownerId, long pid, string accessKey, int knownCommentsCount, int offset, int commentsCountToRead, Action<BackendResult<PhotoWithFullInfo, ResultCode>> callback);

    void CreateComment(long ownerId, long pid, long replyCid, string message, List<string> attachmentIds, Action<BackendResult<ResponseWithId, ResultCode>> callback, string accessKey = "");

    void DeleteComment(long ownerId, long pid, long cid, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void EditComment(long cid, string text, long ownerId, List<string> attachmentIds, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void DeletePhotos(long ownerId, List<long> pids);

    void MovePhotos(long ownerId, string aid, List<long> pids, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void ReorderPhotos(long ownerId, long pid, long beforePid, long afterPid, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void GetPhotos(long userOrGroupId, bool isGroup, string aid, List<long> pids, long feed, string feedType, Action<BackendResult<List<Photo>, ResultCode>> callback);

    void CopyPhotos(long ownerId, long photoId, string accessKey, Action<BackendResult<ResponseWithId, ResultCode>> callback);
  }
}
