using System;
using System.IO;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace VKClient.Common.Library.Posts
{
  public class OutboundUploadVideoAttachment : OutboundAttachmentBase
  {
    private bool _retryFlag;
    private string _filePath;
    private StorageFile _sf;
    private OutboundAttachmentUploadState _uploadState;
    private string _localThumbPath;
    private double _uploadProgress;
    private SaveVideoResponse _saveVideoResponse;
    private Guid _guid;
    private int _duration;
    private bool _isPrivate;
    private long _groupId;
    private Cancellation _c;

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
        if (this._saveVideoResponse == null)
          return "";
        return "video" + (object) this._saveVideoResponse.owner_id + "_" + (object) this._saveVideoResponse.video_id;
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
        this.NotifyPropertyChanged<OutboundAttachmentUploadState>((System.Linq.Expressions.Expression<Func<OutboundAttachmentUploadState>>) (() => this.UploadState));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsUploadingVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsFailedUploadVisibility));
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return this._uploadState != OutboundAttachmentUploadState.Uploading ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override Visibility IsFailedUploadVisibility
    {
      get
      {
        return this._uploadState != OutboundAttachmentUploadState.Failed ? Visibility.Collapsed : Visibility.Visible;
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
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.UploadProgress));
      }
    }

    public Guid Guid
    {
      get
      {
        return this._guid;
      }
    }

    public SaveVideoResponse SaveVideoResponse
    {
      get
      {
        return this._saveVideoResponse;
      }
    }

    public string ResourceUri
    {
      get
      {
        return this._localThumbPath;
      }
    }

    public int Duration
    {
      get
      {
        return this._duration;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return true;
      }
    }

    public OutboundUploadVideoAttachment(StorageFile file, bool isPrivate = true, long groupId = 0)
    {
      this._sf = file;
      this._filePath = file.Path;
      this._isPrivate = isPrivate;
      this._groupId = groupId;
      this.PrepareThumbnail();
    }

    public OutboundUploadVideoAttachment()
    {
    }

    public override Attachment GetAttachment()
    {
      return new Attachment()
      {
        type = "video",
        video = new VKClient.Common.Backend.DataObjects.Video()
        {
          image_big = this.ResourceUri,
          guid = this.Guid,
          duration = this.Duration
        }
      };
    }

    public override void SetRetryFlag()
    {
      this._retryFlag = true;
    }

    private async void PrepareThumbnail()
    {
      try
      {
        this._duration = Convert.ToInt32((await this._sf.Properties.GetVideoPropertiesAsync()).Duration.TotalSeconds);
        StorageItemThumbnail thumbnailAsync = await this._sf.GetThumbnailAsync((ThumbnailMode) 1);
        this._guid = Guid.NewGuid();
        this._localThumbPath = "/" + (object) this._guid;
        ImageCache.Current.TrySetImageForUri(this._localThumbPath, ((IRandomAccessStream) thumbnailAsync).AsStream());
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ResourceUri));
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Failed to prepare video data", ex);
      }
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.WriteString(this._filePath);
      writer.Write((int) this._uploadState);
      writer.Write<SaveVideoResponse>(this._saveVideoResponse, false);
      writer.Write(this._guid.ToString());
      writer.WriteString(this._localThumbPath);
      writer.Write(this._isPrivate);
      writer.Write(this._groupId);
    }

    public override void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this._filePath = reader.ReadString();
      this.UploadState = (OutboundAttachmentUploadState) reader.ReadInt32();
      if (this.UploadState == OutboundAttachmentUploadState.Uploading)
        this.UploadState = OutboundAttachmentUploadState.Failed;
      this._saveVideoResponse = reader.ReadGeneric<SaveVideoResponse>();
      this._guid = new Guid(reader.ReadString());
      int num2 = 2;
      if (num1 >= num2)
        this._localThumbPath = reader.ReadString();
      int num3 = 3;
      if (num1 < num3)
        return;
      this._isPrivate = reader.ReadBoolean();
      this._groupId = reader.ReadInt64();
    }

    public override async void Upload(Action completionCallback, Action<double> progressCallback = null)
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
        if (this._sf == null)
        {
          try
          {
            OutboundUploadVideoAttachment uploadVideoAttachment = this;
            StorageFile storageFile = uploadVideoAttachment._sf;
            StorageFile fileFromPathAsync = await StorageFile.GetFileFromPathAsync(this._filePath);
            uploadVideoAttachment._sf = fileFromPathAsync;
            uploadVideoAttachment = (OutboundUploadVideoAttachment) null;
          }
          catch
          {
            this.UploadState = OutboundAttachmentUploadState.Failed;
            completionCallback();
            return;
          }
        }
        Stream stream = ((IInputStream) await this._sf.OpenAsync((FileAccessMode) 0)).AsStreamForRead();
        this._c = new Cancellation();
        VideoService.Instance.UploadVideo(stream, this._isPrivate, 0L, this._groupId, "", "", (Action<BackendResult<SaveVideoResponse, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
          {
            this.UploadState = OutboundAttachmentUploadState.Failed;
            if (this._retryFlag && !this._c.IsSet)
            {
              this._retryFlag = false;
              this.Upload(completionCallback, progressCallback);
            }
            else
              completionCallback();
          }
          else
          {
            if (this.UploadState == OutboundAttachmentUploadState.Completed)
              return;
            this.UploadState = OutboundAttachmentUploadState.Completed;
            this._saveVideoResponse = res.ResultData;
            EventAggregator.Current.Publish((object) new VideoUploaded()
            {
              guid = this._guid,
              SaveVidResp = this._saveVideoResponse
            });
            completionCallback();
          }
        }), (Action<double>) (p =>
        {
          this.UploadProgress = p;
          if (progressCallback == null)
            return;
          progressCallback(p);
        }), this._c, (PrivacyInfo) null, (PrivacyInfo) null);
      }
    }

    private void CleanupCache()
    {
      ImageCache.Current.TryRemoveUri(this._localThumbPath);
    }

    public override void RemoveAndCancelUpload()
    {
      ImageCache.Current.TryRemoveUri(this._localThumbPath);
      if (this._c == null)
        return;
      this._c.Set();
    }
  }
}
