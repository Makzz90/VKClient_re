using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class BirthdaysViewModel : ViewModelBase, IHandle<FriendsCacheUpdated>, IHandle, IHandle<UserIsLoggedOutEvent>
  {
    private bool _isDataUpdating;

    public ObservableCollection<Birthday> Birthdays{get;private set;}

    public Visibility BlockVisibility
    {
      get
      {
        return (this.Birthdays.Count > 0).ToVisiblity();
      }
    }

    public BirthdaysViewModel()
    {
         this.Birthdays = new ObservableCollection<Birthday>();

      EventAggregator.Current.Subscribe(this);
    }

    public async void UpdateData()
    {
      if (this._isDataUpdating)
        return;
      this._isDataUpdating = true;
      SavedContacts friends = await FriendsCache.Instance.GetFriends();
      this.Birthdays.Clear();
      foreach (User savedUser in friends.SavedUsers)
      {
        if (savedUser.IsBirthdayToday())
          this.Birthdays.Add(new Birthday(savedUser, "", true));
      }
      foreach (User savedUser in friends.SavedUsers)
      {
        if (savedUser.IsBirthdayTomorrow())
          this.Birthdays.Add(new Birthday(savedUser, CommonResources.Tomorrow.ToUpperInvariant(), false));
      }
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.BlockVisibility);
      this._isDataUpdating = false;
    }

    public void Handle(FriendsCacheUpdated message)
    {
      this.UpdateData();
    }

    public void Handle(UserIsLoggedOutEvent message)
    {
      this.Birthdays.Clear();
      // ISSUE: method reference
      base.NotifyPropertyChanged<Visibility>(() => this.BlockVisibility);
    }
  }
}
