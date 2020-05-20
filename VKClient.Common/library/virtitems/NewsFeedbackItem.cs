using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.MoneyTransfers;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
    public class NewsFeedbackItem : VirtualizableItemBase, ISupportChildHeightChange
    {
        private double _fixedHeight;
        private Notification _notification;
        private List<User> _users;
        private List<Group> _groups;
        private UserNotificationTemplateUC _userNotTemplate;
        private bool _showEarlierReplies;
        private CommentItem _commentItem;
        private WallPostItem _wallPostItem;

        public Notification Notification
        {
            get
            {
                return this._notification;
            }
        }

        public override double FixedHeight
        {
            get
            {
                return this._fixedHeight;
            }
        }

        public NewsFeedbackItem(double width, Thickness margin, Notification notification, List<User> users, List<Group> groups, bool showEarliesReplies)
            : base(width, margin, new Thickness())
        {
            this._notification = notification;
            this._users = users;
            this._groups = groups;
            this._showEarlierReplies = false;
            this.GenerateLayout();
        }

        private void GenerateLayout()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (this._notification.ParsedFeedback is List<FeedbackUser>)
                this.ProcessUserFeedback((List<long>)Enumerable.ToList<long>(Enumerable.Select<FeedbackUser, long>((this._notification.ParsedFeedback as List<FeedbackUser>), (Func<FeedbackUser, long>)(u => u.owner_id))));
            else if (this._notification.ParsedFeedback is List<FeedbackCopyInfo>)
                this.ProcessUserFeedback((List<long>)Enumerable.ToList<long>(Enumerable.Select<FeedbackCopyInfo, long>((this._notification.ParsedFeedback as List<FeedbackCopyInfo>), (Func<FeedbackCopyInfo, long>)(u => u.owner_id))));
            else if (this._notification.ParsedFeedback is WallPost)
                this.ProcessWallPostFeedback();
            else if (this._notification.ParsedFeedback is Comment)
                this.ProcessCommentFeedback();
            else if (this._notification.ParsedFeedback is MoneyTransfer)
                this.ProcessMoneyTransfer();
            this._fixedHeight = this._fixedHeight + 2.0;
            stopwatch.Stop();
        }

        private void ProcessCommentFeedback()
        {
            if (this._commentItem == null)
            {
                Comment comment = this._notification.ParsedFeedback as Comment;
                this.GetThumb();
                comment.date = this._notification.Date;//todo:bug
                string highlightedText = this.GetHighlightedText();
                this._commentItem = new CommentItem(this.Width - 32.0, new Thickness(), LikeObjectType.comment, null, null, null, 0, comment, Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == comment.from_id)), null, (Group)Enumerable.FirstOrDefault<Group>(this._groups, (Func<Group, bool>)(g => g.id == -comment.from_id)), new Action<CommentItem>(this.OnCommentFeedbackTap), this.GetLocalizableText(), this.GetThumb(), null, false, true, highlightedText);
                base.VirtualizableChildren.Add(this._commentItem);
            }
            this._commentItem.ViewMargin = new Thickness(16.0, 16.0, 0.0, 0.0);
            this._fixedHeight = this._commentItem.FixedHeight + 16.0;
        }

        private void OnCommentFeedbackTap(CommentItem obj)
        {
            this.ProcessNavigationTap();
        }

        private void ProcessNavigationTap()
        {
            if (this._notification.ParsedParent is WallPost || this._notification.ParsedParent is Comment)
            {
                WallPost wallPost = this._notification.ParsedParent as WallPost;
                if (wallPost == null)
                {
                    Comment parsedParent = this._notification.ParsedParent as Comment;
                    if (parsedParent.post != null)
                    {
                        wallPost = parsedParent.post;
                    }
                    else
                    {
                        if (parsedParent.photo != null)
                        {
                            Navigator.Current.NavigateToPhotoWithComments(parsedParent.photo, null, parsedParent.photo.owner_id, parsedParent.photo.pid, parsedParent.photo.access_key, false, false);
                            return;
                        }
                        if (parsedParent.video != null)
                        {
                            Navigator.Current.NavigateToVideoWithComments(parsedParent.video, parsedParent.video.owner_id, parsedParent.video.vid, parsedParent.video.access_key);
                            return;
                        }
                        if (parsedParent.topic != null)
                        {
                            Navigator.Current.NavigateToGroupDiscussion(-parsedParent.topic.owner_id, parsedParent.topic.tid, parsedParent.topic.title, parsedParent.topic.comments, true, parsedParent.topic.is_closed == 0);
                            return;
                        }
                        if (parsedParent.market != null)
                        {
                            CurrentMarketItemSource.Source = MarketItemSource.feed;
                            Navigator.Current.NavigateToProduct(parsedParent.market.owner_id, parsedParent.market.id);
                        }
                    }
                }
                if (wallPost == null)
                    return;
                Navigator.Current.NavigateToWallPostComments(wallPost.id, wallPost.to_id, true, 0, 0, "");
            }
            else if (this._notification.ParsedParent is Photo)
            {
                Photo parsedParent = this._notification.ParsedParent as Photo;
                Navigator.Current.NavigateToPhotoWithComments(parsedParent, null, parsedParent.owner_id, parsedParent.pid, parsedParent.access_key, false, false);
            }
            else if (this._notification.ParsedParent is VKClient.Common.Backend.DataObjects.Video)
            {
                VKClient.Common.Backend.DataObjects.Video parsedParent = this._notification.ParsedParent as VKClient.Common.Backend.DataObjects.Video;
                Navigator.Current.NavigateToVideoWithComments(parsedParent, parsedParent.owner_id, parsedParent.vid, "");
            }
            else if (this._notification.ParsedParent is Topic)
            {
                Topic parsedParent = this._notification.ParsedParent as Topic;
                Navigator.Current.NavigateToGroupDiscussion(-parsedParent.owner_id, parsedParent.tid, parsedParent.title, parsedParent.comments, true, parsedParent.is_closed == 0);
            }
            else
            {
                if (!(this._notification.ParsedFeedback is MoneyTransfer))
                    return;
                MoneyTransfer parsedFeedback = (MoneyTransfer)this._notification.ParsedFeedback;
                TransferCardView.Show(parsedFeedback.id, parsedFeedback.from_id, parsedFeedback.to_id);
            }
        }

        private void ProcessWallPostFeedback()
        {
            if (this._wallPostItem == null)
            {
                WallPost parsedFeedback = this._notification.ParsedFeedback as WallPost;
                if (parsedFeedback.likes != null)
                    parsedFeedback.likes.can_like = 1;
                parsedFeedback.date = this._notification.Date;//todo:bug
                double width = this.Width;
                Thickness margin = new Thickness();
                int num1 = 1;
                NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo = new NewsItemDataWithUsersAndGroupsInfo();
                List<Group> groups = this._groups;
                wallPostWithInfo.Groups = groups;
                List<User> users = this._users;
                wallPostWithInfo.Profiles = users;
                WallPost wallPost = parsedFeedback;
                wallPostWithInfo.WallPost = wallPost;

                int num2 = 1;
                int num3 = 1;
                int num4 = this._showEarlierReplies ? 1 : 0;
                int num5 = 0;
                int num6 = 0;

                this._wallPostItem = new WallPostItem(width, margin, num1 != 0, wallPostWithInfo, null, num2 != 0, null, num3 != 0, num4 != 0, num5 != 0, num6 != 0, null, null);
                base.VirtualizableChildren.Add(this._wallPostItem);
            }
            this._fixedHeight = this._wallPostItem.FixedHeight + 16.0;
        }

        private void ProcessUserFeedback(List<long> userIds)
        {
            this._fixedHeight = 180.0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            this._userNotTemplate = new UserNotificationTemplateUC();
            this._userNotTemplate.Margin = new Thickness(0.0, 16.0, 0.0, 16.0);
            stopwatch.Stop();
            if (userIds == null || userIds.Count == 0 || Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == userIds[0])) == null)
                return;
            this._fixedHeight = this._userNotTemplate.Configure((List<User>)Enumerable.ToList<User>(Enumerable.Select<long, User>(Enumerable.Take<long>(userIds, 6), (Func<long, User>)(fu => (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == fu))))), this.GetLocalizableText(), UIStringFormatterHelper.FormatDateTimeForUI(this._notification.Date), this.GetHighlightedText(), this.GetThumb(), this._notification.NotType, userIds.Count, this._showEarlierReplies, null) + 16.0;
            this._userNotTemplate.Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((s, e) => this.ProcessNavigationTap()));
        }

        private void ProcessMoneyTransfer()
        {
            this._fixedHeight = 180.0;
            this._userNotTemplate = new UserNotificationTemplateUC();
            ((FrameworkElement)this._userNotTemplate).Margin = (new Thickness(0.0, 16.0, 0.0, 16.0));
            MoneyTransfer transfer = (MoneyTransfer)this._notification.ParsedFeedback;
            User user = (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.id == transfer.OtherUserId));
            if (user == null)
            {
                Group group = (Group)Enumerable.FirstOrDefault<Group>(this._groups, (Func<Group, bool>)(g => g.id == -transfer.OtherUserId));
                if (group != null)
                    user = new User()
                    {
                        id = -group.id,
                        first_name = group.name,
                        photo_max = group.photo_200
                    };
                if (user == null)
                    return;
            }
            UserNotificationTemplateUC userNotTemplate = this._userNotTemplate;
            List<User> users = new List<User>();
            users.Add(user);
            string localizableText = this.GetLocalizableText();
            string dateText = UIStringFormatterHelper.FormatDateTimeForUI(this._notification.Date);
            string highlightedText = this.GetHighlightedText();
            string thumb = this.GetThumb();
            int notType = (int)this._notification.NotType;
            int totalUsersCount = 0;
            int num = this._showEarlierReplies ? 1 : 0;
            string forcedTypeIcon = string.Format("/Resources/FeedbackIconsMoney{0}.png", !transfer.IsOutbox || !transfer.IsCancelled ? "Green" : "Red");
            this._fixedHeight = userNotTemplate.Configure(users, localizableText, dateText, highlightedText, thumb, (NotificationType)notType, totalUsersCount, num != 0, forcedTypeIcon) + 16.0;
            ((UIElement)this._userNotTemplate).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((s, e) => this.ProcessNavigationTap()));
        }

        protected override void GenerateChildren()
        {
            Rectangle rect = new Rectangle();
            double fixedHeight = this.FixedHeight;
            ((FrameworkElement)rect).Height = fixedHeight;
            Thickness thickness1 = new Thickness();
            ((FrameworkElement)rect).Margin = thickness1;
            double width1 = this.Width;
            ((FrameworkElement)rect).Width = width1;
            SolidColorBrush solidColorBrush1 = (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            ((Shape)rect).Fill = ((Brush)solidColorBrush1);
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
            Rectangle rectangle = new Rectangle();
            rectangle.Width = this.Width;
            rectangle.Height = 2.0;
            SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneTableSeparatorBrush"] as SolidColorBrush;
            ((Shape)rectangle).Fill = ((Brush)solidColorBrush2);
            Thickness thickness2 = new Thickness(0.0, this.FixedHeight - 2.0, 0.0, 0.0);
            rectangle.Margin = thickness2;
            this.Children.Add((FrameworkElement)rectangle);
            if (this._userNotTemplate == null)
                return;
            this._userNotTemplate.LoadImages();
            this.Children.Add(this._userNotTemplate);
        }

        private string GetThumb()
        {
            WallPost parsedParent1 = this._notification.ParsedParent as WallPost;
            if (parsedParent1 != null && !((IList)parsedParent1.attachments).IsNullOrEmpty())
            {
                Attachment attachment = (Attachment)Enumerable.FirstOrDefault<Attachment>(parsedParent1.attachments, (Func<Attachment, bool>)(a =>
              {
                  if (!(a.type == "photo"))
                      return a.type == "video";
                  return true;
              }));
                if (attachment != null)
                {
                    if (attachment.photo != null)
                        return attachment.photo.src;
                    if (attachment.video != null)
                        return attachment.video.image;
                }
            }
            Photo parsedParent2 = this._notification.ParsedParent as Photo;
            if (parsedParent2 != null)
                return parsedParent2.src;
            VKClient.Common.Backend.DataObjects.Video parsedParent3 = this._notification.ParsedParent as VKClient.Common.Backend.DataObjects.Video;
            if (parsedParent3 != null)
                return parsedParent3.image;
            Comment parsedParent4 = this._notification.ParsedParent as Comment;
            if (parsedParent4 != null)
            {
                if (parsedParent4.photo != null)
                    return parsedParent4.photo.src;
                if (parsedParent4.video != null)
                    return parsedParent4.video.image;
                if (parsedParent4.market != null)
                    return parsedParent4.market.thumb_photo;
            }
            return "";
        }

        private string GetHighlightedText()
        {
            WallPost parsedParent1 = this._notification.ParsedParent as WallPost;
            if (parsedParent1 != null)
            {
                if (!string.IsNullOrEmpty(parsedParent1.text))
                    return this.CutText(parsedParent1.text);
                if (parsedParent1.IsRepost())
                    return this.CutText(parsedParent1.copy_history[0].text);
                return "";
            }
            Comment parsedParent2 = this._notification.ParsedParent as Comment;
            if (parsedParent2 != null)
                return this.CutText(parsedParent2.text ?? "");
            Topic parsedParent3 = this._notification.ParsedParent as Topic;
            if (parsedParent3 != null)
                return this.CutText(parsedParent3.title ?? "");
            return "";
        }

        private string CutText(string text)
        {
            text = ((string)text).Replace(Environment.NewLine, " ");
            text = UIStringFormatterHelper.SubstituteMentionsWithNames(text);
            if (((string)text).Length > 50)
                text = ((string)text).Substring(0, 50);
            return text;
        }

        private string GetDateAndInfoText()
        {
            return string.Concat(UIStringFormatterHelper.FormatDateTimeForUI(this._notification.Date), " ", this.GetLocalizableText());
        }

        private int GetGender()
        {
            User user = null;
            if (this._notification.ParsedFeedback is List<FeedbackUser>)
            {
                List<FeedbackUser> list = this._notification.ParsedFeedback as List<FeedbackUser>;
                if (list.Count > 1)
                    return 2;
                user = (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == list[0].owner_id));
            }
            else if (this._notification.ParsedFeedback is Comment)
                user = (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == (this._notification.ParsedFeedback as Comment).from_id));
            else if (this._notification.ParsedFeedback is List<FeedbackCopyInfo>)
            {
                List<FeedbackCopyInfo> list = this._notification.ParsedFeedback as List<FeedbackCopyInfo>;
                if (list.Count > 1)
                    return 2;
                user = (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == list[0].owner_id));
            }
            else if (this._notification.ParsedFeedback is MoneyTransfer)
            {
                MoneyTransfer moneyTransfer = (MoneyTransfer)this._notification.ParsedFeedback;
                user = moneyTransfer.IsOutbox ? (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == moneyTransfer.to_id)) : (User)Enumerable.FirstOrDefault<User>(this._users, (Func<User, bool>)(u => u.uid == moneyTransfer.from_id));
            }
            Logger.Instance.Assert(user != null, "User is null in GetGender");
            return user != null && user.sex == 1 ? 1 : 0;
        }

        private string GetLocalizableText()
        {
            int gender = this.GetGender();
            string str = "";
            switch (this._notification.NotType)
            {
                case NotificationType.follow:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_FollowMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_FollowFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_FollowPlural;
                            break;
                    }
                    break;//
                case NotificationType.friend_accepted:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_FriendAcceptedMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_FriendAcceptedFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_FriendAcceptedPlural;
                            break;
                    }
                    break;//
                case NotificationType.mention_comments:
                    if (gender != 0)
                    {
                        if (gender == 1)
                        {
                            str = CommonResources.Notification_MentionCommentsFemale;
                            break;
                        }
                        break;
                    }
                    str = CommonResources.Notification_MentionCommentsMale;
                    break;
                case NotificationType.comment_post:
                    str = CommonResources.Notification_CommentPost;
                    break;
                case NotificationType.comment_photo:
                    if (gender != 0)
                    {
                        if (gender == 1)
                        {
                            str = CommonResources.Notification_CommentPhotoFemale;
                            break;
                        }
                        break;
                    }
                    str = CommonResources.Notification_CommentPhotoMale;
                    break;
                case NotificationType.comment_video:
                    if (gender != 0)
                    {
                        if (gender == 1)
                        {
                            str = CommonResources.Notification_CommentVideoFemale;
                            break;
                        }
                        break;
                    }
                    str = CommonResources.Notification_CommentVideoMale;
                    break;
                case NotificationType.reply_comment:
                case NotificationType.reply_topic:
                case NotificationType.reply_comment_photo:
                case NotificationType.reply_comment_video:
                case NotificationType.reply_comment_market:
                    str = CommonResources.Notification_ReplyCommentOrTopic;
                    break;
                case NotificationType.like_post:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_LikePostMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_LikePostFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_LikePostPlural;
                            break;
                    }
                    break;//
                case NotificationType.like_comment:
                case NotificationType.like_comment_photo:
                case NotificationType.like_comment_video:
                case NotificationType.like_comment_topic:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_LikeCommentMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_LikeCommentFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_LikeCommentPlural;
                            break;
                    }
                    break;//
                case NotificationType.like_photo:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_LikePhotoMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_LikePhotoFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_LikePhotoPlural;
                            break;
                    }
                    break;//
                case NotificationType.like_video:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_LikeVideoMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_LikeVideoFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_LikeVideoPlural;
                            break;
                    }
                    break;//
                case NotificationType.copy_post:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_CopyPostMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_CopyPostFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_CopyPostPlural;
                            break;
                    }
                    break;//
                case NotificationType.copy_photo:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_CopyPhotoMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_CopyPhotoFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_CopyPhotoPlural;
                            break;
                    }
                    break;//
                case NotificationType.copy_video:
                    switch (gender)
                    {
                        case 0:
                            str = CommonResources.Notification_CopyVideoMale;
                            break;
                        case 1:
                            str = CommonResources.Notification_CopyVideoFemale;
                            break;
                        case 2:
                            str = CommonResources.Notification_CopyVideoPlural;
                            break;
                    }
                    break;//
                case NotificationType.mention_comment_photo:
                    str = CommonResources.Notification_MentionInPhotoComment;
                    break;
                case NotificationType.mention_comment_video:
                    str = CommonResources.Notification_MentionInVideoComment;
                    break;
                case NotificationType.money_transfer_received:
                    MoneyTransfer parsedFeedback1 = (MoneyTransfer)this._notification.ParsedFeedback;
                    str = string.Format(gender == 0 ? CommonResources.MoneyTransferSentMale : CommonResources.MoneyTransferSentFemale, ((string)parsedFeedback1.amount.text).Replace(' ', ' '));
                    break;
                case NotificationType.money_transfer_accepted:
                    MoneyTransfer parsedFeedback2 = (MoneyTransfer)this._notification.ParsedFeedback;
                    str = string.Format(gender == 0 ? CommonResources.MoneyTransferAcceptedMale : CommonResources.MoneyTransferAcceptedFemale, ((string)parsedFeedback2.amount.text).Replace(' ', ' '));
                    break;
                case NotificationType.money_transfer_declined:
                    MoneyTransfer parsedFeedback3 = (MoneyTransfer)this._notification.ParsedFeedback;
                    str = string.Format(gender == 0 ? CommonResources.MoneyTransferDeclinedMale : CommonResources.MoneyTransferDeclinedFemale, ((string)parsedFeedback3.amount.text).Replace(' ', ' '));
                    break;
            }
            return str;
        }

        public void RespondToChildHeightChange(IVirtualizable child)
        {
            this.GenerateLayout();
            this.RegenerateChildren();
            this.NotifyHeightChanged();
        }
    }
}
