using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public static class CommentsItemsGeneratorHelper
  {
    public static double MarginBetweenComments
    {
      get
      {
        return 16.0;
      }
    }

    public static VirtualizableItemBase CreateCommentsCountItem(double width, int commentsCount)
    {
      return CommentsItemsGeneratorHelper.CreateCommentsCountItem(width, (Func<string>) (() => CommentsItemsGeneratorHelper.GetTextForCommentsCount(commentsCount)), 0.0, (SolidColorBrush) null);
    }

    public static VirtualizableItemBase CreateCommentsCountItem(double width, Func<string> getText, double topMargin = 0.0, SolidColorBrush background = null)
    {
      TextSeparatorUC commentsCountSeparatorUC = new TextSeparatorUC()
      {
        Text = getText()
      };
      if (background != null)
        commentsCountSeparatorUC.gridViewedFeedback.Background = (Brush) background;
      return (VirtualizableItemBase) new UCItem(480.0, new Thickness(0.0, topMargin, 0.0, 0.0), (Func<UserControlVirtualizable>) (() => (UserControlVirtualizable) commentsCountSeparatorUC), (Func<double>) (() => 56.0), (Action<UserControlVirtualizable>) null, 0.0, false);
    }

    public static ButtonItem CreateReloadButton(double width, int numberToReload)
    {
      return new ButtonItem(width, new Thickness(0.0, 0.0, 0.0, 40.0), CommentsItemsGeneratorHelper.GetTextFor(numberToReload));
    }

    public static CommentItem CreateCommentItem(double width, Comment comment, LikeObjectType likeObjType, long ownerId, User user, User user2, Group group, Action<CommentItem> deleteCallback, Action<CommentItem> replyCallback, Action<CommentItem> editCallback, Action<CommentItem> tapCallback = null)
    {
      return new CommentItem(width - 32.0, new Thickness(8.0, 0.0, 0.0, CommentsItemsGeneratorHelper.MarginBetweenComments), likeObjType, deleteCallback, replyCallback, editCallback, ownerId, comment, user, user2, group, (Action<CommentItem>) null, "", "", (Action<CommentItem>) null, false, false, "");
    }

    public static string GetTextFor(int numberToReload)
    {
      if (numberToReload == 1)
        return CommonResources.PostCommentsPage_PreviousComment;
      return string.Format(UIStringFormatterHelper.FormatNumberOfSomething(numberToReload, CommonResources.PostCommentsPage_PreviousOneCommentsFrm, CommonResources.PostCommentsPage_PreviousTwoFourCommentsFrm, CommonResources.PostCommentsPage_PreviousFiveCommentsFrm, true, null, false), (object) numberToReload);
    }

    public static string GetTextForCommentsCount(int commentsCount)
    {
      if (commentsCount < 0)
        return "";
      if (commentsCount == 0)
        return CommonResources.PostCommentsPage_NoComments;
      return UIStringFormatterHelper.FormatNumberOfSomething(commentsCount, CommonResources.PostCommentPage_OneCommentFrm, CommonResources.PostCommentPage_TwoFourCommentsFrm, CommonResources.PostCommentPage_FiveCommentsFrm, true, null, false);
    }

    public static string GetTextForLikesCount(int count)
    {
      if (count < 0)
        return "";
      if (count == 0)
        return CommonResources.General_NoLikes;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.General_OneUserLikedFrm, CommonResources.General_TwoFourUsersLikedFrm, CommonResources.General_FiveUsersLikedFrm, true, null, false);
    }
  }
}
