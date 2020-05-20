using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKClient.Common.Backend
{
  public class GroupsService
  {
    private static GroupsService _groupsService = new GroupsService();
    private Dictionary<long, Group> _cachedGroups = new Dictionary<long, Group>();

    public static GroupsService Current
    {
      get
      {
        return GroupsService._groupsService;
      }
    }

    public void AddCachedGroups(IEnumerable<Group> groups)
    {
      if (groups == null)
        return;
      foreach (Group group in groups)
        this.AddCachedGroup(group);
    }

    public void AddCachedGroup(Group group)
    {
      if (group == null)
        return;
      this._cachedGroups[group.id] = group;
    }

    public Group GetCachedGroup(long id)
    {
      if (this._cachedGroups.ContainsKey(id))
        return this._cachedGroups[id];
      return  null;
    }

    public void GetUserGroups(long userId, int offset, int count, string filter, Action<BackendResult<GroupsLists, ResultCode>> callback)
    {
      string str = string.Format("\r\n\r\nvar groups = API.groups.get({{ user_id: {0}, extended: 1, fields: \"members_count,activity,start_date,finish_date,verified\", filter: \"{1}\", count: {2}, offset: {3} }});\r\nvar invitations = API.groups.getInvites({{ count: 1, \"fields\": \"members_count\" }});\r\n\r\nvar first_invitation_community = null;\r\nvar first_invitation_inviter = null;\r\n\r\nif (invitations.items.length > 0)\r\n{{\r\n    first_invitation_community = invitations.items[0];\r\n    first_invitation_inviter = API.users.get({{ user_ids: first_invitation_community.invited_by, fields: \"sex\" }})[0];\r\n}}\r\n\r\nvar result = {{ \"groups\": groups }};\r\nif ({4})\r\n{{\r\n    result.invitations =\r\n    {{\r\n        \"count\": invitations.count,\r\n        \"first_invitation\":\r\n        {{\r\n            \"community\": first_invitation_community,\r\n            \"inviter\": first_invitation_inviter\r\n        }}\r\n    }};\r\n}}\r\n\r\nreturn result;\r\n\r\n                ", userId, filter, count, offset, (filter == "events" || filter == "moder" ? "false" : "true"));
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str);
      Action<BackendResult<GroupsLists, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<GroupsLists>(methodName, parameters, callback1, (Func<string, GroupsLists>) (jsonString =>
      {
        GenericRoot<GroupsListWithInvitations> genericRoot = JsonConvert.DeserializeObject<GenericRoot<GroupsListWithInvitations>>(jsonString);
        return new GroupsLists()
        {
          Communities = genericRoot.response.groups,
          Invitations = genericRoot.response.invitations
        };
      }), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetCommunityInvitations(int offset, int count, Action<BackendResult<CommunityInvitationsList, ResultCode>> callback)
    {
      string code = string.Format("\r\n\r\nvar inviters=[];\r\nvar invitations=API.groups.getInvites({{count:{0},offset:{1},\"fields\":\"members_count\"}});\r\n\r\nif (invitations.items.length>0)\r\n    inviters=API.users.get({{user_ids:invitations.items@.invited_by,fields:\"sex\"}});\r\n\r\nreturn\r\n{{\r\n    \"count\":invitations.count,\r\n    \"invitations\":invitations.items,\r\n    \"inviters\":inviters\r\n}};\r\n\r\n                ", count, offset);
      Action<BackendResult<CommunityInvitationsList, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.Execute<CommunityInvitationsList>(code, callback1, (Func<string, CommunityInvitationsList>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<CommunityInvitationsList>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken);
    }

    public void Join(long gid, bool notSure, Action<BackendResult<OwnCounters, ResultCode>> callback, string source = null)
    {
      VKRequestsDispatcher.Execute<OwnCounters>(string.Format("var gid = {0};\r\n\r\nAPI.groups.join({{\"group_id\": gid, \"not_sure\": {1}{2}}});\r\n\r\nreturn API.getCounters();", gid, notSure ? "1" : "0", !string.IsNullOrEmpty(source) ? string.Format(", \"source\": \"{0}\"", source) : ""), callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?());
    }

    public void Leave(long gid, Action<BackendResult<OwnCounters, ResultCode>> callback)
    {
      VKRequestsDispatcher.Execute<OwnCounters>(string.Format("var gid = {0};\r\n\r\nAPI.groups.leave({{\"group_id\": gid}});\r\n\r\nreturn API.getCounters();", gid), callback, new Func<string, OwnCounters>(CountersDeserializerHelper.Deserialize), false, true, new CancellationToken?());
    }

    public void Search(string searchStr, int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["q"] = searchStr;
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["fields"] = "start_date";
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Group>>("groups.search", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetGroupInfo(long groupId, Action<BackendResult<GroupData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<GroupData>("execute.getGroupInfo", new Dictionary<string, string>()
      {
        {
          "groupId",
          groupId.ToString()
        },
        {
          "func_v",
          "2"
        }
      }, callback, (Func<string, GroupData>) (jsonStr =>
      {
        jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
        jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "photos", true);
        jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "mediaSectionsSettings");
        GroupData response = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GroupData>>(jsonStr).response;
        GroupMembershipType groupMembershipType = GroupMembershipType.NotAMember;
        try
        {
          groupMembershipType = (GroupMembershipType) response.group.member_status;
          if (groupMembershipType == GroupMembershipType.InvitationRejected)
            groupMembershipType = GroupMembershipType.NotAMember;
        }
        catch
        {
          if (response.membership.member == 1)
            groupMembershipType = GroupMembershipType.Member;
          if (response.membership.invitation == 1)
            groupMembershipType = GroupMembershipType.InvitationReceived;
          if (response.membership.request == 1)
            groupMembershipType = GroupMembershipType.RequestSent;
        }
        response.group.MembershipType = groupMembershipType;
        if (Enum.IsDefined(typeof (ProfileMainSectionType), response.group.main_section))
          response.group.MainSection = (ProfileMainSectionType) response.group.main_section;
        this.AddCachedGroup(response.group);
        return response;
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetTopics(long gid, int offset, int count, Action<BackendResult<TopicsInfo, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["group_id"] = gid.ToString();
      parameters["extended"] = "1";
      parameters["preview"] = "2";
      parameters["preview_length"] = "40";
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<TopicsInfo>("board.getTopics", parameters, callback, (Func<string, TopicsInfo>) (jsonStr =>
      {
        GetTopicsResponse response = JsonConvert.DeserializeObject<GenericRoot<GetTopicsResponse>>(jsonStr).response;
        return new TopicsInfo()
        {
          can_add_topics = response.can_add_topics,
          profiles = response.profiles,
          topics = response.items,
          TotalCount = response.count,
          groups = response.groups
        };
      }), false, true, new CancellationToken?(),  null);
    }

    public void GetTopicComments(long gid, long tid, int offset, int count, Action<BackendResult<CommentsInfo, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string index1 = "group_id";
      string str1 = gid.ToString();
      dictionary[index1] = str1;
      string index2 = "topic_id";
      string str2 = tid.ToString();
      dictionary[index2] = str2;
      string index3 = "offset";
      string str3 = offset.ToString();
      dictionary[index3] = str3;
      string index4 = "count";
      string str4 = count.ToString();
      dictionary[index4] = str4;
      string index5 = "extended";
      string str5 = "1";
      dictionary[index5] = str5;
      string index6 = "need_likes";
      string str6 = "1";
      dictionary[index6] = str6;
      VKRequestsDispatcher.Execute<CommentsInfo>(string.Format("var cm = API.board.getComments({{\"group_id\":{0}, \"topic_id\":{1}, \"offset\":{2}, \"count\":{3}, \"extended\":1, \"need_likes\":1, \"allow_group_comments\":1  }});\r\nvar g = API.groups.getById({{\"group_id\":{0}}})[0];\r\nreturn {{CommentsResponse:cm, Group:g}};", gid, tid, offset, count), callback, (Func<string, CommentsInfo>) (jsonStr =>
      {
        GroupsService.TopicCommentsResponse response = JsonConvert.DeserializeObject<GenericRoot<GroupsService.TopicCommentsResponse>>(jsonStr).response;
        CommentsResponse commentsResponse = response.CommentsResponse;
        GroupsService.Current.AddCachedGroup(response.Group);
        return new CommentsInfo()
        {
          comments = commentsResponse.items,
          poll = commentsResponse.poll,
          profiles = commentsResponse.profiles,
          groups = commentsResponse.groups,
          TotalCommentsCount = commentsResponse.count
        };
      }), false, true, new CancellationToken?());
    }

    public void AddTopicComment(long gid, long tid, string text, List<string> attachments, Action<BackendResult<Comment, ResultCode>> callback, int stickerId = 0, bool fromGroup = false, string stickerReferrer = "")
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "code",
          string.Format("\r\n\r\nvar new_comment_id = API.board.addComment({{\r\n    group_id: {0},\r\n    topic_id: {1},\r\n    text: \"{2}\",\r\n    from_group: {3},\r\n    sticker_id: {4},\r\n    attachments: \"{5}\",\r\n    sticker_referrer: \"{6}\"\r\n}});\r\n\r\nvar last_comments = API.board.getComments({{\r\n    group_id: {7},\r\n    topic_id: {8},\r\n    need_likes: 1,\r\n    count: 10,\r\n    sort: \"desc\",\r\n    preview_length: 0,\r\n    allow_group_comments: 1\r\n}}).items;\r\n\r\nvar i = last_comments.length - 1;\r\nwhile (i >= 0)\r\n{{\r\n    if (last_comments[i].id == new_comment_id)\r\n        return last_comments[i];\r\n\r\n    i = i - 1;\r\n}}\r\n\r\nreturn null;\r\n\r\n                ", gid, tid, text.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), (fromGroup ? "1" : "0"), stickerId, attachments.GetCommaSeparated(","), stickerReferrer, gid, tid)
        }
      };
      string methodName = "execute";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<Comment, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<Comment>(methodName, parameters, callback1, (Func<string, Comment>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<Comment>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void DeleteComment(long gid, long tid, long cid, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["group_id"] = gid.ToString();
      parameters["topic_id"] = tid.ToString();
      parameters["comment_id"] = cid.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("board.deleteComment", parameters, callback, (Func<string, ResponseWithId>) (j => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void EditComment(long gid, long tid, long cid, string text, List<string> attachmentIds, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["group_id"] = gid.ToString();
      parameters["topic_id"] = tid.ToString();
      parameters["comment_id"] = cid.ToString();
      parameters["text"] = text;
      if (attachmentIds.Count > 0)
        parameters["attachments"] = attachmentIds.GetCommaSeparated(",");
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("board.editComment", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?(),  null);
    }

    public void CreateTopic(long group_id, string title, string text, List<string> attachments, bool fromGroup, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["group_id"] = group_id.ToString();
      dictionary["title"] = title;
      dictionary["text"] = text;
      dictionary["attachments"] = attachments.GetCommaSeparated(",");
      dictionary["from_group"] = fromGroup ? "1" : "0";
      string methodName = "board.addTopic";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ResponseWithId, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(methodName, parameters, callback1, (Func<string, ResponseWithId>) (jsonStr => JsonConvert.DeserializeObject<ResponseWithId>(jsonStr)), num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void GetCatalog(int categoryId, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["category_id"] = categoryId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Group>>("groups.getCatalog", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetCatalogCategoriesPreview(Action<BackendResult<GroupCatalogInfoExtended, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<GroupCatalogInfoExtended>("groups.getCatalogInfo", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void CreateCommunity(string name, string type, int subtype, Action<BackendResult<Group, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<Group>("groups.create", new Dictionary<string, string>()
      {
        {
          "title",
          name
        },
        {
          "type",
          type
        },
        {
          "subtype",
          subtype.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetCommunitySettings(long id, Action<BackendResult<CommunitySettings, ResultCode>> callback)
    {
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", string.Format("\r\n\r\nvar settings = API.groups.getSettings({{ \"group_id\": {0} }});\r\n\r\nif (settings.place != null)\r\n{{\r\n    if (settings.place.country != null)\r\n    {{\r\n        settings.place.country_id = settings.place.country;\r\n        settings.place.country_name = API.database.getCountriesById({{ \"country_ids\": settings.place.country }})[0].title;\r\n    }}\r\n\r\n    if (settings.place.city != null)\r\n    {{\r\n        settings.place.city_id = settings.place.city;\r\n        settings.place.city_name = API.database.getCitiesById({{ \"city_ids\": settings.place.city }})[0].title;\r\n    }}\r\n}}\r\n\r\nvar community = API.groups.getById({{ \"group_id\": {1}, fields: \"age_limits,start_date,finish_date\" }})[0];\r\n\r\nsettings.age_limits = community.age_limits;\r\nsettings.type = community.type;\r\n\r\nsettings.start_date = community.start_date;\r\nsettings.finish_date = community.finish_date;\r\n\r\nif (settings.type == \"event\")\r\n{{\r\n    var event_creator_id = API.groups.getMembers({{ \"group_id\": {2}, \"sort\": \"time_asc\", \"count\": 1, \"filter\": \"managers\" }}).items[0].id;\r\n    settings.event_creator = API.users.get({{ \"users_ids\": event_creator_id }})[0];\r\n\r\n    settings.event_available_organizers = API.groups.get({{ \"filter\": \"admin\", \"extended\": 1, \"count\": 1000 }}).items;\r\n}}\r\n\r\nreturn settings;\r\n\r\n                ", id, id, id));
      Action<BackendResult<CommunitySettings, ResultCode>> callback1 = callback;
      // ISSUE: variable of the null type
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      VKRequestsDispatcher.DispatchRequestToVK<CommunitySettings>(methodName, parameters, callback1, null, num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void SetCommunityInformation(long id, string name, string description, long category, long subcategory, string site, int accessLevel, string domain, int ageLimits, string foundationDate, long? eventOrganizerId, string eventOrganizerPhone, string eventOrganizerEmail, int eventStartDate, int eventFinishDate, Action<BackendResult<int, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "group_id",
          id.ToString()
        },
        {
          "title",
          name
        },
        {
          "description",
          description
        },
        {
          "subject",
          category.ToString()
        },
        {
          "public_category",
          category.ToString()
        },
        {
          "public_subcategory",
          subcategory.ToString()
        },
        {
          "website",
          site
        },
        {
          "access",
          accessLevel.ToString()
        },
        {
          "age_limits",
          ageLimits.ToString()
        }
      };
      if (domain != null)
        parameters.Add("screen_name", domain);
      if (foundationDate != null)
        parameters.Add("public_date", foundationDate);
      if (eventOrganizerId.HasValue)
      {
        parameters.Add("event_group_id", eventOrganizerId.Value.ToString());
        parameters.Add("phone", eventOrganizerPhone);
        parameters.Add("email", eventOrganizerEmail);
        parameters.Add("event_start_date", eventStartDate.ToString());
        parameters.Add("event_finish_date", eventFinishDate.ToString());
      }
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.edit", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetCommunityServices(long id, int wallOrComments, int photos, int videos, int audios, int documents, int discussions, int links, int events, int contacts, int strongLanguageFilter, int keyWordsFilter, string keyWords, Action<BackendResult<int, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.edit", new Dictionary<string, string>()
      {
        {
          "group_id",
          id.ToString()
        },
        {
          "wall",
          wallOrComments.ToString()
        },
        {
          "photos",
          photos.ToString()
        },
        {
          "video",
          videos.ToString()
        },
        {
          "audio",
          audios.ToString()
        },
        {
          "docs",
          documents.ToString()
        },
        {
          "topics",
          discussions.ToString()
        },
        {
          "links",
          links.ToString()
        },
        {
          "events",
          events.ToString()
        },
        {
          "contacts",
          contacts.ToString()
        },
        {
          "obscene_filter",
          strongLanguageFilter.ToString()
        },
        {
          "obscene_stopwords",
          keyWordsFilter.ToString()
        },
        {
          "obscene_words",
          keyWords
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetRequests(long id, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("groups.getRequests", new Dictionary<string, string>()
      {
        {
          "group_id",
          id.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "fields",
          "photo_50,photo_100,photo_max,occupation"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetInvitations(long id, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("groups.getInvitedUsers", new Dictionary<string, string>()
      {
        {
          "group_id",
          id.ToString()
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "fields",
          "photo_50,photo_100,photo_max,occupation"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetBlacklist(long id, int offset, int count, Action<BackendResult<BlockedUsers, ResultCode>> callback)
    {
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", string.Format("\r\n\r\nvar blocked_users = API.groups.getBanned({{ \"group_id\": {0}, \"offset\": {1}, \"count\": {2}, \"fields\": \"photo_50,photo_100,photo_max,sex\" }});\r\nvar managers = API.users.get({{ \"user_ids\": blocked_users.items@.ban_info@.admin_id, \"fields\": \"sex\" }});\r\nreturn {{ \"blocked_users\": blocked_users, \"managers\": managers }};\r\n\r\n                ", id, offset, count));
      Action<BackendResult<BlockedUsers, ResultCode>> callback1 = callback;
      // ISSUE: variable of the null type
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      VKRequestsDispatcher.DispatchRequestToVK<BlockedUsers>(methodName, parameters, callback1, null, num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void UnblockUser(long communityId, long userId, Action<BackendResult<int, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.unbanUser", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "user_id",
          userId.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void BlockUser(long communityId, long userId, int durationUnixTime, int reason, string comment, bool isCommentVisible, Action<BackendResult<int, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.banUser", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "user_id",
          userId.ToString()
        },
        {
          "end_date",
          durationUnixTime.ToString()
        },
        {
          "reason",
          reason.ToString()
        },
        {
          "comment",
          comment
        },
        {
          "comment_visible",
          isCommentVisible ? "1" : "0"
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void HandleRequest(long communityId, long userId, bool isAcception, Action<BackendResult<int, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "user_id",
          userId.ToString()
        }
      };
      VKRequestsDispatcher.DispatchRequestToVK<int>(isAcception ? "groups.approveRequest" : "groups.removeUser", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetCommunity(long communityId, string fields, Action<BackendResult<List<Group>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<List<Group>>("groups.getById", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "fields",
          fields
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void DeleteLink(long communityId, long linkId, Action<BackendResult<int, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.deleteLink", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "link_id",
          linkId.ToString()
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void AddLink(long communityId, string url, string description, Action<BackendResult<GroupLink, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<GroupLink>("groups.addLink", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "link",
          url
        },
        {
          "text",
          description
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void EditLink(long communityId, long linkId, string description, Action<BackendResult<int, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.editLink", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "link_id",
          linkId.ToString()
        },
        {
          "text",
          description
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetSubscribers(long communityId, int offset, int count, string filter, bool needManagersAndContacts, Action<BackendResult<CommunitySubscribers, ResultCode>> callback)
    {
      string str = string.Format("\r\n\r\nvar subscribers = API.groups.getMembers({{ \"group_id\": {0}, \"offset\": {1}, \"count\": {2}, \"filter\": \"{3}\", \"fields\": \"online,online_mobile,photo_max,first_name_acc,last_name_acc\" }});\r\n\r\nvar managers = [];\r\nvar contacts = [];\r\n\r\nif ({4})\r\n{{\r\n    managers = API.groups.getMembers({{ \"group_id\": {5}, \"filter\": \"managers\" }}).items;\r\n    contacts = API.groups.getById({{ \"group_id\": {6}, \"fields\": \"contacts\" }})[0].contacts;\r\n}}\r\n\r\nreturn \r\n{{\r\n    \"subscribers\": subscribers,\r\n    \"managers\": managers,\r\n    \"contacts\": contacts\r\n}};\r\n\r\n", communityId, offset, count, filter, (needManagersAndContacts ? "true" : "false"), communityId, communityId);
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str);
      Action<BackendResult<CommunitySubscribers, ResultCode>> callback1 = callback;
      // ISSUE: variable of the null type
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      VKRequestsDispatcher.DispatchRequestToVK<CommunitySubscribers>(methodName, parameters, callback1, null, num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void SearchSubscribers(string q, long communityId, int offset, int count, bool isFriendsOnly, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "q",
          q
        },
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "group_id",
          communityId.ToString()
        },
        {
          "fields",
          "online,online_mobile,photo_max"
        }
      };
      if (isFriendsOnly)
        parameters.Add("from_list", "friends");
      VKRequestsDispatcher.DispatchRequestToVK<VKList<User>>("users.search", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void GetManagers(long communityId, int offset, int count, bool needContacts, Action<BackendResult<CommunityManagers, ResultCode>> callback)
    {
      string str = string.Format("\r\n\r\nvar managers = API.groups.getMembers({{ \"group_id\": {0}, \"offset\": {1}, \"count\": {2}, \"filter\": \"managers\", \"fields\": \"photo_50,photo_100,photo_max\", \"sort\": \"time_desc\" }});\r\nvar contacts = [];\r\n\r\nif ({3})\r\n    contacts = API.groups.getById({{ \"group_id\": {4}, \"fields\": \"contacts\" }})[0].contacts;\r\n\r\nreturn \r\n{{\r\n    \"managers\": managers,\r\n    \"contacts\": contacts\r\n}};\r\n\r\n", communityId, offset, count, (needContacts ? "true" : "false"), communityId);
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str);
      Action<BackendResult<CommunityManagers, ResultCode>> callback1 = callback;
      // ISSUE: variable of the null type
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      VKRequestsDispatcher.DispatchRequestToVK<CommunityManagers>(methodName, parameters, callback1, null, num1 != 0, num2 != 0, cancellationToken, null);
    }

    public void EditManager(long communityId, long userId, CommunityManagementRole role, bool isContact, string position, string email, string phone, Action<BackendResult<int, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "user_id",
          userId.ToString()
        },
        {
          "is_contact",
          isContact ? "1" : "0"
        },
        {
          "contact_position",
          position
        },
        {
          "contact_email",
          email
        },
        {
          "contact_phone",
          phone
        }
      };
      switch (role)
      {
        case CommunityManagementRole.Moderator:
          parameters.Add("role", "moderator");
          break;
        case CommunityManagementRole.Editor:
          parameters.Add("role", "editor");
          break;
        case CommunityManagementRole.Administrator:
          parameters.Add("role", "administrator");
          break;
      }
      VKRequestsDispatcher.DispatchRequestToVK<int>("groups.editManager", parameters, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public void SetCommunityPlacement(long communityId, long countryId, long cityId, string address, string title, double latitude, double longitude, Action<BackendResult<PlacementEditingResult, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<PlacementEditingResult>("groups.editPlace", new Dictionary<string, string>()
      {
        {
          "group_id",
          communityId.ToString()
        },
        {
          "country_id",
          countryId.ToString()
        },
        {
          "city_id",
          cityId.ToString()
        },
        {
          "address",
          address
        },
        {
          "title",
          title
        },
        {
          "latitude",
          latitude.ToString((IFormatProvider) CultureInfo.InvariantCulture)
        },
        {
          "longitude",
          longitude.ToString((IFormatProvider) CultureInfo.InvariantCulture)
        }
      }, callback,  null, false, true, new CancellationToken?(),  null);
    }

    public class GroupInfo
    {
      public Group group { get; set; }

      public City city { get; set; }

      public Country country { get; set; }

      public GroupMembership membership { get; set; }

      public string suggestedCount { get; set; }

      public string postponedCount { get; set; }
    }

    public class TopicCommentsResponse
    {
      public CommentsResponse CommentsResponse { get; set; }

      public Group Group { get; set; }
    }
  }
}
