using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class UsersSearchDataProvider : ISearchDataProvider<User, FriendHeader>
  {
    private readonly bool _allowGlobalSearch;

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

    public IEnumerable<FriendHeader> LocalItems { get; private set; }

    public Func<VKList<User>, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<User>, ListWithCount<FriendHeader>>) (res =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>() { TotalCount = res.count };
          foreach (User user in res.items)
            listWithCount.List.Add(new FriendHeader(user, false));
          return listWithCount;
        });
      }
    }

    public UsersSearchDataProvider(IEnumerable<FriendHeader> localItems, bool allowGlobalSearch)
    {
      this.LocalItems = localItems;
      this._allowGlobalSearch = allowGlobalSearch;
    }

    public string GetFooterTextForCount(int count)
    {
      if (count == 0)
        return CommonResources.NoPersons;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true,  null, false);
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      if (!this._allowGlobalSearch)
        callback(new BackendResult<VKList<User>, ResultCode>(ResultCode.Succeeded, new VKList<User>()));
      else
        UsersService.Instance.SearchUsers(searchString, offset, count, callback);
    }
  }
}
