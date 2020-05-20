using System;
using System.Collections.ObjectModel;
using System.Windows;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class UpcomingBirthdaysViewModel : ViewModelBase, IHandle<FriendsCacheUpdated>, IHandle, IHandle<UserIsLoggedOutEvent>
  {
    private ObservableCollection<BirthdayInfo> _birthdays = new ObservableCollection<BirthdayInfo>();
    private bool _isInUpdateData;

    public ObservableCollection<BirthdayInfo> Birthdays
    {
      get
      {
        return this._birthdays;
      }
    }

    public bool HaveAny
    {
      get
      {
        return this._birthdays.Count > 0;
      }
    }

    public Visibility HaveAnyVisibility
    {
      get
      {
        return !this.HaveAny ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public UpcomingBirthdaysViewModel()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    public async void UpdateData()
    {
      if (this._isInUpdateData)
        return;
      this._isInUpdateData = true;
      SavedContacts friends = await FriendsCache.Instance.GetFriends();
      this._birthdays.Clear();
      foreach (User savedUser in friends.SavedUsers)
      {
        if (savedUser.IsBirthdayToday())
          this._birthdays.Add(new BirthdayInfo(savedUser, CommonResources.HasABirthdayToday));
      }
      foreach (User savedUser in friends.SavedUsers)
      {
        if (savedUser.IsBirthdayTomorrow())
          this._birthdays.Add(new BirthdayInfo(savedUser, CommonResources.HasABirthdayTomorrow));
      }
      this.NotifyChanged();
      this._isInUpdateData = false;
    }

    public void Handle(FriendsCacheUpdated message)
    {
      this.UpdateData();
    }

    public void Handle(UserIsLoggedOutEvent message)
    {
      this._birthdays.Clear();
      this.NotifyChanged();
    }

    public void NotifyChanged()
    {
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveAny));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HaveAnyVisibility));
    }
  }
}
