using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class OutboundPhotoAttachment : OutboundAttachmentBase
  {
    private string _localUri = "";
    private string _attachmentId = string.Empty;
    private long _userOrGroupId;
    private bool _isGroup;
    private OutboundAttachmentUploadState _uploadState;
    private PostType _postType;
    private double _uploadProgress;
    private Photo _photo;
    private bool _isForUpload;
    private Cancellation _c;
    private bool _retryFlag;

    public override bool IsGeo
    {
      get
      {
        return false;
      }
    }

    public override string AttachmentId
    {
      get
      {
        return this._attachmentId;
      }
    }

    public override OutboundAttachmentUploadState UploadState
    {
      get
      {
        return this._uploadState;
      }
      set
      {
        this._uploadState = value;
        this.NotifyPropertyChanged<OutboundAttachmentUploadState>(() => this.UploadState);
        this.NotifyPropertyChanged<Visibility>(() => this.IsUploadingVisibility);
        this.NotifyPropertyChanged<double>(() => this.ImageOpacity);
        this.NotifyPropertyChanged<Visibility>(() => this.IsFailedUploadVisibility);
      }
    }

    public override Visibility IsFailedUploadVisibility
    {
      get
      {
        if (this._uploadState != OutboundAttachmentUploadState.Failed)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public double UploadProgress
    {
      get
      {
        return this._uploadProgress;
      }
      set
      {
        this._uploadProgress = value;
        this.NotifyPropertyChanged<double>(() => this.UploadProgress);
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        if (this.UploadState != OutboundAttachmentUploadState.Uploading)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public double ImageOpacity
    {
      get
      {
        switch (this.UploadState)
        {
          case OutboundAttachmentUploadState.NotStarted:
            return 0.5;
          case OutboundAttachmentUploadState.Uploading:
            return 0.75;
          case OutboundAttachmentUploadState.Failed:
            return 0.25;
          case OutboundAttachmentUploadState.Completed:
            return 1.0;
          default:
            return 1.0;
        }
      }
    }

    public string LocalUrl
    {
      get
      {
        return this._localUri;
      }
    }

    private string LocalUrlBig
    {
      get
      {
        return string.Concat(this._localUri, "Big");
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return this._isForUpload;
      }
    }

    public override Attachment GetAttachment()
    {
      if (this._photo == null)
        return new Attachment() { type = "photo", photo = new Photo() { src_big = this.LocalUrl, date = Extensions.DateTimeToUnixTimestamp(DateTime.Now.ToUniversalTime(), true), owner_id = AppGlobalStateManager.Current.LoggedInUserId, user_id = AppGlobalStateManager.Current.LoggedInUserId, album_id = -3, can_comment = 0, likes = new Likes() { can_publish = 0, can_like = 0 }, comments = new Comments() { can_post = 0 } } };
      Attachment attachment = new Attachment();
      attachment.type = "photo";
      Photo photo = this._photo;
      attachment.photo = photo;
      return attachment;
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this._localUri);
      writer.Write(this._userOrGroupId);
      writer.Write(this._isGroup);
      writer.Write((int) this._uploadState);
      writer.WriteString(this._attachmentId);
      writer.Write((int) this._postType);
      writer.Write<Photo>(this._photo, false);
      writer.Write(this._isForUpload);
    }

    public override void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this._localUri = reader.ReadString();
      this._userOrGroupId = reader.ReadInt64();
      this._isGroup = reader.ReadBoolean();
      this.UploadState = (OutboundAttachmentUploadState) reader.ReadInt32();
      this._attachmentId = reader.ReadString();
      this._postType = (PostType) reader.ReadInt32();
      this._photo = reader.ReadGeneric<Photo>();
      if (this._uploadState == OutboundAttachmentUploadState.Uploading)
        this.UploadState = OutboundAttachmentUploadState.Failed;
      int num2 = 2;
      if (num1 < num2)
        return;
      this._isForUpload = reader.ReadBoolean();
    }

    public static OutboundPhotoAttachment CreateForUploadNewPhoto(Stream stream, long userOrGroupId = 0, bool isGroup = false, Stream previewStream = null, PostType postType = PostType.WallPost)
    {
      OutboundPhotoAttachment outboundPhotoAttachment = new OutboundPhotoAttachment();
      outboundPhotoAttachment._userOrGroupId = userOrGroupId;
      outboundPhotoAttachment._isGroup = isGroup;
      outboundPhotoAttachment.UploadState = OutboundAttachmentUploadState.NotStarted;
      outboundPhotoAttachment._postType = postType;
      outboundPhotoAttachment._isForUpload = true;
      Guid guid = Guid.NewGuid();
      outboundPhotoAttachment._localUri = string.Concat("/", guid.ToString());
      if (!ImageCache.Current.TrySetImageForUri(outboundPhotoAttachment.LocalUrlBig, stream))
      {
        stream.Close();
        previewStream.Close();
        throw new Exception("Failed to save local attachment");
      }
      if (previewStream != null)
      {
        if (!ImageCache.Current.TrySetImageForUri(outboundPhotoAttachment._localUri, previewStream))
        {
          previewStream.Close();
          throw new Exception("Failed to save local attachment");
        }
        previewStream.Close();
      }
      else
      {
        stream.Position = 0L;
        ImageCache.Current.TrySetImageForUri(outboundPhotoAttachment._localUri, stream);
      }
      stream.Close();
      return outboundPhotoAttachment;
    }

    public static OutboundPhotoAttachment CreateForChoosingExistingPhoto(Photo photo, long userOrGroupId = 0, bool isGroup = false, PostType postType = PostType.WallPost)
    {
      OutboundPhotoAttachment outboundPhotoAttachment = new OutboundPhotoAttachment();
      outboundPhotoAttachment._userOrGroupId = userOrGroupId;
      outboundPhotoAttachment._isGroup = isGroup;
      string str = OutboundPhotoAttachment.ComposeAttachmentId(photo.owner_id, photo.pid, photo.access_key);
      outboundPhotoAttachment._attachmentId = str;
      string srcBig = photo.src_big;
      outboundPhotoAttachment._localUri = srcBig;
      int num1 = 3;
      outboundPhotoAttachment.UploadState = (OutboundAttachmentUploadState) num1;
      int num2 = (int) postType;
      outboundPhotoAttachment._postType = (PostType) num2;
      Photo photo1 = photo;
      outboundPhotoAttachment._photo = photo1;
      return outboundPhotoAttachment;
    }

    private static string ComposeAttachmentId(long ownerId, long id, string accessKey)
    {
      string str0 = string.Format("photo{0}_{1}", ownerId, id);
      if (ownerId != AppGlobalStateManager.Current.LoggedInUserId && !string.IsNullOrEmpty(accessKey))
        str0 = string.Concat(str0, string.Format("_{0}", accessKey));
      return str0;
    }

    public override void Upload(Action completionCallback, Action<double> progressCallback = null)
    {
      Action action;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.UploadState == OutboundAttachmentUploadState.Uploading)
          return;
        if (this.UploadState == OutboundAttachmentUploadState.Completed)
        {
          completionCallback();
        }
        else
        {
          this.UploadState = OutboundAttachmentUploadState.Uploading;
          Stream stream = ImageCache.Current.GetCachedImageStream(this.LocalUrlBig);
          ImagePreprocessor.PreprocessImage(stream, VKConstants.ResizedImageSize, true, (Action<ImagePreprocessResult>) (pres =>
          {
            Stream stream2 = pres.Stream;
            stream.Close();
            byte[] bytes = ImagePreprocessor.ReadFully(stream2);
            stream2.Close();
            this.UploadImpl(bytes, (action = (Action) (() =>
            {
              if (this._uploadState == OutboundAttachmentUploadState.Completed && this._photo != null)
              {
                this.CleanupCache();
                this._localUri = this._photo.src_big;
              }
              completionCallback();
            })), progressCallback);
          }));
        }
      }));
    }

    private void CleanupCache()
    {
      ImageCache.Current.TryRemoveUri(this.LocalUrlBig);
      ImageCache.Current.TryRemoveUri(this.LocalUrl);
    }

    private void UploadImpl(byte[] bytes, Action completionCallback, Action<double> progressCallback)
    {
        switch (this._postType)
        {
            case PostType.WallPost:
                this.UploadToImpl((Action<byte[], Action<BackendResult<Photo, ResultCode>>, Action<double>, Cancellation>)((b, res, p, c) => PhotosService.Current.UploadPhotoToWall(this._userOrGroupId, this._isGroup, b, res, p, c)), bytes, completionCallback, progressCallback);
                break;
            case PostType.Message:
                this.UploadToImpl(new Action<byte[], Action<BackendResult<Photo, ResultCode>>, Action<double>, Cancellation>(MessagesService.Instance.UploadPhoto), bytes, completionCallback, progressCallback);
                break;
        }
    }

    private void UploadToImpl(Action<byte[], Action<BackendResult<Photo, ResultCode>>, Action<double>, Cancellation> uploadAction, byte[] bytes, Action completionCallback, Action<double> progressCallback)
    {
      this._c = new Cancellation();
      this.UploadProgress = 0.0;
      uploadAction.Invoke(bytes, (Action<BackendResult<Photo, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          Photo resultData = res.ResultData;
          this._attachmentId = OutboundPhotoAttachment.ComposeAttachmentId(resultData.owner_id, resultData.pid, resultData.access_key);
          this._photo = res.ResultData;
          MemoryStream memoryStream = new MemoryStream(bytes);
          ImageCache.Current.TrySetImageForUri(this._localUri, (Stream) memoryStream);
          memoryStream.Close();
          this.UploadState = OutboundAttachmentUploadState.Completed;
          completionCallback.Invoke();
        }
        else
        {
          Logger.Instance.Info("!!!!!!!!!!!!FAILED TO UPLOAD",  new object[0]);
          this.UploadState = OutboundAttachmentUploadState.Failed;
          if (this._retryFlag)
          {
            this._retryFlag = false;
            this.Upload(completionCallback,  null);
          }
          else
            completionCallback.Invoke();
        }
      }), (Action<double>) (progress =>
      {
        this.UploadProgress = progress;
        if (progressCallback == null)
          return;
        progressCallback(progress);
      }), this._c);
    }

    public override void SetRetryFlag()
    {
      this._retryFlag = true;
    }

    public override void RemoveAndCancelUpload()
    {
      this.CleanupCache();
      if (this._c == null)
        return;
      this._c.Set();
    }
  }
}
