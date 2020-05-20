using System;
using System.IO;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Posts
{
  public class OutboundAddAttachment : OutboundAttachmentBase
  {
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
        return "";
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundAddAttachment()
    {
      this.IsOnPostPage = true;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      if (callback == null)
        return;
      callback();
    }

    public override Attachment GetAttachment()
    {
      return  null;
    }

    public override void SetRetryFlag()
    {
    }

    public override void RemoveAndCancelUpload()
    {
    }

    public override void Write(BinaryWriter writer)
    {
    }

    public override void Read(BinaryReader reader)
    {
    }
  }
}
