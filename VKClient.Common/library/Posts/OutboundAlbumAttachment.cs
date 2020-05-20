using System;
using System.IO;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class OutboundAlbumAttachment : OutboundAttachmentBase
  {
    private Album _album;

    public string Thumb
    {
      get
      {
        Photo thumb = this._album.thumb;
        if (thumb == null)
          return  null;
        double height = this.Height;
        int reduceSizeForLowMemoryDeviceFactor = 1;
        return thumb.GetAppropriateForScaleFactor(height, reduceSizeForLowMemoryDeviceFactor);
      }
    }

    public string Title
    {
      get
      {
        return this._album.title;
      }
    }

    public string Subtitle
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._album.size, CommonResources.OnePhotoFrm, CommonResources.TwoFourPhotosFrm, CommonResources.FivePhotosFrm, true,  null, false);
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
        return this._album.ToString();
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundAlbumAttachment()
    {
    }

    public OutboundAlbumAttachment(Album album)
    {
      this._album = album;
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback();
    }

    public override Attachment GetAttachment()
    {
      Attachment attachment = new Attachment();
      attachment.type = "album";
      Album album = this._album;
      attachment.album = album;
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
      writer.Write<Album>(this._album, false);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._album = reader.ReadGeneric<Album>();
    }
  }
}
