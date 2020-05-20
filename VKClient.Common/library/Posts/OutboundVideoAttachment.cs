using System;
using System.IO;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public class OutboundVideoAttachment : OutboundAttachmentBase
  {
    private OutboundAttachmentUploadState _uploadState;
    private VKClient.Common.Backend.DataObjects.Video _video;

    public VKClient.Common.Backend.DataObjects.Video Video
    {
      get
      {
        return this._video;
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
      }
    }

    public override bool IsGeo
    {
      get
      {
        return false;
      }
    }

    public string ResourceUri
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this._video.image_big))
          return this._video.image_big;
        if (!string.IsNullOrWhiteSpace(this._video.image_medium))
          return this._video.image_medium;
        return this._video.image;
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public double UploadProgress
    {
      get
      {
        return 0.0;
      }
    }

    public override Visibility IsFailedUploadVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public override string AttachmentId
    {
      get
      {
        long ownerId = this._video.owner_id;
        long id = this._video.id;
        string accessKey = this._video.access_key;
        string str = string.Format("video{0}_{1}", ownerId, id);
        if (ownerId != AppGlobalStateManager.Current.LoggedInUserId && !string.IsNullOrEmpty(accessKey))
          str += string.Format("_{0}", accessKey);
        return str;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundVideoAttachment(VKClient.Common.Backend.DataObjects.Video video)
    {
      this._uploadState = OutboundAttachmentUploadState.Completed;
      this._video = video;
    }

    public OutboundVideoAttachment()
    {
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "video";
      VKClient.Common.Backend.DataObjects.Video video = this._video;
      attachment.video = video;
      return attachment;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(VKConstants.SerializationVersion);
      writer.Write((int) this._uploadState);
      writer.Write<VKClient.Common.Backend.DataObjects.Video>(this._video, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._uploadState = (OutboundAttachmentUploadState) reader.ReadInt32();
      this._video = reader.ReadGeneric<VKClient.Common.Backend.DataObjects.Video>();
    }

    public override void SetRetryFlag()
    {
    }

    public override void RemoveAndCancelUpload()
    {
    }
  }
}
