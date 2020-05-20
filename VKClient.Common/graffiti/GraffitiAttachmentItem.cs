using System;
using System.IO;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.Graffiti
{
  public class GraffitiAttachmentItem : IBinarySerializable
  {
    private string _uri;
    private int _width;
    private int _height;
    private bool _isUploading;
    private Cancellation _cancellation;

    public Doc Doc { get; private set; }

    public bool IsUploaded { get; private set; }

    public string AttachmentId
    {
      get
      {
        if (this.Doc == null)
          return "";
        long ownerId = this.Doc.owner_id;
        long id = this.Doc.id;
        string accessKey = this.Doc.access_key;
        string str = string.Format("doc{0}_{1}", ownerId, id);
        if (ownerId != AppGlobalStateManager.Current.LoggedInUserId && !string.IsNullOrEmpty(accessKey))
          str += string.Format("_{0}", accessKey);
        return str;
      }
    }

    public GraffitiAttachmentItem()
    {
    }

    public GraffitiAttachmentItem(string uri, int width, int height)
    {
      this._uri = uri;
      this._width = width;
      this._height = height;
      Doc doc = new Doc();
      DocPreview docPreview = new DocPreview();
      DocPreviewGraffiti docPreviewGraffiti = new DocPreviewGraffiti();
      docPreviewGraffiti.width = width;
      docPreviewGraffiti.height = height;
      string uri1 = this._uri;
      docPreviewGraffiti.src = uri1;
      docPreview.graffiti = docPreviewGraffiti;
      doc.preview = docPreview;
      this.Doc = doc;
    }

    public void Upload(Action completionCallback, Action<double> progressCallback = null)
    {
      if (this._isUploading)
        return;
      if (this._uri == null)
      {
        Action action = completionCallback;
        if (action == null)
          return;
        action();
      }
      else
      {
        this._isUploading = true;
        Stream stream = ImageCache.Current.GetCachedImageStream(this._uri);
        this._cancellation = new Cancellation();
        DocumentsService.Current.UploadGraffitiDocument("graffiti", ".png", stream, (Action<BackendResult<Doc, ResultCode>>) (response =>
        {
          this.IsUploaded = response.ResultCode == ResultCode.Succeeded;
          if (this.IsUploaded)
          {
            stream.Close();
            this.Doc = response.ResultData;
            ImageCache.Current.TryRemoveUri(this._uri);
          }
          this._isUploading = false;
          completionCallback();
        }), (Action<double>) (progress =>
        {
          Action<double> action = progressCallback;
          if (action == null)
            return;
          double num = progress;
          action(num);
        }), this._cancellation);
      }
    }

    public Attachment CreateAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "doc";
      Doc doc = this.Doc;
      attachment.doc = doc;
      return attachment;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this._uri);
      writer.Write(this._width);
      writer.Write(this._height);
      writer.Write<Doc>(this.Doc, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._uri = reader.ReadString();
      this._width = reader.ReadInt32();
      this._height = reader.ReadInt32();
      this.Doc = reader.ReadGeneric<Doc>();
    }
  }
}
