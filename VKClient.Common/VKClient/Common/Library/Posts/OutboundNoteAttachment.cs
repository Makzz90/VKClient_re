using System;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Posts
{
  public class OutboundNoteAttachment : OutboundAttachmentBase
  {
    private Note _note;

    public string Title
    {
      get
      {
        return this._note.title ?? "";
      }
    }

    public string Subtitle
    {
      get
      {
        return CommonResources.Note;
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsDocument.png";
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
        return this._note.ToString();
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundNoteAttachment()
    {
    }

    public OutboundNoteAttachment(Note note)
    {
      this._note = note;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "note";
      Note note = this._note;
      attachment.note = note;
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
      writer.Write<Note>(this._note, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._note = reader.ReadGeneric<Note>();
    }
  }
}
