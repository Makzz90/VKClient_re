using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Doc : IBinarySerializable
  {
    public static readonly int DOC_TYPE_TEXT = 1;
    public static readonly int DOC_TYPE_ARCHIVE = 2;
    public static readonly int DOC_TYPE_GIF = 3;
    public static readonly int DOC_TYPE_IMAGE = 4;
    public static readonly int DOC_TYPE_AUDIO = 5;
    public static readonly int DOC_TYPE_VIDEO = 6;
    public static readonly int DOC_TYPE_EBOOK = 7;
    public static readonly int DOC_TYPE_UNKNOWN = 8;
    private string _title = "";

    public long id { get; set; }

    public long owner_id { get; set; }

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
      }
    }

    public long size { get; set; }

    public int date { get; set; }

    public int type { get; set; }

    public string ext { get; set; }

    public string url { get; set; }

    public string access_key { get; set; }

    public DocPreview preview { get; set; }

    public string PreviewUri
    {
      get
      {
        DocPreview preview = this.preview;
        if (preview == null)
          return  null;
        DocPreviewPhoto photo = preview.photo;
        if (photo == null)
          return  null;
        List<DocPreviewPhotoSize> sizes = photo.sizes;
        if (sizes == null)
          return  null;
        DocPreviewPhotoSize previewPhotoSize = sizes.FirstOrDefault<DocPreviewPhotoSize>();
        if (previewPhotoSize == null)
          return  null;
        return previewPhotoSize.src;
      }
      set
      {
        this.preview = new DocPreview()
        {
          photo = new DocPreviewPhoto()
          {
            sizes = new List<DocPreviewPhotoSize>()
            {
              new DocPreviewPhotoSize() { src = value }
            }
          }
        };
      }
    }

    public string GraffitiPreviewUri
    {
      get
      {
        DocPreview preview = this.preview;
        if (preview == null)
          return  null;
        DocPreviewGraffiti graffiti = preview.graffiti;
        if (graffiti == null)
          return  null;
        return graffiti.src;
      }
      set
      {
        this.preview = new DocPreview()
        {
          graffiti = new DocPreviewGraffiti()
          {
            src = value
          }
        };
      }
    }

    public string UniqueIdForAttachment
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.access_key))
          return string.Format("doc{0}_{1}_{2}", this.owner_id, this.id, this.access_key);
        return string.Format("doc{0}_{1}", this.owner_id, this.id);
      }
    }

    public bool IsGraffiti
    {
      get
      {
        DocPreview preview = this.preview;
        return (preview != null ? preview.graffiti :  null) != null;
      }
    }

    public bool IsVoiceMessage
    {
      get
      {
        DocPreview preview = this.preview;
        return (preview != null ? preview.audio_msg :  null) != null;
      }
    }

    public Guid guid { get; set; }

    public bool IsVideoGif
    {
      get
      {
        if (this.type != Doc.DOC_TYPE_GIF)
          return false;
        DocPreview preview = this.preview;
        string str;
        if (preview == null)
        {
          str =  null;
        }
        else
        {
          DocPreviewVideo video = preview.video;
          str = video != null ? video.src :  null;
        }
        return !string.IsNullOrEmpty(str);
      }
    }

    public bool IsGif
    {
      get
      {
        string ext = this.ext;
        if (ext == null)
          return false;
        string str = "gif";
        return ext.EndsWith(str);
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(6);
      writer.Write(this.owner_id);
      writer.WriteString(this.title);
      writer.Write(this.size);
      writer.WriteString(this.ext);
      writer.WriteString(this.url);
      writer.Write<DocPreview>(this.preview, false);
      writer.Write(this.guid.ToString());
      writer.WriteString(this.access_key);
      writer.Write(this.type);
      writer.Write(this.id);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.owner_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.size = reader.ReadInt64();
      this.ext = reader.ReadString();
      this.url = reader.ReadString();
      int num2 = 3;
      if (num1 < num2)
      {
        string str1 = reader.ReadString();
        string str2 = reader.ReadString();
        if (string.IsNullOrWhiteSpace(str1) && string.IsNullOrWhiteSpace(str2))
          this.preview =  null;
        else
          this.PreviewUri = !string.IsNullOrWhiteSpace(str2) ? str2 : str1;
      }
      else
        this.preview = reader.ReadGeneric<DocPreview>();
      int num3 = 2;
      if (num1 >= num3)
      {
        string g = reader.ReadString();
        if (!string.IsNullOrEmpty(g))
          this.guid = new Guid(g);
      }
      int num4 = 4;
      if (num1 >= num4)
        this.access_key = reader.ReadString();
      int num5 = 5;
      if (num1 >= num5)
        this.type = reader.ReadInt32();
      int num6 = 6;
      if (num1 < num6)
        return;
      this.id = reader.ReadInt64();
    }

    public void MakeSafety()
    {
      this.title = this.title ?? "";
      this.ext = this.ext ?? "";
      this.url = this.url ?? "";
    }

    public override string ToString()
    {
      return string.Format("doc{0}_{1}", this.owner_id, this.id);
    }

    public Photo ConvertToPhotoPreview()
    {
      Photo photo1 = new Photo();
      DocPreview preview1 = this.preview;
      string str;
      if (preview1 == null)
      {
        str =  null;
      }
      else
      {
        DocPreviewPhoto photo2 = preview1.photo;
        if (photo2 == null)
        {
          str =  null;
        }
        else
        {
          List<DocPreviewPhotoSize> sizes = photo2.sizes;
          if (sizes == null)
          {
            str =  null;
          }
          else
          {
            DocPreviewPhotoSize previewPhotoSize = sizes.LastOrDefault<DocPreviewPhotoSize>();
            str = previewPhotoSize != null ? previewPhotoSize.src :  null;
          }
        }
      }
      photo1.src_xbig = str;
      DocPreview preview2 = this.preview;
      int? nullable1;
      if (preview2 == null)
      {
        nullable1 = new int?();
      }
      else
      {
        DocPreviewPhoto photo2 = preview2.photo;
        if (photo2 == null)
        {
          nullable1 = new int?();
        }
        else
        {
          List<DocPreviewPhotoSize> sizes = photo2.sizes;
          if (sizes == null)
          {
            nullable1 = new int?();
          }
          else
          {
            DocPreviewPhotoSize previewPhotoSize = sizes.LastOrDefault<DocPreviewPhotoSize>();
            nullable1 = previewPhotoSize != null ? new int?(previewPhotoSize.width) : new int?();
          }
        }
      }
      int? nullable2 = nullable1;
      int num1 = nullable2 ?? 0;
      photo1.width = num1;
      DocPreview preview3 = this.preview;
      int? nullable3;
      if (preview3 == null)
      {
        nullable3 = new int?();
      }
      else
      {
        DocPreviewPhoto photo2 = preview3.photo;
        if (photo2 == null)
        {
          nullable3 = new int?();
        }
        else
        {
          List<DocPreviewPhotoSize> sizes = photo2.sizes;
          if (sizes == null)
          {
            nullable3 = new int?();
          }
          else
          {
            DocPreviewPhotoSize previewPhotoSize = sizes.LastOrDefault<DocPreviewPhotoSize>();
            nullable3 = previewPhotoSize != null ? new int?(previewPhotoSize.height) : new int?();
          }
        }
      }
      nullable2 = nullable3;
      int num2 = nullable2 ?? 0;
      photo1.height = num2;
      return photo1;
    }
  }
}
