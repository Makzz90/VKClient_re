using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PickUserViewModel : ViewModelBase, ICollectionDataProvider<List<User>, Group<FriendHeader>>, ICollectionDataProvider<List<FriendsList>, FriendHeader>
  {
    private List<User> _allFriendsRaw = new List<User>();
    private readonly int _sexFilter;
    private readonly long _productId;
    private GenericCollectionViewModel<List<User>, Group<FriendHeader>> _friends;
    private GenericCollectionViewModel<List<FriendsList>, FriendHeader> _lists;
    private PickUserMode _pickUserMode;
    private string _customTitle;
    private bool _isInSelectionMode;
    private int _selectedCount;

    public GenericCollectionViewModel<List<User>, Group<FriendHeader>> Friends
    {
      get
      {
        return this._friends;
      }
    }

    public GenericCollectionViewModel<List<FriendsList>, FriendHeader> Lists
    {
      get
      {
        return this._lists;
      }
    }

    public List<User> AllFriendsRaw
    {
      get
      {
        return this._allFriendsRaw;
      }
    }

    public PickUserMode PickUserMode
    {
      get
      {
        return this._pickUserMode;
      }
      set
      {
        this._pickUserMode = value;
        this.NotifyPropertyChanged<PickUserMode>(() => this.PickUserMode);
        this.NotifyPropertyChanged<string>(() => this.Title);
      }
    }

    public string CustomTitle
    {
      get
      {
        return this._customTitle;
      }
      set
      {
        this._customTitle = value;
        this.NotifyPropertyChanged<string>(() => this.CustomTitle);
        this.NotifyPropertyChanged<string>(() => this.Title);
      }
    }

    public string Title
    {
      get
      {
        if (!string.IsNullOrEmpty(this.CustomTitle))
          return ((string) this.CustomTitle).ToUpperInvariant();
        if (this._pickUserMode == PickUserMode.PickForStickerPackGift)
          return ((string) CommonResources.MoneyTransfers_ChooseRecipient).ToUpperInvariant();
        if (this._pickUserMode != PickUserMode.PickForMessage)
          return "";
        if (!this.IsInSelectionMode)
          return CommonResources.ChooseRecipient;
        string str = CommonResources.ChooseRecipients;
        if (this.SelectedCount > 0)
          str = string.Concat( new object[4]
          {
            str,
            " (",
            this.SelectedCount,
            ")"
          });
        return str;
      }
    }

    public int SelectedCount
    {
      get
      {
        return this._selectedCount;
      }
      set
      {
        if (this._selectedCount == value)
          return;
        this._selectedCount = value;
        this.NotifyPropertyChanged<int>(() => this.SelectedCount);
        this.NotifyPropertyChanged<string>(() => this.Title);
      }
    }

    public bool IsInSelectionMode
    {
      get
      {
        return this._isInSelectionMode;
      }
      set
      {
        if (this._isInSelectionMode == value)
          return;
        this._isInSelectionMode = value;
        this.ApplySelectionMode((IEnumerable<Group<FriendHeader>>) this.Friends.Collection);
        this.SelectedCount = 0;
        this.NotifyPropertyChanged<string>(() => this.Title);
      }
    }

    public Func<List<User>, ListWithCount<Group<FriendHeader>>> ConverterFunc
    {
      get
      {
        return (Func<List<User>, ListWithCount<Group<FriendHeader>>>) (list =>
        {
          bool flag = list.Count >= 20;
          int num = this._pickUserMode != PickUserMode.PickForStickerPackGift ? 5 : 0;
          List<FriendHeader> list1 = (List<FriendHeader>)Enumerable.ToList<FriendHeader>(Enumerable.Select<User, FriendHeader>(((IEnumerable<User>)list).Take<User>(flag ? num : 20), (Func<User, FriendHeader>)(s => new FriendHeader(s, false))));
          list1.ForEach((Action<FriendHeader>) (f => f.Initial = new char?()));
          if (flag)
              list1.AddRange((IEnumerable<FriendHeader>)Enumerable.Select<User, FriendHeader>(list, (Func<User, FriendHeader>)(s => new FriendHeader(s, false))));
          IEnumerable<Group<FriendHeader>> groups;
          if (flag)
              groups = (IEnumerable<Group<FriendHeader>>)Enumerable.Select<IGrouping<char?, FriendHeader>, Group<FriendHeader>>(Enumerable.OrderBy<IGrouping<char?, FriendHeader>, char?>(Enumerable.GroupBy<FriendHeader, char?>(list1, (Func<FriendHeader, char?>)(fh => fh.Initial)), (Func<IGrouping<char?, FriendHeader>, char?>)(g => g.Key)), (Func<IGrouping<char?, FriendHeader>, Group<FriendHeader>>)(g => new Group<FriendHeader>(g.Key.ToString(), g, false)));
          else
            groups = (IEnumerable<Group<FriendHeader>>) new List<Group<FriendHeader>>()
            {
              new Group<FriendHeader>("", (IEnumerable<FriendHeader>) list1, false)
            };
          ListWithCount<Group<FriendHeader>> listWithCount = new ListWithCount<Group<FriendHeader>>();
          listWithCount.List = (List<Group<FriendHeader>>) Enumerable.ToList<Group<FriendHeader>>(groups);
          this.ApplySelectionMode((IEnumerable<Group<FriendHeader>>) listWithCount.List);
          return listWithCount;
        });
      }
    }

    Func<List<FriendsList>, ListWithCount<FriendHeader>> ICollectionDataProvider<List<FriendsList>, FriendHeader>.ConverterFunc
    {
      get
      {
        return (Func<List<FriendsList>, ListWithCount<FriendHeader>>) (fl =>
        {
            List<FriendHeader> list = (List<FriendHeader>)Enumerable.ToList<FriendHeader>(Enumerable.Select<FriendsList, FriendHeader>(fl, (Func<FriendsList, FriendHeader>)(l => new FriendHeader(l))));
          this.ApplySelectionMode((IEnumerable<FriendHeader>) list);
          return new ListWithCount<FriendHeader>() { TotalCount = fl.Count, List = list };
        });
      }
    }

    private PickUserViewModel()
    {
      this._friends = new GenericCollectionViewModel<List<User>, Group<FriendHeader>>((ICollectionDataProvider<List<User>, Group<FriendHeader>>) this);
      this._lists = new GenericCollectionViewModel<List<FriendsList>, FriendHeader>((ICollectionDataProvider<List<FriendsList>, FriendHeader>) this);
    }

    public PickUserViewModel(PickUserMode mode, int sexFilter = 0)
      : this()
    {
      this.PickUserMode = mode;
      this._sexFilter = sexFilter;
      if (mode != PickUserMode.PickForPrivacy)
        return;
      this.IsInSelectionMode = true;
    }

    public PickUserViewModel(long productId)
      : this()
    {
      this._productId = productId;
      this.PickUserMode = PickUserMode.PickForStickerPackGift;
    }

    private void ApplySelectionMode(IEnumerable<Group<FriendHeader>> headers)
    {
      IEnumerator<Group<FriendHeader>> enumerator = headers.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Group<FriendHeader> current = enumerator.Current;
          current.IsInSelectedState = this.IsInSelectionMode;
          this.ApplySelectionMode((IEnumerable<FriendHeader>) current);
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    private void ApplySelectionMode(IEnumerable<FriendHeader> headers)
    {
      IEnumerator<FriendHeader> enumerator = headers.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          FriendHeader current = enumerator.Current;
          current.IsInSelectedState = this.IsInSelectionMode;
          current.IsSelected = false;
          if (this._isInSelectionMode)
            current.PropertyChanged += new PropertyChangedEventHandler(this.fh_PropertyChanged);
          else
            current.PropertyChanged -= new PropertyChangedEventHandler(this.fh_PropertyChanged);
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    private void fh_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "IsSelected"))
        return;
      if ((sender as FriendHeader).IsSelected)
        this.SelectedCount = this.SelectedCount + 1;
      else
        this.SelectedCount = this.SelectedCount - 1;
    }

    public async void GetData(GenericCollectionViewModel<List<User>, Group<FriendHeader>> caller, int offset, int count, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      if (this._pickUserMode == PickUserMode.PickForStickerPackGift)
      {
        StoreService.Instance.GetFriendsList(this._productId, (Action<BackendResult<List<User>, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
            this._allFriendsRaw = result.ResultData;
          Action<BackendResult<List<User>, ResultCode>> action = callback;
          if (action == null)
            return;
          BackendResult<List<User>, ResultCode> backendResult = result;
          action(backendResult);
        }));
      }
      else
      {
        SavedContacts friends = (SavedContacts) await FriendsCache.Instance.GetFriends();
        if ((DateTime.UtcNow - friends.SyncedDate).TotalMinutes < 60.0)
        {
          List<User> resultData =  null;
          if (friends.SavedUsers != null)
              resultData = this._sexFilter > 0 ? (List<User>)Enumerable.ToList<User>(Enumerable.Where<User>(friends.SavedUsers, (Func<User, bool>)(u => u.sex == this._sexFilter))) : friends.SavedUsers;
          this._allFriendsRaw = resultData;
          callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded, resultData));
        }
        else
          UsersService.Instance.GetFriendsForCurrentUser((Action<BackendResult<List<User>, ResultCode>>) (res =>
          {
            if (res.ResultCode == ResultCode.Succeeded)
            {
              List<User> userList =  null;
              if (res.ResultData != null)
                  userList = this._sexFilter > 0 ? (List<User>)Enumerable.ToList<User>(Enumerable.Where<User>(res.ResultData, (Func<User, bool>)(u => u.sex == this._sexFilter))) : res.ResultData;
              this._allFriendsRaw = userList;
            }
            callback(res);
          }));
      }
    }

    public string GetFooterTextForCount(int count)
    {
      if (count == 0)
        return CommonResources.NoFriends;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, true,  null, false);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, Group<FriendHeader>> caller, int count)
    {
      return this.GetFooterTextForCount(count);
    }

    internal List<FriendHeader> GetAllSelected()
    {
      List<FriendHeader> friendHeaderList = new List<FriendHeader>();
      IEnumerator<Group<FriendHeader>> enumerator1 = ((Collection<Group<FriendHeader>>) this.Friends.Collection).GetEnumerator();
      try
      {
        while (enumerator1.MoveNext())
        {
          IEnumerator<FriendHeader> enumerator2 = ((Collection<FriendHeader>) enumerator1.Current).GetEnumerator();
          try
          {
            while (enumerator2.MoveNext())
            {
              FriendHeader current = enumerator2.Current;
              if (current.IsSelected)
                friendHeaderList.Add(current);
            }
          }
          finally
          {
            if (enumerator2 != null)
              enumerator2.Dispose();
          }
        }
      }
      finally
      {
        if (enumerator1 != null)
          enumerator1.Dispose();
      }
      IEnumerator<FriendHeader> enumerator3 = ((Collection<FriendHeader>) this.Lists.Collection).GetEnumerator();
      try
      {
        while (enumerator3.MoveNext())
        {
          FriendHeader current = enumerator3.Current;
          if (current.IsSelected)
            friendHeaderList.Add(current);
        }
      }
      finally
      {
        if (enumerator3 != null)
          enumerator3.Dispose();
      }
      return friendHeaderList;
    }

    public void GetData(GenericCollectionViewModel<List<FriendsList>, FriendHeader> caller, int offset, int count, Action<BackendResult<List<FriendsList>, ResultCode>> callback)
    {
      if (EditPrivacyViewModel.FriendsAndListsCached != null)
        callback(new BackendResult<List<FriendsList>, ResultCode>(ResultCode.Succeeded, EditPrivacyViewModel.FriendsAndListsCached.friendLists));
      else
        UsersService.Instance.GetLists(callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<FriendsList>, FriendHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoFriendsLists;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendListFrm, CommonResources.TwoFourFriendsListsFrm, CommonResources.FiveFriendsListsFrm, true,  null, false);
    }
  }
}
