using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.BLExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class CommentItem : VirtualizableItemBase, IHandle<CommentEdited>, IHandle
    {
        private static ThemeHelper _themeHelper = new ThemeHelper();
        private Thickness _mainTextBlockMargin = new Thickness(74.0, 40.0, 0.0, 0.0);
        private Comment _comment;
        private User _user;
        private User _user2;
        private Group _group;
        private double _height;
        private double _textWidth;
        private TextItem _textTimeItem;
        private long _owner_id;
        //private long _ownerId;
        private List<MenuItem> _contextMenu;
        private Action<CommentItem> _deleteCommentCallback;
        private Action<CommentItem> _replyCommentCallback;
        private Action<CommentItem> _editCommentCallback;
        private Action<CommentItem> _tapCommentCallback;
        private MenuItem _deleteCommentMenuItem;
        private MenuItem _replyMenuItem;
        private MenuItem _copyMenuItem;
        //private MenuItem _likeMenuItem;
        //private MenuItem _unlikeMenuItem;
        private MenuItem _editMenuItem;
        private MenuItem _likesMenuItem;
        private MenuItem _reportMenuItem;
        private Action<CommentItem> _seeAllLikesCallback;
        private string _extraText;
        private string _thumbSrc;
        private bool _preview;
        private LikeObjectType _likeObjectType;
        private bool _isNotificationComment;
        private string _highlightedText;
        private const bool _friendsOnly = false;
        private const bool _isCommentAttachmends = true;
        private double _topMarginDate;
        private TextItem _textBlockName;
        private StackPanel _replyAndLikesContainer;
        private TextBlock _likesBlock;
        private Border _likesIcon;
        private const int LIKES_REPLY_HEIGHT = 32;
        private const int LIKES_REPLY_MARGIN_BOTTOM = 4;

        public Comment Comment
        {
            get
            {
                return this._comment;
            }
        }

        public long OwnerId
        {
            get
            {
                return this._owner_id;
            }
        }

        public string Name
        {
            get
            {
                string str = "";
                if (this._user != null)
                    str = this._user.Name;
                if (this._group != null)
                    str = this._group.name ?? "";
                return str;
            }
        }

        public string NameWithoutLastName
        {
            get
            {
                string str = "";
                if (this._user != null)
                    str = this._user.first_name;
                if (this._group != null)
                    str = this._group.name ?? "";
                return str;
            }
        }

        public string ImageSrc
        {
            get
            {
                string str = "";
                if (this._user != null)
                    str = this._user.photo_max;
                if (this._group != null)
                    str = this._group.photo_200;
                return str;
            }
        }

        public string DateText
        {
            get
            {
                string str = UIStringFormatterHelper.FormatDateTimeForUI(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double)this._comment.date, true));
                if (this._comment.reply_to_uid > 0L && this._user2 != null)
                    str = str + " " + string.Format(CommonResources.ToSmbdFrm, this._user2.first_name_dat);
                else if (this._comment.reply_to_uid < 0L)
                    str = str + " " + CommonResources.ToCommunity;
                if (!string.IsNullOrEmpty(this._extraText))
                    str = str + " " + this._extraText;
                return str;
            }
        }

        public string LikesCountStr
        {
            get
            {
                int count = this._comment.likes.count;
                if (count != 0)
                    return count.ToString();
                return "";
            }
        }

        public int LikesCount
        {
            get
            {
                if (this._comment.likes != null)
                    return this._comment.likes.count;
                return 0;
            }
        }

        private bool CanReport
        {
            get
            {
                if (this._comment.from_id == AppGlobalStateManager.Current.LoggedInUserId)
                    return false;
                if (this._likeObjectType != LikeObjectType.comment && this._likeObjectType != LikeObjectType.photo_comment)
                    return this._likeObjectType == LikeObjectType.video_comment;
                return true;
            }
        }

        public override double FixedHeight
        {
            get
            {
                if (!this._isNotificationComment)
                    return this._height;
                return this._height + 16.0;
            }
        }

        public CommentItem(double width, Thickness margin, LikeObjectType likeObjectType, Action<CommentItem> deleteCommentCallback, Action<CommentItem> replyCommentCallback, Action<CommentItem> editCommentCallback, long owner_id, Comment comment, User user, User user2, Group group, Action<CommentItem> tapCommentCallback = null, string extraText = "", string thumbSrc = "", Action<CommentItem> seeAllLikesCallback = null, bool preview = false, bool isNotificationComment = false, string hightlightedText = "")
            : base(width, margin, new Thickness())
        {
            this._preview = preview;
            this._extraText = extraText;
            this._thumbSrc = thumbSrc;
            this._deleteCommentCallback = deleteCommentCallback;
            this._replyCommentCallback = replyCommentCallback;
            this._editCommentCallback = editCommentCallback;
            this._tapCommentCallback = tapCommentCallback;
            this._seeAllLikesCallback = seeAllLikesCallback;
            this._likeObjectType = likeObjectType;
            this._comment = comment;
            this._user = user;
            this._user2 = user2;
            this._group = group;
            if (this._group == null && this._comment.from_id < 0L)
                this._group = GroupsService.Current.GetCachedGroup(-this._comment.from_id);
            this._highlightedText = hightlightedText;
            this._owner_id = owner_id;
            this._isNotificationComment = isNotificationComment;
            this.CreateVirtualizableChildren();
            this.UpdateLikesIndicator();
            this.HookupTapEvent();
            EventAggregator.Current.Subscribe(this);
        }

        private void HookupTapEvent()
        {
            ((UIElement)this._view).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((s, e) =>
            {
                if (this._tapCommentCallback == null)
                    return;
                e.Handled = true;
                this._tapCommentCallback(this);
            }));
        }

        private void UpdateContextMenu()
        {
            this._contextMenu = new List<MenuItem>();
            if (this._replyCommentCallback != null)
            {
                this._replyMenuItem = new MenuItem();
                this._replyMenuItem.Header = CommonResources.CommentItem_Reply;
                // ISSUE: method pointer
                this._replyMenuItem.Click += new RoutedEventHandler(this.replyMenuItem_Tap);
                this._contextMenu.Add(this._replyMenuItem);
            }
            if (this._comment.likes.count > 0)
            {
                this._likesMenuItem = new MenuItem();
                this._likesMenuItem.Header = CommonResources.Comment_Likes;
                // ISSUE: method pointer
                this._likesMenuItem.Click += new RoutedEventHandler(this._likesMenuItem_Tap);
                this._contextMenu.Add(this._likesMenuItem);
            }
            if (this._comment.CanEdit() && this._editCommentCallback != null)
            {
                this._editMenuItem = new MenuItem();
                this._editMenuItem.Header = CommonResources.CommentItem_EditComment;
                // ISSUE: method pointer
                this._editMenuItem.Click += new RoutedEventHandler(this._editMenuItem_Tap);
                this._contextMenu.Add(this._editMenuItem);
            }
            this._copyMenuItem = new MenuItem();
            this._copyMenuItem.Header = CommonResources.CommentItem_Copy;
            // ISSUE: method pointer
            this._copyMenuItem.Click += new RoutedEventHandler(this.copyMenuItem_Tap);
            this._contextMenu.Add(this._copyMenuItem);
            if (this.CanReport)
            {
                this._reportMenuItem = new MenuItem();
                this._reportMenuItem.Header = CommonResources.Report.ToLowerInvariant();
                // ISSUE: method pointer
                this._reportMenuItem.Click += new RoutedEventHandler(this._reportMenuItem_Click);
                this._contextMenu.Add(this._reportMenuItem);
            }
            if (this._comment.CanDelete(this._owner_id) && this._deleteCommentCallback != null)
            {
                this._deleteCommentMenuItem = new MenuItem();
                this._deleteCommentMenuItem.Header = CommonResources.CommentItem_DeleteComment;
                // ISSUE: method pointer
                this._deleteCommentMenuItem.Click += new RoutedEventHandler(this.deleteCommentMenuItem_Tap);
                this._contextMenu.Add(this._deleteCommentMenuItem);
            }
            this._textBlockName.SetMenu((List<MenuItem>)Enumerable.ToList<MenuItem>(this._contextMenu));
        }

        private void _reportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ReportContentHelper.ReportComment(this._comment.from_id, this._comment.id, this._likeObjectType);
        }

        private void _likesMenuItem_Tap(object sender, RoutedEventArgs e)
        {
            Navigator.Current.NavigateToLikesPage(this.OwnerId, this.Comment.cid, (int)this._likeObjectType, this.LikesCount, false);
        }

        private void copyMenuItem_Tap(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this._comment.text);
        }

        private void replyMenuItem_Tap(object sender, RoutedEventArgs e)
        {
            this._replyCommentCallback(this);
        }

        private void deleteCommentMenuItem_Tap(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.DeleteComment, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this._deleteCommentCallback(this);
        }

        private void _editMenuItem_Tap(object sender, RoutedEventArgs e)
        {
            this._editCommentCallback(this);
        }

        private double MeasureTextWidth()
        {
            TextBlock textBlock = new TextBlock();
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            double num = 20.0;
            textBlock.FontSize = num;
            string likesCountStr = this.LikesCountStr;
            textBlock.Text = likesCountStr;
            return ((FrameworkElement)textBlock).ActualWidth;
        }

        private void CreateVirtualizableChildren()
        {
            double num = this._isNotificationComment ? 0.0 : 16.0;
            // ISSUE: method pointer
            this._textBlockName = new TextItem(this.Width, new Thickness(74.0, num - 6.0, 0.0, 0.0), this.Name, false, 25.333, "Segoe WP", 0.0, Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush, true, new Action(this.NavigateToSource));
            base.VirtualizableChildren.Add((IVirtualizable)this._textBlockName);
            this._textWidth = this.Width - 74.0;
            if (!string.IsNullOrEmpty(this._comment.text))
            {
                NewsTextItem newsTextItem = new NewsTextItem(this._textWidth, new Thickness(74.0, 28.0 + num, 0.0, 0.0), this._comment.text ?? "", this._preview, null, 0.0, null, 28.0, null, false, 0.0, (HorizontalAlignment)0, "", (TextAlignment)1, false, null, false, false);
                base.VirtualizableChildren.Add((IVirtualizable)newsTextItem);
                Thickness margin = newsTextItem.Margin;
                // ISSUE: explicit reference operation
                this._topMarginDate = ((Thickness)@margin).Top + newsTextItem.FixedHeight + 4.0;
            }
            else
                this._topMarginDate = 28.0 + num;
            if (!((IList)this._comment.Attachments).IsNullOrEmpty())
            {
                string itemId = (this._comment.from_id != 0L && this._comment.id > 0L) ? string.Format("{0}_{1}", this._comment.from_id, this._comment.id) : "";
                AttachmentsItem attachmentsItem = new AttachmentsItem(this._textWidth, new Thickness(73.0, this._topMarginDate, 0.0, 0.0), this._comment.Attachments, null, itemId, false, true, false, false, 0.0, false, false, "", null, null, false);
                base.VirtualizableChildren.Add(attachmentsItem);
                this._topMarginDate += attachmentsItem.FixedHeight + 12.0;
            }
            if (!this._isNotificationComment && !this._preview)
            {
                double arg_2F6_0 = 40.0;
                Thickness arg_2F6_1 = new Thickness(base.Width - 32.0, 5.0, 0.0, 0.0);
                Func<UserControlVirtualizable> arg_2F6_2 = delegate
                {
                    Rectangle expr_05 = new Rectangle();
                    expr_05.Fill = ((SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"]);
                    expr_05.HorizontalAlignment = HorizontalAlignment.Right;
                    expr_05.VerticalAlignment = (0);
                    expr_05.Margin = (new Thickness(-8.0, -4.0, -8.0, -4.0));
                    expr_05.Height = (48.0);
                    expr_05.Width = (56.0);
                    Rectangle rectangle = expr_05;
                    VKClient.Common.UC.MoreActionsUC expr_85 = new VKClient.Common.UC.MoreActionsUC();
                    expr_85.LayoutRoot.Children.Insert(0, rectangle);
                    expr_85.TapCallback = new Action(this._onMoreOptionsTap);
                    return expr_85;
                };
                Func<double> arg_2F6_3 = new Func<double>(() => { return 40.0; });

                UCItem uCItem = new UCItem(arg_2F6_0, arg_2F6_1, arg_2F6_2, arg_2F6_3, null, 0.0, false);
                base.VirtualizableChildren.Add(uCItem);
            }
            this._textTimeItem = new TextItem(this._textWidth - 136.0, new Thickness(73.0, this._topMarginDate, 0.0, 0.0), this.DateText, true, 20.0, "Segoe WP", VKConstants.LineHeight, Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush, true, null);
            base.VirtualizableChildren.Add((IVirtualizable)this._textTimeItem);
            Thickness textMargin = this._textTimeItem.TextMargin;
            // ISSUE: explicit reference operation
            this._height = ((Thickness)@textMargin).Top + this._textTimeItem.FixedHeight;
            if (!string.IsNullOrEmpty(this._highlightedText))
            {
                TextItem textItem = new TextItem(this.Width - 72.0, new Thickness(72.0, this._height, 0.0, 0.0), this._highlightedText, false, 20.0, "Segoe WP", 23.0, Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush, true, null);
                base.VirtualizableChildren.Add((IVirtualizable)textItem);
                this._height = this._height + textItem.FixedHeight;
            }
            if (!string.IsNullOrEmpty(this._thumbSrc))
            {
                base.VirtualizableChildren.Add((IVirtualizable)new VirtualizableImage(80.0, 80.0, new Thickness(77.0, this._height + 6.0, 0.0, 0.0), this._thumbSrc, new Action<VirtualizableImage>(this.OnThumbTap), "", true, true, (Stretch)3, null, -1.0, false, false));
                this._height = this._height + 86.0;
            }
            base.VirtualizableChildren.Add((IVirtualizable)new VirtualizableImage(62.0, 62.0, new Thickness(0.0, num, 0.0, 0.0), this.ImageSrc, new Action<VirtualizableImage>(this.AvaTap), "", true, true, (Stretch)3, null, -1.0, false, true));
        }

        private void AvaTap(VirtualizableImage obj)
        {
            this.NavigateToSource();
        }

        private void NavigateToSource()
        {
            if (this._comment.from_id > 0L)
                Navigator.Current.NavigateToUserProfile(this._comment.from_id, this.Name, "", false);
            else
                Navigator.Current.NavigateToGroup(-this._comment.from_id, this.Name, false);
        }

        private void OnThumbTap(VirtualizableImage obj)
        {
            if (this._tapCommentCallback == null)
                return;
            this._tapCommentCallback(this);
        }

        public void Like(bool like)
        {
            LikesService.Current.AddRemoveLike(like, this._owner_id, this._comment.cid, this._likeObjectType, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }), "");
            if (like)
            {
                ++this._comment.likes.count;
                this._comment.likes.user_likes = 1;
            }
            else
            {
                --this._comment.likes.count;
                if (this._comment.likes.count < 0)
                    this._comment.likes.count = 0;
                this._comment.likes.user_likes = 0;
                this._comment.likes.can_like = 1;
            }
            this.UpdateLikesIndicator();
            this.UpdateContextMenu();
        }

        protected override void GenerateChildren()
        {
            Rectangle rectangle1 = new Rectangle();
            double num1 = 480.0;
            ((FrameworkElement)rectangle1).Width = num1;
            double num2 = 2.0;
            ((FrameworkElement)rectangle1).Height = num2;
            SolidColorBrush solidColorBrush1 = Application.Current.Resources["PhoneTableSeparatorBrush"] as SolidColorBrush;
            ((Shape)rectangle1).Fill = ((Brush)solidColorBrush1);
            Thickness thickness1 = new Thickness(-16.0, this._isNotificationComment ? this.FixedHeight : -1.0, 0.0, 0.0);
            ((FrameworkElement)rectangle1).Margin = thickness1;
            Rectangle rectangle2 = rectangle1;
            if (!this._isNotificationComment)
            {
                Rectangle rect = new Rectangle();
                double num3 = 480.0;
                ((FrameworkElement)rect).Width = num3;
                double num4 = this.FixedHeight + 16.0;
                ((FrameworkElement)rect).Height = num4;
                SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneNewsBackgroundBrush"] as SolidColorBrush;
                ((Shape)rect).Fill = ((Brush)solidColorBrush2);
                Thickness thickness2 = new Thickness(-16.0, 0.0, 0.0, 0.0);
                ((FrameworkElement)rect).Margin = thickness2;
                List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(rect).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                        this.Children.Add((FrameworkElement)enumerator.Current);
                }
                finally
                {
                    enumerator.Dispose();
                }
                this.Children.Add((FrameworkElement)rectangle2);
            }
            this.PrepareLikesAndReplyView();
            this.UpdateContextMenu();
        }

        private void PrepareLikesAndReplyView()
        {
            if (this._isNotificationComment || this._preview)
                return;
            ImageBrush imageBrush1 = new ImageBrush();
            ImageBrush imageBrush2 = new ImageBrush();
            ImageLoader.SetImageBrushMultiResSource(imageBrush1, "/Resources/Reply24px.png");
            ImageLoader.SetImageBrushMultiResSource(imageBrush2, "/Resources/Like24px.png");
            Border border1 = new Border();
            SolidColorBrush solidColorBrush1 = (SolidColorBrush)Application.Current.Resources["PhoneGreyIconBrush"];
            border1.Background = ((Brush)solidColorBrush1);
            Thickness thickness1 = new Thickness(16.0, 12.0, 16.0, 12.0);
            ((FrameworkElement)border1).Margin = thickness1;
            ImageBrush imageBrush3 = imageBrush1;
            ((UIElement)border1).OpacityMask = ((Brush)imageBrush3);
            double num1 = 24.0;
            ((FrameworkElement)border1).Height = num1;
            double num2 = 24.0;
            ((FrameworkElement)border1).Width = num2;
            Border border2 = border1;
            Border border3 = new Border();
            Thickness thickness2 = new Thickness(16.0, 12.0, 16.0, 12.0);
            ((FrameworkElement)border3).Margin = thickness2;
            ImageBrush imageBrush4 = imageBrush2;
            ((UIElement)border3).OpacityMask = ((Brush)imageBrush4);
            double num3 = 24.0;
            ((FrameworkElement)border3).Height = num3;
            double num4 = 24.0;
            ((FrameworkElement)border3).Width = num4;
            this._likesIcon = border3;
            TextBlock textBlock = new TextBlock();
            Thickness thickness3 = new Thickness(-10.0, 9.0, 16.0, 0.0);
            ((FrameworkElement)textBlock).Margin = thickness3;
            this._likesBlock = textBlock;
            Grid grid1 = new Grid();
            SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Transparent);
            ((Panel)grid1).Background = ((Brush)solidColorBrush2);
            Grid grid2 = grid1;
            MetroInMotion.SetTilt((DependencyObject)grid2, 2.5);
            ((PresentationFrameworkCollection<UIElement>)((Panel)grid2).Children).Add((UIElement)border2);
            StackPanel stackPanel1 = new StackPanel();
            int num5 = 1;
            stackPanel1.Orientation = ((Orientation)num5);
            SolidColorBrush solidColorBrush3 = new SolidColorBrush(Colors.Transparent);
            ((Panel)stackPanel1).Background = ((Brush)solidColorBrush3);
            StackPanel stackPanel2 = stackPanel1;
            MetroInMotion.SetTilt((DependencyObject)stackPanel2, 2.5);
            ((PresentationFrameworkCollection<UIElement>)((Panel)stackPanel2).Children).Add((UIElement)this._likesIcon);
            ((PresentationFrameworkCollection<UIElement>)((Panel)stackPanel2).Children).Add((UIElement)this._likesBlock);
            StackPanel stackPanel3 = new StackPanel();
            int num6 = 1;
            stackPanel3.Orientation = ((Orientation)num6);
            SolidColorBrush solidColorBrush4 = new SolidColorBrush(Colors.Transparent);
            ((Panel)stackPanel3).Background = ((Brush)solidColorBrush4);
            this._replyAndLikesContainer = stackPanel3;
            if (this._replyCommentCallback != null)
                ((PresentationFrameworkCollection<UIElement>)((Panel)this._replyAndLikesContainer).Children).Add((UIElement)grid2);
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._replyAndLikesContainer).Children).Add((UIElement)stackPanel2);
            this.Children.Add((FrameworkElement)this._replyAndLikesContainer);
            this.UpdateLikesIndicator();
            ((UIElement)grid2).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((o, e) =>
            {
                e.Handled = true;
                this._replyCommentCallback(this);
            }));
            ((UIElement)stackPanel2).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((o, e) =>
            {
                e.Handled = true;
                this.Like(this._comment.likes.user_likes == 0);
            }));
        }

        private void UpdateLikesIndicator()
        {
            if (this._replyAndLikesContainer == null)
                return;
            this._likesBlock.Text = this.LikesCountStr;
            ((UIElement)this._likesBlock).Visibility = (this._likesBlock.Text != "" ? Visibility.Visible : Visibility.Collapsed);
            this._likesIcon.Background = ((Brush)Application.Current.Resources[this._comment.likes.user_likes == 1 ? "PhoneActiveIconBrush" : "PhoneGreyIconBrush"]);
            this._likesBlock.Foreground = ((Brush)Application.Current.Resources[this._comment.likes.user_likes == 1 ? "PhoneNewsActionLikedForegroundBrush" : "PhoneNewsActionForegroundBrush"]);
            ((FrameworkElement)this._replyAndLikesContainer).Margin = (new Thickness(this.Width - (100.0 + (((UIElement)this._likesBlock).Visibility == Visibility.Visible ? this.MeasureTextWidth() + 6.0 : 0.0)), this._height - 36.0, 0.0, 0.0));
        }

        private void _onMoreOptionsTap()
        {
            ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject)this._textBlockName.View);
            if (contextMenu == null)
                return;
            contextMenu.IsOpen = true;
        }

        protected override void ReleaseResourcesOnUnload()
        {
            base.ReleaseResourcesOnUnload();
            this._contextMenu = null;
            this._textBlockName.ResetMenu();
        }

        public void Handle(CommentEdited message)
        {
            if (message.Comment.from_id != this._comment.from_id || message.Comment.cid != this._comment.cid || this.Parent == null)
                return;
            this.Parent.Substitute((IVirtualizable)this, (IVirtualizable)new CommentItem(this.Width, this.Margin, this._likeObjectType, this._deleteCommentCallback, this._replyCommentCallback, this._editCommentCallback, this._owner_id, message.Comment, this._user, this._user2, this._group, this._tapCommentCallback, this._extraText, this._thumbSrc, this._seeAllLikesCallback, this._preview, false, ""));
        }
    }
}
