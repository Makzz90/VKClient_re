using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.UC.MapAttachments;
using VKClient.Common.Utils;

using System.Globalization;

namespace VKClient.Common.Library
{
    public class AttachmentsItem : VirtualizableItemBase
    {
        private const double MARGIN_MIN_VALUE = 8.0;
        private readonly List<Attachment> _attachments;
        private readonly Geo _geo;
        private readonly string _itemId;
        private readonly bool _friendsOnly;
        private readonly bool _isCommentAttachments;
        private readonly bool _isMessage;
        private bool _isHorizontal;
        private readonly double _horizontalWidth;
        private readonly bool _rightAlign;
        private readonly bool _wallPostWithCommentsPage;
        private readonly string _hyperlinkId;
        private readonly double _verticalWidth;
        private readonly string _parentPostId;
        private readonly User _parentDialogUser;
        private readonly bool _isForwardedMessage;
        private readonly List<Attachment> _gifAttachments;
        private readonly List<Attachment> _photoAttachments;
        private readonly List<Attachment> _videoAttachments;
        private readonly List<Attachment> _albumAttachments;
        private readonly List<Attachment> _marketAlbumAttachments;
        private readonly List<Attachment> _audioAttachments;
        private readonly List<Attachment> _docImageAttachments;
        private readonly WallPost _wallAttachment;
        private readonly Comment _commentAttachment;
        private readonly Gift _giftAttachment;
        private readonly Sticker _sticker;
        private readonly Link _link;
        private readonly Poll _poll;
        private readonly Product _product;
        private readonly Doc _graffitiDoc;
        private readonly Doc _voiceMessageDoc;
        private readonly MoneyTransfer _moneyTransfer;
        private bool _containsPhotoVideoAlbum;
        private GiftItem _giftItem;
        private GraffitiItem _graffitiItem;
        private VoiceMessageItem _voiceMessageItem;
        private WallPostAttachmentItem _wallPostAttItem;
        private CommentAttachmentItem _commentAttItem;
        private StickerItem _stickerItem;
        private UCItem _productItem;
        private UCItem _productMessageItem;
        private UCItem _moneyTransferItem;
        private UCItem _linkItem;
        private UCItem _pollItem;
        private List<UCItem> _docImageItemsList;
        private List<GenericAttachmentItem> _genericItemsList;
        private AudioAttachmentsItem _audioItem;
        private UCItem _geoItem;
        private ThumbsItem _thumbsItem;
        private double _height;

        public bool IsHorizontal
        {
            get
            {
                return this._isHorizontal;
            }
            set
            {
                if (this._isHorizontal == value)
                    return;
                this._isHorizontal = value;
                this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
                this.CreateOrUpdateLayout();
            }
        }

        public bool IsLastAttachmentMedia { get; private set; }

        public override double FixedHeight
        {
            get
            {
                return this._height;
            }
        }

        public AttachmentsItem(double width, Thickness margin, List<Attachment> attachments, Geo geo, string itemId, bool friendsOnly = false, bool isCommentAttachments = false, bool isMessage = false, bool isHorizontal = false, double horizontalWidth = 0.0, bool rightAlign = false, bool wallPostWithCommentsPage = false, string hyperlinkId = "", string parentPostId = null, User parentDialogUser = null, bool isForwardedMessage = false)
            : base(width, margin, default(Thickness))
        {
            this._hyperlinkId = hyperlinkId;
            this._itemId = itemId;
            this._attachments = attachments ?? new List<Attachment>();
            this._gifAttachments = new List<Attachment>();
            this._photoAttachments = new List<Attachment>();
            this._videoAttachments = new List<Attachment>();
            this._albumAttachments = new List<Attachment>();
            this._marketAlbumAttachments = new List<Attachment>();
            this._audioAttachments = new List<Attachment>();
            this._docImageAttachments = new List<Attachment>();
            if (this._attachments != null)
            {
                foreach (Attachment attachment in this._attachments)
                {
                    string type = attachment.type;

                    //uint stringHash = PrivateImplementationDetails.ComputeStringHash(type);
                    /*
                  if (stringHash <= 2322801903U)
                  {
                    if (stringHash <= 1665450665U)
                    {
                      if ((int) stringHash != 232457833)
                      {
                        if ((int) stringHash != 611394536)
                        {
                          if ((int) stringHash == 1665450665 && type == "market_album")
                          {
                            this._marketAlbumAttachments.Add(attachment);
                            continue;
                          }
                          continue;
                        }
                        if (!(type == "wall_ads"))
                          continue;
                      }
                      else
                      {
                        if (type == "link")
                        {
                          Link link = attachment.link;
                          if ((link != null ? link.photo : (Photo) null) != null && link.photo.width > 0 && link.photo.height > 0)
                          {
                            this._link = link;
                            continue;
                          }
                          continue;
                        }
                        continue;
                      }
                    }
                    else
                    {
                      if (stringHash <= 1876330552U)
                      {
                        if ((int) stringHash != 1694181484)
                        {
                          if ((int) stringHash == 1876330552 && type == "poll")
                          {
                            this._poll = attachment.poll;
                            continue;
                          }
                          continue;
                        }
                        if (type == "album")
                        {
                          this._albumAttachments.Add(attachment);
                          continue;
                        }
                        continue;
                      }
                      if ((int) stringHash != -2128144669)
                      {
                        if ((int) stringHash == -1972165393 && type == "gift")
                        {
                          this._giftAttachment = attachment.gift;
                          continue;
                        }
                        continue;
                      }
                      if (type == "photo")
                      {
                        this._photoAttachments.Add(attachment);
                        continue;
                      }
                      continue;
                    }
                  }
                  else if (stringHash <= 3343004700U)
                  {
                    if ((int) stringHash != -1490670315)
                    {
                      if ((int) stringHash != -1242026383)
                      {
                        if ((int) stringHash == -951962596 && type == "sticker")
                        {
                          this._sticker = attachment.sticker;
                          continue;
                        }
                        continue;
                      }
                      if (type == "market")
                      {
                        this._product = attachment.market;
                        continue;
                      }
                      continue;
                    }
                    if (!(type == "wall"))
                      continue;
                  }
                  else
                  {
                    if (stringHash <= 3472427884U)
                    {
                      if ((int) stringHash != -896901342)
                      {
                        if ((int) stringHash == -822539412 && type == "video")
                        {
                          this._videoAttachments.Add(attachment);
                          continue;
                        }
                        continue;
                      }
                      if (type == "wall_reply")
                      {
                        this._commentAttachment = attachment.wall_reply;
                        continue;
                      }
                      continue;
                    }
                    if ((int) stringHash != -530499175)
                    {
                      if ((int) stringHash == -362233003 && type == "doc")
                      {
                        if (!attachment.doc.IsVideoGif)
                        {
                          if (new DocumentHeader(attachment.doc, 0, false).HasThumbnail)
                          {
                            this._docImageAttachments.Add(attachment);
                            continue;
                          }
                          continue;
                        }
                        this._gifAttachments.Add(attachment);
                        continue;
                      }
                      continue;
                    }
                    if (type == "audio")
                    {
                      this._audioAttachments.Add(attachment);
                      continue;
                    }
                    continue;
                  }*/
                    if (type == "money_transfer")
                    {
                        this._moneyTransfer = attachment.money_transfer;
                        continue;
                    }
                    else if (type == "wall_ads")
                    {
                        continue;
                    }
                    else if (type == "link")
                    {
                        Link link = attachment.link;
                        if ((link != null ? link.photo : null) != null )
                        {
                            this._link = link;
                            continue;
                        }
                        continue;
                    }
                    else if (type == "album")
                    {
                        this._albumAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "market_album")
                    {
                        this._marketAlbumAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "photo")
                    {
                        this._photoAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "poll")
                    {
                        this._poll = attachment.poll;
                        continue;
                    }
                    else if (type == "wall")
                    {
                        this._wallAttachment = attachment.wall;
                        continue;
                    }
                    else if (type == "gift")
                    {
                        this._giftAttachment = attachment.gift;
                        continue;
                    }
                    else if (type == "sticker")
                    {
                        this._sticker = attachment.sticker;
                        continue;
                    }
                    else if (type == "market")
                    {
                        this._product = attachment.market;
                        continue;
                    }
                    else if (type == "video")
                    {
                        this._videoAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "wall_reply")
                    {
                        this._commentAttachment = attachment.wall_reply;
                        continue;
                    }
                    else if (type == "doc")
                    {
                        if (attachment.doc.IsGraffiti)// UPDATE: 4.8.0
                        {
                            this._graffitiDoc = attachment.doc;
                            continue;
                        }
                        if (attachment.doc.IsVoiceMessage)// UPDATE: 4.12.0
                        {
                            this._voiceMessageDoc = attachment.doc;
                            continue;
                        }
                        if (attachment.doc.IsVideoGif)
                        {
                            this._gifAttachments.Add(attachment);
                            continue;
                        }
                        if (new DocumentHeader(attachment.doc, 0, false).HasThumbnail)
                        {
                            this._docImageAttachments.Add(attachment);
                            continue;
                        }
                        continue;
                        
                    }
                    else if (type == "audio")
                    {
                        this._audioAttachments.Add(attachment);
                        continue;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("AttachmentsItem.AttachmentsItem " + type);
                    }

                }
            }
            this._geo = geo;
            this._friendsOnly = friendsOnly;
            this._isCommentAttachments = isCommentAttachments;
            this._isMessage = isMessage;
            this._isHorizontal = isHorizontal;
            this._parentPostId = parentPostId;
            this._parentDialogUser = parentDialogUser;
            this._isForwardedMessage = isForwardedMessage;
            this._horizontalWidth = horizontalWidth;
            this._verticalWidth = width;
            this._wallPostWithCommentsPage = wallPostWithCommentsPage;
            this._rightAlign = rightAlign;
            if (this._isHorizontal)
                this.Width = horizontalWidth;
            this.CreateOrUpdateLayout();
        }
        /*
      public AttachmentsItem(double width, Thickness margin, List<Attachment> attachments, Geo geo, string itemId, bool friendsOnly = false, bool isCommentAttachments = false, bool isMessage = false, bool isHorizontal = false, double horizontalWidth = 0.0, bool rightAlign = false, bool wallPostWithCommentsPage = false, string hyperlinkId = "", string parentPostId = null, User parentDialogUser = null, bool isForwardedMessage = false)
        : base(width, margin,  new Thickness())
      {
        this._hyperlinkId = hyperlinkId;
        this._itemId = itemId;
        this._attachments = attachments ?? new List<Attachment>();
        this._gifAttachments = new List<Attachment>();
        this._photoAttachments = new List<Attachment>();
        this._videoAttachments = new List<Attachment>();
        this._albumAttachments = new List<Attachment>();
        this._marketAlbumAttachments = new List<Attachment>();
        this._audioAttachments = new List<Attachment>();
        this._docImageAttachments = new List<Attachment>();
        if (this._attachments != null)
        {
          foreach (Attachment attachment in this._attachments)
          {
            string type = attachment.type;
            // ISSUE: reference to a compiler-generated method
            uint stringHash = <PrivateImplementationDetails>.ComputeStringHash(type);
            if (stringHash <= 2166822627U)
            {
              if (stringHash <= 1367801971U)
              {
                if ((int) stringHash != 232457833)
                {
                  if ((int) stringHash != 611394536)
                  {
                    if ((int) stringHash == 1367801971 && type == "money_transfer")
                    {
                      this._moneyTransfer = attachment.money_transfer;
                      continue;
                    }
                    continue;
                  }
                  if (!(type == "wall_ads"))
                    continue;
                }
                else
                {
                  if (type == "link")
                  {
                    Link link = attachment.link;
                    if ((link != null ? link.photo :  null) != null)
                    {
                      this._link = link;
                      continue;
                    }
                    continue;
                  }
                  continue;
                }
              }
              else
              {
                if (stringHash <= 1694181484U)
                {
                  if ((int) stringHash != 1665450665)
                  {
                    if ((int) stringHash == 1694181484 && type == "album")
                    {
                      this._albumAttachments.Add(attachment);
                      continue;
                    }
                    continue;
                  }
                  if (type == "market_album")
                  {
                    this._marketAlbumAttachments.Add(attachment);
                    continue;
                  }
                  continue;
                }
                if ((int) stringHash != 1876330552)
                {
                  if ((int) stringHash == -2128144669 && type == "photo")
                  {
                    this._photoAttachments.Add(attachment);
                    continue;
                  }
                  continue;
                }
                if (type == "poll")
                {
                  this._poll = attachment.poll;
                  continue;
                }
                continue;
              }
            }
            else if (stringHash <= 3343004700U)
            {
              if (stringHash <= 2804296981U)
              {
                if ((int) stringHash != -1972165393)
                {
                  if ((int) stringHash != -1490670315 || !(type == "wall"))
                    continue;
                }
                else
                {
                  if (type == "gift")
                  {
                    this._giftAttachment = attachment.gift;
                    continue;
                  }
                  continue;
                }
              }
              else
              {
                if ((int) stringHash != -1242026383)
                {
                  if ((int) stringHash == -951962596 && type == "sticker")
                  {
                    this._sticker = attachment.sticker;
                    continue;
                  }
                  continue;
                }
                if (type == "market")
                {
                  this._product = attachment.market;
                  continue;
                }
                continue;
              }
            }
            else
            {
              if (stringHash <= 3472427884U)
              {
                if ((int) stringHash != -896901342)
                {
                  if ((int) stringHash == -822539412 && type == "video")
                  {
                    this._videoAttachments.Add(attachment);
                    continue;
                  }
                  continue;
                }
                if (type == "wall_reply")
                {
                  this._commentAttachment = attachment.wall_reply;
                  continue;
                }
                continue;
              }
              if ((int) stringHash != -530499175)
              {
                if ((int) stringHash == -362233003 && type == "doc")
                {
                  if (attachment.doc.IsGraffiti)
                  {
                    this._graffitiDoc = attachment.doc;
                    continue;
                  }
                  if (attachment.doc.IsVoiceMessage)
                  {
                    this._voiceMessageDoc = attachment.doc;
                    continue;
                  }
                  if (!attachment.doc.IsVideoGif)
                  {
                    if (new DocumentHeader(attachment.doc, 0, false, 0L).HasThumbnail)
                    {
                      this._docImageAttachments.Add(attachment);
                      continue;
                    }
                    continue;
                  }
                  this._gifAttachments.Add(attachment);
                  continue;
                }
                continue;
              }
              if (type == "audio")
              {
                this._audioAttachments.Add(attachment);
                continue;
              }
              continue;
            }
            this._wallAttachment = attachment.wall;
          }
        }
        this._geo = geo;
        this._friendsOnly = friendsOnly;
        this._isCommentAttachments = isCommentAttachments;
        this._isMessage = isMessage;
        this._isHorizontal = isHorizontal;
        this._parentPostId = parentPostId;
        this._parentDialogUser = parentDialogUser;
        this._isForwardedMessage = isForwardedMessage;
        this._horizontalWidth = horizontalWidth;
        this._verticalWidth = width;
        this._wallPostWithCommentsPage = wallPostWithCommentsPage;
        this._rightAlign = rightAlign;
        if (this._isHorizontal)
          this.Width = horizontalWidth;
        this.CreateOrUpdateLayout();
      }
        */
        private void CreateOrUpdateLayout()
        {
            this.IsLastAttachmentMedia = false;
            double num1 = 0.0;
            this._containsPhotoVideoAlbum = this._photoAttachments.Count > 0 || this._videoAttachments.Count > 0 || (this._albumAttachments.Count > 0 || this._marketAlbumAttachments.Count > 0) || this._gifAttachments.Count > 0;
            double sticker = this.CreateSticker(num1);
            if (sticker != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + sticker;
            }
            double gift = this.CreateGift(num1);
            if (gift != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + gift;
            }
            double moneyTransfer = this.CreateMoneyTransfer(num1);
            if (moneyTransfer != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + moneyTransfer;
            }
            double graffiti = this.CreateGraffiti(num1);
            if (graffiti != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + graffiti;
            }
            double voiceMessage = this.CreateVoiceMessage(num1);
            if (voiceMessage != 0.0)
            {
                this.IsLastAttachmentMedia = false;
                num1 += 8.0 + voiceMessage;
            }
            if (this._containsPhotoVideoAlbum)
            {
                double photoVideoAlbum = this.CreatePhotoVideoAlbum(num1);
                if (photoVideoAlbum != 0.0)
                {
                    this.IsLastAttachmentMedia = true;
                    num1 += 8.0 + photoVideoAlbum;
                }
            }
            double product = this.CreateProduct(num1);
            if (product != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + product;
            }
            if (!this._containsPhotoVideoAlbum)
            {
                double snippet = this.CreateSnippet(num1);
                if (snippet != 0.0)
                {
                    this.IsLastAttachmentMedia = true;
                    num1 += 8.0 + snippet;
                }
            }
            int num2 = !this._containsPhotoVideoAlbum ? 1 : 0;
            if (num2 != 0)
            {
                double map = this.CreateMap(num1, true);
                if (map != 0.0)
                {
                    this.IsLastAttachmentMedia = true;
                    num1 += 8.0 + map;
                }
            }
            double audio = this.CreateAudio(num1);
            if (audio != 0.0)
            {
                this.IsLastAttachmentMedia = false;
                num1 += 8.0 + audio;
            }
            double poll = this.CreatePoll(num1);
            if (poll != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + poll;
            }
            double imageDocs = this.CreateImageDocs(num1);
            if (imageDocs != 0.0)
            {
                this.IsLastAttachmentMedia = false;
                num1 += 8.0 + imageDocs;
            }
            double generic = this.CreateGeneric(num1);
            if (generic != 0.0)
            {
                this.IsLastAttachmentMedia = false;
                num1 += 8.0 + generic;
            }
            double wallPost = this.CreateWallPost(num1);
            if (wallPost != 0.0)
            {
                this.IsLastAttachmentMedia = false;
                num1 += 8.0 + wallPost;
            }
            double comment = this.CreateComment(num1);
            if (comment != 0.0)
            {
                this.IsLastAttachmentMedia = false;
                num1 += 8.0 + comment;
            }
            if (num2 == 0)
            {
                double map = this.CreateMap(num1, false);
                if (map != 0.0)
                {
                    this.IsLastAttachmentMedia = false;
                    num1 += 8.0 + map;
                }
            }
            this._height = Math.Max(0.0, num1);
        }

        private double CreateGift(double topMargin)
        {
            if (this._giftAttachment == null)
                return 0.0;
            topMargin += 2.0;
            if (this._giftItem == null)
            {
                this._giftItem = new GiftItem(this._verticalWidth + 8.0, new Thickness(-4.0, topMargin, -4.0, 0.0), this._giftAttachment, this._horizontalWidth + 8.0, this._isHorizontal);
                this.VirtualizableChildren.Add((IVirtualizable)this._giftItem);
            }
            else
            {
                this._giftItem.IsHorizontal = this._isHorizontal;
                this._giftItem.Margin = new Thickness(-4.0, topMargin, -4.0, 0.0);
            }
            return this._giftItem.FixedHeight;
        }

        private double CreateGraffiti(double topMargin)
        {
            if (this._graffitiDoc == null)
                return 0.0;
            topMargin += 8.0;
            if (this._graffitiItem != null)
            {
                this._graffitiItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
                this._graffitiItem.IsHorizontal = this._isHorizontal;
            }
            else
            {
                this._graffitiItem = new GraffitiItem(this._verticalWidth, new Thickness(0.0, topMargin, 0.0, 0.0), this._attachments, this._isHorizontal, this._horizontalWidth, this._rightAlign);
                this.VirtualizableChildren.Add((IVirtualizable)this._graffitiItem);
            }
            return this._graffitiItem.FixedHeight;
        }

        private double CreateVoiceMessage(double topMargin)
        {
            if (this._voiceMessageDoc == null)
                return 0.0;
            topMargin += 8.0;
            if (this._voiceMessageItem != null)
            {
                this._voiceMessageItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
                this._voiceMessageItem.IsHorizontal = this._isHorizontal;
            }
            else
            {
                this._voiceMessageItem = new VoiceMessageItem(this._verticalWidth, new Thickness(0.0, topMargin, 0.0, 0.0), this._voiceMessageDoc, this._isHorizontal, this._horizontalWidth);
                this.VirtualizableChildren.Add((IVirtualizable)this._voiceMessageItem);
            }
            return this._voiceMessageItem.FixedHeight;
        }

        private double CreateWallPost(double topMargin)
        {
            if (this._wallAttachment == null)
                return 0.0;
            topMargin += 8.0;
            if (this._wallPostAttItem == null)
            {
                this._wallPostAttItem = new WallPostAttachmentItem(this._wallAttachment, new Thickness(0.0, topMargin, 0.0, 0.0));
                this.VirtualizableChildren.Add((IVirtualizable)this._wallPostAttItem);
            }
            this._wallPostAttItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
            return this._wallPostAttItem.FixedHeight;
        }

        private double CreateComment(double topMargin)
        {
            if (this._commentAttachment == null)
                return 0.0;
            topMargin += 8.0;
            if (this._commentAttItem == null)
            {
                this._commentAttItem = new CommentAttachmentItem(this._commentAttachment, new Thickness(0.0, topMargin, 0.0, 0.0));
                this.VirtualizableChildren.Add((IVirtualizable)this._commentAttItem);
            }
            this._commentAttItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
            return this._commentAttItem.FixedHeight;
        }

        private double CreateSticker(double topMargin)
        {
            if (this._sticker == null)
                return 0.0;
            topMargin += 8.0;
            if (this._stickerItem != null)
            {
                this._stickerItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
                this._stickerItem.IsHorizontal = this._isHorizontal;
            }
            else
            {
                this._stickerItem = new StickerItem(this._verticalWidth, new Thickness(0.0, topMargin, 0.0, 0.0), this._sticker, this._isHorizontal, this._horizontalWidth, this._rightAlign);
                this.VirtualizableChildren.Add((IVirtualizable)this._stickerItem);
            }
            return this._stickerItem.FixedHeight;
        }

        private double CreateProduct(double topMargin)
        {
            if (this._product == null)
                return 0.0;
            topMargin += this._isMessage ? 0.0 : 8.0;
            double num1 = this._isMessage || this._isCommentAttachments ? 16.0 : 0.0;
            if (this._isMessage)
            {
                if (this._productMessageItem != null)
                {
                    double num2 = num1 - 24.0;
                    this._productMessageItem.Margin = new Thickness(-num2, topMargin, -num2, 0.0);
                    this._productMessageItem.IsLandscape = this.IsHorizontal;
                }
                else
                {
                    this._productMessageItem = this.CreateSnippetItem(this._product, topMargin);
                    this.VirtualizableChildren.Add((IVirtualizable)this._productMessageItem);
                }
                return this._productMessageItem.FixedHeight;
            }
            if (this._productItem != null)
            {
                this._productItem.Margin = new Thickness(-num1, topMargin, -num1, 0.0);
            }
            else
            {
                this._productItem = this.CreateSnippetItem(this._product, topMargin);
                this.VirtualizableChildren.Add((IVirtualizable)this._productItem);
            }
            return this._productItem.FixedHeight;
        }

        private UCItem CreateSnippetItem(Product product, double topMargin)
        {
            Link link = new Link();
            link.title = product.title;
            link.caption = CommonResources.Product;
            link.photo = new Photo()
            {
                photo_75 = product.thumb_photo
            };
            LinkProduct linkProduct = new LinkProduct(product);
            link.product = linkProduct;
            string str = string.Format("https://vk.com/product{0}_{1}", product.owner_id, product.id);
            link.url = str;
            return this.CreateSnippetItem(link, topMargin);
        }

        private double CreateMoneyTransfer(double topMargin)
        {
            if (this._moneyTransfer == null)
                return 0.0;
            double num1 = 16.0;
            if (this._moneyTransferItem != null)
            {
                double num2 = num1 - 24.0;
                this._moneyTransferItem.Margin = new Thickness(-num2, topMargin, -num2, 0.0);
                this._moneyTransferItem.IsLandscape = this.IsHorizontal;
            }
            else
            {
                this._moneyTransferItem = this.CreateSnippetItem(this._moneyTransfer, topMargin);
                this.VirtualizableChildren.Add((IVirtualizable)this._moneyTransferItem);
            }
            return this._moneyTransferItem.FixedHeight;
        }

        private UCItem CreateSnippetItem(MoneyTransfer moneyTransfer, double topMargin)
        {
            NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            numberFormatInfo.NumberGroupSeparator = " ";
            int number = this._moneyTransfer.amount.amount / 100;
            string str = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.OneRoubleForm, CommonResources.TwoFourRoublesForm, CommonResources.FiveRoublesForm, false, null, false);
            return this.CreateSnippetItem(new Link() { title = string.Format("{0} {1}", number.ToString("#,#", (IFormatProvider)numberFormatInfo), str), caption = CommonResources.MoneyTransfer, money_transfer = moneyTransfer }, topMargin);
        }

        private double CreateSnippet(double topMargin)
        {
            if (this._link == null)
                return 0.0;
            topMargin += 8.0;
            if (this._isMessage)
                topMargin -= 8.0;
            double num = this._isCommentAttachments ? 0.0 : 8.0;
            if (this._linkItem != null)
            {
                if (this._isMessage)
                    this._linkItem.IsLandscape = this.IsHorizontal;
                else
                    this._linkItem.Margin = new Thickness(num, topMargin, num, 0.0);
            }
            else
            {
                this._linkItem = this.CreateSnippetItem(this._link, topMargin);
                this.VirtualizableChildren.Add((IVirtualizable)this._linkItem);
            }
            return this._linkItem.FixedHeight;
        }

        private UCItem CreateSnippetItem(Link link, double topMargin)
        {
            double num1 = this._isCommentAttachments || this._isMessage ? 0.0 : 8.0;
            double portraitWidth = this._verticalWidth - num1 * 2.0;
            double landscapeWidth = this._horizontalWidth - num1 * 2.0;
            double num2 = (double)ScaleFactor.GetRealScaleFactor() / 100.0;
            double num3 = portraitWidth / 2.0 * num2;
            Photo photo = link.photo;
            int? nullable1 = photo != null ? new int?(photo.width) : new int?();
            double? nullable2 = nullable1.HasValue ? new double?(nullable1.GetValueOrDefault()) : new double?();
            double num4 = num3;
            bool isBigSnippet = (nullable2.GetValueOrDefault() >= num4 ? (nullable2.HasValue ? 1 : 0) : 0) != 0;
            NewsLinkUCBase tmpUC = !isBigSnippet ? (!this._isMessage ? (NewsLinkUCBase)new NewsLinkMediumUC() : (NewsLinkUCBase)new MessagesLinkMediumUC(this._isForwardedMessage)) : (!this._isMessage ? (NewsLinkUCBase)new NewsLinkUC() : (NewsLinkUCBase)new MessagesLinkUC());
            double width = portraitWidth;
            if (this._isMessage)
            {
                topMargin += 4.0;
                if (this._isHorizontal)
                    width = landscapeWidth;
            }
            tmpUC.Initialize(link, width, this._isMessage ? "" : this._parentPostId);
            return new UCItem(portraitWidth, new Thickness(num1, topMargin, num1, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                NewsLinkUCBase newsLinkUcBase = !isBigSnippet ? (!this._isMessage ? (NewsLinkUCBase)new NewsLinkMediumUC() : (NewsLinkUCBase)new MessagesLinkMediumUC(this._isForwardedMessage)) : (!this._isMessage ? (NewsLinkUCBase)new NewsLinkUC() : (NewsLinkUCBase)new MessagesLinkUC());
                width = portraitWidth;
                if (this._isMessage && this._isHorizontal)
                    width = landscapeWidth;
                newsLinkUcBase.Initialize(link, width, this._isMessage ? "" : this._parentPostId);
                return (UserControlVirtualizable)newsLinkUcBase;
            }), (Func<double>)(() => tmpUC.CalculateTotalHeight()), null, this._isMessage ? landscapeWidth : 0.0, this._isMessage && this._isHorizontal);
        }

        private double CreatePoll(double topMargin)
        {
            if (this._poll == null)
                return 0.0;
            topMargin += 8.0;
            if (this._pollItem != null)
            {
                this._pollItem.Margin = new Thickness(8.0, topMargin, 8.0, 0.0);
            }
            else
            {
                PollUC tmpUC = new PollUC();
                tmpUC.Initialize(this._poll, 0L);
                this._pollItem = new UCItem(this._verticalWidth - 16.0, new Thickness(8.0, topMargin, 8.0, 0.0), (Func<UserControlVirtualizable>)(() =>
                {
                    PollUC pollUc = new PollUC();
                    Poll poll = this._poll;
                    long topicId = 0;
                    pollUc.Initialize(poll, topicId);
                    return (UserControlVirtualizable)pollUc;
                }), (Func<double>)(() => tmpUC.CalculateTotalHeight()), null, 0.0, false);
                this.VirtualizableChildren.Add((IVirtualizable)this._pollItem);
            }
            return this._pollItem.FixedHeight;
        }

        private double CreateImageDocs(double topMargin)
        {
            if (this._docImageAttachments.Count == 0)
                return 0.0;
            double num1 = topMargin + 8.0;
            int num2 = 0;
            double num3 = this._isMessage || this._isCommentAttachments ? -16.0 : 0.0;
            if (this._docImageItemsList != null)
            {
                foreach (UCItem docImageItems in this._docImageItemsList)
                {
                    docImageItems.Margin = new Thickness(num3, num1, 0.0, 0.0);
                    num1 += docImageItems.FixedHeight;
                    ++num2;
                }
            }
            else
            {
                this._docImageItemsList = new List<UCItem>();
                List<IEnumerable<Attachment>> list1 = (List<IEnumerable<Attachment>>)Enumerable.ToList<IEnumerable<Attachment>>(this._docImageAttachments.Partition<Attachment>(this._isMessage || this._isCommentAttachments ? 1 : 2));
                for (int index = 0; index < list1.Count; ++index)
                {
                    List<Attachment> list2 = (List<Attachment>)Enumerable.ToList<Attachment>(list1[index]);
                    Doc doc1 = list2[0].doc;
                    Doc doc2 = null;
                    if (list2.Count > 1)
                        doc2 = list2[1].doc;
                    UCItem ucItem = new UCItem(this._verticalWidth - num3, new Thickness(num3, num1, 0.0, 0.0), (Func<UserControlVirtualizable>)(() =>
                    {
                        DocImageAttachmentUC imageAttachmentUc = new DocImageAttachmentUC();
                        List<Attachment> attachments = this._attachments;
                        //Doc doc1 = doc1;
                        //Doc doc2 = doc2;
                        imageAttachmentUc.Initialize(attachments, doc1, doc2);
                        return (UserControlVirtualizable)imageAttachmentUc;
                    }), (Func<double>)(() => 152.0), null, 0.0, false);
                    if (this._isCommentAttachments)
                        CurrentMediaSource.GifPlaySource = StatisticsActionSource.comments;
                    else if (this._isMessage)
                        CurrentMediaSource.GifPlaySource = StatisticsActionSource.messages;
                    this.VirtualizableChildren.Add((IVirtualizable)ucItem);
                    this._docImageItemsList.Add(ucItem);
                    num1 += ucItem.FixedHeight;
                    ++num2;
                }
            }
            if (num2 <= 0)
                return 0.0;
            return num1 - topMargin - 8.0;
        }

        private double CreateGeneric(double topMargin)
        {
            topMargin += (this._isMessage ? 0.0 : 8.0);
            double num = topMargin;
            int num2 = 0;
            double num3 = (this._isMessage || this._isCommentAttachments) ? 16.0 : 0.0;
            double width = this._verticalWidth + num3 * 2.0;
            double horizontalWidth = this._horizontalWidth + num3 * 2.0;
            if (this._genericItemsList != null)
            {
                using (List<GenericAttachmentItem>.Enumerator enumerator = this._genericItemsList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        GenericAttachmentItem current = enumerator.Current;
                        current.Margin = new Thickness(-num3, num, -num3, 0.0);
                        current.IsHorizontal = this.IsHorizontal;
                        num += current.FixedHeight;
                        num2++;
                    }
                    goto IL_17F;
                }
            }
            this._genericItemsList = new List<GenericAttachmentItem>();
            using (IEnumerator<Attachment> enumerator2 = Enumerable.Where<Attachment>(this._attachments, (Attachment a) => (a.doc != null && !a.doc.IsGraffiti && !a.doc.IsVoiceMessage && !new DocumentHeader(a.doc, 0, false, 0L).HasThumbnail) || a.note != null || a.Page != null || (a.link != null && this._linkItem == null)).GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Attachment current2 = enumerator2.Current;
                    GenericAttachmentItem genericAttachmentItem = new GenericAttachmentItem(width, new Thickness(-num3, num, -num3, 0.0), current2, this._isHorizontal, horizontalWidth, this._hyperlinkId);
                    base.VirtualizableChildren.Add(genericAttachmentItem);
                    this._genericItemsList.Add(genericAttachmentItem);
                    num += genericAttachmentItem.FixedHeight;
                    num2++;
                }
            }
        IL_17F:
            if (num2 <= 0)
            {
                return 0.0;
            }
            return num - topMargin;
        }

        private double CreateAudio(double topMargin)
        {
            List<AudioObj> list = this._audioAttachments.Where<Attachment>((Func<Attachment, bool>)(a => a.audio != null)).Select<Attachment, AudioObj>((Func<Attachment, AudioObj>)(a => a.audio)).ToList<AudioObj>();
            if (list.Count == 0)
                return 0.0;
            topMargin += this._isMessage ? 0.0 : 8.0;
            double num = this._isMessage || this._isCommentAttachments ? 16.0 : 0.0;
            double width = this._verticalWidth + num * 2.0;
            double horizontalWidth = this._horizontalWidth + num * 2.0;
            if (this._audioItem != null)
            {
                this._audioItem.Margin = new Thickness(-num, topMargin, -num, 0.0);
                this._audioItem.IsHorizontal = this.IsHorizontal;
            }
            else
            {
                this._audioItem = new AudioAttachmentsItem(width, new Thickness(-num, topMargin, -num, 0.0), list, this._isCommentAttachments, this._isMessage, horizontalWidth, this._isHorizontal);
                this.VirtualizableChildren.Add((IVirtualizable)this._audioItem);
            }
            return this._audioItem.FixedHeight;
        }

        private double CreateMap(double topMargin, bool showMap)
        {
            if (this._geo == null)
                return 0.0;
            topMargin += 8.0;
            if (this._geoItem != null)
            {
                this._geoItem.IsLandscape = this.IsHorizontal;
                this._geoItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
            }
            else
            {
                this._geoItem = !this._isMessage ? (!(this._geo.type == "place") ? (showMap ? this.CreateMapPointFull(topMargin) : this.CreateMapPointSmall(topMargin)) : (showMap ? this.CreateMapPlaceFull(topMargin) : this.CreateMapPlaceSmall(topMargin))) : this.CreateMapPointSimple(topMargin);
                this.VirtualizableChildren.Add((IVirtualizable)this._geoItem);
            }
            return this._geoItem.FixedHeight;
        }

        private UCItem CreateMapPointSimple(double topMargin)
        {
            double portraitWidth = this._verticalWidth;
            double landscapeWidth = this._horizontalWidth;
            this._geoItem = new UCItem(portraitWidth, new Thickness(0.0, topMargin, 0.0, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                MapPointSimpleAttachmentUC simpleAttachmentUc = new MapPointSimpleAttachmentUC();
                Geo geo = this._geo;
                simpleAttachmentUc.Initialize(geo);
                return (UserControlVirtualizable)simpleAttachmentUc;
            }), (Func<double>)(() => MapPointSimpleAttachmentUC.CalculateTotalHeight(this.IsHorizontal ? landscapeWidth : portraitWidth)), null, landscapeWidth, this.IsHorizontal);
            return this._geoItem;
        }

        private UCItem CreateMapPointFull(double topMargin)
        {
            double portraitWidth = this._verticalWidth - 16.0;
            double landscapeWidth = this._horizontalWidth - 16.0;
            this._geoItem = new UCItem(portraitWidth, new Thickness(8.0, topMargin, 8.0, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                MapPointFullAttachmentUC fullAttachmentUc = new MapPointFullAttachmentUC();
                Geo geo = this._geo;
                fullAttachmentUc.Initialize(geo);
                return (UserControlVirtualizable)fullAttachmentUc;
            }), (Func<double>)(() => MapPointFullAttachmentUC.CalculateTotalHeight(this.IsHorizontal ? landscapeWidth : portraitWidth)), null, landscapeWidth, this.IsHorizontal);
            return this._geoItem;
        }

        private UCItem CreateMapPointSmall(double topMargin)
        {
            double width = this._verticalWidth - 16.0;
            double landscapeWidth = this._horizontalWidth - 16.0;
            this._geoItem = new UCItem(width, new Thickness(8.0, topMargin, 8.0, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                MapPointSmallAttachmentUC smallAttachmentUc = new MapPointSmallAttachmentUC();
                Geo geo = this._geo;
                smallAttachmentUc.Initialize(geo);
                return (UserControlVirtualizable)smallAttachmentUc;
            }), (Func<double>)(() => 40.0), null, landscapeWidth, this.IsHorizontal);
            return this._geoItem;
        }

        private UCItem CreateMapPlaceFull(double topMargin)
        {
            double portraitWidth = this._verticalWidth - 16.0;
            double landscapeWidth = this._horizontalWidth - 16.0;
            this._geoItem = new UCItem(portraitWidth, new Thickness(8.0, topMargin, 8.0, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                MapPlaceFullAttachmentUC fullAttachmentUc = new MapPlaceFullAttachmentUC();
                Geo geo = this._geo;
                fullAttachmentUc.Initialize(geo);
                return (UserControlVirtualizable)fullAttachmentUc;
            }), (Func<double>)(() => MapPlaceFullAttachmentUC.CalculateTotalHeight(this.IsHorizontal ? landscapeWidth : portraitWidth)), null, landscapeWidth, this.IsHorizontal);
            return this._geoItem;
        }

        private UCItem CreateMapPlaceSmall(double topMargin)
        {
            double width = this._verticalWidth - 16.0;
            double landscapeWidth = this._horizontalWidth - 16.0;
            this._geoItem = new UCItem(width, new Thickness(8.0, topMargin, 8.0, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                MapPlaceSmallAttachmentUC smallAttachmentUc = new MapPlaceSmallAttachmentUC();
                Geo geo = this._geo;
                smallAttachmentUc.Initialize(geo);
                return (UserControlVirtualizable)smallAttachmentUc;
            }), (Func<double>)(() => 72.0), null, landscapeWidth, this.IsHorizontal);
            return this._geoItem;
        }

        private double CreatePhotoVideoAlbum(double topMargin)
        {
            topMargin += 8.0;
            if (this._thumbsItem == null)
            {
                double width = this._isCommentAttachments || this._isMessage ? this._verticalWidth : 480.0;
                Thickness margin = new Thickness(0.0, topMargin, 0.0, 0.0);
                this._thumbsItem = new ThumbsItem(width, margin, this._attachments, this._friendsOnly, this._itemId, this._isCommentAttachments, this._isMessage, this._isHorizontal, this._horizontalWidth, 0.0, this._parentPostId);
                this.VirtualizableChildren.Add((IVirtualizable)this._thumbsItem);
            }
            else
            {
                this._thumbsItem.IsHorizontal = this._isHorizontal;
                this._thumbsItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
            }
            return this._thumbsItem.FixedHeight;
        }

        public void ProcessTapOnThumbsItem()
        {
            ThumbsItem thumbsItem = this.VirtualizableChildren.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(v => v is ThumbsItem)) as ThumbsItem;
            if (thumbsItem == null)
                return;
            thumbsItem.Image_Tap(null);
        }
    }
}
