using System;
using System.Collections.Generic;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public interface ISupportCommentsAndLikes
  {
    int TotalCommentsCount { get; }

    long OwnerId { get; }

    long ItemId { get; }

    CommentType CommentType { get; }

    LikeObjectType LikeObjectType { get; }

    bool CanComment { get; }

    List<Comment> Comments { get; }

    List<User> Users { get; }

    List<Group> Groups { get; }

    List<User> Users2 { get; }

    List<long> LikesAllIds { get; }

    int LikesCount { get; }

    int RepostsCount { get; }

    bool UserLiked { get; }

    bool CanRepost { get; }

    void LoadMoreComments(int countToLoad, Action<bool> callback);

    void AddComment(Comment comment, List<string> attachmentIds, bool fromGroup, Action<bool, Comment> callback, string stickerReferrer = "");

    void DeleteComment(long cid);
  }
}
