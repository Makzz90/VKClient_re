using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
    public class WallPostAttachmentItem : VirtualizableItemBase
    {
        private WallPost _wallPost;
        private double _marginLeft = 12.0;
        private double _marginTop = 40.0;
        private double _height;
        private double _verticalWidth=356;
        private double _horizontalWidth=512;

        public override double FixedHeight//после GenerateLayout
        {
            get
            {
                return this._height;
            }
        }
        
        public WallPostAttachmentItem(WallPost wallPost, Thickness margin) : base(300.0, margin, new Thickness())
        {
            this._wallPost = wallPost;
            //
            this.GenerateLayout();
        }

        private void GenerateLayout()
        {
            double num1 = this._marginTop;
            /*
            VirtualizableImage a = new VirtualizableImage(40.0, 40.0, new Thickness(10, 0, 0, 0), "https://pp.userapi.com/c637424/v637424389/1abb0/RnkBYW_Ucjw.jpg", new Action<VirtualizableImage>(this.imageTap), "1", false, true, (Stretch)3, null, -1.0, false, true);

            base.VirtualizableChildren.Add(a);
            */


            //ForwardedHeaderItem:
            /*
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Foreground = Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush;
            textBlock1.FontFamily = new FontFamily("Segoe WP Semibold");
            textBlock1.Margin = (new Thickness(60.0, -8.0, 0.0, 0.0));
            textBlock1.Text = ("this._mvm.UIUserName" ?? "");
            this.Children.Add(textBlock1);
            */
            TextBlock textBlock3 = new TextBlock();
            textBlock3.Foreground = Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush;
            textBlock3.Margin = (new Thickness(60.0, 18.0, 0.0, 0.0));
            textBlock3.Text = UIStringFormatterHelper.FormatDateForMessageUI(Extensions.UnixTimeStampToDateTime((double)this._wallPost.date, true));
            this.Children.Add(textBlock3);
            
            NewsTextItem _textItem = new NewsTextItem(this._verticalWidth - this._marginLeft * 2.0 - 3.0, new Thickness(this._marginLeft + 0, num1, 0.0, 0.0), this._wallPost.text, false, null, 20.0/*25.33*/, new FontFamily("Segoe WP Semilight"), 24.0/*32.0*/, (Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush), true, this._horizontalWidth - this._marginLeft * 2.0 - 160.0, HorizontalAlignment.Left, "", TextAlignment.Left, true, null, false, false);
            num1 += _textItem.FixedHeight;
            base.VirtualizableChildren.Add(_textItem);

            AttachmentsItem a0 = new AttachmentsItem(base.Width, new Thickness(this._marginLeft, num1, 0, 0), this._wallPost.attachments, this._wallPost.geo, this._wallPost.id.ToString(), false, true);
            num1 += a0.FixedHeight;
            base.VirtualizableChildren.Add(a0);
            num1 += this._marginTop;
            this._height = num1;
        }

        protected override void GenerateChildren()
        {
            base.GenerateChildren();
            HyperlinkButton hyperlinkButton = new HyperlinkButton();
            hyperlinkButton.Margin = new Thickness(0, -8.0, 0.0, 0.0);
            hyperlinkButton.Content = CommonResources.Conversation_WallPost;
            hyperlinkButton.Click += new RoutedEventHandler(this.hypLink_Click);
            this.Children.Add(hyperlinkButton);

            Rectangle rect = new Rectangle();//Та самая полоска слева от пересылаемого сообщения
            rect.Fill = Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush;
            rect.Opacity = 0.5;
            rect.Width = 3.0;
            rect.Height = this._height - this._marginTop;
            this.Children.Add(rect);

            
        }
        //
        private void imageTap(VirtualizableImage obj)
        {
            //long uid = this._mvm.AssociatedUser.uid;
           // if (uid == 0L)
            //    return;
           // Navigator.Current.NavigateToUserProfile(uid, "", "", false);
        }
        //

        private void hypLink_Click(object sender, RoutedEventArgs e)
        {
            long toId = this._wallPost.to_id;
            long id = this._wallPost.id;
            long pollId = 0;
            long pollOwnerId = 0;
            if (this._wallPost.attachments != null)
            {
                Attachment attachment = Enumerable.FirstOrDefault<Attachment>(this._wallPost.attachments, (Func<Attachment, bool>)(a => a.poll != null));
                if (attachment != null)
                {
                    pollId = attachment.poll.poll_id;
                    pollOwnerId = this._wallPost.copy_history == null || this._wallPost.copy_history.Count <= 0 ? this._wallPost.to_id : this._wallPost.copy_history[0].owner_id;
                }
            }
            Navigator.Current.NavigateToWallPostComments(id, toId, false, pollId, pollOwnerId, "");
        }
    }
}
