using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
        this.NotifyPropertyChanged<PickUserMode>((Expression<Func<PickUserMode>>) (() => this.PickUserMode));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
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
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.CustomTitle));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
      }
    }

    public string Title
    {
      get
      {
        if (!string.IsNullOrEmpty(this.CustomTitle))
          return this.CustomTitle.ToUpperInvariant();
        if (this._pickUserMode != PickUserMode.PickForMessage)
          return "";
        if (!this.IsInSelectionMode)
          return CommonResources.ChooseRecipient;
        string str = CommonResources.ChooseRecipients;
        if (this.SelectedCount > 0)
          str = str + " (" + (object) this.SelectedCount + ")";
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
        this.NotifyPropertyChanged<int>((Expression<Func<int>>) (() => this.SelectedCount));
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
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
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
      }
    }

    public Func<List<User>, ListWithCount<Group<FriendHeader>>> ConverterFunc
    {
      get
      {
        return (Func<List<User>, ListWithCount<Group<FriendHeader>>>) (list =>
        {
          List<FriendHeader> friendHeaderList = new List<FriendHeader>();
          bool flag = list.Count >= 20;
          List<FriendHeader> list1 = list.Take<User>(flag ? 5 : 20).Select<User, FriendHeader>((Func<User, FriendHeader>) (s => new FriendHeader(s, false))).ToList<FriendHeader>();
          list1.ForEach((Action<FriendHeader>) (f => f.Initial = new char?()));
          if (flag)
            list1.AddRange(list.Select<User, FriendHeader>((Func<User, FriendHeader>) (s => new FriendHeader(s, false))));
          IEnumerable<Group<FriendHeader>> source;
          if (flag)
            source = list1.GroupBy<FriendHeader, char?>((Func<FriendHeader, char?>) (fh => fh.Initial)).OrderBy<IGrouping<char?, FriendHeader>, char?>((Func<IGrouping<char?, FriendHeader>, char?>) (g => g.Key)).Select<IGrouping<char?, FriendHeader>, Group<FriendHeader>>((Func<IGrouping<char?, FriendHeader>, Group<FriendHeader>>) (g => new Group<FriendHeader>(g.Key.ToString(), (IEnumerable<FriendHeader>) g, false)));
          else
            source = (IEnumerable<Group<FriendHeader>>) new List<Group<FriendHeader>>()
            {
              new Group<FriendHeader>("", (IEnumerable<FriendHeader>) list1, false)
            };
          ListWithCount<Group<FriendHeader>> listWithCount = new ListWithCount<Group<FriendHeader>>();
          listWithCount.List = source.ToList<Group<FriendHeader>>();
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
          List<FriendHeader> list = fl.Select<FriendsList, FriendHeader>((Func<FriendsList, FriendHeader>) (l => new FriendHeader(l))).ToList<FriendHeader>();
          this.ApplySelectionMode((IEnumerable<FriendHeader>) list);
          return new ListWithCount<FriendHeader>()
          {
            TotalCount = fl.Count,
            List = list
          };
        });
      }
    }

    public PickUserViewModel(PickUserMode mode, int sexFilter = 0)
    {
      this.PickUserMode = mode;
      this._sexFilter = sexFilter;
      this._friends = new GenericCollectionViewModel<List<User>, Group<FriendHeader>>((ICollectionDataProvider<List<User>, Group<FriendHeader>>) this);
      this._lists = new GenericCollectionViewModel<List<FriendsList>, FriendHeader>((ICollectionDataProvider<List<FriendsList>, FriendHeader>) this);
      if (mode != PickUserMode.PickForPrivacy)
        return;
      this.IsInSelectionMode = true;
    }

    private void ApplySelectionMode(IEnumerable<Group<FriendHeader>> headers)
    {
      foreach (Group<FriendHeader> header in headers)
      {
        header.IsInSelectedState = this.IsInSelectionMode;
        this.ApplySelectionMode((IEnumerable<FriendHeader>) header);
      }
    }

    private void ApplySelectionMode(IEnumerable<FriendHeader> headers)
    {
      foreach (FriendHeader header in headers)
      {
        header.IsInSelectedState = this.IsInSelectionMode;
        header.IsSelected = false;
        if (this._isInSelectionMode)
          header.PropertyChanged += new PropertyChangedEventHandler(this.fh_PropertyChanged);
        else
          header.PropertyChanged -= new PropertyChangedEventHandler(this.fh_PropertyChanged);
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
      SavedContacts friends = await FriendsCache.Instance.GetFriends();
      if ((DateTime.UtcNow - friends.SyncedDate).TotalMinutes < 60.0)
      {
        List<User> resultData = (List<User>) null;
        if (friends.SavedUsers != null)
          resultData = this._sexFilter > 0 ? friends.SavedUsers.Where<User>((Func<User, bool>) (u => u.sex == this._sexFilter)).ToList<User>() : friends.SavedUsers;
        this._allFriendsRaw = resultData;
        callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded, resultData));
      }
      else
        UsersService.Instance.GetFriendsForCurrentUser((Action<BackendResult<List<User>, ResultCode>>) (res =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
          {
            List<User> userList = (List<User>) null;
            if (res.ResultData != null)
              userList = this._sexFilter > 0 ? res.ResultData.Where<User>((Func<User, bool>) (u => u.sex == this._sexFilter)).ToList<User>() : res.ResultData;
            this._allFriendsRaw = userList;
          }
          callback(res);
        }));
    }

    public string GetFooterTextForCount(int count)
    {
      if (count == 0)
        return CommonResources.NoFriends;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, true, null, false);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, Group<FriendHeader>> caller, int count)
    {
      return this.GetFooterTextForCount(count);
    }

    internal List<FriendHeader> GetAllSelected()
    {
      List<FriendHeader> friendHeaderList = new List<FriendHeader>();
      foreach (Collection<FriendHeader> collection in (Collection<Group<FriendHeader>>) this.Friends.Collection)
      {
        foreach (FriendHeader friendHeader in collection)
        {
          if (friendHeader.IsSelected)
            friendHeaderList.Add(friendHeader);
        }
      }
      foreach (FriendHeader friendHeader in (Collection<FriendHeader>) this.Lists.Collection)
      {
        if (friendHeader.IsSelected)
          friendHeaderList.Add(friendHeader);
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
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendListFrm, CommonResources.TwoFourFriendsListsFrm, CommonResources.FiveFriendsListsFrm, true, null, false);
    }
  }
}
