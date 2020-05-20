using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
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
        this.ProcessUserFeedback((this._notification.ParsedFeedback as List<FeedbackUser>).Select<FeedbackUser, long>((Func<FeedbackUser, long>) (u => u.owner_id)).ToList<long>());
      else if (this._notification.ParsedFeedback is List<FeedbackCopyInfo>)
        this.ProcessUserFeedback((this._notification.ParsedFeedback as List<FeedbackCopyInfo>).Select<FeedbackCopyInfo, long>((Func<FeedbackCopyInfo, long>) (u => u.owner_id)).ToList<long>());
      else if (this._notification.ParsedFeedback is WallPost)
        this.ProcessWallPostFeedback();
      else if (this._notification.ParsedFeedback is Comment)
        this.ProcessCommentFeedback();
      this._fixedHeight = this._fixedHeight + 2.0;
      stopwatch.Stop();
    }

    private void ProcessCommentFeedback()
    {
      if (this._commentItem == null)
      {
        Comment comment = this._notification.ParsedFeedback as Comment;
        this.GetThumb();
        double width = this.Width - 32.0;
        comment.date = this._notification.date;
        string highlightedText = this.GetHighlightedText();
        this._commentItem = new CommentItem(width, new Thickness(), LikeObjectType.comment, (Action<CommentItem>) null, (Action<CommentItem>) null, (Action<CommentItem>) null, 0L, comment, this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == comment.from_id)), (User) null, this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -comment.from_id)), new Action<CommentItem>(this.OnCommentFeedbackTap), this.GetLocalizableText(), this.GetThumb(), (Action<CommentItem>) null, false, true, highlightedText);
        this.VirtualizableChildren.Add((IVirtualizable) this._commentItem);
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
          Comment comment = this._notification.ParsedParent as Comment;
          if (comment.post != null)
          {
            wallPost = comment.post;
          }
          else
          {
            if (comment.photo != null)
            {
              Navigator.Current.NavigateToPhotoWithComments(comment.photo, (PhotoWithFullInfo) null, comment.photo.owner_id, comment.photo.pid, comment.photo.access_key, false, false);
              return;
            }
            if (comment.video != null)
            {
              Navigator.Current.NavigateToVideoWithComments(comment.video, comment.video.owner_id, comment.video.vid, comment.video.access_key);
              return;
            }
            if (comment.topic != null)
            {
              Navigator.Current.NavigateToGroupDiscussion(-comment.topic.owner_id, comment.topic.tid, comment.topic.title, comment.topic.comments, true, comment.topic.is_closed == 0);
              return;
            }
            if (comment.market != null)
            {
              CurrentMarketItemSource.Source = MarketItemSource.feed;
              Navigator.Current.NavigateToProduct(comment.market.owner_id, comment.market.id);
            }
          }
        }
        if (wallPost == null)
          return;
        Navigator.Current.NavigateToWallPostComments(wallPost.id, wallPost.to_id, true, 0L, 0L, "");
      }
      else if (this._notification.ParsedParent is Photo)
      {
        Photo photo = this._notification.ParsedParent as Photo;
        Navigator.Current.NavigateToPhotoWithComments(photo, (PhotoWithFullInfo) null, photo.owner_id, photo.pid, photo.access_key, false, false);
      }
      else if (this._notification.ParsedParent is VKClient.Common.Backend.DataObjects.Video)
      {
          VKClient.Common.Backend.DataObjects.Video video = this._notification.ParsedParent as VKClient.Common.Backend.DataObjects.Video;
        Navigator.Current.NavigateToVideoWithComments(video, video.owner_id, video.vid, "");
      }
      else
      {
        if (!(this._notification.ParsedParent is Topic))
          return;
        Topic topic = this._notification.ParsedParent as Topic;
        Navigator.Current.NavigateToGroupDiscussion(-topic.owner_id, topic.tid, topic.title, topic.comments, true, topic.is_closed == 0);
      }
    }

    private void ProcessWallPostFeedback()
    {
      if (this._wallPostItem == null)
      {
        WallPost wallPost = this._notification.ParsedFeedback as WallPost;
        if (wallPost.likes != null)
          wallPost.likes.can_like = 1;
        wallPost.date = this._notification.date;
        double width = this.Width;
        Thickness margin = new Thickness();
        int num1 = 1;
        NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo = new NewsItemDataWithUsersAndGroupsInfo();
        wallPostWithInfo.Groups = this._groups;
        wallPostWithInfo.Profiles = this._users;
        wallPostWithInfo.WallPost = wallPost;
        object local1 = null;
        int num2 = 1;
        object local2 = null;
        int num3 = 1;
        int num4 = this._showEarlierReplies ? 1 : 0;
        int num5 = 0;
        int num6 = 0;
        object local3 = null;
        object local4 = null;
        this._wallPostItem = new WallPostItem(width, margin, num1 != 0, wallPostWithInfo, (Action<WallPostItem>) local1, num2 != 0, (Action<long, User, Group>) local2, num3 != 0, num4 != 0, num5 != 0, num6 != 0, (NewsFeedAdsItem) local3, (Func<List<MenuItem>>) local4);
        this.VirtualizableChildren.Add((IVirtualizable) this._wallPostItem);
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
      if (userIds == null || userIds.Count == 0 || this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == userIds[0])) == null)
        return;
      this._fixedHeight = this._userNotTemplate.Configure(userIds.Take<long>(6).Select<long, User>((Func<long, User>) (fu => this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == fu)))).ToList<User>(), this.GetLocalizableText(), UIStringFormatterHelper.FormatDateTimeForUI(this._notification.date), this.GetHighlightedText(), this.GetThumb(), this._notification.NotType, userIds.Count, this._showEarlierReplies) + 16.0;
      this._userNotTemplate.Tap += (EventHandler<System.Windows.Input.GestureEventArgs>) ((s, e) => this.ProcessNavigationTap());
    }

    protected override void GenerateChildren()
    {
      Rectangle rect = new Rectangle();
      double fixedHeight = this.FixedHeight;
      rect.Height = fixedHeight;
      Thickness thickness1 = new Thickness();
      rect.Margin = thickness1;
      double width1 = this.Width;
      rect.Width = width1;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneNewsBackgroundBrush"];
      rect.Fill = (Brush) solidColorBrush1;
      foreach (FrameworkElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
        this.Children.Add(coverByRectangle);
      Rectangle rectangle = new Rectangle();
      double width2 = this.Width;
      rectangle.Width = width2;
      double num = 2.0;
      rectangle.Height = num;
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneTableSeparatorBrush"] as SolidColorBrush;
      rectangle.Fill = (Brush) solidColorBrush2;
      Thickness thickness2 = new Thickness(0.0, this.FixedHeight - 2.0, 0.0, 0.0);
      rectangle.Margin = thickness2;
      this.Children.Add((FrameworkElement) rectangle);
      if (this._userNotTemplate == null)
        return;
      this._userNotTemplate.LoadImages();
      this.Children.Add((FrameworkElement) this._userNotTemplate);
    }

    private string GetThumb()
    {
      WallPost wallPost = this._notification.ParsedParent as WallPost;
      if (wallPost != null && !wallPost.attachments.IsNullOrEmpty())
      {
        Attachment attachment = wallPost.attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>) (a =>
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
      Photo photo = this._notification.ParsedParent as Photo;
      if (photo != null)
        return photo.src;
      VKClient.Common.Backend.DataObjects.Video video = this._notification.ParsedParent as VKClient.Common.Backend.DataObjects.Video;
      if (video != null)
        return video.image;
      Comment comment = this._notification.ParsedParent as Comment;
      if (comment != null)
      {
        if (comment.photo != null)
          return comment.photo.src;
        if (comment.video != null)
          return comment.video.image;
        if (comment.market != null)
          return comment.market.thumb_photo;
      }
      return "";
    }

    private string GetHighlightedText()
    {
      WallPost wallPost = this._notification.ParsedParent as WallPost;
      if (wallPost != null)
      {
        if (!string.IsNullOrEmpty(wallPost.text))
          return this.CutText(wallPost.text);
        if (wallPost.IsRepost())
          return this.CutText(wallPost.copy_history[0].text);
        return "";
      }
      Comment comment = this._notification.ParsedParent as Comment;
      if (comment != null)
        return this.CutText(comment.text ?? "");
      Topic topic = this._notification.ParsedParent as Topic;
      if (topic != null)
        return this.CutText(topic.title ?? "");
      return "";
    }

    private string CutText(string text)
    {
      text = text.Replace(Environment.NewLine, " ");
      text = UIStringFormatterHelper.SubstituteMentionsWithNames(text);
      if (text.Length > 50)
        text = text.Substring(0, 50);
      return text;
    }

    private string GetDateAndInfoText()
    {
      return UIStringFormatterHelper.FormatDateTimeForUI(this._notification.date) + " " + this.GetLocalizableText();
    }

    private int GetGender()
    {
      User user = (User) null;
      if (this._notification.ParsedFeedback is List<FeedbackUser>)
      {
        List<FeedbackUser> list = this._notification.ParsedFeedback as List<FeedbackUser>;
        if (list.Count > 1)
          return 2;
        user = this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == list[0].owner_id));
      }
      else if (this._notification.ParsedFeedback is Comment)
        user = this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == (this._notification.ParsedFeedback as Comment).from_id));
      else if (this._notification.ParsedFeedback is List<FeedbackCopyInfo>)
      {
        List<FeedbackCopyInfo> list = this._notification.ParsedFeedback as List<FeedbackCopyInfo>;
        if (list.Count > 1)
          return 2;
        user = this._users.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == list[0].owner_id));
      }
      Logger.Instance.Assert(user != null, "User is null in GetGender");
      return user != null && user.sex == 1 ? 1 : 0;
    }

    private string GetLocalizableText()
    {
      int gender = this.GetGender();
      string str = "";
        /*
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
        case NotificationType.mention_comment_photo:
          str = CommonResources.Notification_MentionInPhotoComment;
          break;
        case NotificationType.mention_comment_video:
          str = CommonResources.Notification_MentionInVideoComment;
          break;
      }*/
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
