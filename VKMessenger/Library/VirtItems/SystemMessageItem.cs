using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;

namespace VKMessenger.Library.VirtItems
{
    public class SystemMessageItem : VirtualizableItemBase
    {
        private readonly double ATTACHMENT_WIDTH = 200.0;
        private MessageViewModel _mvm;
        private double _verticalWidth;
        private double _horizontalWidth;
        private bool _isHorizontal;
        private NewsTextItem _newsTextItem;
        private UnreadItem _unreadItem;
        private AttachmentsItem _attachmentsItem;
        private double _fixedHeight;
        private const bool _friendsOnly = false;
        private const bool _isCommentAttachments = false;
        private const bool _isMessage = true;

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

        public bool IsUnreadItem
        {
            get
            {
                return this._mvm.Message.action == "UNREAD_MESSAGES";
            }
        }

        public override double FixedHeight
        {
            get
            {
                return this._fixedHeight;
            }
        }

        public SystemMessageItem(double verticalWidth, Thickness margin, MessageViewModel mvm, double horizontalWidth, bool isHorizontal)
            : base(verticalWidth, margin, new Thickness())
        {
            this._mvm = mvm;
            this._verticalWidth = verticalWidth;
            this._horizontalWidth = horizontalWidth;
            this._isHorizontal = isHorizontal;
            this.Width = this._isHorizontal ? horizontalWidth : verticalWidth;
            this.CreateOrUpdateLayout();
        }

        private void CreateOrUpdateLayout()
        {
            double num;
            if (this.IsUnreadItem)
            {
                if (this._unreadItem == null)
                {
                    this._unreadItem = new UnreadItem(this._verticalWidth, new Thickness(), this._isHorizontal, this._horizontalWidth);
                    this.VirtualizableChildren.Add((IVirtualizable)this._unreadItem);
                }
                else
                    this._unreadItem.IsHorizontal = this._isHorizontal;
                num = this._unreadItem.FixedHeight;
            }
            else
            {
                if (this._newsTextItem == null)
                {
                    this._newsTextItem = new NewsTextItem(this._verticalWidth, new Thickness(), this.GetText(), false, null, 0.0, null, 0.0, (Brush)(Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush), this._isHorizontal, this._horizontalWidth, (HorizontalAlignment)1, "", (TextAlignment)0, true, null, false, false);
                    this.VirtualizableChildren.Add((IVirtualizable)this._newsTextItem);
                }
                else
                    this._newsTextItem.IsHorizontalOrientation = this._isHorizontal;
                num = this._newsTextItem.FixedHeight + 12.0;
                if (this._mvm.Message.attachments != null && this._mvm.Message.attachments.Count > 0)
                {
                    if (this._attachmentsItem == null)
                    {
                        this._attachmentsItem = new AttachmentsItem(this.ATTACHMENT_WIDTH, new Thickness((this.Width - this.ATTACHMENT_WIDTH) / 2.0, num, 0.0, 0.0), this._mvm.Message.attachments, null, "", false, false, true, this._isHorizontal, this.ATTACHMENT_WIDTH, false, false, "", null, null, false);
                        this.VirtualizableChildren.Add((IVirtualizable)this._attachmentsItem);
                    }
                    else
                    {
                        this._attachmentsItem.IsHorizontal = this._isHorizontal;
                        this._attachmentsItem.Margin = new Thickness((this.Width - this.ATTACHMENT_WIDTH) / 2.0, num, 0.0, 0.0);
                    }
                    num += this._attachmentsItem.FixedHeight;
                }
            }
            this._fixedHeight = num;
        }

        private string GetText()
        {
            return SystemMessageTextHelper.GenerateText(this._mvm.Message, this._mvm.AssociatedUser, this._mvm.AssociatedUser2, true);
        }
    }
}
