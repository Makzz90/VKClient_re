using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
    public class ConversationHeader : ViewModelBase, IBinarySerializable, IHandle<NotificationSettingsChangedEvent>, IHandle, IHandle<MessagesFromGroupAllowedDeniedEvent>
    {
        private static readonly Thickness _onlineMargin = new Thickness(12.0, 0.0, 12.0, 0.0);
        private static readonly Thickness _onlineMobileMargin = new Thickness(12.0, 0.0, 20.0, 0.0);
        private static readonly Thickness _offlineMargin = new Thickness(12.0, 0.0, 0.0, 0.0);
        private static readonly Thickness _mutedMargin = new Thickness(12.0, 0.0, 24.0, 0.0);
        private static readonly Thickness _dateTextMarginUserThumb = new Thickness(0.0, 61.0, 12.0, 0.0);
        private static readonly Thickness _dateTextMarginNoUserThumb = new Thickness(0.0, 53.0, 12.0, 0.0);
        private string _uiTitle = string.Empty;
        private string _uiBody = string.Empty;
        private string _uiDate = string.Empty;
        private bool _isRead = true;
        private ConversationAvatarViewModel _conversationAvatarVM = new ConversationAvatarViewModel();
        private Visibility _typingVisibility = Visibility.Collapsed;
        public Message _message;
        public List<User> _associatedUsers;
        private bool _isMessagesFromGroupDenied;
        private int _unread;
        private bool _isOnline;
        private bool _isOnlineMobile;
        private string _typingStr;
        private bool _changingNotifications;
        private bool _isMessageFromGroupBusy;

        public ConversationAvatarViewModel ConversationAvatarVM
        {
            get
            {
                return this._conversationAvatarVM;
            }
        }

        public int Unread
        {
            get
            {
                return this._unread;
            }
            set
            {
                this._unread = value;
                this.NotifyPropertyChanged<int>((Expression<Func<int>>)(() => this.Unread));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.HaveUnreadMessagesVisibility));
            }
        }

        public Visibility HaveUnreadMessagesVisibility
        {
            get
            {
                if (this.Unread <= 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public SolidColorBrush HaveUnreadMessagesBackground
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources[((!this.AreNotificationsDisabled ? "Phone" : "PhoneMuted" + "ConversationNewMessagesCountBackgroundBrush"))];
            }
        }

        public string UITitle
        {
            get
            {
                return this._uiTitle;
            }
            set
            {
                if (!(this._uiTitle != value))
                    return;
                this._uiTitle = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UITitle));
            }
        }

        public string UIBody
        {
            get
            {
                return this._uiBody;
            }
            set
            {
                if (!(this._uiBody != value))
                    return;
                this._uiBody = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UIBody));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UIBodyNoUserThumb));
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UIBodyUserThumb));
            }
        }

        public string UIBodyNoUserThumb
        {
            get
            {
                if (this.NoUserThumbVisibility != Visibility.Visible)
                    return "";
                return this.UIBody;
            }
        }

        public string UIBodyUserThumb
        {
            get
            {
                if (this.UserThumbVisibility != Visibility.Visible)
                    return "";
                return this.UIBody;
            }
        }

        public string UIDate
        {
            get
            {
                return this._uiDate;
            }
            set
            {
                if (!(this._uiDate != value))
                    return;
                this._uiDate = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UIDate));
            }
        }

        public bool IsMessagesFromGroupDenied
        {
            get
            {
                return this._isMessagesFromGroupDenied;
            }
            set
            {
                if (this._isMessagesFromGroupDenied == value)
                    return;
                this._isMessagesFromGroupDenied = value;
                User user = this._associatedUsers.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == (long)this._message.uid));
                if (user != null)
                {
                    user.is_messages_blocked = value ? 1 : 0;
                    UsersService.Instance.SetCachedUser(user);
                }
                this.RespondToSettingsChange();
            }
        }

        public bool IsRead
        {
            get
            {
                return this._isRead;
            }
            set
            {
                if (this._isRead == value)
                    return;
                this._isRead = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsRead));
                this.NotifyPropertyChanged<FontFamily>((Expression<Func<FontFamily>>)(() => this.FontFamily));
                this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.TextForegroundBrush));
                this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.TextBackgroundBrush));
                this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.MainBackgroundBrush));
            }
        }

        public SolidColorBrush TextForegroundBrush
        {
            get
            {
                return !this.IsRead ? (SolidColorBrush)Application.Current.Resources["PhoneDialogsTextUnreadForegroundBrush"] : (SolidColorBrush)Application.Current.Resources["PhoneDialogsTextForegroundBrush"];
            }
        }

        public SolidColorBrush TextBackgroundBrush
        {
            get
            {
                return this._message.@out != 1 || this.IsRead ? (SolidColorBrush)Application.Current.Resources["PhoneDialogsBackgroundBrush"] : (SolidColorBrush)Application.Current.Resources["PhoneDialogsUnreadBackgroundBrush"];
            }
        }

        public SolidColorBrush MainBackgroundBrush
        {
            get
            {
                if (this._message.@out == 0 && !this.IsRead)
                    return (SolidColorBrush)Application.Current.Resources["PhoneDialogsUnreadBackgroundBrush"];
                return (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            }
        }

        public bool IsChat
        {
            get
            {
                return (uint)this._message.chat_id > 0U;
            }
        }

        public Visibility IsChatVisibility
        {
            get
            {
                if (!this.IsChat || !string.IsNullOrEmpty(this._message.photo_200))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility IsNotChatVisibility
        {
            get
            {
                if (this.IsChatVisibility != Visibility.Visible)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public long UserOrChatId
        {
            get
            {
                return this.IsChat ? (long)this._message.chat_id : (long)this._message.uid;
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                if (!this.IsRead)
                    return new FontFamily("Segoe WP Semibold");
                return new FontFamily("Segoe WP");
            }
        }

        public Visibility NoUserThumbVisibility
        {
            get
            {
                if (this.UserThumbVisibility != Visibility.Visible)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility UserThumbVisibility
        {
            get
            {
                if (!this.IsChat && this._message.@out != 1)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string UserThumb
        {
            get
            {
                if (this._message.@out == 1)
                    return AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max;
                if (this.IsChat)
                {
                    User user = this.User;
                    if (user != null)
                        return user.photo_max;
                }
                return "";
            }
        }

        public User User
        {
            get
            {
                return this._associatedUsers.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == (long)this._message.uid));
            }
        }

        public User User2
        {
            get
            {
                return this._associatedUsers.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == this._message.action_mid));
            }
        }

        public bool IsOnline
        {
            get
            {
                return this._isOnline;
            }
            set
            {
                if (this._isOnline == value)
                    return;
                this._isOnline = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsOnline));
                this.NotifyPropertyChanged<Thickness>((Expression<Func<Thickness>>)(() => this.IsOnlineOrOnlineMobileMargin));
                this.NotifyPropertyChanged<Thickness>((Expression<Func<Thickness>>)(() => this.TitleMargin));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.IsOnlineVisibility));
            }
        }

        public bool IsOnlineMobile
        {
            get
            {
                return this._isOnlineMobile;
            }
            set
            {
                if (this._isOnlineMobile == value)
                    return;
                this._isOnlineMobile = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsOnlineMobile));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.IsOnlineVisibility));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.IsOnlineMobileVisibility));
                this.NotifyPropertyChanged<Thickness>((Expression<Func<Thickness>>)(() => this.IsOnlineOrOnlineMobileMargin));
                this.NotifyPropertyChanged<Thickness>((Expression<Func<Thickness>>)(() => this.TitleMargin));
            }
        }

        public Visibility IsOnlineMobileVisibility
        {
            get
            {
                if (!this.IsOnlineMobile)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Thickness IsOnlineOrOnlineMobileMargin
        {
            get
            {
                if (this.AreNotificationsDisabled)
                    return ConversationHeader._mutedMargin;
                if (this.IsOnlineMobile)
                    return ConversationHeader._onlineMobileMargin;
                if (this.IsOnline)
                    return ConversationHeader._onlineMargin;
                return ConversationHeader._offlineMargin;
            }
        }

        public Visibility IsOnlineVisibility
        {
            get
            {
                if (!this.IsOnline || this.IsOnlineMobile)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Thickness DateTextMargin
        {
            get
            {
                if (this.UserThumbVisibility != Visibility.Visible)
                    return ConversationHeader._dateTextMarginNoUserThumb;
                return ConversationHeader._dateTextMarginUserThumb;
            }
        }

        public bool AreNotificationsDisabled
        {
            get
            {
                if (this._message != null && this.IsChat)
                    return this._message.push_settings.AreDisabledNow;
                return false;
            }
            set
            {
                int num = value ? -1 : 0;
                if (this._message.push_settings.disabled_until == num)
                    return;
                this._message.push_settings.disabled_until = num;
                this.RespondToSettingsChange();
            }
        }

        public Visibility NotificationsDisabledVisibility
        {
            get
            {
                if (!this.AreNotificationsDisabled)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Thickness TitleMargin
        {
            get
            {
                if (this.AreNotificationsDisabled)
                    return new Thickness(0.0, 0.0, 22.0, 0.0);
                if (this.IsOnlineMobileVisibility == Visibility.Visible)
                    return new Thickness(0.0, 0.0, 17.0, 0.0);
                if (this.IsOnlineVisibility == Visibility.Visible)
                    return new Thickness(0.0, 0.0, 13.0, 0.0);
                return new Thickness();
            }
        }

        public string TypingStr
        {
            get
            {
                return this._typingStr;
            }
            set
            {
                this._typingStr = value;
                this.NotifyPropertyChanged("TypingStr");
            }
        }

        public Visibility TypingVisibility
        {
            get
            {
                return this._typingVisibility;
            }
            set
            {
                this._typingVisibility = value;
                this.NotifyPropertyChanged("TypingVisibility");
            }
        }

        public UsersTypingHelper UsersTypingHelper { get; set; }

        public ObservableCollection<MenuItemData> MenuItems
        {
            get
            {
                ObservableCollection<MenuItemData> observableCollection1 = new ObservableCollection<MenuItemData>();
                if (this.IsChat)
                {
                    string str = this.AreNotificationsDisabled ? CommonResources.TurnOnNotifications : CommonResources.TurnOffNotifications;
                    observableCollection1.Add(new MenuItemData()
                    {
                        Tag = "disableEnable",
                        Title = str
                    });
                }
                if (this.UserOrChatId < 0L && this.UserOrChatId > -2000000000L)
                {
                    ObservableCollection<MenuItemData> observableCollection2 = observableCollection1;
                    MenuItemData menuItemData = new MenuItemData();
                    menuItemData.Tag = "messagesFromGroup";
                    string str = this._isMessagesFromGroupDenied ? CommonResources.MessagesFromGroupAllow.ToLowerInvariant() : CommonResources.MessagesFromGroupDeny.ToLowerInvariant();
                    menuItemData.Title = str;
                    observableCollection2.Add(menuItemData);
                }
                observableCollection1.Add(new MenuItemData()
                {
                    Tag = "delete",
                    Title = CommonResources.Conversation_Delete
                });
                return observableCollection1;
            }
        }

        public bool HaveEmoji
        {
            get
            {
                return BrowserNavigationService.ContainsEmoji(this.UIBody);
            }
        }

        public ConversationHeader(Message message, List<User> associatedUsers, int unread)
            : this()
        {
            this.SetMessageAndUsers(message, associatedUsers);
            this._unread = unread;
        }

        public ConversationHeader()
        {
            EventAggregator.Current.Subscribe(this);
        }

        public void SetMessageAndUsers(Message message, List<User> associatedUsers)
        {
            this._message = message;
            this._associatedUsers = associatedUsers;
            if (this._message == null || this._associatedUsers == null)
                return;
            this.RefreshUIProperties(false);
        }

        public virtual void RefreshUIProperties(bool suppressBodyRefresh = false)
        {
            this.IsRead = this._message.read_state == 1;
            string defaultAvatar = "";
            List<long> chatParticipantsIds = new List<long>();
            if (this._message.chat_id != 0)
            {
                defaultAvatar = this._message.photo_200;
                chatParticipantsIds = this._message.chat_active_str.ParseCommaSeparated();
                this.IsOnline = false;
                this.IsOnlineMobile = false;
                this.UITitle = this._message.title;
                if (!suppressBodyRefresh)
                    this.UIBody = this.GetHeaderBody();
                this.UIDate = this.FormatUIDate(this._message.date);
            }
            else
            {
                User user = this._associatedUsers.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == (long)this._message.uid));
                if (user != null)
                {
                    this.UITitle = this.FormatTitleForUser(user);
                    defaultAvatar = user.photo_max;
                    if (user.id < 0L && user.id > -2000000000L)
                        this.IsMessagesFromGroupDenied = user.is_messages_blocked == 1;
                }
                else
                    this.UITitle = "user_id " + this._message.uid;
                if (!suppressBodyRefresh)
                    this.UIBody = this.GetHeaderBody();
                this.UIDate = this.FormatUIDate(this._message.date);
            }
            this._conversationAvatarVM.Initialize(defaultAvatar, (uint)this._message.chat_id > 0U, chatParticipantsIds, this._associatedUsers);
        }

        public static string GetMessageHeaderText(Message message, User user, User user2)
        {
            if (!string.IsNullOrWhiteSpace(message.body))
            {
                string input = BrowserNavigationService.Regex_DomainMention.Replace(message.body, "[$2|$4]");
                return BrowserNavigationService.Regex_Mention.Replace(input, "$4");
            }
            if (!string.IsNullOrWhiteSpace(message.action))
                return SystemMessageTextHelper.GenerateText(message, user, user2, false);
            if (message.attachments != null && message.attachments.Count > 0)
            {
                Attachment firstAttachment = message.attachments.First<Attachment>();
                int num = message.attachments.Count<Attachment>((Func<Attachment, bool>)(a => a.type == firstAttachment.type));
                string lowerInvariant = firstAttachment.type.ToLowerInvariant();

                if (lowerInvariant == "money_transfer")
                    return CommonResources.MoneyTransfer;
                else if (lowerInvariant == "link")
                    return CommonResources.Link;
                else if (lowerInvariant == "wall")
                    return CommonResources.Conversation_WallPost;
                else if (lowerInvariant == "gift")
                    return CommonResources.Gift;
                else if (lowerInvariant == "photo")
                {
                    if (num == 1)
                        return CommonResources.Conversations_OnePhoto;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourPhotosFrm, num);
                    return string.Format(CommonResources.Conversations_FiveOrMorePhotosFrm, num);
                }
                else if (lowerInvariant == "wall_reply")
                    return CommonResources.Comment;
                else if (lowerInvariant == "sticker")
                    return CommonResources.Conversation_Sticker;
                else if (lowerInvariant == "market")
                    return CommonResources.Product;
                else if (lowerInvariant == "doc")
                {
                    Doc doc1 = firstAttachment.doc;
                    if ((doc1 != null ? (doc1.IsGraffiti ? 1 : 0) : 0) != 0)
                        return CommonResources.Graffiti;
                    Doc doc2 = firstAttachment.doc;
                    if ((doc2 != null ? (doc2.IsVoiceMessage ? 1 : 0) : 0) != 0)
                        return CommonResources.VoiceMessage;
                    if (num == 1)
                        return CommonResources.Conversations_OneDocument;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourDocumentsFrm, num);
                    return string.Format(CommonResources.Conversations_FiveMoreDocumentsFrm, num);
                }

                else if (lowerInvariant == "audio")
                {
                    if (num == 1)
                        return CommonResources.Conversations_OneAudio;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourAudioFrm, num);
                    return string.Format(CommonResources.Conversations_FiveOrMoreAudioFrm, num);
                }

                else if (lowerInvariant == "video")
                {
                    if (num == 1)
                        return CommonResources.Conversations_OneVideo;
                    if (num < 5)
                        return string.Format(CommonResources.Conversations_TwoFourVideosFrm, num);
                    return string.Format(CommonResources.Conversations_FiveOrMoreVideosFrm, num);
                }
            }


            if (message.geo != null)
                return CommonResources.Conversations_Location;
            if (message.fwd_messages == null || message.fwd_messages.Count <= 0)
                return string.Empty;
            int count = message.fwd_messages.Count;
            if (count == 1)
                return CommonResources.Conversations_OneForwardedMessage;
            if (count < 5)
                return string.Format(CommonResources.Conversations_TwoFourForwardedMessagesFrm, count);
            return string.Format(CommonResources.Conversations_FiveMoreForwardedMessagesFrm, count);
        }

        private string GetHeaderBody()
        {
            return ConversationHeader.GetMessageHeaderText(this._message, this.User, this.User2);
        }

        protected string FormatUIDate(int unixDateTime)
        {
            return UIStringFormatterHelper.FormatDateForUIShort(Extensions.UnixTimeStampToDateTime((double)unixDateTime, true));
        }

        protected virtual string FormatTitleForUser(User user)
        {
            string str = string.Format("{0} {1}", user.first_name, user.last_name);
            this.IsOnline = (uint)user.online > 0U;
            this.IsOnlineMobile = (uint)user.online_mobile > 0U;
            return str;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write<Message>(this._message, false);
            writer.WriteList<User>((IList<User>)this._associatedUsers, 10000);
            writer.Write(this._unread);
        }

        public void Read(BinaryReader reader)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            reader.ReadInt32();
            this._message = reader.ReadGeneric<Message>();
            long elapsedMilliseconds1 = stopwatch.ElapsedMilliseconds;
            this._associatedUsers = reader.ReadList<User>();
            this._unread = reader.ReadInt32();
            long elapsedMilliseconds2 = stopwatch.ElapsedMilliseconds;
            this.RefreshUIProperties(false);
            long elapsedMilliseconds3 = stopwatch.ElapsedMilliseconds;
        }

        internal bool Matches(List<string> query)
        {
            bool flag = false;
            foreach (string str in query)
            {
                string searchTerm = str;
                flag = ((IEnumerable<string>)this.UITitle.ToLowerInvariant().Split(' ')).Any<string>((Func<string, bool>)(s => s.StartsWith(searchTerm.ToLowerInvariant())));
                if (!flag)
                {
                    flag = ((IEnumerable<string>)this.UIBody.ToLowerInvariant().Split(' ')).Any<string>((Func<string, bool>)(s => s.StartsWith(searchTerm.ToLowerInvariant())));
                    if (flag)
                        break;
                }
                else
                    break;
            }
            return flag;
        }

        public void DisableEnableNotifications(Action<bool> resultCallback)
        {
            if (this._changingNotifications)
                return;
            this._changingNotifications = true;
            string notificationsUri = AppGlobalStateManager.Current.GlobalState.NotificationsUri;
            if (!string.IsNullOrEmpty(notificationsUri))
            {
                AccountService.Instance.SetSilenceMode(notificationsUri, this.AreNotificationsDisabled ? 0 : -1, (Action<BackendResult<object, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    this._changingNotifications = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                    resultCallback(res.ResultCode == ResultCode.Succeeded);
                }))), this.IsChat ? this.UserOrChatId : 0, !this.IsChat ? this.UserOrChatId : 0L);
            }
            else
            {
                this._changingNotifications = false;
                this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                resultCallback(true);
            }
        }

        internal void AllowDenyMessagesFromGroup(Action<bool> callback)
        {
            if (this.UserOrChatId > 0L || this.UserOrChatId < -2000000000L || this._isMessageFromGroupBusy)
                return;
            this._isMessageFromGroupBusy = true;
            long groupId = Math.Abs(this.UserOrChatId);
            this.SetInProgress(true, "");
            MessagesService.Instance.AllowDenyMessagesFromGroup(groupId, this.IsMessagesFromGroupDenied, (Action<BackendResult<int, ResultCode>>)(result =>
            {
                this._isMessageFromGroupBusy = false;
                this.SetInProgress(false, "");
                bool flag = result.ResultCode == ResultCode.Succeeded;
                if (flag)
                    this.IsMessagesFromGroupDenied = !this.IsMessagesFromGroupDenied;
                Action<bool> action = callback;
                if (action == null)
                    return;
                int num = flag ? 1 : 0;
                action(num != 0);
            }));
        }

        public void SetUserIsTypingWithDelayedReset(long userId)
        {
            if (this.UsersTypingHelper == null)
                this.UsersTypingHelper = new UsersTypingHelper(this);
            this.UsersTypingHelper.SetUserIsTypingWithDelayedReset(userId);
        }

        internal void RespondToSettingsChange()
        {
            this.NotifyPropertyChanged<ObservableCollection<MenuItemData>>((Expression<Func<ObservableCollection<MenuItemData>>>)(() => this.MenuItems));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.NotificationsDisabledVisibility));
            this.NotifyPropertyChanged<SolidColorBrush>((Expression<Func<SolidColorBrush>>)(() => this.HaveUnreadMessagesBackground));
            this.NotifyPropertyChanged<Thickness>((Expression<Func<Thickness>>)(() => this.IsOnlineOrOnlineMobileMargin));
            this.NotifyPropertyChanged<Thickness>((Expression<Func<Thickness>>)(() => this.TitleMargin));
            EventAggregator current = EventAggregator.Current;
            NotificationSettingsChangedEvent settingsChangedEvent = new NotificationSettingsChangedEvent();
            int num = this.AreNotificationsDisabled ? 1 : 0;
            settingsChangedEvent.AreNotificationsDisabled = num != 0;
            long userOrChatId = this.UserOrChatId;
            settingsChangedEvent.ChatId = userOrChatId;
            current.Publish(settingsChangedEvent);
        }

        public void Handle(NotificationSettingsChangedEvent message)
        {
            if (message.ChatId != this.UserOrChatId || !this.IsChat)
                return;
            this.AreNotificationsDisabled = message.AreNotificationsDisabled;
        }

        public void Handle(MessagesFromGroupAllowedDeniedEvent message)
        {
            if (message.UserOrGroupId != this.UserOrChatId || this.IsChat)
                return;
            this.IsMessagesFromGroupDenied = !message.IsAllowed;
        }
    }
}
