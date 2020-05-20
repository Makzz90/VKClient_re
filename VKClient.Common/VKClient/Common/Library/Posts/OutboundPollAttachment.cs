using System;
using System.IO;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Posts
{
  public class OutboundPollAttachment : OutboundAttachmentBase, IHandleTap
  {
    private Poll _poll;

    public Poll Poll
    {
      get
      {
        return this._poll;
      }
      set
      {
        this._poll = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Subtitle));
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
        throw new NotImplementedException();
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return Visibility.Collapsed;
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
        return "poll" + (object) this._poll.owner_id + "_" + (object) this._poll.poll_id;
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsPoll.png";
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Poll;
      }
    }

    public string Subtitle
    {
      get
      {
        if (this._poll != null)
          return this._poll.question;
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

    public OutboundPollAttachment(Poll poll)
    {
      this._poll = poll;
    }

    public OutboundPollAttachment()
    {
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "poll";
      Poll poll = this._poll;
      attachment.poll = poll;
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
      writer.Write<Poll>(this._poll, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._poll = reader.ReadGeneric<Poll>();
    }

    public void OnTap()
    {
      Navigator.Current.NavigateToCreateEditPoll(this._poll.owner_id, this._poll.poll_id, this._poll);
    }
  }
}
