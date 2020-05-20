using System;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class LikesViewModel : ViewModelBase, ICollectionDataProvider<LikesList, FriendHeader>
  {
    private int _allCount = -1;
    private long _ownerId;
    private long _itemId;
    private LikeObjectType _type;
    //private bool _isLoading;
    private GenericCollectionViewModel<LikesList, FriendHeader> _all;
    private GenericCollectionViewModel<LikesList, FriendHeader> _shared;
    private GenericCollectionViewModel<LikesList, FriendHeader> _friends;

    public GenericCollectionViewModel<LikesList, FriendHeader> All
    {
      get
      {
        return this._all;
      }
    }

    public GenericCollectionViewModel<LikesList, FriendHeader> Shared
    {
      get
      {
        return this._shared;
      }
    }

    public GenericCollectionViewModel<LikesList, FriendHeader> Friends
    {
      get
      {
        return this._friends;
      }
    }

    public string Title
    {
      get
      {
        if (this._allCount < 0)
          return " ";
        return UIStringFormatterHelper.FormatNumberOfSomething(this._allCount, CommonResources.LikesPage_OneLikedFrm, CommonResources.LikesPage_TwoFourLikedFrm, CommonResources.LikesPage_FiveLikedFrm, true, null, false);
      }
    }

    public Func<LikesList, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
        return (Func<LikesList, ListWithCount<FriendHeader>>) (l =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
          listWithCount.TotalCount = l.AllCount;
          foreach (long allId in l.AllIds)
          {
            long id = allId;
            if (id > 0L)
            {
              User user = l.All.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == id));
              if (user != null)
                listWithCount.List.Add(new FriendHeader(user, false));
            }
            if (id < 0L)
            {
              Group group = l.AllGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -id));
              if (group != null)
                listWithCount.List.Add(new FriendHeader(group));
            }
          }
          return listWithCount;
        });
      }
    }

    public LikesViewModel(long ownerId, long itemId, LikeObjectType type, int knownCount)
    {
      this._ownerId = ownerId;
      this._itemId = itemId;
      this._type = type;
      this._allCount = knownCount;
      this._all = new GenericCollectionViewModel<LikesList, FriendHeader>((ICollectionDataProvider<LikesList, FriendHeader>) this);
      this._all.LoadCount = 60;
      this._all.ReloadCount = 100;
      this._friends = new GenericCollectionViewModel<LikesList, FriendHeader>((ICollectionDataProvider<LikesList, FriendHeader>) this);
      this._friends.LoadCount = 60;
      this._friends.ReloadCount = 100;
      this._shared = new GenericCollectionViewModel<LikesList, FriendHeader>((ICollectionDataProvider<LikesList, FriendHeader>) this);
      this._shared.LoadCount = 60;
      this._shared.ReloadCount = 100;
    }

    public void GetData(GenericCollectionViewModel<LikesList, FriendHeader> caller, int offset, int count, Action<BackendResult<LikesList, ResultCode>> callback)
    {
      LikesService.Current.GetLikesList(this._type, this._ownerId, this._itemId, count, offset, caller == this._shared, caller == this._friends, (Action<BackendResult<LikesList, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded && caller == this._all)
        {
          this._allCount = res.ResultData.AllCount;
          this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
        }
        callback(res);
      }));
    }

    public string GetFooterTextForCount(int count)
    {
      if (count <= 0)
        return CommonResources.NoPersons;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<LikesList, FriendHeader> caller, int count)
    {
      return this.GetFooterTextForCount(count);
    }
  }
}
