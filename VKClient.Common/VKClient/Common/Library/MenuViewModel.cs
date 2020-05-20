using System;
using System.Windows;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class MenuViewModel : ViewModelBase, IHandle<CountersChanged>, IHandle, IHandle<BaseDataChangedEvent>
  {
    private UpcomingBirthdaysViewModel _upcomingBirthdaysVM = new UpcomingBirthdaysViewModel();

    public UpcomingBirthdaysViewModel UpcomingBirthdaysVM
    {
      get
      {
        return this._upcomingBirthdaysVM;
      }
    }

    public string UserPic
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max;
      }
    }

    public string UserStatus
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.LoggedInUser.activity;
      }
    }

    public Visibility UserStatusVisibility
    {
      get
      {
        return string.IsNullOrWhiteSpace(this.UserStatus) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string UserName
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.LoggedInUser.Name;
      }
    }

    public Visibility FeedbackCountVisibility
    {
      get
      {
        return this.FeedbackCount <= 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int FeedbackCount
    {
      get
      {
        return CountersManager.Current.Counters.notifications;
      }
    }

    public string FeedbackCountStr
    {
      get
      {
        return this.FormatForUI(this.FeedbackCount);
      }
    }

    public Visibility MessagesCountVisibility
    {
      get
      {
        return this.MessagesCount <= 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int MessagesCount
    {
      get
      {
        return CountersManager.Current.Counters.messages;
      }
    }

    public string MessagesCountStr
    {
      get
      {
        if (this.MessagesCount <= 0)
          return "";
        return this.MessagesCount.ToString();
      }
    }

    public Visibility GroupRequestsVisibility
    {
      get
      {
        return this.GroupRequestsCount <= 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int GroupRequestsCount
    {
      get
      {
        return CountersManager.Current.Counters.groups;
      }
    }

    public string GroupRequestsCountStr
    {
      get
      {
        return this.FormatForUI(this.GroupRequestsCount);
      }
    }

    public Visibility FriendRequestsVisibility
    {
      get
      {
        return this.FriendRequestsCount <= 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int FriendRequestsCount
    {
      get
      {
        return CountersManager.Current.Counters.friends;
      }
    }

    public string FriendRequestsCountStr
    {
      get
      {
        return this.FormatForUI(this.FriendRequestsCount);
      }
    }

    public Visibility GamesMenuItemVisibility
    {
      get
      {
        return !AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility GamesRequestsVisibility
    {
      get
      {
        return this.GamesRequestsCount <= 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int GamesRequestsCount
    {
      get
      {
        if (!AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled)
          return 0;
        return CountersManager.Current.Counters.app_requests;
      }
    }

    public string GamesRequestsCountStr
    {
      get
      {
        return this.FormatForUI(this.GamesRequestsCount);
      }
    }

    public int TotalCount
    {
      get
      {
        return this.GetSumAll();
      }
    }

    public string TotalCountStr
    {
      get
      {
        return this.FormatForUI(this.TotalCount);
      }
    }

    public bool HaveAnyNotifications
    {
      get
      {
        return this.TotalCount > 0;
      }
    }

    public double HaveAnyNotificationsOpacity
    {
      get
      {
        return this.HaveAnyNotifications ? 1.0 : 0.0;
      }
    }

    public Visibility HelpMenuVisibility
    {
      get
      {
        return !(AppGlobalStateManager.Current.GlobalState.SupportUri != "") ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public MenuViewModel()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    private int GetSumAll()
    {
      return this.FeedbackCount + this.GroupRequestsCount + this.FriendRequestsCount + this.MessagesCount + this.GamesRequestsCount;
    }

    public void Handle(CountersChanged message)
    {
      this.NotifyCountersPropChanged();
    }

    public void Handle(BaseDataChangedEvent message)
    {
      this.NotifyUserPropChanged();
      this.NotifyCountersPropChanged();
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HelpMenuVisibility));
      this._upcomingBirthdaysVM.UpdateData();
    }

    private void NotifyCountersPropChanged()
    {
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FeedbackCountVisibility));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.FeedbackCount));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.GroupRequestsCount));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.GroupRequestsVisibility));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.FriendRequestsCount));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FriendRequestsVisibility));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FriendRequestsCountStr));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.GamesMenuItemVisibility));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.GamesRequestsCount));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.GamesRequestsVisibility));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.GamesRequestsCountStr));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.GroupRequestsCountStr));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FeedbackCountStr));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.MessagesCount));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.MessagesCountStr));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.MessagesCountVisibility));
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.TotalCount));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.TotalCountStr));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveAnyNotifications));
      this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.HaveAnyNotificationsOpacity));
    }

    private void NotifyUserPropChanged()
    {
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UserPic));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UserStatus));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.UserStatusVisibility));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UserName));
    }

    private string FormatForUI(int count)
    {
      if (count <= 0)
        return "";
      return UIStringFormatterHelper.FormatForUIShort((long) count);
    }
  }
}
