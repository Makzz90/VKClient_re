using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
    public class SettingsNotificationsViewModel : ViewModelBase
    {
        public static List<PickableItem> DoNotDisturbOptions = new List<PickableItem>()
    {
      new PickableItem()
      {
        ID = 1L,
        Name = string.Format(CommonResources.OneHourFrm, "1")
      },
      new PickableItem()
      {
        ID = 2L,
        Name = string.Format(CommonResources.TwoFourHoursFrm, "2")
      },
      new PickableItem()
      {
        ID = 3L,
        Name = string.Format(CommonResources.TwoFourHoursFrm, "3")
      },
      new PickableItem()
      {
        ID = 5L,
        Name = string.Format(CommonResources.FiveHoursFrm, "5")
      },
      new PickableItem()
      {
        ID = 8L,
        Name = string.Format(CommonResources.FiveHoursFrm, "8")
      }
    };
        private bool _isInProgress;

        public new bool IsInProgress
        {
            get
            {
                return this._isInProgress;
            }
            set
            {
                this._isInProgress = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsInProgress));
                this.SetInProgress(this._isInProgress, "");
            }
        }

        public bool TempDisabled
        {
            get
            {
                if (this.PushNotificationsEnabled)
                    return AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil >= DateTime.UtcNow;
                return false;
            }
        }

        public string TempDisabledString
        {
            get
            {
                if (this.TempDisabled)
                {
                    DateTime dateTime = AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil + (DateTime.Now - DateTime.UtcNow);
                    return CommonResources.Settings_DisabledNotifications + " " + dateTime.ToShortTimeString();
                }
                return "";
            }
        }

        public bool PushNotificationsEnabledAndNotTempDisabled
        {
            get
            {
                if (this.PushNotificationsEnabled)
                    return !this.TempDisabled;
                return false;
            }
        }

        public bool PushNotificationsEnabled
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled;
            }
            set
            {
                if (this._isInProgress || value == this.PushNotificationsEnabled)
                    return;
                AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabled));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabledAndNotTempDisabled));
                this.IsInProgress = true;
                PushNotificationsManager.Instance.UpdateDeviceRegistration((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    this.IsInProgress = false;
                    if (res)
                        return;
                    AppGlobalStateManager.Current.GlobalState.PushNotificationsEnabled = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabled));
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabledAndNotTempDisabled));
                }))));
            }
        }

        private PushSettings GlobalPushSettings
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.PushSettings;
            }
        }

        public bool NewPrivateMessagesNotifications
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.PushSettings.msg;
            }
            set
            {
                if (this._isInProgress || this.NewPrivateMessagesNotifications == value)
                    return;
                AppGlobalStateManager.Current.GlobalState.PushSettings.msg = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NewPrivateMessagesNotifications));
                this.SetPushSetting("msg", this.GetMsgValue(), (Action)(() =>
                {
                    AppGlobalStateManager.Current.GlobalState.PushSettings.msg = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NewPrivateMessagesNotifications));
                }), "", "");
            }
        }

        public bool NewChatMessagesNotifications
        {
            get
            {
                return this.GlobalPushSettings.chat;
            }
            set
            {
                if (this._isInProgress || this.NewChatMessagesNotifications == value)
                    return;
                this.GlobalPushSettings.chat = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NewChatMessagesNotifications));
                this.SetPushSetting("chat", this.GetChatValue(), (Action)(() =>
                {
                    this.GlobalPushSettings.chat = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NewChatMessagesNotifications));
                }), "", "");
            }
        }

        public bool ShowTextInNotification
        {
            get
            {
                return !this.GlobalPushSettings.msg_no_text;
            }
            set
            {
                if (this._isInProgress || this.ShowTextInNotification == value)
                    return;
                this.GlobalPushSettings.msg_no_text = !value;
                this.GlobalPushSettings.chat_no_text = !value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.ShowTextInNotification));
                this.SetPushSetting("msg", this.GetMsgValue(), (Action)(() =>
                {
                    this.GlobalPushSettings.msg_no_text = value;
                    this.GlobalPushSettings.chat_no_text = value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.ShowTextInNotification));
                }), "chat", this.GetChatValue());
            }
        }

        public bool LikesNotifications
        {
            get
            {
                return this.GlobalPushSettings.like;
            }
            set
            {
                if (this._isInProgress || this.LikesNotifications == value)
                    return;
                this.GlobalPushSettings.like = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.LikesNotifications));
                this.SetPushSetting("like", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.like = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.LikesNotifications));
                }), "", "");
            }
        }

        public bool RepostsNotifications
        {
            get
            {
                return this.GlobalPushSettings.repost;
            }
            set
            {
                if (this._isInProgress || this.RepostsNotifications == value)
                    return;
                this.GlobalPushSettings.repost = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.RepostsNotifications));
                this.SetPushSetting("repost", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.repost = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.RepostsNotifications));
                }), "", "");
            }
        }

        public bool WallPostsNotifications
        {
            get
            {
                return this.GlobalPushSettings.wall_post;
            }
            set
            {
                if (this._isInProgress || this.WallPostsNotifications == value)
                    return;
                this.GlobalPushSettings.wall_post = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.WallPostsNotifications));
                this.SetPushSetting("wall_post", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.wall_post = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.WallPostsNotifications));
                }), "", "");
            }
        }

        public bool NewsSubscriptions
        {
            get
            {
                return this.GlobalPushSettings.new_post;
            }
            set
            {
                if (this._isInProgress || this.NewsSubscriptions == value)
                    return;
                this.GlobalPushSettings.new_post = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NewsSubscriptions));
                this.SetPushSetting("new_post", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.new_post = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.NewsSubscriptions));
                }), "", "");
            }
        }

        public bool CommentsNotifications
        {
            get
            {
                return this.GlobalPushSettings.comment;
            }
            set
            {
                if (this._isInProgress || this.CommentsNotifications == value)
                    return;
                this.GlobalPushSettings.comment = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CommentsNotifications));
                this.SetPushSetting("comment", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.comment = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CommentsNotifications));
                }), "", "");
            }
        }

        public bool MentionsNotifications
        {
            get
            {
                return this.GlobalPushSettings.mention;
            }
            set
            {
                if (this._isInProgress || this.MentionsNotifications == value)
                    return;
                this.GlobalPushSettings.mention = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.MentionsNotifications));
                this.SetPushSetting("mention", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.mention = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.MentionsNotifications));
                }), "", "");
            }
        }

        public bool RepliesNotifications
        {
            get
            {
                return this.GlobalPushSettings.reply;
            }
            set
            {
                if (this._isInProgress || this.RepliesNotifications == value)
                    return;
                this.GlobalPushSettings.reply = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.RepliesNotifications));
                this.SetPushSetting("reply", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.reply = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.RepliesNotifications));
                }), "", "");
            }
        }

        public bool FriendRequestsNotifications
        {
            get
            {
                return this.GlobalPushSettings.friend;
            }
            set
            {
                if (this._isInProgress || this.FriendRequestsNotifications == value)
                    return;
                this.GlobalPushSettings.friend = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FriendRequestsNotifications));
                this.SetPushSetting("friend", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.friend = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FriendRequestsNotifications));
                }), "", "");
            }
        }

        public bool GroupInvitationsNotifications
        {
            get
            {
                return this.GlobalPushSettings.group_invite;
            }
            set
            {
                if (this._isInProgress || this.GroupInvitationsNotifications == value)
                    return;
                this.GlobalPushSettings.group_invite = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.GroupInvitationsNotifications));
                this.SetPushSetting("group_invite", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.group_invite = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.GroupInvitationsNotifications));
                }), "", "");
            }
        }

        public bool GamesNotifications
        {
            get
            {
                return this.GlobalPushSettings.app_request;
            }
            set
            {
                if (this._isInProgress || this.GamesNotifications == value)
                    return;
                this.GlobalPushSettings.app_request = value;
                this.NotifyPropertyChanged("GamesNotifications");
                this.SetPushSetting("app_request", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.app_request = !value;
                    this.NotifyPropertyChanged("GamesNotifications");
                }), "", "");
            }
        }

        public Visibility GamesNotificationsVisibility
        {
            get
            {
                return !AppGlobalStateManager.Current.GlobalState.GamesSectionEnabled ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool GroupAcceptedNotifications
        {
            get
            {
                return this.GlobalPushSettings.group_accepted;
            }
            set
            {
                if (this._isInProgress || this.GroupAcceptedNotifications == value)
                    return;
                this.GlobalPushSettings.group_accepted = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.GroupAcceptedNotifications));
                this.SetPushSetting("group_accepted", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.group_accepted = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.GroupAcceptedNotifications));
                }), "", "");
            }
        }

        public bool BirthdaysNotifications
        {
            get
            {
                return this.GlobalPushSettings.birthday;
            }
            set
            {
                if (this._isInProgress || this.BirthdaysNotifications == value)
                    return;
                this.GlobalPushSettings.birthday = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.BirthdaysNotifications));
                this.SetPushSetting("birthday", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.birthday = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.BirthdaysNotifications));
                }), "", "");
            }
        }

        public bool ForthcomingEventsNotifications
        {
            get
            {
                return this.GlobalPushSettings.event_soon;
            }
            set
            {
                if (this._isInProgress || this.ForthcomingEventsNotifications == value)
                    return;
                this.GlobalPushSettings.event_soon = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.ForthcomingEventsNotifications));
                this.SetPushSetting("event_soon", PushSettings.GetOnOffStr(value), (Action)(() =>
                {
                    this.GlobalPushSettings.event_soon = !value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.ForthcomingEventsNotifications));
                }), "", "");
            }
        }

        public bool InAppSound
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.SoundEnabled;
            }
            set
            {
                AppGlobalStateManager.Current.GlobalState.SoundEnabled = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.InAppSound));
            }
        }

        public bool IsAppVibration
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.VibrationsEnabled;
            }
            set
            {
                AppGlobalStateManager.Current.GlobalState.VibrationsEnabled = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsAppVibration));
            }
        }

        public bool InAppBanner
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.NotificationsEnabled;
            }
            set
            {
                AppGlobalStateManager.Current.GlobalState.NotificationsEnabled = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.InAppBanner));
            }
        }

        public void Disable(int seconds)
        {
            if (this._isInProgress)
                return;
            DateTime savedValue = AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil;
            AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil = seconds != 0 ? DateTime.UtcNow + TimeSpan.FromSeconds((double)seconds) : DateTime.MinValue;
            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.TempDisabledString));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.TempDisabled));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabledAndNotTempDisabled));
            this.IsInProgress = true;
            AccountService.Instance.SetSilenceMode(AppGlobalStateManager.Current.GlobalState.RegisteredDeviceId, seconds, (Action<BackendResult<object, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.IsInProgress = false;
                if (res.ResultCode == ResultCode.Succeeded)
                    return;
                AppGlobalStateManager.Current.GlobalState.PushNotificationsBlockedUntil = savedValue;
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.TempDisabledString));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.TempDisabled));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.PushNotificationsEnabledAndNotTempDisabled));
            }))), 0L, 0L);
        }

        private string GetMsgValue()
        {
            if (!this.NewPrivateMessagesNotifications)
                return PushSettings.Off;
            if (this.ShowTextInNotification)
                return PushSettings.On;
            return PushSettings.NoText;
        }

        private string GetChatValue()
        {
            if (!this.NewChatMessagesNotifications)
                return PushSettings.Off;
            if (this.ShowTextInNotification)
                return PushSettings.On;
            return PushSettings.NoText;
        }

        public void SetPushSetting(string key, string value, Action rollbackCallback, string key2 = "", string value2 = "")
        {
            this.IsInProgress = true;
            if (!string.IsNullOrEmpty(AppGlobalStateManager.Current.GlobalState.RegisteredDeviceId))
                AccountService.Instance.SetPushSettings(AppGlobalStateManager.Current.GlobalState.DeviceId, key, value, key2, value2, (Action<BackendResult<ResponseWithId, ResultCode>>)(res =>
                {
                    this.IsInProgress = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        return;
                    Execute.ExecuteOnUIThread((Action)(() => rollbackCallback()));
                }));
            else
                PushNotificationsManager.Instance.UpdateDeviceRegistration((Action<bool>)(res =>
                {
                    this.IsInProgress = false;
                    if (res)
                        return;
                    Execute.ExecuteOnUIThread((Action)(() => rollbackCallback()));
                }));
        }
    }
}
