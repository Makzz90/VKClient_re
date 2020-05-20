using System;
using System.IO;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class OutboundMarketAlbumAttachment : OutboundAttachmentBase
  {
    private MarketAlbum _marketAlbum;

    public string Thumb
    {
      get
      {
        Photo photo = this._marketAlbum.photo;
        if (photo == null)
          return null;
        double height = this.Height;
        int reduceSizeForLowMemoryDeviceFactor = 1;
        return photo.GetAppropriateForScaleFactor(height, reduceSizeForLowMemoryDeviceFactor);
      }
    }

    public string Title
    {
      get
      {
        return this._marketAlbum.Title;
      }
    }

    public string Subtitle
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._marketAlbum.count, CommonResources.OneProductFrm, CommonResources.TwoFourProductsFrm, CommonResources.FiveProductsFrm, true, null, false);
      }
    }

    public Visibility RemoveVisibility
    {
      get
      {
        return Visibility.Visible;
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
        return this._marketAlbum.ToString();
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundMarketAlbumAttachment()
    {
    }

    public OutboundMarketAlbumAttachment(MarketAlbum marketAlbum)
    {
      this._marketAlbum = marketAlbum;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "market_album";
      MarketAlbum marketAlbum = this._marketAlbum;
      attachment.market_album = marketAlbum;
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
      writer.Write<MarketAlbum>(this._marketAlbum, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._marketAlbum = reader.ReadGeneric<MarketAlbum>();
    }
  }
}
