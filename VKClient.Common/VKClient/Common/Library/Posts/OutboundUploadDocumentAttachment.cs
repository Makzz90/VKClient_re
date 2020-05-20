using System;
using System.IO;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using Windows.Storage;
using Windows.Storage.FileProperties;
namespace VKClient.Common.Library.Posts
{
  public class OutboundUploadDocumentAttachment : OutboundAttachmentBase
  {
    private DocumentHeader _documentHeader;
    private string _title;
    private string _subTitle;
    private string _thumb;
    private OutboundAttachmentUploadState _uploadState;
    private double _uploadProgress;
    private Doc _savedDoc;
    private StorageFile _sf;
    private string _filePath;
    private Guid _guid;
    private string _localThumbPath;
    private Cancellation _cancellation;
    private bool _retryFlag;

    public DocumentHeader DocumentHeader
    {
      get
      {
        return this._documentHeader;
      }
      set
      {
        this._documentHeader = value;
        this.NotifyPropertyChanged("DocumentHeader");
        if (this._documentHeader == null)
          return;
        this.Title = this._documentHeader.Name;
        this.Subtitle = this._documentHeader.GetSizeString();
        Doc document = this._documentHeader.Document;
        this.Thumb = document != null ? document.PreviewUri : null;
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsDocument.png";
      }
    }

    public string Thumb
    {
      get
      {
        return this._thumb;
      }
      private set
      {
        if (value == null)
          return;
        this._thumb = value;
        this.NotifyPropertyChanged("Thumb");
      }
    }

    public string Title
    {
      get
      {
        return this._title;
      }
      private set
      {
        this._title = value;
        this.NotifyPropertyChanged("Title");
      }
    }

    public string Subtitle
    {
      get
      {
        return this._subTitle;
      }
      private set
      {
        this._subTitle = value;
        this.NotifyPropertyChanged("Subtitle");
      }
    }

    public Visibility RemoveVisibility
    {
      get
      {
        return Visibility.Visible;
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
        if (this.SavedDoc == null)
          return "";
        return "doc" + (object) this.SavedDoc.owner_id + "_" + (object) this.SavedDoc.id;
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
        this.NotifyPropertyChanged<OutboundAttachmentUploadState>((System.Linq.Expressions.Expression<Func<OutboundAttachmentUploadState>>) (() => this.UploadState));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsUploadingVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsFailedUploadVisibility));
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return this._uploadState != OutboundAttachmentUploadState.Uploading ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override Visibility IsFailedUploadVisibility
    {
      get
      {
        return this._uploadState != OutboundAttachmentUploadState.Failed ? Visibility.Collapsed : Visibility.Visible;
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
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.UploadProgress));
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
        this.DocumentHeader = new DocumentHeader(this._savedDoc, 0, false);
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return true;
      }
    }

    public OutboundUploadDocumentAttachment()
    {
    }

    public OutboundUploadDocumentAttachment(StorageFile file)
    {
      this._sf = file;
      this._filePath = file.Path;
      this.PrepareThumbnail();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "doc";
      Doc doc = this.SavedDoc ?? new Doc();
      attachment.doc = doc;
      return attachment;
    }

    private async void PrepareThumbnail()
    {
      try
      {
        Doc savedDoc = new Doc();
        BasicProperties basicPropertiesAsync = await this._sf.GetBasicPropertiesAsync();
        savedDoc.title = this._sf.Name;
        savedDoc.size = (long) basicPropertiesAsync.Size;
        string fileType = this._sf.FileType;
        savedDoc.ext = fileType.StartsWith(".") ? fileType.Substring(1) : fileType;
        savedDoc.guid = Guid.NewGuid();
        try
        {
          StorageItemThumbnail thumbnailAsync = await this._sf.GetThumbnailAsync((ThumbnailMode) 1);
          this._localThumbPath = "/" + (object) savedDoc.guid;
          ImageCache.Current.TrySetImageForUri(this._localThumbPath, (thumbnailAsync).AsStream());
          savedDoc.PreviewUri = this._localThumbPath;
        }
        catch
        {
        }
        this.SavedDoc = savedDoc;
        savedDoc = (Doc) null;
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
      writer.Write((int) this._uploadState);
      writer.WriteString(this._localThumbPath);
      writer.Write<Doc>(this.SavedDoc, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._filePath = reader.ReadString();
      this.UploadState = (OutboundAttachmentUploadState) reader.ReadInt32();
      this._localThumbPath = reader.ReadString();
      if (this.UploadState == OutboundAttachmentUploadState.Uploading)
        this.UploadState = OutboundAttachmentUploadState.Failed;
      this.SavedDoc = reader.ReadGeneric<Doc>();
    }

    public override async void Upload(Action completionCallback, Action<double> progressCallback = null)
    {
      if (this.UploadState == OutboundAttachmentUploadState.Uploading)
        return;
      if (this.UploadState == OutboundAttachmentUploadState.Completed)
      {
        completionCallback();
      }
      else
      {
        this.UploadState = OutboundAttachmentUploadState.Uploading;
        if (this._sf == null)
        {
          try
          {
            OutboundUploadDocumentAttachment documentAttachment = this;
            StorageFile storageFile = documentAttachment._sf;
            StorageFile fileFromPathAsync = await StorageFile.GetFileFromPathAsync(this._filePath);
            documentAttachment._sf = fileFromPathAsync;
            documentAttachment = (OutboundUploadDocumentAttachment) null;
          }
          catch
          {
            this.UploadState = OutboundAttachmentUploadState.Failed;
            completionCallback();
            return;
          }
        }
        Stream stream = ( await this._sf.OpenAsync((FileAccessMode) 0)).AsStreamForRead();
        this._cancellation = new Cancellation();
        DocumentsService.Current.UploadDocument(0L, this._sf.Name, this._sf.FileType, stream, (Action<BackendResult<Doc, ResultCode>>) (response =>
        {
          if (response.ResultCode == ResultCode.Succeeded)
          {
            if (this.UploadState == OutboundAttachmentUploadState.Completed)
              return;
            this.UploadState = OutboundAttachmentUploadState.Completed;
            this._guid = this.SavedDoc.guid;
            this.SavedDoc = response.ResultData;
            this.SavedDoc.guid = this._guid;
            EventAggregator.Current.Publish((object) new DocUploaded()
            {
              Doc = this.SavedDoc
            });
            completionCallback();
          }
          else
          {
            this.UploadState = OutboundAttachmentUploadState.Failed;
            if (this._retryFlag && !this._cancellation.IsSet)
            {
              this._retryFlag = false;
              this.Upload(completionCallback, progressCallback);
            }
            else
              completionCallback();
          }
        }), (Action<double>) (progress =>
        {
          this.UploadProgress = progress;
          Action<double> action = progressCallback;
          if (action == null)
            return;
          double num = progress;
          action(num);
        }), this._cancellation);
      }
    }

    public override void SetRetryFlag()
    {
      this._retryFlag = true;
    }

    public override void RemoveAndCancelUpload()
    {
      ImageCache.Current.TryRemoveUri(this._localThumbPath);
      Cancellation cancellation = this._cancellation;
      if (cancellation == null)
        return;
      cancellation.Set();
    }
  }
}
