using System;
using System.IO;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class OutboundTimerAttachment : OutboundAttachmentBase, IHandleTap
  {
    public TimerAttachment Timer { get; private set; }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsTimer.png";
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.AttachmentTimer_Publish;
      }
    }

    public string Subtitle
    {
      get
      {
        if (this.Timer == null)
          return "";
        return UIStringFormatterHelper.FormatDateTimeForTimerAttachment(this.Timer.ScheduledPublishDateTime);
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
        return "timestamp";
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundTimerAttachment()
    {
    }

    public OutboundTimerAttachment(TimerAttachment timer)
    {
      this.Timer = timer;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      throw new NotImplementedException();
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
      writer.Write(this.Timer.ScheduledPublishDateTime.Ticks);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.Timer = new TimerAttachment()
      {
        ScheduledPublishDateTime = new DateTime(reader.ReadInt64())
      };
    }

    public void OnTap()
    {
      if (this.Timer == null)
        return;
      Navigator.Current.NavigateToPostSchedule(new DateTime?(this.Timer.ScheduledPublishDateTime));
    }
  }
}
