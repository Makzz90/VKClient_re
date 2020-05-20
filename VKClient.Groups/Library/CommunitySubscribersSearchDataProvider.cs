using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;
using System.Linq;
namespace VKClient.Groups.Library
{
  public class CommunitySubscribersSearchDataProvider : ISearchDataProvider<User, LinkHeader>
  {
    private readonly long _communityId;
    private readonly GroupType _communityType;
    private readonly List<User> _managers;
    private readonly bool _isManagement;
    private readonly bool _isFriendsOnly;

    public IEnumerable<LinkHeader> LocalItems { get; private set; }

    public string GlobalGroupName
    {
      get
      {
        return "";
      }
    }

    public string LocalGroupName
    {
      get
      {
        return "";
      }
    }

    public Func<VKList<User>, ListWithCount<LinkHeader>> ConverterFunc
    {
        get
        {
            return (Func<VKList<User>, ListWithCount<LinkHeader>>)(list =>
            {
                ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
                listWithCount.TotalCount = list.count;
                CommunityManagementRole currentUserRole = CommunityManagementRole.Unknown;
                User user1 = this._managers.FirstOrDefault<User>((Func<User, bool>)(m => m.id == AppGlobalStateManager.Current.LoggedInUserId));
                if (user1 != null)
                    currentUserRole = user1.Role;
                foreach (User user2 in list.items)
                {
                    User user = user2;
                    User user3 = this._managers.FirstOrDefault<User>((Func<User, bool>)(m => m.id == user.id));
                    if (user3 != null)
                        user.Role = user3.Role;
                    LinkHeader linkHeader = new LinkHeader(user, currentUserRole, this._isManagement);
                    listWithCount.List.Add(linkHeader);
                }
                return listWithCount;
            });
        }
    }

    public CommunitySubscribersSearchDataProvider(long communityId, GroupType communityType, List<User> managers, bool isManagement, bool isFriendsOnly)
    {
      this._communityId = communityId;
      this._communityType = communityType;
      this._managers = managers;
      this._isManagement = isManagement;
      this._isFriendsOnly = isFriendsOnly;
    }

    public void GetData(string q, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      GroupsService.Current.SearchSubscribers(q, this._communityId, offset, count, this._isFriendsOnly, callback);
    }

    public string GetFooterTextForCount(int count)
    {
      if (this._communityType == GroupType.PublicPage)
      {
        if (count <= 0)
          return GroupResources.NoSubscribersYet;
        return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneSubscriberFrm, GroupResources.TwoFourSubscribersFrm, GroupResources.FiveSubscribersFrm, true,  null, false);
      }
      if (count <= 0)
        return GroupResources.NoParticipantsYet;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneMemberFrm, GroupResources.TwoFourMembersFrm, GroupResources.FiveMembersFrm, true,  null, false);
    }
  }
}
