using System;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Posts
{
  public class OutboundLinkAttachment : OutboundAttachmentBase
  {
    private Link _link;

    public string Title
    {
      get
      {
        return this._link.title ?? CommonResources.Link;
      }
    }

    public string Subtitle
    {
      get
      {
        return this._link.description ?? "";
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsLink.png";
      }
    }

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
        return this._link.url;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundLinkAttachment()
    {
    }

    public OutboundLinkAttachment(Link link)
    {
      this._link = link;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "link";
      Link link = this._link;
      attachment.link = link;
      return attachment;
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
      writer.Write<Link>(this._link, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._link = reader.ReadGeneric<Link>();
    }
  }
}
