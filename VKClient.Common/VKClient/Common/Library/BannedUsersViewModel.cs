using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class BannedUsersViewModel : ViewModelBase, ICollectionDataProvider<VKList<User>, FriendHeader>, IHandle<UserIsBannedOrUnbannedEvent>, IHandle
  {
    private GenericCollectionViewModel<VKList<User>, FriendHeader> _bannedVM;
    private int _selectedCount;
    private bool _isDeleting;

    public GenericCollectionViewModel<VKList<User>, FriendHeader> BannedVM
    {
      get
      {
        return this._bannedVM;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.BannedUsers_BannedUsersTitle;
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
        this._selectedCount = value;
        this.NotifyPropertyChanged<int>((Expression<Func<int>>) (() => this.SelectedCount));
      }
    }

    public Func<VKList<User>, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<User>, ListWithCount<FriendHeader>>) (list =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
          listWithCount.TotalCount = list.count;
          foreach (User user in list.items)
          {
            FriendHeader friendHeader = new FriendHeader(user, false);
            friendHeader.IsInSelectedState = true;
            friendHeader.PropertyChanged += new PropertyChangedEventHandler(this.p_PropertyChanged);
            listWithCount.List.Add(friendHeader);
          }
          return listWithCount;
        });
      }
    }

    public BannedUsersViewModel()
    {
      this._bannedVM = new GenericCollectionViewModel<VKList<User>, FriendHeader>((ICollectionDataProvider<VKList<User>, FriendHeader>) this);
      EventAggregator.Current.Subscribe((object) this);
      this._bannedVM.NoItemsDescription = string.Format(CommonResources.BlackList_NoUsersDescFrm, (object) (Environment.NewLine + Environment.NewLine));
    }

    public void GetData(GenericCollectionViewModel<VKList<User>, FriendHeader> caller, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      AccountService.Instance.GetBannedUsers(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<User>, FriendHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPersons;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
    }

    private void p_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "IsSelected"))
        return;
      bool flag = false;
      if (sender is FriendHeader)
        flag = (sender as FriendHeader).IsSelected;
      if (flag)
        this.SelectedCount = this.SelectedCount + 1;
      else
        this.SelectedCount = this.SelectedCount - 1;
    }

    internal void DeleteSelected()
    {
      if (this._isDeleting)
        return;
      this._isDeleting = true;
      List<FriendHeader> fhToDelete = this._bannedVM.Collection.Where<FriendHeader>((Func<FriendHeader, bool>) (fh => fh.IsSelected)).ToList<FriendHeader>();
      if (fhToDelete.Count <= 0)
        return;
      AccountService.Instance.UnbanUsers(fhToDelete.Select<FriendHeader, long>((Func<FriendHeader, long>) (fh => fh.UserId)).ToList<long>(), (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            foreach (FriendHeader friendHeader in fhToDelete)
              this._bannedVM.Delete(friendHeader);
            this.SelectedCount = this.SelectedCount - fhToDelete.Count;
          }));
        this._isDeleting = false;
      }));
    }

    public void Handle(UserIsBannedOrUnbannedEvent message)
    {
      if (message.IsBanned)
      {
        FriendHeader friendHeader = new FriendHeader(message.user, false);
        friendHeader.IsInSelectedState = true;
        friendHeader.PropertyChanged += new PropertyChangedEventHandler(this.p_PropertyChanged);
        this.BannedVM.Insert(friendHeader, 0);
      }
      else
      {
        FriendHeader friendHeader = this.BannedVM.Collection.FirstOrDefault<FriendHeader>((Func<FriendHeader, bool>) (fh => fh.UserId == message.user.uid));
        if (friendHeader == null)
          return;
        this.BannedVM.Delete(friendHeader);
      }
    }
  }
}
