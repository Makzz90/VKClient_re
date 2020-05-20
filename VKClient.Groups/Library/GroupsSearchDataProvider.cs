using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using System.Linq;

namespace VKClient.Groups.Library
{
  public class GroupsSearchDataProvider : ISearchDataProvider<Group, GroupHeader>
  {
    private long _excludedId;
    private bool _isManagedOnly;

    public string LocalGroupName
    {
      get
      {
        return "";
      }
    }

    public string GlobalGroupName
    {
      get
      {
        return CommonResources.GlobalSearch.ToUpperInvariant();
      }
    }

    public IEnumerable<GroupHeader> LocalItems { get; private set; }

    public Func<VKList<Group>, ListWithCount<GroupHeader>> ConverterFunc
    {
        get
        {
            return (Func<VKList<Group>, ListWithCount<GroupHeader>>)(result =>
            {
                ListWithCount<GroupHeader> list = new ListWithCount<GroupHeader>()
                {
                    TotalCount = result.count
                };
                list.List = result.items.Where<Group>((Func<Group, bool>)(g =>
                {
                    if (g.id != this._excludedId)
                        return true;
                    --list.TotalCount;
                    return false;
                })).Select<Group, GroupHeader>((Func<Group, GroupHeader>)(g => new GroupHeader(g, (User)null))).ToList<GroupHeader>();
                return list;
            });
        }
    }

    public GroupsSearchDataProvider(IEnumerable<GroupHeader> localItems, long excludedId = 0, bool isManagedOnly = false)
    {
      this.LocalItems = localItems;
      this._excludedId = excludedId;
      this._isManagedOnly = isManagedOnly;
    }

    public string GetFooterTextForCount(int count)
    {
      if (count == 0)
        return CommonResources.NoCommunites;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCommunityFrm, CommonResources.TwoFourCommunitiesFrm, CommonResources.FiveCommunitiesFrm, true,  null, false);
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      if (this._isManagedOnly)
      {
        BackendResult<VKList<Group>, ResultCode> backendResult = new BackendResult<VKList<Group>, ResultCode>()
        {
          ResultCode = ResultCode.Succeeded,
          ResultData = new VKList<Group>()
        };
        callback(backendResult);
      }
      else
        GroupsService.Current.Search(searchString, offset, count, callback);
    }
  }
}
