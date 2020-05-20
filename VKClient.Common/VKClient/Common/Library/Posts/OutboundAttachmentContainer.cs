using System;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public class OutboundAttachmentContainer : IBinarySerializable
  {
    private OutboundGeoAttachment _geoAttachment;
    private OutboundPhotoAttachment _photoAttachment;
    private OutboundVideoAttachment _videoAttachment;
    private OutboundDocumentAttachment _documentAttachment;
    private OutboundAudioAttachment _audioAttachment;
    private OutboundUploadVideoAttachment _uploadVideoAttachment;
    private OutboundWallPostAttachment _wallPostAttachment;
    private OutboundForwardedMessages _forwardedMessages;
    private OutboundUploadDocumentAttachment _uploadDocumentAttachment;
    private OutboundTimerAttachment _timerAttachment;
    private OutboundLinkAttachment _linkAttachment;
    private OutboundProductAttachment _productAttachment;
    private OutboundNoteAttachment _noteAttachment;
    private OutboundMarketAlbumAttachment _marketAlbumAttachment;
    private OutboundAlbumAttachment _albumAttachment;
    private IOutboundAttachment _outboundAttachment;
    private OutboundPollAttachment _pollAttachment;

    public IOutboundAttachment OutboundAttachment
    {
      get
      {
        return this._outboundAttachment;
      }
    }

    public bool IsGeoAttachment
    {
      get
      {
        return this._geoAttachment != null;
      }
    }

    public OutboundAttachmentContainer(IOutboundAttachment outboundAttachment)
    {
      this._outboundAttachment = outboundAttachment;
      if (outboundAttachment is OutboundPhotoAttachment)
        this._photoAttachment = outboundAttachment as OutboundPhotoAttachment;
      else if (outboundAttachment is OutboundGeoAttachment)
        this._geoAttachment = outboundAttachment as OutboundGeoAttachment;
      else if (outboundAttachment is OutboundVideoAttachment)
        this._videoAttachment = outboundAttachment as OutboundVideoAttachment;
      else if (outboundAttachment is OutboundAudioAttachment)
        this._audioAttachment = outboundAttachment as OutboundAudioAttachment;
      else if (outboundAttachment is OutboundDocumentAttachment)
        this._documentAttachment = outboundAttachment as OutboundDocumentAttachment;
      else if (outboundAttachment is OutboundUploadVideoAttachment)
        this._uploadVideoAttachment = outboundAttachment as OutboundUploadVideoAttachment;
      else if (outboundAttachment is OutboundWallPostAttachment)
        this._wallPostAttachment = outboundAttachment as OutboundWallPostAttachment;
      else if (outboundAttachment is OutboundForwardedMessages)
        this._forwardedMessages = outboundAttachment as OutboundForwardedMessages;
      else if (outboundAttachment is OutboundUploadDocumentAttachment)
        this._uploadDocumentAttachment = outboundAttachment as OutboundUploadDocumentAttachment;
      else if (outboundAttachment is OutboundPollAttachment)
        this._pollAttachment = outboundAttachment as OutboundPollAttachment;
      else if (outboundAttachment is OutboundTimerAttachment)
        this._timerAttachment = outboundAttachment as OutboundTimerAttachment;
      else if (outboundAttachment is OutboundLinkAttachment)
        this._linkAttachment = outboundAttachment as OutboundLinkAttachment;
      else if (outboundAttachment is OutboundProductAttachment)
        this._productAttachment = outboundAttachment as OutboundProductAttachment;
      else if (outboundAttachment is OutboundNoteAttachment)
        this._noteAttachment = outboundAttachment as OutboundNoteAttachment;
      else if (outboundAttachment is OutboundMarketAlbumAttachment)
      {
        this._marketAlbumAttachment = outboundAttachment as OutboundMarketAlbumAttachment;
      }
      else
      {
        if (!(outboundAttachment is OutboundAlbumAttachment))
          throw new Exception("Unknown attachment type");
        this._albumAttachment = outboundAttachment as OutboundAlbumAttachment;
      }
    }

    public OutboundAttachmentContainer()
    {
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(8);
      writer.Write<OutboundPhotoAttachment>(this._photoAttachment, false);
      writer.Write<OutboundGeoAttachment>(this._geoAttachment, false);
      writer.Write<OutboundVideoAttachment>(this._videoAttachment, false);
      writer.Write<OutboundAudioAttachment>(this._audioAttachment, false);
      writer.Write<OutboundDocumentAttachment>(this._documentAttachment, false);
      writer.Write<OutboundUploadVideoAttachment>(this._uploadVideoAttachment, false);
      writer.Write<OutboundWallPostAttachment>(this._wallPostAttachment, false);
      writer.Write<OutboundForwardedMessages>(this._forwardedMessages, false);
      writer.Write<OutboundUploadDocumentAttachment>(this._uploadDocumentAttachment, false);
      writer.Write<OutboundPollAttachment>(this._pollAttachment, false);
      writer.Write<OutboundTimerAttachment>(this._timerAttachment, false);
      writer.Write<OutboundLinkAttachment>(this._linkAttachment, false);
      writer.Write<OutboundProductAttachment>(this._productAttachment, false);
      writer.Write<OutboundNoteAttachment>(this._noteAttachment, false);
      writer.Write<OutboundMarketAlbumAttachment>(this._marketAlbumAttachment, false);
      writer.Write<OutboundAlbumAttachment>(this._albumAttachment, false);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this._photoAttachment = reader.ReadGeneric<OutboundPhotoAttachment>();
      if (this._photoAttachment != null)
        this._outboundAttachment = (IOutboundAttachment) this._photoAttachment;
      this._geoAttachment = reader.ReadGeneric<OutboundGeoAttachment>();
      if (this._geoAttachment != null)
        this._outboundAttachment = (IOutboundAttachment) this._geoAttachment;
      this._videoAttachment = reader.ReadGeneric<OutboundVideoAttachment>();
      if (this._videoAttachment != null)
        this._outboundAttachment = (IOutboundAttachment) this._videoAttachment;
      this._audioAttachment = reader.ReadGeneric<OutboundAudioAttachment>();
      if (this._audioAttachment != null)
        this._outboundAttachment = (IOutboundAttachment) this._audioAttachment;
      this._documentAttachment = reader.ReadGeneric<OutboundDocumentAttachment>();
      if (this._documentAttachment != null)
        this._outboundAttachment = (IOutboundAttachment) this._documentAttachment;
      int num2 = 2;
      if (num1 >= num2)
      {
        this._uploadVideoAttachment = reader.ReadGeneric<OutboundUploadVideoAttachment>();
        if (this._uploadVideoAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._uploadVideoAttachment;
      }
      int num3 = 3;
      if (num1 >= num3)
      {
        this._wallPostAttachment = reader.ReadGeneric<OutboundWallPostAttachment>();
        if (this._wallPostAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._wallPostAttachment;
        this._forwardedMessages = reader.ReadGeneric<OutboundForwardedMessages>();
        if (this._forwardedMessages != null)
          this._outboundAttachment = (IOutboundAttachment) this._forwardedMessages;
      }
      int num4 = 4;
      if (num1 >= num4)
      {
        this._uploadDocumentAttachment = reader.ReadGeneric<OutboundUploadDocumentAttachment>();
        if (this._uploadDocumentAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._uploadDocumentAttachment;
        this._pollAttachment = reader.ReadGeneric<OutboundPollAttachment>();
        if (this._pollAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._pollAttachment;
        this._timerAttachment = reader.ReadGeneric<OutboundTimerAttachment>();
        if (this._timerAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._timerAttachment;
      }
      int num5 = 5;
      if (num1 >= num5)
      {
        this._linkAttachment = reader.ReadGeneric<OutboundLinkAttachment>();
        if (this._linkAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._linkAttachment;
      }
      int num6 = 6;
      if (num1 >= num6)
      {
        this._productAttachment = reader.ReadGeneric<OutboundProductAttachment>();
        if (this._productAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._productAttachment;
      }
      int num7 = 7;
      if (num1 >= num7)
      {
        this._noteAttachment = reader.ReadGeneric<OutboundNoteAttachment>();
        if (this._noteAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._noteAttachment;
      }
      int num8 = 8;
      if (num1 >= num8)
      {
        this._marketAlbumAttachment = reader.ReadGeneric<OutboundMarketAlbumAttachment>();
        if (this._marketAlbumAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._marketAlbumAttachment;
        this._albumAttachment = reader.ReadGeneric<OutboundAlbumAttachment>();
        if (this._albumAttachment != null)
          this._outboundAttachment = (IOutboundAttachment) this._albumAttachment;
      }
      if (this._outboundAttachment == null)
        throw new Exception("Outbound Attachment is NULL");
    }
  }
}
