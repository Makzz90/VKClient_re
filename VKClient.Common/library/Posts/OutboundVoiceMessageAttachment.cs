using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKClient.Common.Library.Posts
{
  public class OutboundVoiceMessageAttachment : OutboundAttachmentBase
  {
    private OutboundAttachmentUploadState _uploadState;
    private double _uploadProgress;
    private Doc _savedDoc;
    private StorageFile _file;
    private string _filePath;
    private int _duration;
    private List<int> _waveform;
    private Cancellation _cancellation;
    private bool _retryFlag;

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
        if (this.SavedDoc != null)
          return string.Format("doc{0}_{1}", this.SavedDoc.owner_id, this.SavedDoc.id);
        return "";
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
        base.NotifyPropertyChanged<OutboundAttachmentUploadState>(() => this.UploadState);
                base.NotifyPropertyChanged<Visibility>(() => this.IsUploadingVisibility);
                base.NotifyPropertyChanged<Visibility>(() => this.IsFailedUploadVisibility);
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        if (this._uploadState != OutboundAttachmentUploadState.Uploading)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public override Visibility IsFailedUploadVisibility
    {
      get
      {
        if (this._uploadState != OutboundAttachmentUploadState.Failed)
          return Visibility.Collapsed;
        return Visibility.Visible;
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
        base.NotifyPropertyChanged<double>(() => this.UploadProgress);
      }
    }

    private Doc SavedDoc
    {
      get
      {
        return this._savedDoc;
      }
      set
      {
        this._savedDoc = value;
        this.NotifyPropertyChanged("SavedDoc");
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return true;
      }
    }

    public OutboundVoiceMessageAttachment()
    {
    }

    public OutboundVoiceMessageAttachment(StorageFile file, int duration, List<int> waveform)
    {
      this._file = file;
      this._filePath = file.Path;
      this._duration = duration;
      this._waveform = waveform;
      this.PrepareDoc();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "doc";
      Doc doc = this.SavedDoc ?? new Doc();
      attachment.doc = doc;
      return attachment;
    }

    private void PrepareDoc()
    {
      try
      {
        Doc doc = new Doc();
        Guid guid = Guid.NewGuid();
        doc.guid = guid;
        DocPreview docPreview = new DocPreview();
        DocPreviewVoiceMessage previewVoiceMessage = new DocPreviewVoiceMessage();
        previewVoiceMessage.link_ogg = this._filePath;
        previewVoiceMessage.link_mp3 = this._filePath;
        previewVoiceMessage.duration = this._duration;
        List<int> waveform = this._waveform;
        previewVoiceMessage.waveform = waveform;
        docPreview.audio_msg = previewVoiceMessage;
        doc.preview = docPreview;
        this.SavedDoc = doc;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Failed to prepare doc data", ex);
      }
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this._filePath);
      writer.Write(this._duration);
      BinarySerializerExtensions.WriteList(writer, this._waveform);
      writer.Write((int) this._uploadState);
      writer.Write<Doc>(this.SavedDoc, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._filePath = reader.ReadString();
      this._duration = reader.ReadInt32();
      this._waveform = BinarySerializerExtensions.ReadListInt(reader);
      this.UploadState = (OutboundAttachmentUploadState) reader.ReadInt32();
      if (this.UploadState == OutboundAttachmentUploadState.Uploading)
        this.UploadState = OutboundAttachmentUploadState.Failed;
      this.SavedDoc = reader.ReadGeneric<Doc>();
    }

    public override async void Upload(Action completionCallback, Action<double> progressCallback = null)
    {
        if (this.UploadState != OutboundAttachmentUploadState.Uploading)
        {
            if (this.UploadState == OutboundAttachmentUploadState.Completed)
            {
                completionCallback.Invoke();
            }
            else
            {
                this.UploadState = OutboundAttachmentUploadState.Uploading;
                if (this._file == null)
                {
                    try
                    {
                        OutboundVoiceMessageAttachment outboundVoiceMessageAttachment = this;
                        StorageFile arg_BF_0 = outboundVoiceMessageAttachment._file;
                        StorageFile file = await StorageFile.GetFileFromPathAsync(this._filePath);
                        outboundVoiceMessageAttachment._file = file;
                        outboundVoiceMessageAttachment = null;
                    }
                    catch
                    {
                        this.UploadState = OutboundAttachmentUploadState.Failed;
                        completionCallback.Invoke();
                        return;
                    }
                }
                Stream stream = (await this._file.OpenAsync(0)).AsStreamForRead();
                this._cancellation = new Cancellation();
                DocumentsService.Current.UploadVoiceMessageDocument(stream, this._waveform, delegate(BackendResult<Doc, ResultCode> response)
                {
                    if (response.ResultCode == ResultCode.Succeeded)
                    {
                        if (this.UploadState != OutboundAttachmentUploadState.Completed)
                        {
                            this.UploadState = OutboundAttachmentUploadState.Completed;
                            Guid guid = this.SavedDoc.guid;
                            this.SavedDoc = response.ResultData;
                            this.SavedDoc.guid = guid;
                            EventAggregator.Current.Publish(new VoiceMessageUploaded
                            {
                                VoiceMessageDoc = this.SavedDoc
                            });
                            completionCallback.Invoke();
                            return;
                        }
                    }
                    else
                    {
                        this.UploadState = OutboundAttachmentUploadState.Failed;
                        if (this._retryFlag && !this._cancellation.IsSet)
                        {
                            this._retryFlag = false;
                            this.Upload(completionCallback, progressCallback);
                            return;
                        }
                        completionCallback.Invoke();
                    }
                }, delegate(double progress)
                {
                    this.UploadProgress = progress;
                    Action<double> expr_12 = progressCallback;
                    if (expr_12 == null)
                    {
                        return;
                    }
                    expr_12.Invoke(progress);
                }, this._cancellation);
            }
        }
    }

    public override void SetRetryFlag()
    {
      this._retryFlag = true;
    }

    public override void RemoveAndCancelUpload()
    {
      Cancellation cancellation = this._cancellation;
      if (cancellation == null)
        return;
      cancellation.Set();
    }
  }
}
