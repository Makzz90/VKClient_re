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
        private readonly Doc _graffitiDoc;//NEW: 4.8.0
        private bool _containsPhotoVideoAlbum;
        private GiftItem _giftItem;
        private GraffitiItem _graffitiItem;//NEW: 4.8.0
        private WallPostAttachmentItem _wallPostAttItem;
        private CommentAttachmentItem _commentAttItem;
        private StickerItem _stickerItem;
        private UCItem _productItem;
        private GenericAttachmentItem _productSmallItem;
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

        public AttachmentsItem(double width, Thickness margin, List<Attachment> attachments, Geo geo, string itemId, bool friendsOnly = false, bool isCommentAttachments = false, bool isMessage = false, bool isHorizontal = false, double horizontalWidth = 0.0, bool rightAlign = false, bool wallPostWithCommentsPage = false, string hyperlinkId = "")
            : base(width, margin, new Thickness())
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
                    if (type == "photo")
                    {
                        this._photoAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "link")
                    {
                        Link link = attachment.link;
                        if ((link != null ? link.photo : null) != null && link.photo.width > 0 && link.photo.height > 0)
                        {
                            this._link = link;
                            continue;
                        }
                        continue;
                    }
                    else if (type == "audio")
                    {
                        this._audioAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "doc")
                    {
                        if (attachment.doc.IsGraffiti)// UPDATE: 4.8.0
                        {
                            this._graffitiDoc = attachment.doc;
                            continue;
                        }
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
                    else if (type == "album")
                    {
                        this._albumAttachments.Add(attachment);
                        continue;
                    }
                    else if (type == "poll")
                    {
                        this._poll = attachment.poll;
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
                    else if (type == "market_album")
                    {
                        this._marketAlbumAttachments.Add(attachment);
                        continue;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("AttachmentsItem.AttachmentsItem " + type);
                    }

                    this._wallAttachment = attachment.wall;
                }
            }
            this._geo = geo;
            this._friendsOnly = friendsOnly;
            this._isCommentAttachments = isCommentAttachments;
            this._isMessage = isMessage;
            this._isHorizontal = isHorizontal;
            this._horizontalWidth = horizontalWidth;
            this._verticalWidth = width;
            this._wallPostWithCommentsPage = wallPostWithCommentsPage;
            this._rightAlign = rightAlign;
            if (this._isHorizontal)
                this.Width = horizontalWidth;
            this.CreateOrUpdateLayout();
        }

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

            double graffiti = this.CreateGraffiti(num1);// UPDATE: 4.8.0
            if (graffiti != 0.0)
            {
                this.IsLastAttachmentMedia = true;
                num1 += 8.0 + graffiti;
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
            topMargin += 8.0;
            if (this._giftItem == null)
            {
                this._giftItem = new GiftItem(this._verticalWidth, new Thickness(0.0, topMargin, 0.0, 0.0), this._giftAttachment, this._horizontalWidth, this._isHorizontal);
                this.VirtualizableChildren.Add((IVirtualizable)this._giftItem);
            }
            else
            {
                this._giftItem.IsHorizontal = this._isHorizontal;
                this._giftItem.Margin = new Thickness(0.0, topMargin, 0.0, 0.0);
            }
            return this._giftItem.FixedHeight;
        }

        // NEW: 4.8.0
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
            double num = this._isMessage || this._isCommentAttachments ? 16.0 : 0.0;
            double width = this._verticalWidth + num * 2.0;
            double horizontalWidth = this._horizontalWidth + num * 2.0;
            if (this._isMessage)
            {
                if (this._productSmallItem != null)
                {
                    this._productSmallItem.Margin = new Thickness(-num, topMargin, -num, 0.0);
                    this._productSmallItem.IsHorizontal = this.IsHorizontal;
                }
                else
                {
                    this._productSmallItem = new GenericAttachmentItem(width, new Thickness(-num, topMargin, -num, 0.0), this._attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>)(a => a.market != null)), this._isHorizontal, horizontalWidth, this._hyperlinkId);
                    this.VirtualizableChildren.Add((IVirtualizable)this._productSmallItem);
                }
                return this._productSmallItem.FixedHeight;
            }
            if (this._productItem != null)
            {
                this._productItem.Margin = new Thickness(-num, topMargin, -num, 0.0);
            }
            else
            {
                Link link = new Link();
                link.title = this._product.title;
                link.caption = CommonResources.Product;
                link.photo = new Photo()
                {
                    photo_75 = this._product.thumb_photo
                };
                LinkProduct linkProduct = new LinkProduct(this._product);
                link.product = linkProduct;
                string str = string.Format("http://vk.com/product{0}_{1}", (object)this._product.owner_id, (object)this._product.id);
                link.url = str;
                this._productItem = this.CreateSnippetItem(link, topMargin);
                this.VirtualizableChildren.Add((IVirtualizable)this._productItem);
            }
            return this._productItem.FixedHeight;
        }

        private double CreateSnippet(double topMargin)
        {
            if (this._link == null || this._isMessage)
                return 0.0;
            topMargin += 8.0;
            double num = this._isCommentAttachments ? 0.0 : 8.0;
            if (this._linkItem != null)
            {
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
            double num1 = this._isCommentAttachments ? 0.0 : 8.0;
            double portraitWidth = this._verticalWidth - num1 * 2.0;
            double num2 = (double)ScaleFactor.GetRealScaleFactor() / 100.0;
            double num3 = portraitWidth / 2.0 * num2;
            bool isBigSnippet = (double)link.photo.width >= num3;
            NewsLinkUCBase tmpUC = !isBigSnippet ? (NewsLinkUCBase)new NewsLinkMediumUC() : (NewsLinkUCBase)new NewsLinkUC();
            tmpUC.Initialize(link, portraitWidth);
            return new UCItem(portraitWidth, new Thickness(num1, topMargin, num1, 0.0), (Func<UserControlVirtualizable>)(() =>
            {
                NewsLinkUCBase newsLinkUcBase = !isBigSnippet ? (NewsLinkUCBase)new NewsLinkMediumUC() : (NewsLinkUCBase)new NewsLinkUC();
                newsLinkUcBase.Initialize(link, portraitWidth);
                return (UserControlVirtualizable)newsLinkUcBase;
            }), (Func<double>)(() => tmpUC.CalculateTotalHeight()), (Action<UserControlVirtualizable>)null, 0.0, false);
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
                }), (Func<double>)(() => tmpUC.CalculateTotalHeight()), (Action<UserControlVirtualizable>)null, 0.0, false);
                this.VirtualizableChildren.Add((IVirtualizable)this._pollItem);
            }
            return this._pollItem.FixedHeight;
        }

        private double CreateImageDocs(double topMargin)
        {
            if (this._docImageAttachments.Count == 0)
                return 0.0;
            double top = topMargin + 8.0;
            int num = 0;
            double left = this._isMessage || this._isCommentAttachments ? -16.0 : 0.0;
            if (this._docImageItemsList != null)
            {
                foreach (UCItem docImageItems in this._docImageItemsList)
                {
                    docImageItems.Margin = new Thickness(left, top, 0.0, 0.0);
                    top += docImageItems.FixedHeight;
                    ++num;
                }
            }
            else
            {
                this._docImageItemsList = new List<UCItem>();
                List<IEnumerable<Attachment>> list1 = this._docImageAttachments.Partition<Attachment>(this._isMessage || this._isCommentAttachments ? 1 : 2).ToList<IEnumerable<Attachment>>();
                for (int index = 0; index < list1.Count; ++index)
                {
                    List<Attachment> list2 = list1[index].ToList<Attachment>();
                    Doc doc1 = list2[0].doc;
                    Doc doc2 = (Doc)null;
                    if (list2.Count > 1)
                        doc2 = list2[1].doc;
                    UCItem ucItem = new UCItem(this._verticalWidth - left, new Thickness(left, top, 0.0, 0.0), (Func<UserControlVirtualizable>)(() =>
                    {
                        DocImageAttachmentUC imageAttachmentUc = new DocImageAttachmentUC();
                        List<Attachment> attachments = this._attachments;
                        //Doc doc1 = doc1;
                        //Doc doc2 = doc2;
                        imageAttachmentUc.Initialize(attachments, doc1, doc2);
                        return (UserControlVirtualizable)imageAttachmentUc;
                    }), (Func<double>)(() => 152.0), (Action<UserControlVirtualizable>)null, 0.0, false);
                    if (this._isCommentAttachments)
                        CurrentMediaSource.GifPlaySource = StatisticsActionSource.comments;
                    else if (this._isMessage)
                        CurrentMediaSource.GifPlaySource = StatisticsActionSource.messages;
                    this.VirtualizableChildren.Add((IVirtualizable)ucItem);
                    this._docImageItemsList.Add(ucItem);
                    top += ucItem.FixedHeight;
                    ++num;
                }
            }
            if (num <= 0)
                return 0.0;
            return top - topMargin - 8.0;
        }

        private double CreateGeneric(double topMargin)
        {
            topMargin += this._isMessage ? 0.0 : 8.0;
            double top = topMargin;
            int num1 = 0;
            double num2 = this._isMessage || this._isCommentAttachments ? 16.0 : 0.0;
            double width = this._verticalWidth + num2 * 2.0;
            double horizontalWidth = this._horizontalWidth + num2 * 2.0;
            if (this._genericItemsList != null)
            {
                foreach (GenericAttachmentItem genericItems in this._genericItemsList)
                {
                    genericItems.Margin = new Thickness(-num2, top, -num2, 0.0);
                    genericItems.IsHorizontal = this.IsHorizontal;
                    top += genericItems.FixedHeight;
                    ++num1;
                }
            }
            else
            {
                this._genericItemsList = new List<GenericAttachmentItem>();
                foreach (Attachment attachment in this._attachments.Where<Attachment>((Func<Attachment, bool>)(a =>
                {
                    if (a.doc != null && !new DocumentHeader(a.doc, 0, false).HasThumbnail || (a.note != null || a.Page != null))
                        return true;
                    if (a.link != null)
                        return this._linkItem == null;
                    return false;
                })))
                {
                    GenericAttachmentItem genericAttachmentItem = new GenericAttachmentItem(width, new Thickness(-num2, top, -num2, 0.0), attachment, this._isHorizontal, horizontalWidth, this._hyperlinkId);
                    this.VirtualizableChildren.Add((IVirtualizable)genericAttachmentItem);
                    this._genericItemsList.Add(genericAttachmentItem);
                    top += genericAttachmentItem.FixedHeight;
                    ++num1;
                }
            }
            if (num1 <= 0)
                return 0.0;
            return top - topMargin;
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
            }), (Func<double>)(() => MapPointSimpleAttachmentUC.CalculateTotalHeight(this.IsHorizontal ? landscapeWidth : portraitWidth)), (Action<UserControlVirtualizable>)null, landscapeWidth, this.IsHorizontal);
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
            }), (Func<double>)(() => MapPointFullAttachmentUC.CalculateTotalHeight(this.IsHorizontal ? landscapeWidth : portraitWidth)), (Action<UserControlVirtualizable>)null, landscapeWidth, this.IsHorizontal);
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
            }), (Func<double>)(() => 40.0), (Action<UserControlVirtualizable>)null, landscapeWidth, this.IsHorizontal);
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
            }), (Func<double>)(() => MapPlaceFullAttachmentUC.CalculateTotalHeight(this.IsHorizontal ? landscapeWidth : portraitWidth)), (Action<UserControlVirtualizable>)null, landscapeWidth, this.IsHorizontal);
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
            }), (Func<double>)(() => 72.0), (Action<UserControlVirtualizable>)null, landscapeWidth, this.IsHorizontal);
            return this._geoItem;
        }

        private double CreatePhotoVideoAlbum(double topMargin)
        {
            topMargin += 8.0;
            if (this._thumbsItem == null)
            {
                this._thumbsItem = new ThumbsItem(this._isCommentAttachments || this._isMessage ? this._verticalWidth : 480.0, new Thickness(0.0, topMargin, 0.0, 0.0), this._attachments, this._friendsOnly, this._itemId, this._isCommentAttachments, this._isMessage, this._isHorizontal, this._horizontalWidth, 0.0);
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
