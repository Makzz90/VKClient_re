using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
    public class SettingsViewModel : ViewModelBase, IHandle<BaseDataChangedEvent>, IHandle
    {
        private User _currentUser;

        public User CurrentUser
        {
            get
            {
                return this._currentUser;
            }
            set
            {
                this._currentUser = value;
                this.NotifyPropertyChanged<string>(() => this.UserPhoto);
                this.NotifyPropertyChanged<string>(() => this.UserName);
                this.NotifyPropertyChanged<string>(() => this.UserStatus);
            }
        }

        public string UserPhoto
        {
            get
            {
                User currentUser = this._currentUser;
                return (currentUser != null ? currentUser.photo_max : null) ?? "";
            }
        }

        public string UserName
        {
            get
            {
                User currentUser = this._currentUser;
                return (currentUser != null ? currentUser.Name : null) ?? "";
            }
        }

        public string UserStatus
        {
            get
            {
                User currentUser = this._currentUser;
                return (currentUser != null ? currentUser.activity : null) ?? "";
            }
        }

        public Visibility MoneyTransfersVisibility
        {
            get
            {
                if (!AppGlobalStateManager.Current.GlobalState.MoneyTransfersEnabled)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public SettingsViewModel()
        {
            EventAggregator.Current.Subscribe(this);
        }

        public void LoadCurrentUser()
        {
            User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
            if (loggedInUser != null)
            {
                this._currentUser = loggedInUser;
            }
            else
            {
                UsersService instance = UsersService.Instance;
                List<long> userIds = new List<long>();
                long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
                userIds.Add(loggedInUserId);
                Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(result =>
                {
                    if (result.ResultCode != ResultCode.Succeeded)
                        return;
                    this._currentUser = result.ResultData.First<User>();
                });
                instance.GetUsers(userIds, callback);
            }
        }

        public void Handle(BaseDataChangedEvent message)
        {
            User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
            if (loggedInUser == null)
                return;
            this.CurrentUser = loggedInUser;
        }

        //
        internal double px_per_tick = 96.0 / 10.0 / 2.0;

        public double UserAvatarRadius
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick;
            }
        }
        //
    }
}
