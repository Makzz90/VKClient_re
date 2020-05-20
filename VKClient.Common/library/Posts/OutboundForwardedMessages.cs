using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKClient.Common.Library.Posts
{
  public class OutboundForwardedMessages : OutboundAttachmentBase
  {
    private List<Message> _messages;

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
        return this._messages.Count.ToString();
      }
    }

    public string Subtitle
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._messages.Count, CommonResources.OneMessageFrm, CommonResources.TwoFourMessagesFrm, CommonResources.FiveMessagesFrm, false,  null, false);
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

    public List<Message> Messages
    {
      get
      {
        return this._messages;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundForwardedMessages(List<Message> messages)
    {
      this._messages = messages;
    }

    public OutboundForwardedMessages()
    {
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
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
      writer.Write(1);
      writer.WriteList<Message>((IList<Message>) this._messages, 10000);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._messages = reader.ReadList<Message>();
    }
  }
}
