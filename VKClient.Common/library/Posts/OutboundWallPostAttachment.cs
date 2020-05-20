using System;
using System.IO;
using System.Windows;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Posts
{
  public class OutboundWallPostAttachment : OutboundAttachmentBase
  {
    private WallPost _wallPost;

    public override OutboundAttachmentUploadState UploadState
    {
      get
      {
        return OutboundAttachmentUploadState.Completed;
      }
      set
      {
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsPost.png";
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Conversation_WallPost;
      }
    }

    public string Subtitle
    {
      get
      {
        if (this._wallPost != null)
          return this._wallPost.text;
        return  null;
      }
    }

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
        return this._wallPost.GetRepostObjectType().ToString() + this._wallPost.to_id + "_" + this._wallPost.id;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundWallPostAttachment(WallPost wallPost)
    {
      this._wallPost = wallPost;
    }

    public OutboundWallPostAttachment()
    {
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      return new Attachment() { wall = this._wallPost, type = this._wallPost.GetRepostObjectType().ToString() };
    }

    public override void SetRetryFlag()
    {
    }

    public override void RemoveAndCancelUpload()
    {
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write<WallPost>(this._wallPost, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._wallPost = reader.ReadGeneric<WallPost>();
    }
  }
}
