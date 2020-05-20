using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
    public class AttachmentViewModel : ViewModelBase, IBinarySerializable
    {
        private string _resourceDescription;
        //private string _accessKey;
        private AttachmentType _attachmentType;
        private double _latitude;
        private double _longitude;
        private int _mediaDurationSeconds;
        private AudioObj _audio;
        private Photo _photo;
        private WallPost _wallPost;
        private double _stickerDimension;
        private HorizontalAlignment _stickerAlignment;
        private string _documentImageUri;
        private Attachment _attachment;
        private Geo _geo;
        private Comment _comment;

        public long MediaId { get; private set; }

        public long MediaOwnerId { get; private set; }

        public string AccessKey { get; set; }

        public string ResourceUri { get; set; }

        public string VideoUri { get; set; }

        public bool IsExternalVideo { get; set; }

        public AttachmentType AttachmentType
        {
            get
            {
                return this._attachmentType;
            }
            private set
            {
                this._attachmentType = value;
            }
        }

        public string ResourceDescription
        {
            get
            {
                return this._resourceDescription;
            }
            set
            {
                this._resourceDescription = value;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ResourceDescription));
            }
        }

        public string Artist { get; private set; }

        public int AudioContentRestricted { get; private set; }

        private bool IsAudioContentRestricted
        {
            get
            {
                return this.AudioContentRestricted > 0;
            }
        }

        public double TrackOpacity
        {
            get
            {
                return this.IsAudioContentRestricted ? 0.4 : 1.0;
            }
        }

        public string UniqueId { get; private set; }

        public double Latitude
        {
            get
            {
                return this._latitude;
            }
        }

        public double Longitude
        {
            get
            {
                return this._longitude;
            }
        }

        public string MediaDuration
        {
            get
            {
                if (this._mediaDurationSeconds < 3600)
                    return TimeSpan.FromSeconds((double)this._mediaDurationSeconds).ToString("m\\:ss");
                return TimeSpan.FromSeconds((double)this._mediaDurationSeconds).ToString("h\\:mm\\:ss");
            }
        }

        public AudioObj Audio
        {
            get
            {
                return this._audio;
            }
            set
            {
                AudioObj audio = this._audio;
                this._audio = value;
            }
        }

        public Photo Photo
        {
            get
            {
                return this._photo;
            }
            set
            {
                this._photo = value;
            }
        }

        public WallPost WallPost
        {
            get
            {
                return this._wallPost;
            }
        }

        public double StickerDimension
        {
            get
            {
                return this._stickerDimension;
            }
        }

        public HorizontalAlignment StickerAlignment
        {
            get
            {
                return this._stickerAlignment;
            }
        }

        public string DocumentImageUri
        {
            get
            {
                return this._documentImageUri;
            }
        }

        public Attachment Attachment
        {
            get
            {
                return this._attachment;
            }
        }

        public Geo Geo
        {
            get
            {
                return this._geo;
            }
        }

        public Comment Comment
        {
            get
            {
                return this._comment;
            }
        }

        public Gift Gift { get; set; }

        public string ParentPostId { get; set; }

        public bool IsDocumentImageAttachement
        {
            get
            {
                bool flag = false;
                if (this.AttachmentType == AttachmentType.Document && !string.IsNullOrEmpty(this.ResourceDescription))
                {
                    string upperInvariant = ((string)this.ResourceDescription).ToUpperInvariant();
                    List<string>.Enumerator enumerator = VKConstants.SupportedImageExtensions.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            string current = enumerator.Current;
                            if (((string)upperInvariant).EndsWith(current))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
                return flag;
            }
        }

        public bool IsDocumentGraffitiAttachment
        {
            get
            {
                if (this.AttachmentType == AttachmentType.Document)
                {
                    Attachment attachment = this.Attachment;
                    bool? nullable1;
                    if (attachment == null)
                    {
                        nullable1 = new bool?();
                    }
                    else
                    {
                        Doc doc = attachment.doc;
                        nullable1 = doc != null ? new bool?(doc.IsGraffiti) : new bool?();
                    }
                    bool? nullable2 = nullable1;
                    if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                        return true;
                }
                return false;
            }
        }

        public AttachmentViewModel(Attachment attachment, Message message)
            : this(attachment/*, null*/)
        {
            if (message == null)
                return;
            if (message.@out == 1)
                this._stickerAlignment = (HorizontalAlignment)2;
            else
                this._stickerAlignment = (HorizontalAlignment)0;
        }

        public AttachmentViewModel(Attachment attachment, string parentPostId = null)
        {
            this._attachment = attachment;
            if (attachment.type == "photo")
            {
                this.AttachmentType = AttachmentType.Photo;
                this.ResourceUri = attachment.photo.src_big;
                this.AccessKey = attachment.photo.access_key ?? "";
                this.Photo = attachment.photo;
            }
            if (attachment.type == "video")
            {
                this.AttachmentType = AttachmentType.Video;
                this.ResourceUri = attachment.video.image_big;
                if (string.IsNullOrEmpty(this.ResourceUri))
                    this.ResourceUri = attachment.video.image_medium;
                this._mediaDurationSeconds = attachment.video.duration;
                this.MediaId = attachment.video.vid;
                this.MediaOwnerId = attachment.video.owner_id;
                this.AccessKey = attachment.video.access_key ?? "";
            }
            if (attachment.type == "doc")
            {
                this.AttachmentType = AttachmentType.Document;
                if (attachment.doc != null)
                {
                    this.ResourceDescription = attachment.doc.title;
                    this.ResourceUri = attachment.doc.url;
                    this._documentImageUri = !attachment.doc.IsGraffiti ? attachment.doc.PreviewUri : attachment.doc.GraffitiPreviewUri;
                }
            }
            if (attachment.type == "audio")
            {
                this.AttachmentType = AttachmentType.Audio;
                AudioObj audio = attachment.audio;
                if (audio != null)
                {
                    this.ResourceUri = audio.url;
                    this.ResourceDescription = audio.title ?? "";
                    this.ResourceDescription = ((string)this.ResourceDescription).Trim();
                    this.Artist = audio.artist ?? "";
                    this.Artist = ((string)this.Artist).Trim();
                    this.AudioContentRestricted = audio.content_restricted;
                    this.MediaId = audio.aid;
                    this.MediaOwnerId = audio.owner_id;
                    this._audio = audio;
                }
            }
            if (attachment.type == "wall")
            {
                this.AttachmentType = AttachmentType.WallPost;
                if (attachment.wall != null)
                {
                    this._wallPost = attachment.wall;
                    this.ResourceUri = string.Concat(new object[4]
                  {
                    "https://vk.com/wall",
                    attachment.wall.to_id,
                    "_",
                    attachment.wall.id
                  });
                }
            }
            if (attachment.type == "wall_reply")
            {
                this.AttachmentType = AttachmentType.WallReply;
                if (attachment.wall_reply != null)
                {
                    this._comment = attachment.wall_reply;
                    this.ResourceUri = string.Concat(new object[4]
          {
            "https://vk.com/wall",
            attachment.wall_reply.owner_id,
            "_",
            attachment.wall_reply.post_id
          });
                }
            }
            if (attachment.type == "sticker")
            {
                this.AttachmentType = AttachmentType.Sticker;
                if (attachment.sticker != null)
                {
                    this.TryReadStickerData(attachment.sticker.photo_64, 64);
                    this.TryReadStickerData(attachment.sticker.photo_128, 128);
                    this.TryReadStickerData(attachment.sticker.photo_256, 256);
                }
            }
            if (attachment.type == "gift")
            {
                this.AttachmentType = AttachmentType.Gift;
                this.Gift = attachment.gift;
            }
            this.ParentPostId = parentPostId;
            this.UniqueId = Guid.NewGuid().ToString();
        }

        public AttachmentViewModel(Geo geo)
        {
            this._geo = geo;
            this.AttachmentType = AttachmentType.Geo;
            string[] strArray = (geo.coordinates).Split(new char[1] { ' ' });
            if (strArray.Length > 1)
            {
                double.TryParse(strArray[0], NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out this._latitude);
                double.TryParse(strArray[1], NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out this._longitude);
            }
            this.InitializeUIPropertiesForGeoAttachment();
        }

        public AttachmentViewModel()
        {
            this._resourceDescription = string.Empty;
            //this._accessKey = "";
        }

        private void TryReadStickerData(string resourceUri, int dimension)
        {
            if (!string.IsNullOrWhiteSpace(resourceUri))
            {
                this.ResourceUri = resourceUri;
                this._stickerDimension = (double)(dimension * 100 / ScaleFactor.GetScaleFactor());
            }
            if (this._stickerDimension < 200.0)
                return;
            this._stickerDimension = this._stickerDimension / 1.5;
        }

        private void InitializeUIPropertiesForGeoAttachment()
        {
            this.ResourceUri = (MapsService.Current.GetMapUri(this.Latitude, this.Longitude, 15, 310, 1.8)).ToString();
            MapsService.Current.ReverseGeocodeToAddress(this.Latitude, this.Longitude, (Action<BackendResult<string, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this.ResourceDescription = res.ResultData;
            }));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(7);
            writer.Write((int)this._attachmentType);
            writer.WriteString(this.ResourceUri);
            writer.WriteString(this._resourceDescription);
            writer.Write(this.Latitude);
            writer.Write(this.Longitude);
            writer.Write(this._mediaDurationSeconds);
            writer.Write(this.MediaId);
            writer.Write(this.MediaOwnerId);
            writer.WriteString(this.VideoUri);
            writer.Write(this.IsExternalVideo);
            writer.WriteString(this.Artist);
            writer.WriteString(this.UniqueId);
            writer.Write<AudioObj>(this.Audio, false);
            writer.WriteString(this.AccessKey);
            writer.Write<Photo>(this.Photo, false);
            writer.Write<WallPost>(this.WallPost, false);
            writer.Write(this._stickerDimension);
            writer.Write((int)this._stickerAlignment);
            writer.WriteString(this._documentImageUri);
            writer.Write<Attachment>(this._attachment, false);
            writer.Write<Geo>(this._geo, false);
            writer.Write<Comment>(this._comment, false);
            writer.WriteString(this.ParentPostId);
            writer.Write(this.AudioContentRestricted);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            if (num1 >= 1)
            {
                this._attachmentType = (AttachmentType)reader.ReadInt32();
                this.ResourceUri = reader.ReadString();
                this._resourceDescription = reader.ReadString();
                this._latitude = reader.ReadDouble();
                this._longitude = reader.ReadDouble();
                this._mediaDurationSeconds = reader.ReadInt32();
                this.MediaId = reader.ReadInt64();
                this.MediaOwnerId = reader.ReadInt64();
                this.VideoUri = reader.ReadString();
                this.IsExternalVideo = reader.ReadBoolean();
                this.Artist = reader.ReadString();
                this.UniqueId = reader.ReadString();
                this.Audio = reader.ReadGeneric<AudioObj>();
                this.AccessKey = reader.ReadString();
                this.Photo = reader.ReadGeneric<Photo>();
                this._wallPost = reader.ReadGeneric<WallPost>();
                if (this._attachmentType == AttachmentType.Geo && string.IsNullOrEmpty(this._resourceDescription))
                    this.InitializeUIPropertiesForGeoAttachment();
            }
            if (num1 >= 2)
            {
                this._stickerDimension = reader.ReadDouble();
                this._stickerAlignment = (HorizontalAlignment)reader.ReadInt32();
            }
            if (num1 >= 3)
                this._documentImageUri = reader.ReadString();
            if (num1 >= 4)
            {
                this._attachment = reader.ReadGeneric<Attachment>();
                this._geo = reader.ReadGeneric<Geo>();
            }
            if (num1 >= 5)
                this._comment = reader.ReadGeneric<Comment>();
            if (num1 >= 6)
                this.ParentPostId = reader.ReadString();
            if (num1 < 7)
                return;
            this.AudioContentRestricted = reader.ReadInt32();
        }

        public void NotifyResourceUriChanged()
        {
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ResourceUri));
        }
    }
}
