using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Backend;
using VKMessenger.Utils;
using VKMessenger.Views;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKMessenger.Library
{
    public class ConversationViewModel : ViewModelBase, IBinarySerializableWithTrimSupport, IBinarySerializable, IHandle<NotificationSettingsChangedEvent>, IHandle, IHandle<MessagesFromGroupAllowedDeniedEvent>
    {
        public static Func<MessageViewModel, MessageViewModel, int> _comparisonFunc = (Func<MessageViewModel, MessageViewModel, int>)((m1, m2) =>
        {
            if (m2.Message.mid > 0 && m1.Message.mid > 0)
                return m2.Message.mid - m1.Message.mid;
            return m2.Message.date - m1.Message.date;
        });
        public static readonly string UNREAD_ITEM_ACTION = "UNREAD_MESSAGES";
        private readonly int _numberOfMessagesToStore = 10;
        private string _uiTitle = string.Empty;
        private string _uiSubtitle = string.Empty;
        private Visibility _notificationsDisabledVisibility = Visibility.Collapsed;
        private OutboundMessageViewModel _outboundMessage = new OutboundMessageViewModel();
        private ObservableCollection<MessageViewModel> _messages = new ObservableCollection<MessageViewModel>();
        private ConversationAvatarViewModel _conversationAvatarVM = new ConversationAvatarViewModel();
        private DelayedExecutor _delayedExecutorResetTypingStatus = new DelayedExecutor(10000);
        private string _savedSubtitle = string.Empty;
        private DateTime _lastTimeUserIsTypingWasCalled = DateTime.MinValue;
        private long _userOrChatId;
        private bool _isChat;
        //private bool _isInitialized;
        private IScroll _scroll;
        private UserStatus _userStatus;
        private User _user;
        private ChatExtended _chat;
        private int _skipped;
        private bool _isMessagesFromGroupDenied;
        private string _chatTypeStatusText;
        private bool _isInSelectionMode;
        private bool _isBusyLoadingMessages;
        private bool _isBusyLoadingHeaderInfo;
        private UsersTypingHelper _usersTypingHelper;
        private bool _changingNotifications;
        private bool _isMessageFromGroupBusy;
        private bool _isInProgressPinToStart;

        private int Skipped
        {
            get
            {
                return this._skipped;
            }
            set
            {
                if (this._skipped == value)
                    return;
                this._skipped = value;
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.ArrowDownDarkVisibility));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.ArrowDownLightVisibility));
            }
        }

        public ConversationAvatarViewModel ConversationAvatarVM
        {
            get
            {
                return this._conversationAvatarVM;
            }
        }

        private bool IsDarkTheme
        {
            get
            {
                return (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == 0;
            }
        }

        private bool ShowArrowDown
        {
            get
            {
                return this._skipped > 0;
            }
        }

        public bool AreNotificationsDisabled
        {
            get
            {
                if (this._chat == null)
                    return false;
                return this._chat.push_settings.AreDisabledNow;
            }
            set
            {
                int num1 = value ? -1 : 0;
                if (this.AreNotificationsDisabled == value)
                    return;
                this._chat.push_settings.disabled_until = num1;
                EventAggregator current = EventAggregator.Current;
                NotificationSettingsChangedEvent settingsChangedEvent = new NotificationSettingsChangedEvent();
                int num2 = this.AreNotificationsDisabled ? 1 : 0;
                settingsChangedEvent.AreNotificationsDisabled = num2 != 0;
                long userOrCharId = this.UserOrCharId;
                settingsChangedEvent.ChatId = userOrCharId;
                current.Publish((object)settingsChangedEvent);
            }
        }

        public bool IsMessagesFromGroupDenied
        {
            get
            {
                if (this._user == null)
                    return false;
                long id = this._user.id;
                if (id > 0L || id < -2000000000L)
                    return false;
                return this._user.is_messages_blocked == 1;
            }
            private set
            {
                if (this._user == null || this._isMessagesFromGroupDenied == value)
                    return;
                this._isMessagesFromGroupDenied = value;
                this._user.is_messages_blocked = value ? 1 : 0;
                UsersService.Instance.SetCachedUser(this._user);
                EventAggregator current = EventAggregator.Current;
                MessagesFromGroupAllowedDeniedEvent allowedDeniedEvent = new MessagesFromGroupAllowedDeniedEvent();
                int num = !value ? 1 : 0;
                allowedDeniedEvent.IsAllowed = num != 0;
                long id = this._user.id;
                allowedDeniedEvent.UserOrGroupId = id;
                current.Publish((object)allowedDeniedEvent);
            }
        }

        public Visibility ArrowDownDarkVisibility
        {
            get
            {
                if (!this.IsDarkTheme || !this.ShowArrowDown)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility ArrowDownLightVisibility
        {
            get
            {
                if (this.IsDarkTheme || !this.ShowArrowDown)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool IsOnScreen { get; set; }

        public bool NoMessages
        {
            get
            {
                if (!this._isBusyLoadingMessages)
                    return this.Messages.Count == 0;
                return false;
            }
        }

        public string UITitle
        {
            get
            {
                return this._uiTitle.ToUpperInvariant();
            }
            private set
            {
                if (!(this._uiTitle != value))
                    return;
                this._uiTitle = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UITitle));
            }
        }

        public string Title
        {
            get
            {
                return this._uiTitle;
            }
        }

        public Visibility NotificationsDisabledVisibility
        {
            get
            {
                return this._notificationsDisabledVisibility;
            }
            set
            {
                if (this._notificationsDisabledVisibility == value)
                    return;
                this._notificationsDisabledVisibility = value;
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.NotificationsDisabledVisibility));
            }
        }

        public string UISubtitle
        {
            get
            {
                return this._uiSubtitle;
            }
            set
            {
                if (!(this._uiSubtitle != value))
                    return;
                this._uiSubtitle = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.UISubtitle));
            }
        }

        public string ChatTypeStatusText
        {
            get
            {
                return this._chatTypeStatusText;
            }
            set
            {
                if (!(this._chatTypeStatusText != value))
                    return;
                this._chatTypeStatusText = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.ChatTypeStatusText));
            }
        }

        public bool IsChat
        {
            get
            {
                return this._isChat;
            }
        }

        public bool IsDialogWithOtherUser
        {
            get
            {
                if (this.IsDialogWithUserOrChat)
                    return !this.IsChat;
                return false;
            }
        }

        public bool IsDialogWithUserOrChat
        {
            get
            {
                if (this.IsChat)
                    return true;
                if (this.User != null && this.User.id > 0L)
                    return this.User.id != AppGlobalStateManager.Current.LoggedInUserId;
                return false;
            }
        }

        public bool IsDialogWithCommunity
        {
            get
            {
                if (!this.IsChat)
                    return this.UserOrCharId < 0L;
                return false;
            }
        }

        public long UserOrCharId
        {
            get
            {
                return this._userOrChatId;
            }
        }

        public ChatExtended Chat
        {
            get
            {
                return this._chat;
            }
        }

        public User User
        {
            get
            {
                return this._user;
            }
        }

        public bool CanAddMembers { get; private set; }

        public bool IsKickedFromChat
        {
            get
            {
                ChatExtended chat = this._chat;
                if ((chat != null ? chat.users : (List<User>)null) != null)
                    return !this._chat.users.Any<User>();
                return true;
            }
        }

        public ObservableCollection<MessageViewModel> Messages
        {
            get
            {
                return this._messages;
            }
            private set
            {
                this._messages = value;
                this.NotifyPropertyChanged<ObservableCollection<MessageViewModel>>((Expression<Func<ObservableCollection<MessageViewModel>>>)(() => this.Messages));
            }
        }

        public OutboundMessageViewModel OutboundMessageVM
        {
            get
            {
                return this._outboundMessage;
            }
            private set
            {
                this._outboundMessage = value;
                this.NotifyPropertyChanged<OutboundMessageViewModel>((Expression<Func<OutboundMessageViewModel>>)(() => this.OutboundMessageVM));
            }
        }

        public IScroll Scroll
        {
            get
            {
                return this._scroll;
            }
            set
            {
                this._scroll = value;
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
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    foreach (MessageViewModel message in (Collection<MessageViewModel>)this.Messages)
                    {
                        if (!this._isInSelectionMode)
                            message.IsSelected = false;
                        message.IsInSelectionMode = this._isInSelectionMode;
                    }
                }));
            }
        }

        public List<Photo> AttachedPhotos
        {
            get
            {
                List<Photo> photoList = new List<Photo>();
                foreach (MessageViewModel message in (Collection<MessageViewModel>)this.Messages)
                {
                    photoList.AddRange(message.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Photo)).Select<AttachmentViewModel, Photo>((Func<AttachmentViewModel, Photo>)(p => p.Photo ?? new Photo())));
                    foreach (MessageViewModel forwardedMessage in (Collection<MessageViewModel>)message.ForwardedMessages)
                        photoList.AddRange(forwardedMessage.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Photo)).Select<AttachmentViewModel, Photo>((Func<AttachmentViewModel, Photo>)(p => p.Photo ?? new Photo())));
                }
                return photoList;
            }
        }

        public bool CanDettachProductAttachment { get; set; }

        public List<User> ChatMembers
        {
            get
            {
                ChatExtended chat = this._chat;
                if (chat == null)
                    return (List<User>)null;
                return chat.users;
            }
        }

        public ConversationViewModel()
        {
            EventAggregator.Current.Subscribe((object)this);
        }

        public void InitializeWith(long userOrChatId, bool isChat)
        {
            //this._isInitialized = true;
            this._isChat = isChat;
            this._userOrChatId = userOrChatId;
            this.OutboundMessageVM = new OutboundMessageViewModel(isChat, userOrChatId);
        }

        public void LoadFromLastUnread(Action<bool> callback = null)
        {
            this.LoadMessagesAsyncWithParams(-13, 21, false, false, callback, new int?(-1), false);
        }

        public void LoadFromMessageId(int messageId, Action<bool> callback = null, bool scrollToMessageId = false)
        {
            this.LoadMessagesAsyncWithParams(-13, 21, false, true, callback, new int?(messageId), scrollToMessageId);
        }

        public void LoadMoreConversations(Action<bool> callback = null)
        {
            this.LoadMessagesAsyncWithParams(this._messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(mvm => mvm.Message.mid > 0)).Count<MessageViewModel>() + this._skipped, 16, false, false, callback, new int?(), false);
        }

        public void LoadNewerConversations(Action<bool> callback = null)
        {
            if (this._skipped <= 0 || this._messages.Count <= 0)
                return;
            this.LoadMessagesAsyncWithParams(-15, 15, false, true, callback, new int?(this._messages.Last<MessageViewModel>().Message.id), false);
        }

        public void RefreshConversations()
        {
            this.LoadMessagesAsyncWithParams(0, 8, true, true, (Action<bool>)null, new int?(), false);
        }

        public void DeleteMessages(List<MessageViewModel> messageViewModels, Action callback = null)
        {
            this.RemoveUnreadMessagesItem();
            BackendServices.MessagesService.DeleteMessages(messageViewModels.Select<MessageViewModel, int>((Func<MessageViewModel, int>)(vm => vm.Message.id)).Where<int>((Func<int, bool>)(id => (uint)id > 0U)).ToList<int>(), (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    foreach (MessageViewModel messageViewModel in messageViewModels)
                    {
                        this.Messages.Remove(messageViewModel);
                        messageViewModel.CancelUpload();
                    }
                    if (callback == null)
                        return;
                    callback();
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }))));
        }

        public void SendMessage(string messageText = "")
        {
            this.SendMessage(messageText, (StickerItemData)null, "", (GraffitiAttachmentItem)null);
        }

        internal void SendSticker(StickerItemData stickerItemData, string stickerReferrer)
        {
            this.SendMessage("", stickerItemData, stickerReferrer, (GraffitiAttachmentItem)null);
        }

        public void SendGraffiti(GraffitiAttachmentItem graffitiAttachmentItem)
        {
            this.SendMessage("", (StickerItemData)null, "", graffitiAttachmentItem);
        }

        private void SendMessage(string messageText, StickerItemData stickerData, string stickerReferrer, GraffitiAttachmentItem graffitiAttachmentItem)
        {
            string str = messageText.Replace("\r\n", "\r").Replace("\r", "\r\n").Trim();
            this.RemoveUnreadMessagesItem();
            OutboundMessageViewModel outboundMessage = new OutboundMessageViewModel(this._isChat, this._userOrChatId);
            if (stickerData != null)
            {
                outboundMessage.StickerItem = stickerData;
                outboundMessage.StickerReferrer = stickerReferrer;
            }
            else if (graffitiAttachmentItem != null)
            {
                outboundMessage.GraffitiAttachmentItem = graffitiAttachmentItem;
            }
            else
            {
                outboundMessage.MessageText = str;
                outboundMessage.Attachments = this._outboundMessage.Attachments;
                this._outboundMessage.Attachments = new ObservableCollection<IOutboundAttachment>();
            }
            MessageViewModel messageViewModel = new MessageViewModel(outboundMessage);
            this._messages.AddOrdered<MessageViewModel>(messageViewModel, ConversationViewModel._comparisonFunc, true);
            if (!this.CanDettachProductAttachment && stickerData == null)
            {
                OutboundProductAttachment productAttachment = outboundMessage.Attachments.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a is OutboundProductAttachment)) as OutboundProductAttachment;
                Product product = productAttachment != null ? productAttachment.Product : (Product)null;
                if (product != null)
                    EventAggregator.Current.Publish((object)new MarketContactEvent(string.Format("{0}_{1}", (object)product.owner_id, (object)product.id), MarketContactAction.write));
            }
            messageViewModel.OutboundMessageVM.MessageSent += (EventHandler)((o, e) =>
            {
                if (messageViewModel.OutboundMessageVM.OutboundMessageStatus != OutboundMessageStatus.Delivered)
                    return;
                this.IsMessagesFromGroupDenied = false;
            });
            messageViewModel.Send();
            if (this._skipped > 0)
                this.RefreshConversations();
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.NoMessages));
            MessengerStateManagerInstance.Current.EnsureOnlineStatusIsSet(false);
        }

        public void AddForwardedMessagesToOutboundMessage(IList<Message> forwardedMessages)
        {
            this.OutboundMessageVM.RemoveForwardedMessages();
            this.OutboundMessageVM.Attachments.Add((IOutboundAttachment)new OutboundForwardedMessages(forwardedMessages.ToList<Message>()));
        }

        private void EnsureUserAndChatIdSet(Message message)
        {
            if (this._isChat)
                message.chat_id = (int)this._userOrChatId;
            else
                message.uid = (int)this._userOrChatId;
        }

        public void UserIsTyping()
        {
            if ((DateTime.Now - this._lastTimeUserIsTypingWasCalled).TotalSeconds <= 4.0)
                return;
            BackendServices.MessagesService.SetUserIsTyping(this._userOrChatId, this._isChat, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(r => { }));
            this._lastTimeUserIsTypingWasCalled = DateTime.Now;
        }

        public void LoadHeaderInfoAsync()
        {
            if (this._isBusyLoadingHeaderInfo)
                return;
            this._isBusyLoadingHeaderInfo = true;
            if (this._isChat)
            {
                BackendServices.MessagesService.GetChat(this._userOrChatId, (Action<BackendResult<ChatExtended, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                        this._chat = res.ResultData;
                    this.RefreshUIPropertiesSafe();
                    this._isBusyLoadingHeaderInfo = false;
                }));
            }
            else
            {
                this.RefreshUIPropertiesSafe();
                UsersService instance = UsersService.Instance;
                List<long> userIds = new List<long>();
                userIds.Add(this._userOrChatId);
                Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(r =>
                {
                    if (r.ResultCode == ResultCode.Succeeded)
                    {
                        this._user = r.ResultData.First<User>();
                        this.CanAddMembers = this._user.friend_status == 3;
                        this.IsMessagesFromGroupDenied = this._user.is_messages_blocked == 1;
                        this.RefreshUIPropertiesSafe();
                        UsersService.Instance.GetStatus(this._userOrChatId, (Action<BackendResult<UserStatus, ResultCode>>)(res =>
                        {
                            if (res.ResultCode == ResultCode.Succeeded)
                            {
                                this._userStatus = res.ResultData;
                                this.RefreshUIPropertiesSafe();
                            }
                            this._isBusyLoadingHeaderInfo = false;
                        }));
                    }
                    else
                        this._isBusyLoadingHeaderInfo = false;
                });
                instance.GetUsers(userIds, callback);
            }
        }

        private void RefreshUIPropertiesSafe()
        {
            if (this._isChat)
                ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this._chat != null)
                    {
                        this.UITitle = this._chat.title;
                        this.NotificationsDisabledVisibility = this._chat.push_settings.AreDisabledNow ? Visibility.Visible : Visibility.Collapsed;
                        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.AreNotificationsDisabled));
                        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsKickedFromChat));
                        if (this._chat.users != null)
                        {
                            this._conversationAvatarVM.Initialize(this._chat.photo_200, true, this._chat.users.Select<User, long>((Func<User, long>)(u => u.uid)).ToList<long>(), this._chat.users);
                            this.UISubtitle = UIStringFormatterHelper.FormatNumberOfSomething(this._chat.users.Count, CommonResources.Conversation_OnePerson, CommonResources.Conversation_TwoToFourPersonsFrm, CommonResources.Conversation_FiveOrMorePersionsFrm, true, (string)null, false);
                        }
                    }
                    this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsDialogWithOtherUser));
                }));
            else
                ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this._user != null)
                    {
                        this._conversationAvatarVM.Initialize(this._user.photo_max, false, new List<long>(), new List<User>());
                        this.UITitle = this._user.first_name + " " + this._user.last_name;
                        this.NotificationsDisabledVisibility = Visibility.Collapsed;
                        this.CanAddMembers = this._user.friend_status == 3;
                        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsMessagesFromGroupDenied));
                        if (this._userStatus != null)
                            this.UISubtitle = this._userOrChatId >= 0L || this._userOrChatId <= -2000000000L ? this._userStatus.GetUserStatusString(this._user.sex % 2 == 0) : CommonResources.Community;
                    }
                    this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsDialogWithOtherUser));
                }));
        }

        private void LoadMessagesAsyncWithParams(int offset, int count, bool resetCollection, bool showProgress = true, Action<bool> callback = null, int? startMessageId = null, bool scrollToMessageId = false)
        {
            if (this._isBusyLoadingMessages)
                return;
            this._isBusyLoadingMessages = true;
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.NoMessages));
            if (showProgress || this.Messages.Count == 0)
                this.SetInProgress(true, CommonResources.Conversation_LoadingMessages);
            bool scrolledToUnreadItem = false;
            ObservableCollection<MessageViewModel> messages1 = this._messages;
            Func<MessageViewModel, bool> func = (Func<MessageViewModel, bool>)(m => m.Message.action == ConversationViewModel.UNREAD_ITEM_ACTION);
            if (messages1.Any<MessageViewModel>(func) && this._scroll != null)
            {
                int? nullable = startMessageId;
                int num = -1;
                if ((nullable.GetValueOrDefault() == num ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                {
                    this._scroll.ScrollToUnreadItem();
                    scrolledToUnreadItem = true;
                }
            }
            BackendServices.MessagesService.GetHistory(this._userOrChatId, this._isChat, offset, count, startMessageId, (Action<BackendResult<MessageListResponse, ResultCode>>)(result =>
            {
                this.SetInProgress(false, "");
                if (callback != null)
                    callback(result.ResultCode == ResultCode.Succeeded);
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    List<Message> messages = result.ResultData.Messages;
                    UsersService.Instance.GetUsers(Message.GetAssociatedUserIds(messages, true).Select<long, long>((Func<long, long>)(uid => uid)).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>)(r =>
                    {
                        if (r.ResultCode == ResultCode.Succeeded)
                        {
                            if (startMessageId.HasValue)
                                this.Skipped = result.ResultData.Skipped;
                            else if (offset == 0)
                                this.Skipped = 0;
                            List<User> resultData = r.ResultData;
                            List<MessageViewModel> messagesViewModels = new List<MessageViewModel>(messages.Count);
                            messages.ForEach((Action<Message>)(m =>
                            {
                                this.EnsureUserAndChatIdSet(m);
                                messagesViewModels.Add(new MessageViewModel(m));
                            }));
                            ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() =>
                            {
                                int? nullable;
                                if (offset != 0)
                                {
                                    nullable = startMessageId;
                                    int num1 = -1;
                                    if ((nullable.GetValueOrDefault() == num1 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                                    {
                                        nullable = startMessageId;
                                        int num2 = 0;
                                        if ((nullable.GetValueOrDefault() > num2 ? (nullable.HasValue ? 1 : 0) : 0) != 0 && this._messages.Any<MessageViewModel>())
                                        {
                                            nullable = startMessageId;
                                            int id = this._messages.First<MessageViewModel>().Message.id;
                                            if ((nullable.GetValueOrDefault() < id ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                                                goto label_6;
                                        }
                                        else
                                            goto label_6;
                                    }
                                }
                                MessageViewModel oldest = messagesViewModels.FirstOrDefault<MessageViewModel>();
                                if (oldest != null && !this._messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.mid == oldest.Message.mid)))
                                    resetCollection = true;
                            label_6:
                                if (resetCollection)
                                {
                                    List<MessageViewModel> list = this._messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.id == 0)).ToList<MessageViewModel>();
                                    this._messages.Clear();
                                    foreach (MessageViewModel messageViewModel in list)
                                        this._messages.Add(messageViewModel);
                                }
                                if (!resetCollection)
                                {
                                    foreach (MessageViewModel message in (Collection<MessageViewModel>)this._messages)
                                    {
                                        MessageViewModel mvm = message;
                                        if (mvm.Message != null)
                                        {
                                            MessageViewModel messageViewModel = messagesViewModels.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m =>
                                            {
                                                if (m.Message != null)
                                                    return m.Message.mid == mvm.Message.mid;
                                                return false;
                                            }));
                                            if (messageViewModel != null && messageViewModel.Message.read_state != mvm.Message.read_state)
                                            {
                                                mvm.Message.read_state = messageViewModel.Message.read_state;
                                                mvm.RefreshUIProperties();
                                            }
                                        }
                                    }
                                }
                                messagesViewModels.ForEach((Action<MessageViewModel>)(messageVM =>
                                {
                                    messageVM.IsInSelectionMode = this.IsInSelectionMode;
                                    if (this._messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.mid == messageVM.Message.mid)))
                                        return;
                                    this._messages.AddOrdered<MessageViewModel>(messageVM, ConversationViewModel._comparisonFunc, false);
                                }));
                                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.NoMessages));
                                nullable = startMessageId;
                                int num3 = -1;
                                if ((nullable.GetValueOrDefault() == num3 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                                    this.EnsureUnreadItem();
                                if (this._scroll != null)
                                {
                                    nullable = startMessageId;
                                    int num1 = -1;
                                    if ((nullable.GetValueOrDefault() == num1 ? (nullable.HasValue ? 1 : 0) : 0) != 0 && !scrolledToUnreadItem)
                                    {
                                        this._scroll.ScrollToUnreadItem();
                                        goto label_34;
                                    }
                                }
                                if (((this._scroll == null ? 0 : (messagesViewModels.Count > 0 ? 1 : 0)) & (resetCollection ? 1 : 0)) != 0)
                                    this._scroll.ScrollToBottom(false, false);
                                else if (((this._scroll == null ? 0 : (offset == 0 ? 1 : 0)) & (showProgress ? 1 : 0)) != 0)
                                    this._scroll.ScrollToBottom(true, false);
                                else if (this._scroll != null & scrollToMessageId)
                                {
                                    nullable = startMessageId;
                                    int num1 = 0;
                                    if ((nullable.GetValueOrDefault() > num1 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                                        this._scroll.ScrollToMessageId((long)startMessageId.Value);
                                }
                            label_34:
                                this.SetReadStatusIfNeeded();
                                this._isBusyLoadingMessages = false;
                            }));
                        }
                        else
                            this._isBusyLoadingMessages = false;
                    }));
                }
                else
                    this._isBusyLoadingMessages = false;
            }));
        }

        private void EnsureUnreadItem()
        {
            this.RemoveUnreadMessagesItem();
            int num = 0;
            for (int index = this._messages.Count - 1; index >= 0 && (this._messages[index].Message.@out == 0 && this._messages[index].Message.read_state == 0); --index)
                ++num;
            if (num <= 0)
                return;
            MessageViewModel messageViewModel = new MessageViewModel(new Message()
            {
                action = ConversationViewModel.UNREAD_ITEM_ACTION
            });
            if (this._messages.Count < num)
                return;
            this._messages.Insert(this._messages.Count - num, messageViewModel);
        }

        public void RemoveUnreadMessagesItem()
        {
            MessageViewModel messageViewModel = this._messages.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.Message.action == ConversationViewModel.UNREAD_ITEM_ACTION));
            if (messageViewModel == null)
                return;
            this._messages.Remove(messageViewModel);
        }

        private void SetReadStatusIfNeeded()
        {
            List<Message> listToBeMarkedAsRead = new List<Message>();
            foreach (MessageViewModel message1 in (Collection<MessageViewModel>)this.Messages)
            {
                Message message2 = message1.Message;
                if (message2 != null && message2.read_state == 0 && (message2.@out == 0 || (long)message2.user_id == AppGlobalStateManager.Current.LoggedInUserId))
                    listToBeMarkedAsRead.Add(message2);
            }
            if (listToBeMarkedAsRead.Count <= 0)
                return;
            BackendServices.MessagesService.MarkAsRead(listToBeMarkedAsRead.Select<Message, long>((Func<Message, long>)(m => (long)m.mid)).ToList<long>(), this.IsChat ? this.UserOrCharId + 2000000000L : this.UserOrCharId, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => listToBeMarkedAsRead.ForEach((Action<Message>)(m => m.read_state = 1))));
            }));
        }

        public void Write(BinaryWriter writer)
        {
            this.DoWrite(writer, false);
        }

        public void Read(BinaryReader reader)
        {
            try
            {
                int num1 = reader.ReadInt32();
                this._userOrChatId = reader.ReadInt64();
                this._isChat = reader.ReadBoolean();
                this._userStatus = reader.ReadGeneric<UserStatus>();
                this._user = reader.ReadGeneric<User>();
                
                if (num1 == 1)
                    reader.ReadGeneric<VKClient.Audio.Base.DataObjects.Chat>();
                this._messages = new ObservableCollection<MessageViewModel>(reader.ReadList<MessageViewModel>());
                this._outboundMessage = reader.ReadGeneric<OutboundMessageViewModel>();
                this._isInSelectionMode = reader.ReadBoolean();
                
                if (num1 >= 2)
                    this._chat = reader.ReadGeneric<ChatExtended>();
                //this._isInitialized = true;
                this.RefreshUIPropertiesSafe();
            }
            catch (Exception ex)
            {
                //ServiceLocator.Resolve<IAppStateInfo>().ReportException(ex);
                throw ex;
            }
        }

        public void WriteTrimmed(BinaryWriter writer)
        {
            this.DoWrite(writer, true);
        }

        private void DoWrite(BinaryWriter writer, bool trim)
        {
            try
            {
                writer.Write(2);
                writer.Write(this._userOrChatId);
                writer.Write(this._isChat);
                writer.Write<UserStatus>(this._userStatus, false);
                writer.Write<User>(this._user, false);
                if (trim)
                    writer.WriteList<MessageViewModel>((IList<MessageViewModel>)this.TrimMessageViewModels(), 10000);
                else
                    writer.WriteList<MessageViewModel>((IList<MessageViewModel>)this._messages.ToList<MessageViewModel>(), 10000);
                writer.Write<OutboundMessageViewModel>(this._outboundMessage, false);
                writer.Write(this._isInSelectionMode);
                writer.Write<ChatExtended>(this._chat, false);
            }
            catch (Exception ex)
            {
                //ServiceLocator.Resolve<IAppStateInfo>().ReportException(ex);
                throw ex;
            }
        }

        public void TrimMessages()
        {
            this.Messages = new ObservableCollection<MessageViewModel>(this.TrimMessageViewModels());
        }

        private List<MessageViewModel> TrimMessageViewModels()
        {
            return this._messages.Skip<MessageViewModel>(Math.Max(0, this._messages.Count - this._numberOfMessagesToStore)).Take<MessageViewModel>(this._numberOfMessagesToStore).ToList<MessageViewModel>();
        }

        internal void ProcessInstantUpdates(List<LongPollServerUpdateData> updates)
        {
            List<MessageViewModel> newOrRestoredMessages = new List<MessageViewModel>();
            List<long> readMessages = new List<long>();
            List<long> deletedMessagesIds = new List<long>();
            bool atLeastOneNewMessage = false;
            bool flag = false;
            foreach (LongPollServerUpdateData update1 in updates)
            {
                LongPollServerUpdateData update = update1;
                if (this._userOrChatId == (update.isChat ? update.chat_id : update.user_id) && update.isChat == this.IsChat)
                {
                    if ((update.UpdateType == LongPollServerUpdateType.MessageHasBeenAdded || update.UpdateType == LongPollServerUpdateType.MessageHasBeenRestored) && (update.user != null && update.message != null))
                    {
                        this.EnsureUserAndChatIdSet(update.message);
                        newOrRestoredMessages.Add(new MessageViewModel(update.message));
                        if (update.UpdateType == LongPollServerUpdateType.MessageHasBeenAdded)
                        {
                            atLeastOneNewMessage = true;
                            flag = this.IsChat && (this.IsKickedFromChat || update.message.action == "chat_photo_update" || update.message.action == "chat_photo_remove");
                        }
                    }
                    if (update.UpdateType == LongPollServerUpdateType.MessageHasBeenRead)
                        readMessages.Add(update.message_id);
                    if (update.UpdateType == LongPollServerUpdateType.MessageHasBeenDeleted)
                        deletedMessagesIds.Add(update.message_id);
                    if (update.UpdateType == LongPollServerUpdateType.UserIsTyping || update.UpdateType == LongPollServerUpdateType.UserIsTypingInChat)
                        Execute.ExecuteOnUIThread((Action)(() =>
                        {
                            if (this._usersTypingHelper == null)
                                this._usersTypingHelper = new UsersTypingHelper(this);
                            this._usersTypingHelper.SetUserIsTypingWithDelayedReset(update.user_id);
                        }));
                    if (update.UpdateType == LongPollServerUpdateType.UserBecameOnline)
                    {
                        if (this._userStatus == null)
                            this._userStatus = new UserStatus();
                        this._userStatus.online = 1L;
                        this.RefreshUIPropertiesSafe();
                    }
                    if (update.UpdateType == LongPollServerUpdateType.UserBecameOffline)
                    {
                        if (this._userStatus == null)
                            this._userStatus = new UserStatus();
                        this._userStatus.online = 0L;
                        this._userStatus.time = (long)Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
                        this.RefreshUIPropertiesSafe();
                    }
                }
            }
            if (newOrRestoredMessages.Count <= 0 && readMessages.Count <= 0 && deletedMessagesIds.Count <= 0)
                return;
            int delayInMilliseconds = 0;
            List<MessageViewModel> source = newOrRestoredMessages;
            Func<MessageViewModel, bool> func = (Func<MessageViewModel, bool>)(m => m.Message.@out == 1);
            if (source.Any<MessageViewModel>(func))
                delayInMilliseconds = 1500;
            DelayedExecutorUtil.Execute((Action)(() => ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() =>
            {
                double num1 = this._scroll == null ? 0.0 : this._scroll.VerticalOffset;
                Logger.Instance.Info("Applying new messages");
                if (this._skipped == 0)
                {
                    foreach (MessageViewModel messageViewModel in newOrRestoredMessages)
                    {
                        MessageViewModel newMessage = messageViewModel;
                        if (!this._messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(mes =>
                        {
                            if (mes.Message.mid == newMessage.Message.mid || mes.OutboundMessageVM != null && mes.OutboundMessageVM.DeliveredMessageId == (long)newMessage.Message.mid)
                                return true;
                            if (mes.SendStatus == OutboundMessageStatus.SendingNow)
                                return mes.Message.body == newMessage.Message.body;
                            return false;
                        })))
                        {
                            Logger.Instance.Info("Adding instant update new message {0}, viewModelHashCode={1}", (object)newMessage.Message.mid, (object)this.GetHashCode());
                            newMessage.IsInSelectionMode = this.IsInSelectionMode;
                            this._messages.AddOrdered<MessageViewModel>(newMessage, ConversationViewModel._comparisonFunc, true);
                            UsersTypingHelper usersTypingHelper = this._usersTypingHelper;
                            if (usersTypingHelper != null)
                            {
                                long userId = (long)newMessage.Message.user_id;
                                usersTypingHelper.SetUserIsNotTyping(userId);
                            }
                            this.ChatTypeStatusText = string.Empty;
                        }
                    }
                    if (newOrRestoredMessages.Count > 0 && this.IsOnScreen)
                    {
                        this.RemoveUnreadMessagesItem();
                        this.SetReadStatusIfNeeded();
                    }
                    if (!this.IsOnScreen)
                        this.EnsureUnreadItem();
                }
                foreach (long num2 in readMessages)
                {
                    long readMessageId = num2;
                    MessageViewModel messageViewModel = this._messages.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m => (long)m.Message.mid == readMessageId));
                    if (messageViewModel != null)
                    {
                        messageViewModel.Message.read_state = 1;
                        messageViewModel.RefreshUIProperties();
                    }
                }
                foreach (long num2 in deletedMessagesIds)
                {
                    long deletedMessagesId = num2;
                    MessageViewModel messageViewModel = this._messages.FirstOrDefault<MessageViewModel>((Func<MessageViewModel, bool>)(m => (long)m.Message.mid == deletedMessagesId));
                    if (messageViewModel != null)
                        this._messages.Remove(messageViewModel);
                }
                if (((this._scroll == null ? 0 : (this._messages.Count > 0 ? 1 : 0)) & (atLeastOneNewMessage ? 1 : 0)) != 0 && num1 < 50.0)
                    this._scroll.ScrollToBottom(true, false);
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.NoMessages));
            }))), delayInMilliseconds);
            if (!flag)
                return;
            this.LoadHeaderInfoAsync();
        }

        internal void DeleteDialog()
        {
            BackendServices.MessagesService.DeleteDialog(this._userOrChatId, this._isChat, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => { }));
            this.Messages.Clear();
        }

        internal void EnsureConversationIsUpToDate(bool force, long startMessageId = 0, Action<bool> callback = null)
        {
            if (!force && this._skipped > 0)
            {
                if (callback == null)
                    return;
                callback(true);
            }
            else if (startMessageId == -1L)
                this.LoadFromLastUnread(callback);
            else if (startMessageId > 0L)
                this.LoadFromMessageId((int)startMessageId, callback, force);
            else
                this.LoadMessagesAsyncWithParams(0, 8, false, false, callback, new int?(), false);
        }

        public void DisableEnableNotifications(Action<bool> resultCallback)
        {
            if (this._changingNotifications)
                return;
            this._changingNotifications = true;
            string notificationsUri = AppGlobalStateManager.Current.GlobalState.NotificationsUri;
            if (!string.IsNullOrEmpty(notificationsUri))
            {
                this.SetInProgress(true, "");
                AccountService.Instance.SetSilenceMode(notificationsUri, this.AreNotificationsDisabled ? 0 : -1, (Action<BackendResult<object, ResultCode>>)(res =>
                {
                    this._changingNotifications = false;
                    this.SetInProgress(false, "");
                    if (res.ResultCode == ResultCode.Succeeded)
                        this.AreNotificationsDisabled = !this.AreNotificationsDisabled;
                    resultCallback(res.ResultCode == ResultCode.Succeeded);
                }), this._isChat ? this._userOrChatId : 0, !this._isChat ? this._userOrChatId : 0L);
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
            if (this._userOrChatId > 0L || this._userOrChatId < -2000000000L || this._isMessageFromGroupBusy)
                return;
            this._isMessageFromGroupBusy = true;
            long groupId = Math.Abs(this._userOrChatId);
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

        internal void PinToStart()
        {
            if (this._isInProgressPinToStart || string.IsNullOrEmpty(this.UITitle))
                return;
            this._isInProgressPinToStart = true;
            this.SetInProgressMain(true, "");
            if (!this._isChat)
            {
                string error = CommonResources.Error;
                UsersService.Instance.GetUsersForTile(this._user.uid, (Action<BackendResult<List<User>, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                    {
                        this._isInProgressPinToStart = false;
                        this.SetInProgressMain(false, "");
                        Execute.ExecuteOnUIThread((Action)(() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
                    }
                    else
                    {
                        string str = string.Format(CommonResources.DialogWithFrm, (object)res.ResultData[0].first_name);
                        List<string> imageUris = new List<string>();
                        imageUris.Add(res.ResultData[0].photo_max);
                        string title = str;
                        this.DoCreateTile(imageUris, title);
                    }
                }))));
            }
            else
                BackendServices.ChatService.GetChatInfo(this._userOrChatId, (Action<BackendResult<ChatInfo, ResultCode>>)(res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                    {
                        this._isInProgressPinToStart = false;
                        this.SetInProgressMain(false, "");
                        Execute.ExecuteOnUIThread((Action)(() => MessageBox.Show(CommonResources.Error)));
                    }
                    else
                    {
                        List<string> list = res.ResultData.chat_participants.Select<ChatUser, string>((Func<ChatUser, string>)(c => c.photo_max)).ToList<string>();
                        if (!string.IsNullOrWhiteSpace(res.ResultData.chat.photo_200))
                            list.Insert(0, res.ResultData.chat.photo_200);
                        this.DoCreateTile(list, res.ResultData.chat.title);
                    }
                }));
        }

        private void DoCreateTile(List<string> imageUris, string title)
        {
            SecondaryTileCreator.CreateTileForConversation(this._userOrChatId, this._isChat, title, imageUris, (Action<bool>)(res =>
            {
                this._isInProgressPinToStart = false;
                this.SetInProgressMain(false, "");
                if (res)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
            }));
        }

        public void Handle(NotificationSettingsChangedEvent message)
        {
            if (message.ChatId != this.UserOrCharId || !this.IsChat)
                return;
            this.AreNotificationsDisabled = message.AreNotificationsDisabled;
            this.RefreshUIPropertiesSafe();
        }

        public void Handle(MessagesFromGroupAllowedDeniedEvent message)
        {
            if (message.UserOrGroupId != this.UserOrCharId || this.IsChat)
                return;
            this.IsMessagesFromGroupDenied = !message.IsAllowed;
            this.RefreshUIPropertiesSafe();
        }

        public void AddAttachmentsFromRepository()
        {
            WallPost parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("WallPostAttachment") as WallPost;
            if (parameterForIdAndReset1 != null)
                this.OutboundMessageVM.AddWallPostAttachment(parameterForIdAndReset1);
            GeoCoordinate parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("NewPositionToBeAttached") as GeoCoordinate;
            if (parameterForIdAndReset2 != (GeoCoordinate)null)
                this.OutboundMessageVM.AddGeoAttachment(parameterForIdAndReset2.Latitude, parameterForIdAndReset2.Longitude);
            List<Stream> parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            List<Stream> parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
            ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosSizes");
            if (parameterForIdAndReset3 != null)
            {
                for (int index = 0; index < parameterForIdAndReset3.Count; ++index)
                {
                    Stream stream = parameterForIdAndReset3[index];
                    Stream previewStream = (Stream)null;
                    if (parameterForIdAndReset4 != null && parameterForIdAndReset4.Count > index)
                        previewStream = parameterForIdAndReset4[index];
                    this.OutboundMessageVM.AddPictureAttachment(stream, previewStream);
                }
            }
            Photo parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (parameterForIdAndReset5 != null)
                this.OutboundMessageVM.AddExistingPhotoAttachment(parameterForIdAndReset5);
            Video parameterForIdAndReset6 = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as Video;
            if (parameterForIdAndReset6 != null)
                this.OutboundMessageVM.AddExistingVideoAttachment(parameterForIdAndReset6);
            AudioObj parameterForIdAndReset7 = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (parameterForIdAndReset7 != null)
                this.OutboundMessageVM.AddExistingAudioAttachment(parameterForIdAndReset7);
            Doc parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (parameterForIdAndReset8 != null)
                this.OutboundMessageVM.AddExistingDocAttachment(parameterForIdAndReset8);
            Product parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("ShareProduct") as Product;
            if (parameterForIdAndReset9 != null)
                this.OutboundMessageVM.AddProductAttachment(parameterForIdAndReset9, this.CanDettachProductAttachment);
            FileOpenPickerContinuationEventArgs parameterForIdAndReset10 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if (parameterForIdAndReset10 != null && parameterForIdAndReset10.Files.Any<StorageFile>() || ParametersRepository.Contains("PickedPhotoDocuments"))
            {
                object parameterForIdAndReset11 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
                IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset10 != null ? parameterForIdAndReset10.Files : (IReadOnlyList<StorageFile>)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
                VKClient.Common.Library.Posts.AttachmentType result;
                if (parameterForIdAndReset11 != null && Enum.TryParse<VKClient.Common.Library.Posts.AttachmentType>(parameterForIdAndReset11.ToString(), out result))
                {
                    foreach (StorageFile file in (IEnumerable<StorageFile>)storageFileList)
                    {
                        if (result != VKClient.Common.Library.Posts.AttachmentType.VideoFromPhone)
                        {
                            if (result == VKClient.Common.Library.Posts.AttachmentType.DocumentFromPhone || result == VKClient.Common.Library.Posts.AttachmentType.DocumentPhoto)
                                this.OutboundMessageVM.AddUploadDocAttachment(file);
                        }
                        else
                            this.OutboundMessageVM.AddUploadVideoAttachment(file);
                    }
                }
            }
            List<StorageFile> parameterForIdAndReset12 = ParametersRepository.GetParameterForIdAndReset("ChosenDocuments") as List<StorageFile>;
            if (parameterForIdAndReset12 != null && parameterForIdAndReset12.Count > 0)
            {
                foreach (StorageFile file in parameterForIdAndReset12)
                    this.OutboundMessageVM.AddUploadDocAttachment(file);
            }
            List<StorageFile> parameterForIdAndReset13 = ParametersRepository.GetParameterForIdAndReset("ChosenVideos") as List<StorageFile>;
            if (parameterForIdAndReset13 == null || parameterForIdAndReset13.Count <= 0)
                return;
            foreach (StorageFile file in parameterForIdAndReset13)
                this.OutboundMessageVM.AddUploadVideoAttachment(file);
        }
    }
}
