using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
    public class SettingsNewViewModel : ViewModelBase, IHandle<BaseDataChangedEvent>, IHandle
    {
        //private bool _isInProgress;
        private User _currentUser;

        public string FullName
        {
            get
            {
                if (this.CurrentUser == null)
                    return string.Empty;
                return this.CurrentUser.Name;
            }
        }

        public User CurrentUser
        {
            get
            {
                return this._currentUser;
            }
            set
            {
                this._currentUser = value;
                this.NotifyPropertyChanged<User>((Expression<Func<User>>)(() => this.CurrentUser));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.FullName));
            }
        }

        public SettingsNewViewModel()
        {
            EventAggregator.Current.Subscribe((object)this);
        }

        public void LoadCurrentUser()
        {
            User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
            if (loggedInUser == null)
            {
                UsersService instance = UsersService.Instance;
                List<long> userIds = new List<long>();
                userIds.Add(AppGlobalStateManager.Current.LoggedInUserId);
                Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    this.CurrentUser = res.ResultData.First<User>();
                });
                instance.GetUsers(userIds, callback);
            }
            else
                this.CurrentUser = loggedInUser;
        }

        //internal void SetUserPicture(Stream stream, string fileName)
        //{
        //    int num = this._isInProgress ? 1 : 0;
        //}

        public void Handle(BaseDataChangedEvent message)
        {
            User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
            if (loggedInUser == null)
                return;
            this.CurrentUser.photo_max = loggedInUser.photo_max;
            this.NotifyPropertyChanged<User>((Expression<Func<User>>)(() => this.CurrentUser));
        }
    }
}
