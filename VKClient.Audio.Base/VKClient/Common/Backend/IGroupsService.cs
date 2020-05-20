using System;
using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public interface IGroupsService
  {
    void GetUserGroups(long uid, int offset, int count, Action<BackendResult<GroupsLists, ResultCode>> callback);

    void GetGroupInvitations(int offset, int count, Action<BackendResult<GroupsListWithCount, ResultCode>> callback);

    void Join(long gid, bool notSure, Action<BackendResult<OwnCounters, ResultCode>> callback);

    void Leave(long gid, Action<BackendResult<OwnCounters, ResultCode>> callback);

    void Search(string searchStr, int offset, int count, Action<BackendResult<GroupsListWithCount, ResultCode>> callback);

    void GetGroupInfo(long gid, Action<BackendResult<Group, ResultCode>> callback);

    void GetMembers(long gid, int offset, int count, Action<BackendResult<UsersListWithCount, ResultCode>> callback);

    void GetTopics(long gid, int offset, int count, Action<BackendResult<TopicsInfo, ResultCode>> callback);

    void GetTopicComments(long gid, long tid, int offset, int count, Action<BackendResult<CommentsInfo, ResultCode>> callback);

    void CreateTopic(long group_id, string title, string text, List<string> attachments, bool fromGroup, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void AddTopicComment(long gid, long tid, string text, List<string> attachments, Action<BackendResult<ResponseWithId, ResultCode>> callback, int stickerId = 0);

    void DeleteComment(long gid, long tid, long cid, Action<BackendResult<ResponseWithId, ResultCode>> callback);

    void EditComment(long gid, long tid, long cid, string text, List<string> attachmentIds, Action<BackendResult<ResponseWithId, ResultCode>> callback);
  }
}
