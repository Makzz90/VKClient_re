using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Utils;

namespace VKMessenger.Library.VirtItems
{
    public class MessageContentItem : VirtualizableItemBase
    {
        private static readonly double MIN_WIDTH = 150.0;
        private static readonly int MAX_LEVEL = 4;
        private static readonly int MARGIN_BETWEEN = 8;
        private double _marginLeft = 12.0;
        private double _marginTop = 6.0;
        private MessageViewModel _mvm;
        private double _height;
        private bool _isHorizontalOrientation;
        private double _verticalWidth;
        private double _horizontalWidth;
        private NewsTextItem _textItem;
        private MessageFooterItem _messageFooterItem;
        private AttachmentsItem _attachmentsItem;
        private GiftCaptionItem _giftCaptionItem;
        private GiftStickersButtonItem _giftStickersButtonItem;
        private bool _handledDelivered;
        private int _lvl;
        private ForwardedHeaderItem _forwardedHeaderItem;
        private List<MessageContentItem> _forwardedList;
        private const bool _isFriendsOnly = false;
        private const bool _isCommentAttachments = false;
        private const bool _isMessage = true;

        public bool IsHorizontalOrientation
        {
            get
            {
                return this._isHorizontalOrientation;
            }
            set
            {
                if (this._isHorizontalOrientation == value)
                    return;
                this._isHorizontalOrientation = value;
                this.Width = this._isHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
                this.GenerateLayout();
            }
        }

        public List<MessageContentItem> ForwardedList
        {
            get
            {
                return this._forwardedList;
            }
        }

        private double ForwardedMarginLeft
        {
            get
            {
                return this._mvm.IsForwarded ? 0.0 : 0.0;
            }
        }

        public override double FixedHeight
        {
            get
            {
                return this._height;
            }
        }

        public MessageContentItem(double verticalWidth, Thickness margin, MessageViewModel mvm, double horizontalWidth, bool isHorizontalOrientation, int lvl = 0)
            : base(verticalWidth, margin, new Thickness())
        {
            this._mvm = mvm;
            this._lvl = lvl;
            this._verticalWidth = verticalWidth;
            this._horizontalWidth = horizontalWidth;
            this._isHorizontalOrientation = isHorizontalOrientation;
            this.Width = isHorizontalOrientation ? horizontalWidth : verticalWidth;
            this.GenerateLayout();
        }

        protected override void GenerateChildren()
        {
            base.GenerateChildren();
            this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
            if (!this._mvm.IsForwarded)
                return;
            Rectangle rect = new Rectangle();//Та самая полоска слева от пересылаемого сообщения
            rect.Fill = Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush;
            rect.Opacity = 0.5;
            rect.Width = 3.0;
            rect.Margin = new Thickness(0.0, this._marginTop, 0.0, 0.0);
            rect.Height = this._height - this._marginTop;
            using (List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(rect).GetEnumerator())
            {
                while (enumerator.MoveNext())
                    this.Children.Add(enumerator.Current);
            }
        }

        protected override void ReleaseResourcesOnUnload()
        {
            base.ReleaseResourcesOnUnload();
            this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
        }

        private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "UIStatusDelivered") || this._mvm.UIStatusDelivered != Visibility.Visible || this._handledDelivered)
                return;
            AttachmentsItem attachmentsItem = Enumerable.FirstOrDefault<IVirtualizable>(this.VirtualizableChildren, (Func<IVirtualizable, bool>)(vc => vc is AttachmentsItem)) as AttachmentsItem;
            ThumbsItem thumbsItem = (attachmentsItem != null ? Enumerable.FirstOrDefault<IVirtualizable>(attachmentsItem.VirtualizableChildren, (Func<IVirtualizable, bool>)(vc => vc is ThumbsItem)) : null) as ThumbsItem;
            if (thumbsItem != null)
            {
                VirtualizableState currentState = thumbsItem.CurrentState;
                thumbsItem.ChangeState(VirtualizableState.Unloaded);
                thumbsItem.PrepareThumbsList();
                thumbsItem.ChangeState(currentState);
            }
            this._handledDelivered = true;
        }

        private void GenerateLayout()
        {
            double num1 = this._marginTop;
            if (this._mvm.IsForwarded)//Идёт добавление шапки
            {
                if (this._forwardedHeaderItem == null)
                {
                    this._forwardedHeaderItem = new ForwardedHeaderItem(this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0), this._mvm);
                    base.VirtualizableChildren.Add(this._forwardedHeaderItem);
                }
                num1 = num1 + this._forwardedHeaderItem.FixedHeight + (double)MessageContentItem.MARGIN_BETWEEN;
            }
            string body = this._mvm.Message.body;
            bool flag1 = !string.IsNullOrWhiteSpace(body);
            bool flag2 = this._mvm.Attachments != null && Enumerable.Any<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Gift));
            bool isStickersGift = false;
            long stickersProductId = 0;
            bool isForwarded = this._mvm.IsForwarded;
            bool flag3 = this._mvm.Message.@out == 0;
            if (flag2)
            {
                AttachmentViewModel m0 = Enumerable.FirstOrDefault<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Gift));
                Gift gift = m0 != null ? m0.Gift : null;
                if (gift != null)
                {
                    stickersProductId = gift.stickers_product_id;
                    isStickersGift = (ulong)stickersProductId > 0UL;
                }
            }
            if (flag1 && !flag2)
            {
                if (this._textItem == null)
                {
                    this._textItem = new NewsTextItem(this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0), body, false, null, 25.33, new FontFamily("Segoe WP Semilight"), 32.0, (Brush)(Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush), this._isHorizontalOrientation, this._horizontalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, (HorizontalAlignment)0, "", (TextAlignment)1, true, null, false, false);
                    base.VirtualizableChildren.Add((IVirtualizable)this._textItem);
                }
                this._textItem.IsHorizontalOrientation = this.IsHorizontalOrientation;
                this._textItem.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0);
                num1 += this._textItem.FixedHeight;
            }
            if (this._mvm.Attachments != null && ((Collection<AttachmentViewModel>)this._mvm.Attachments).Count > 0)
            {
                if (this._textItem != null)
                    num1 += (double)MessageContentItem.MARGIN_BETWEEN;
                Geo geo1 = (Geo)Enumerable.FirstOrDefault<Geo>(Enumerable.Select<AttachmentViewModel, Geo>(Enumerable.Where<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a => a.Geo != null)), (Func<AttachmentViewModel, Geo>)(a => a.Geo)));
                List<Attachment> list = Enumerable.ToList<Attachment>(Enumerable.Select<AttachmentViewModel, Attachment>(Enumerable.Where<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a => a.Attachment != null)), (Func<AttachmentViewModel, Attachment>)(a => a.Attachment)));
                if (this._attachmentsItem == null)
                {
                    double num2 = this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft;
                    double num3 = this._horizontalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft;
                    if (num2 > MessageContentItem.MIN_WIDTH)
                    {
                        string str = this._mvm.Message.from_id != 0L ? this._mvm.Message.from_id.ToString() : "";
                        double width = num2;
                        Thickness margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0);
                        List<Attachment> attachments = list;
                        Geo geo2 = geo1;
                        string itemId = str;
                        int num4 = 0;
                        int num5 = 0;
                        int num6 = 1;
                        int num7 = this._isHorizontalOrientation ? 1 : 0;
                        double horizontalWidth = num3;
                        int num8 = this._mvm.Message.@out == 1 ? 1 : 0;
                        int num9 = 0;
                        string hyperlinkId = "";
                        // ISSUE: variable of the null type

                        User parentDialogUser = this._mvm != null ? this._mvm.AssociatedUser : null;
                        int num10 = this._mvm != null ? (this._mvm.IsForwarded ? 1 : 0) : 0;
                        this._attachmentsItem = new AttachmentsItem(width, margin, attachments, geo2, itemId, num4 != 0, num5 != 0, num6 != 0, num7 != 0, horizontalWidth, num8 != 0, num9 != 0, hyperlinkId, null, parentDialogUser, num10 != 0);
                        base.VirtualizableChildren.Add((IVirtualizable)this._attachmentsItem);
                    }
                }
                if (this._attachmentsItem != null)
                {
                    this._attachmentsItem.IsHorizontal = this.IsHorizontalOrientation;
                    this._attachmentsItem.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0);
                    num1 += this._attachmentsItem.FixedHeight;
                }
            }
            if (flag2)
            {
                if (this._giftCaptionItem == null)
                {
                    this._giftCaptionItem = new GiftCaptionItem(this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, this._horizontalWidth - this._marginLeft - this.ForwardedMarginLeft, this.IsHorizontalOrientation, new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0), isStickersGift, isForwarded);
                    base.VirtualizableChildren.Add((IVirtualizable)this._giftCaptionItem);
                }
                this._giftCaptionItem.IsLandscape = this.IsHorizontalOrientation;
                this._giftCaptionItem.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0);
                num1 += this._giftCaptionItem.FixedHeight;
                if (flag1)
                {
                    if (this._textItem == null)
                    {
                        SolidColorBrush solidColorBrush = isForwarded ? Application.Current.Resources["PhoneAlmostBlackBrush"] as SolidColorBrush : Application.Current.Resources["PhoneDialogGiftMessageForegroundBrush"] as SolidColorBrush;
                        this._textItem = new NewsTextItem(this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0), body, false, null, 21.3, new FontFamily("Segoe WP"), 28.0, (Brush)solidColorBrush, this._isHorizontalOrientation, this._horizontalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, isForwarded ? (HorizontalAlignment)0 : (HorizontalAlignment)1, "", isForwarded ? (TextAlignment)1 : (TextAlignment)0, true, null, false, false);
                        base.VirtualizableChildren.Add((IVirtualizable)this._textItem);
                    }
                    this._textItem.IsHorizontalOrientation = this.IsHorizontalOrientation;
                    this._textItem.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0);
                    num1 += this._textItem.FixedHeight;
                }
                if (flag3 & isStickersGift && !isForwarded)
                {
                    if (flag1)
                        num1 += (double)(MessageContentItem.MARGIN_BETWEEN * 2);
                    if (this._giftStickersButtonItem == null)
                    {
                        this._giftStickersButtonItem = new GiftStickersButtonItem(this._verticalWidth, this._horizontalWidth, this.IsHorizontalOrientation, new Thickness(0.0, num1, 0.0, 0.0), stickersProductId);
                        base.VirtualizableChildren.Add(this._giftStickersButtonItem);
                    }
                    this._giftStickersButtonItem.IsLandscape = this.IsHorizontalOrientation;
                    this._giftStickersButtonItem.Margin = new Thickness(0.0, num1, 0.0, 0.0);
                    num1 += this._giftStickersButtonItem.FixedHeight;
                }
            }
            if (this._mvm.ForwardedMessages != null && (this._mvm.ForwardedMessages).Count > 0)
            {
                if (this._textItem != null || this._attachmentsItem != null)
                    num1 += (double)MessageContentItem.MARGIN_BETWEEN;
                if (this._forwardedList == null)
                {
                    this._forwardedList = new List<MessageContentItem>();
                    using (IEnumerator<MessageViewModel> enumerator = (this._mvm.ForwardedMessages).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MessageViewModel current = enumerator.Current;
                            double verticalWidth = this._verticalWidth - this._marginLeft - this.ForwardedMarginLeft;
                            double horizontalWidth = this._horizontalWidth - this._marginLeft - this.ForwardedMarginLeft;
                            if (verticalWidth > MessageContentItem.MIN_WIDTH && this._lvl < MessageContentItem.MAX_LEVEL)
                            {
                                MessageContentItem messageContentItem = new MessageContentItem(verticalWidth, new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0), current, horizontalWidth, this.IsHorizontalOrientation, this._lvl + 1);
                                base.VirtualizableChildren.Add((IVirtualizable)messageContentItem);
                                this._forwardedList.Add(messageContentItem);
                                num1 += messageContentItem.FixedHeight;
                                num1 += (double)MessageContentItem.MARGIN_BETWEEN;
                            }
                        }
                    }
                }
                else
                {
                    foreach (MessageContentItem forwarded in this._forwardedList)
                    {
                        forwarded.IsHorizontalOrientation = this.IsHorizontalOrientation;
                        forwarded.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, num1, 0.0, 0.0);
                        num1 += forwarded.FixedHeight;
                        num1 += (double)MessageContentItem.MARGIN_BETWEEN;
                    }
                }
                if (Enumerable.Any<MessageContentItem>(this._forwardedList))
                    num1 -= (double)MessageContentItem.MARGIN_BETWEEN;
            }
            if (!this._mvm.IsForwarded)
            {
                if (this._messageFooterItem == null)
                {
                    this._messageFooterItem = new MessageFooterItem(this._verticalWidth - this._marginLeft * 2.0, new Thickness(this._marginLeft, num1, 0.0, 0.0), this._mvm, this.IsHorizontalOrientation, this._horizontalWidth - this._marginLeft * 2.0);
                    base.VirtualizableChildren.Add(this._messageFooterItem);
                }
                else
                {
                    this._messageFooterItem.IsHorizontalOrientation = this.IsHorizontalOrientation;
                    this._messageFooterItem.Margin = new Thickness(this._marginLeft, num1, 0.0, 0.0);
                }
                num1 += this._messageFooterItem.FixedHeight + (double)MessageContentItem.MARGIN_BETWEEN;
            }
            if (!this._mvm.IsForwarded)
                num1 += this._marginTop;
            this._height = num1;
        }
    }
}
