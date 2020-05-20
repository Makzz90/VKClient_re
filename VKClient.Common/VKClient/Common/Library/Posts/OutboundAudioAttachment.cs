using System;
using System.IO;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public class OutboundAudioAttachment : OutboundAttachmentBase
  {
    private AudioObj _audio;

    public string Title
    {
      get
      {
        return this._audio.title;
      }
    }

    public string Performer
    {
      get
      {
        return this._audio.artist;
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsAudio.png";
      }
    }

    public string Subtitle
    {
      get
      {
        return this._audio.artist;
      }
    }

    public AudioObj Audio
    {
      get
      {
        return this._audio;
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
        string str1 = "audio";
        long num = this._audio.owner_id;
        string string1 = num.ToString();
        string str2 = "_";
        num = this._audio.aid;
        string string2 = num.ToString();
        return str1 + string1 + str2 + string2;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundAudioAttachment(AudioObj audio)
    {
      this._audio = audio;
    }

    public OutboundAudioAttachment()
    {
    }

    public override Attachment GetAttachment()
    {
      return new Attachment()
      {
        audio = this._audio,
        type = "audio"
      };
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(VKConstants.SerializationVersion);
      writer.Write<AudioObj>(this._audio, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._audio = reader.ReadGeneric<AudioObj>();
    }

    public override void SetRetryFlag()
    {
    }

    public override void RemoveAndCancelUpload()
    {
    }
  }
}
