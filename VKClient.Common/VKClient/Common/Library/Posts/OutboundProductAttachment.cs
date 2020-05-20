using System;
using System.IO;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public class OutboundProductAttachment : OutboundAttachmentBase
  {
    private Product _product;

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

    public string Thumb
    {
      get
      {
        return this._product.thumb_photo;
      }
    }

    public string Title
    {
      get
      {
        return this._product.title;
      }
    }

    public string Subtitle
    {
      get
      {
        Price price = this._product.price;
        if (string.IsNullOrEmpty(price != null ? price.text : null))
          return "";
        return this._product.price.text;
      }
    }

    public Visibility RemoveVisibility
    {
      get
      {
        return !this.CanDettach ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public override Visibility IsFailedUploadVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public override string AttachmentId
    {
      get
      {
        return this._product.ToString();
      }
    }

    public Product Product
    {
      get
      {
        return this._product;
      }
    }

    public bool CanDettach { get; private set; }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundProductAttachment(Product product, bool canDettach)
    {
      this._product = product;
      this.CanDettach = canDettach;
    }

    public OutboundProductAttachment()
    {
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "market";
      Product product = this._product;
      attachment.market = product;
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
      writer.Write<Product>(this._product, false);
      writer.Write(this.CanDettach);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._product = reader.ReadGeneric<Product>();
      this.CanDettach = reader.ReadBoolean();
    }
  }
}
