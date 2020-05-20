using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti;
using VKClient.Common.Library.Posts;
using VKClient.Common.Utils;
using VKMessenger.Backend;
using Windows.Storage;

namespace VKMessenger.Library
{
    public class OutboundMessageViewModel : ViewModelBase, IBinarySerializable
    {
        private string _messageText = string.Empty;
        private ObservableCollection<IOutboundAttachment> _attachments = new ObservableCollection<IOutboundAttachment>();
        private Guid _uploadJobId = Guid.Empty;
        private OutboundMessageStatus _outboundMessageStatus;
        private long _deliveredMessageId;
        private DateTime _deliveryDateTime;
        private bool _isChat;
        private long _userOrChatId;
        private double _uploadProgress;

        public OutboundMessageStatus OutboundMessageStatus
        {
            get
            {
                return this._outboundMessageStatus;
            }
            set
            {
                if (this._outboundMessageStatus == value)
                    return;
                this._outboundMessageStatus = value;
                this.NotifyPropertyChanged("OutboundMessageStatus");
            }
        }

        public DateTime DeliveryDateTime
        {
            get
            {
                return this._deliveryDateTime;
            }
        }

        public long DeliveredMessageId
        {
            get
            {
                return this._deliveredMessageId;
            }
        }

        public string MessageText
        {
            get
            {
                return this._messageText;
            }
            set
            {
                this._messageText = value;
            }
        }

        public bool HasForwardedMessages
        {
            get
            {
                return this.Attachments.Any<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundForwardedMessages));
            }
        }

        public ObservableCollection<IOutboundAttachment> Attachments
        {
            get
            {
                return this._attachments;
            }
            set
            {
                this._attachments = value;
                this.NotifyPropertyChanged<ObservableCollection<IOutboundAttachment>>((Expression<Func<ObservableCollection<IOutboundAttachment>>>)(() => this.Attachments));
            }
        }

        public bool HaveGeoAttachment
        {
            get
            {
                return this.Attachments.Any<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.IsGeo));
            }
        }

        public int NumberOfAttAllowedToAdd
        {
            get
            {
                return 10 - this.Attachments.Count + (this.HaveGeoAttachment ? 1 : 0) + (this.HasForwardedMessages ? 1 : 0);
            }
        }

        public bool IsChat
        {
            get
            {
                return this._isChat;
            }
        }

        public long UserOrChatId
        {
            get
            {
                return this._userOrChatId;
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
                this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.UploadProgress));
            }
        }

        public int CountUploadableAttachments
        {
            get
            {
                return this.Attachments.Count<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.IsUploadAttachment));
            }
        }

        public StickerItemData StickerItem { get; set; }

        public string StickerReferrer { get; set; }

        public GraffitiAttachmentItem GraffitiAttachmentItem { get; set; }

        public event EventHandler UploadFinished;

        public event EventHandler MessageSent;

        public OutboundMessageViewModel(bool isChat, long userOrChatId)
        {
            this._isChat = isChat;
            this._userOrChatId = userOrChatId;
        }

        public OutboundMessageViewModel()
        {
        }

        public void AddPictureAttachment(Stream stream, Stream previewStream)
        {
            this.Attachments.Add((IOutboundAttachment)OutboundPhotoAttachment.CreateForUploadNewPhoto(stream, 0, false, previewStream, PostType.Message));
        }

        public void AddUploadDocAttachment(StorageFile file)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundUploadDocumentAttachment(file));
        }

        public void AddUploadVideoAttachment(StorageFile file)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundUploadVideoAttachment(file, true, 0L));
        }

        public void AddVoiceMessageAttachment(StorageFile file, int duration, List<int> waveform)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundVoiceMessageAttachment(file, duration, waveform));
        }

        public void AddGeoAttachment(double latitude, double longitude)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundGeoAttachment(latitude, longitude));
        }

        public void Send()
        {
            if (this._outboundMessageStatus == OutboundMessageStatus.Delivered || this._outboundMessageStatus == OutboundMessageStatus.SendingNow)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.UploadFinished == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.UploadFinished((object)this, EventArgs.Empty);
            }
            else
            {
                this._outboundMessageStatus = OutboundMessageStatus.SendingNow;
                this._uploadJobId = Guid.NewGuid();
                this.UploadProgress = 0.0;
                this.StartSendingByAttachmentInd(0, this._uploadJobId);
            }
        }

        public void CancelUpload()
        {
            this._uploadJobId = Guid.Empty;
            this.OutboundMessageStatus = OutboundMessageStatus.Failed;
        }

        private void UploadProgressHandler(double deltaProgress)
        {
            if (this.CountUploadableAttachments <= 0)
                return;
            this.UploadProgress = this.UploadProgress + deltaProgress / (double)this.CountUploadableAttachments;
        }

        private void StartSendingByAttachmentInd(int attachmentInd, Guid jobId)
        {
            if (jobId != this._uploadJobId)
                return;
            if ((attachmentInd >= this.Attachments.Count || this.StickerItem != null) && this.GraffitiAttachmentItem == null)
                this.DoSend();
            else if (this.GraffitiAttachmentItem != null)
            {
                double previousProgress = 0.0;
                this.GraffitiAttachmentItem.Upload((Action)(() =>
                {
                    if (!(jobId == this._uploadJobId))
                        return;
                    if (!this.GraffitiAttachmentItem.IsUploaded)
                    {
                        // ISSUE: reference to a compiler-generated field
                        EventHandler uploadFinished = this.UploadFinished;
                        if (uploadFinished != null)
                        {
                            EventArgs empty = EventArgs.Empty;
                            uploadFinished((object)this, empty);
                        }
                        this.OutboundMessageStatus = OutboundMessageStatus.Failed;
                    }
                    else
                        this.DoSend();
                }), (Action<double>)(progress =>
                {
                    this.UploadProgressHandler(progress - previousProgress);
                    previousProgress = progress;
                }));
            }
            else
            {
                IOutboundAttachment currentAttachment = this.Attachments[attachmentInd];
                if (currentAttachment.IsUploadAttachment)
                {
                    double previousProgress = 0.0;
                    currentAttachment.Upload((Action)(() =>
                    {
                        if (!(jobId == this._uploadJobId))
                            return;
                        if (currentAttachment.UploadState != OutboundAttachmentUploadState.Completed)
                        {
                            // ISSUE: reference to a compiler-generated field
                            if (this.UploadFinished != null)
                            {
                                // ISSUE: reference to a compiler-generated field
                                this.UploadFinished((object)this, EventArgs.Empty);
                            }
                            this.OutboundMessageStatus = OutboundMessageStatus.Failed;
                        }
                        else
                            this.StartSendingByAttachmentInd(attachmentInd + 1, jobId);
                    }), (Action<double>)(progress =>
                    {
                        this.UploadProgressHandler(progress - previousProgress);
                        previousProgress = progress;
                    }));
                }
                else
                    this.StartSendingByAttachmentInd(attachmentInd + 1, jobId);
            }
        }

        private void DoSend()
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest();
            int num1 = this._isChat ? 1 : 0;
            sendMessageRequest.IsChat = num1 != 0;
            long userOrChatId = this._userOrChatId;
            sendMessageRequest.UserOrCharId = userOrChatId;
            string messageText = this._messageText;
            sendMessageRequest.MessageBody = messageText;
            StickerItemData stickerItem = this.StickerItem;
            int num2 = stickerItem != null ? stickerItem.StickerId : 0;
            sendMessageRequest.StickerId = num2;
            string stickerReferrer = this.StickerReferrer;
            sendMessageRequest.StickerReferrer = stickerReferrer;
            SendMessageRequest request = sendMessageRequest;
            if (this.GraffitiAttachmentItem != null && this.GraffitiAttachmentItem.IsUploaded)
                request.AttachmentIds = new List<string>()
        {
          this.GraffitiAttachmentItem.AttachmentId
        };
            else if (this.Attachments.Count > 0)
            {
                request.AttachmentIds = new List<string>();
                foreach (IOutboundAttachment outboundAttachment in this.Attachments.Where<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => !a.IsGeo)))
                    request.AttachmentIds.Add(outboundAttachment.AttachmentId);
            }
            OutboundGeoAttachment outboundGeoAttachment = this.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.IsGeo)) as OutboundGeoAttachment;
            if (outboundGeoAttachment != null)
            {
                request.Latitude = outboundGeoAttachment.Latitude;
                request.Longitude = outboundGeoAttachment.Longitude;
                request.IsGeoAttached = true;
            }
            OutboundForwardedMessages forwardedMessages = this.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundForwardedMessages)) as OutboundForwardedMessages;
            if (forwardedMessages != null)
                request.ForwardedMessagesIds = new List<int>(forwardedMessages.Messages.Select<Message, int>((Func<Message, int>)(m => m.mid)));
            // ISSUE: reference to a compiler-generated field
            if (this.UploadFinished != null)
            {
                // ISSUE: reference to a compiler-generated field
                this.UploadFinished((object)this, EventArgs.Empty);
            }
            MessagesService.Instance.SendMessage(request, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                {
                    this._deliveredMessageId = res.ResultData.response;
                    this._deliveryDateTime = DateTime.UtcNow;
                    Logger.Instance.Info("OutboundMessageViewModel deliveryId = " + (object)this._deliveredMessageId);
                    this.OutboundMessageStatus = OutboundMessageStatus.Delivered;
                }
                else
                    this.OutboundMessageStatus = OutboundMessageStatus.Failed;
                // ISSUE: reference to a compiler-generated field
                if (this.MessageSent == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.MessageSent((object)this, EventArgs.Empty);
            }));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.WriteString(this._messageText);
            writer.WriteList<OutboundAttachmentContainer>((IList<OutboundAttachmentContainer>)this._attachments.Select<IOutboundAttachment, OutboundAttachmentContainer>((Func<IOutboundAttachment, OutboundAttachmentContainer>)(a => new OutboundAttachmentContainer(a))).ToList<OutboundAttachmentContainer>(), 10000);
            writer.Write((int)this._outboundMessageStatus);
            writer.Write(this._deliveredMessageId);
            writer.Write(this._isChat);
            writer.Write(this._userOrChatId);
            writer.Write(this._deliveryDateTime);
            writer.Write<StickerItemData>(this.StickerItem, false);
            writer.WriteString(this.StickerReferrer);
            writer.Write<GraffitiAttachmentItem>(this.GraffitiAttachmentItem, false);
        }

        public void Read(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            this._messageText = reader.ReadString();
            List<OutboundAttachmentContainer> source = reader.ReadList<OutboundAttachmentContainer>();
            this._attachments.Clear();
            foreach (IOutboundAttachment outboundAttachment in source.Select<OutboundAttachmentContainer, IOutboundAttachment>((Func<OutboundAttachmentContainer, IOutboundAttachment>)(c => c.OutboundAttachment)))
                this._attachments.Add(outboundAttachment);
            this._outboundMessageStatus = (OutboundMessageStatus)reader.ReadInt32();
            if (this._outboundMessageStatus == OutboundMessageStatus.SendingNow)
                this._outboundMessageStatus = OutboundMessageStatus.Failed;
            this._deliveredMessageId = reader.ReadInt64();
            this._isChat = reader.ReadBoolean();
            this._userOrChatId = reader.ReadInt64();
            this._deliveryDateTime = reader.ReadDateTime();
            this.StickerItem = reader.ReadGeneric<StickerItemData>();
            if (num >= 2)
                this.StickerReferrer = reader.ReadString();
            if (num < 3)
                return;
            this.GraffitiAttachmentItem = reader.ReadGeneric<GraffitiAttachmentItem>();
        }

        internal void RemoveForwardedMessages()
        {
            this.Attachments.Remove(this.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundForwardedMessages)));
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.HasForwardedMessages));
        }

        internal void AddExistingDocAttachment(Doc pickedDocument)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundDocumentAttachment(pickedDocument));
        }

        internal void AddExistingPhotoAttachment(Photo pickedPhoto)
        {
            this.Attachments.Add((IOutboundAttachment)OutboundPhotoAttachment.CreateForChoosingExistingPhoto(pickedPhoto, 0, false, PostType.Message));
        }

        internal void AddExistingVideoAttachment(Video pickedVideo)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundVideoAttachment(pickedVideo));
        }

        internal void AddExistingAudioAttachment(AudioObj pickedAudio)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundAudioAttachment(pickedAudio));
        }

        public void AddWallPostAttachment(WallPost wallPost)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundWallPostAttachment(wallPost));
        }

        public void AddProductAttachment(Product product, bool canDettach)
        {
            this.Attachments.Add((IOutboundAttachment)new OutboundProductAttachment(product, canDettach));
        }

        internal void RemoveAttachment(IOutboundAttachment outboundAttCont)
        {
            outboundAttCont.RemoveAndCancelUpload();
            this.Attachments.Remove(outboundAttCont);
        }
    }
}
