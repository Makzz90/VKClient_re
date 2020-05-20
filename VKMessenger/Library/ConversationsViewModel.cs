using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
    public class ConversationsViewModel : ViewModelBase, IBinarySerializableWithTrimSupport, IBinarySerializable, IHandle<NotificationTurnOnOff>, IHandle, ICollectionDataProvider<MessageListResponse, ConversationHeader>
    {
        private static object _instLock = new object();
        private readonly int numberOfItemsToLoadDefault = 30;
        private GenericCollectionViewModel<MessageListResponse, ConversationHeader> _convGenCol;
        //private bool _isLoading;
        private int _totalCount;
        private int _numberOfUnreadMessages;
        private static ConversationsViewModel _instance;

        public ObservableCollection<ConversationHeader> Conversations
        {
            get
            {
                return this.ConversationsGenCol.Collection;
            }
        }

        public GenericCollectionViewModel<MessageListResponse, ConversationHeader> ConversationsGenCol
        {
            get
            {
                return this._convGenCol;
            }
            set
            {
                this._convGenCol = value;
                this.NotifyPropertyChanged<GenericCollectionViewModel<MessageListResponse, ConversationHeader>>((Expression<Func<GenericCollectionViewModel<MessageListResponse, ConversationHeader>>>)(() => this.ConversationsGenCol));
            }
        }

        public int NumberOfUnreadMessages
        {
            get
            {
                return this._numberOfUnreadMessages;
            }
            set
            {
                this._numberOfUnreadMessages = value;
                this.NotifyPropertyChanged<int>((Expression<Func<int>>)(() => this.NumberOfUnreadMessages));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.UnreadMessagesCountVisibility));
            }
        }

        public Visibility UnreadMessagesCountVisibility
        {
            get
            {
                if (this.NumberOfUnreadMessages == 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool NeedRefresh { get; set; }

        public static bool IsInstanceNull
        {
            get
            {
                return ConversationsViewModel._instance == null;
            }
        }

        public static ConversationsViewModel Instance
        {
            get
            {
                if (ConversationsViewModel._instance == null)
                {
                    lock (ConversationsViewModel._instLock)
                    {
                        ConversationsViewModel conversationsViewModel = new ConversationsViewModel();
                        CacheManager.TryDeserialize((IBinarySerializable)conversationsViewModel, "CNVRS2" + (object)AppGlobalStateManager.Current.LoggedInUserId, CacheManager.DataType.CachedData);
                        conversationsViewModel.NeedRefresh = true;
                        if (ConversationsViewModel._instance == null)
                            ConversationsViewModel._instance = conversationsViewModel;
                    }
                }
                return ConversationsViewModel._instance;
            }
            set
            {
                ConversationsViewModel._instance = value;
            }
        }

        public Func<MessageListResponse, ListWithCount<ConversationHeader>> ConverterFunc
        {
            get
            {
                return (Func<MessageListResponse, ListWithCount<ConversationHeader>>)(mlr =>
                {
                    this.NeedRefresh = false;
                    return new ListWithCount<ConversationHeader>()
                    {
                        List = ConversationsViewModel.GetConversationHeaders(mlr.DialogHeaders, mlr.Users),
                        TotalCount = mlr.TotalCount
                    };
                });
            }
        }

        public ConversationsViewModel()
        {
            this._convGenCol = new GenericCollectionViewModel<MessageListResponse, ConversationHeader>((ICollectionDataProvider<MessageListResponse, ConversationHeader>)this)
            {
                NoContentText = CommonResources.NoContent_Messages,
                NoContentImage = "../Resources/NoContentImages/Messages.png"
            };
            this._convGenCol.LoadCount = 30;
            this._convGenCol.ReloadCount = 50;
            EventAggregator.Current.Subscribe((object)this);
        }

        public void LoadMoreConversations(Action callback)
        {
            Logger.Instance.Info("LoadMoreConversations");
            this._convGenCol.LoadData(false, true, (Action<BackendResult<MessageListResponse, ResultCode>>)null, false);
        }

        public void RefreshConversations(bool suppressLoadingMessage = false)
        {
            Logger.Instance.Info("RefreshConversations");
            this._convGenCol.LoadData(true, suppressLoadingMessage, (Action<BackendResult<MessageListResponse, ResultCode>>)null, false);
        }

        public static List<ConversationHeader> GetConversationHeaders(List<DialogHeaderInfo> dialogHeaderInfoList, List<User> users)
        {
            List<ConversationHeader> conversationHeaderList = new List<ConversationHeader>();
            foreach (DialogHeaderInfo dialogHeaderInfo in dialogHeaderInfoList)
            {
                List<long> userIdsForMessage = dialogHeaderInfo.message.GetAssociatedUserIds(true);
                ConversationHeader conversationHeader = new ConversationHeader(dialogHeaderInfo.message, users.Where<User>((Func<User, bool>)(u => userIdsForMessage.Contains((long)(int)u.uid))).ToList<User>(), dialogHeaderInfo.unread);
                conversationHeaderList.Add(conversationHeader);
            }
            return conversationHeaderList;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteList<ConversationHeader>((IList<ConversationHeader>)this.Conversations, 10000);
            writer.Write(this.NumberOfUnreadMessages);
            writer.Write(this._totalCount);
        }

        public void WriteTrimmed(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteList<ConversationHeader>((IList<ConversationHeader>)this.Conversations.Take<ConversationHeader>(this.numberOfItemsToLoadDefault).ToList<ConversationHeader>(), 10000);
            writer.Write(this.NumberOfUnreadMessages);
            writer.Write(this._totalCount);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            List<ConversationHeader> conversationHeaderList = reader.ReadList<ConversationHeader>();
            this.NumberOfUnreadMessages = reader.ReadInt32();
            this._totalCount = reader.ReadInt32();
            this.ConversationsGenCol = new GenericCollectionViewModel<MessageListResponse, ConversationHeader>((ICollectionDataProvider<MessageListResponse, ConversationHeader>)this)
            {
                NoContentText = CommonResources.NoContent_Messages,
                NoContentImage = "../Resources/NoContentImages/Messages.png"
            };
            this.ConversationsGenCol.ReadData(new ListWithCount<ConversationHeader>()
            {
                TotalCount = this._totalCount,
                List = conversationHeaderList
            });
        }

        public void IncreaseNumberOfUnreadMessagesBy(int count)
        {
            Execute.ExecuteOnUIThread((Action)(() => this.NumberOfUnreadMessages = Math.Max(0, this.NumberOfUnreadMessages + count)));
        }

        internal void ProcessInstantUpdates(List<LongPollServerUpdateData> updates)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this != ConversationsViewModel.Instance)
                    return;
                Dictionary<long, bool> dictionary1 = new Dictionary<long, bool>();
                Dictionary<long, bool> dictionary2 = new Dictionary<long, bool>();
                Dictionary<long, bool> dictionary3 = new Dictionary<long, bool>();
                List<Message> newMessages = new List<Message>();
                foreach (LongPollServerUpdateData update in updates)
                {
                    LongPollServerUpdateData longPollServerUpdateData = update;
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.NewCounter)
                        CountersManager.Current.SetUnreadMessages(longPollServerUpdateData.Counter);
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.UserIsTyping || longPollServerUpdateData.UpdateType == LongPollServerUpdateType.UserIsTypingInChat)
                    {
                        long chatId = longPollServerUpdateData.chat_id;
                        long userId = longPollServerUpdateData.user_id;
                        bool isChat = longPollServerUpdateData.UpdateType == LongPollServerUpdateType.UserIsTypingInChat;
                        ConversationHeader conversationHeader = this.Conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>)(c =>
                        {
                            if (c.IsChat == isChat)
                                return c.UserOrChatId == (isChat ? chatId : userId);
                            return false;
                        }));
                        if (conversationHeader != null)
                        {
                            long userId1 = userId;
                            conversationHeader.SetUserIsTypingWithDelayedReset(userId1);
                        }
                    }
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.UserBecameOnline)
                    {
                        dictionary1[longPollServerUpdateData.user_id] = true;
                        if (longPollServerUpdateData.Platform != 7)
                            dictionary2[longPollServerUpdateData.user_id] = true;
                    }
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.UserBecameOffline)
                        dictionary1[longPollServerUpdateData.user_id] = false;
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.MessageHasBeenRead)
                        dictionary3[longPollServerUpdateData.message_id] = true;
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.IncomingMessagesRead)
                    {
                        long id = longPollServerUpdateData.chat_id == 0L ? longPollServerUpdateData.user_id : longPollServerUpdateData.chat_id;
                        ConversationHeader conversationHeader = this.Conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>)(c => c.UserOrChatId == id));
                        if (conversationHeader != null)
                            conversationHeader.Unread = 0;
                    }
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.MessageHasBeenAdded && longPollServerUpdateData.message != null)
                    {
                        newMessages.Add(longPollServerUpdateData.message);
                        Extensions.UnixTimeStampToDateTime((double)longPollServerUpdateData.message.date, true);
                        if (longPollServerUpdateData.message.@out == 0 && !longPollServerUpdateData.IsHistoricData && string.IsNullOrWhiteSpace(longPollServerUpdateData.message.action))
                        {
                            bool isChat = (uint)longPollServerUpdateData.message.chat_id > 0U;
                            int id = isChat ? longPollServerUpdateData.message.chat_id : longPollServerUpdateData.message.uid;
                            if (!isChat)
                            {
                                if (longPollServerUpdateData.user != null)
                                {
                                    string str1 = longPollServerUpdateData.user.first_name ?? "";
                                    string str2 = longPollServerUpdateData.user.last_name ?? "";
                                    string imageSrc = longPollServerUpdateData.user.photo_max ?? "";
                                    MessengerStateManagerInstance.Current.HandleInAppNotification(str1 + " " + str2, ConversationHeader.GetMessageHeaderText(longPollServerUpdateData.message, longPollServerUpdateData.user, (User)null), (long)id, isChat.ToString(), imageSrc);
                                }
                                else
                                {
                                    UsersService instance = UsersService.Instance;
                                    List<long> userIds = new List<long>();
                                    userIds.Add((long)longPollServerUpdateData.message.uid);
                                    Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(res =>
                                    {
                                        if (res.ResultCode != ResultCode.Succeeded || res.ResultData == null || res.ResultData.Count <= 0)
                                            return;
                                        MessengerStateManagerInstance.Current.HandleInAppNotification(res.ResultData.First<User>().first_name + " " + res.ResultData.First<User>().last_name, ConversationHeader.GetMessageHeaderText(longPollServerUpdateData.message, res.ResultData.First<User>(), (User)null), (long)id, isChat.ToString(), res.ResultData.First<User>().photo_max);
                                    });
                                    instance.GetUsers(userIds, callback);
                                }
                            }
                            else
                            {
                                bool flag = !longPollServerUpdateData.message.push_settings.AreDisabledNow;
                                if (!flag)
                                {
                                    string body = longPollServerUpdateData.message.body;
                                    if (!string.IsNullOrWhiteSpace(body))
                                    {
                                        foreach (object match in BrowserNavigationService.Regex_Mention.Matches(body))
                                        {
                                            if (match.ToString().ToLower().Contains(string.Format("[id{0}|", (object)AppGlobalStateManager.Current.LoggedInUserId)))
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    string imageSrc = longPollServerUpdateData.user.photo_max ?? "";
                                    string messageHeaderText = ConversationHeader.GetMessageHeaderText(longPollServerUpdateData.message, longPollServerUpdateData.user, (User)null);
                                    MessengerStateManagerInstance.Current.HandleInAppNotification(longPollServerUpdateData.message.title, messageHeaderText, (long)id, true.ToString(), imageSrc);
                                }
                            }
                        }
                    }
                    if (longPollServerUpdateData.UpdateType == LongPollServerUpdateType.ChatParamsWereChanged)
                    {
                        long chatId = longPollServerUpdateData.chat_id;
                        Func<ConversationHeader, bool> func;
                        VKMessenger.Backend.BackendServices.ChatService.GetChatInfo(chatId, (Action<BackendResult<ChatInfo, ResultCode>>)(res =>
                        {
                            if (res.ResultCode != ResultCode.Succeeded)
                                return;
                            ChatInfo chatInfo = res.ResultData;
                            Execute.ExecuteOnUIThread((Action)(() =>
                            {
                                ConversationHeader conversationHeader = this.Conversations.FirstOrDefault<ConversationHeader>( (func = (Func<ConversationHeader, bool>)(ch =>
                                {
                                    if (ch.IsChat)
                                        return ch.UserOrChatId == chatId;
                                    return false;
                                })));
                                if (conversationHeader == null)
                                    return;
                                conversationHeader._message.title = chatInfo.chat.title;
                                conversationHeader._associatedUsers.Clear();
                                foreach (ChatUser chatParticipant in chatInfo.chat_participants)
                                {
                                    User user = new User()
                                    {
                                        uid = chatParticipant.uid,
                                        online = chatParticipant.online,
                                        photo_max = chatParticipant.photo_max,
                                        first_name = chatParticipant.first_name,
                                        last_name = chatParticipant.last_name,
                                        first_name_acc = chatParticipant.first_name_acc,
                                        last_name_acc = chatParticipant.last_name_acc
                                    };
                                    conversationHeader._associatedUsers.Add(user);
                                }
                                conversationHeader._message.chat_active_str = chatInfo.chat_participants.Select<ChatUser, long>((Func<ChatUser, long>)(c => c.uid)).ToList<long>().GetCommaSeparated();
                                conversationHeader.RefreshUIProperties(true);
                            }));
                        }));
                    }
                }
                if (updates != null)
                {
                    int count = updates.Count<LongPollServerUpdateData>((Func<LongPollServerUpdateData, bool>)(u =>
                    {
                        if (u.UpdateType == LongPollServerUpdateType.MessageHasBeenAdded && !u.@out)
                            return !u.IsHistoricData;
                        return false;
                    }));
                    if (count > 0)
                        this.IncreaseNumberOfUnreadMessagesBy(count);
                }
                List<ConversationHeader> updatedHeaders = new List<ConversationHeader>();
                List<ConversationHeader> conversationHeaderList = new List<ConversationHeader>();
                if (dictionary1.Count > 0 || dictionary3.Count > 0)
                {
                    foreach (ConversationHeader conversation in (Collection<ConversationHeader>)this.Conversations)
                    {
                        bool flag = false;
                        if (dictionary3.ContainsKey((long)conversation._message.mid))
                        {
                            conversation._message.read_state = dictionary3[(long)conversation._message.mid] ? 1 : 0;
                            flag = true;
                        }
                        foreach (User associatedUser in conversation._associatedUsers)
                        {
                            if (dictionary1.ContainsKey(associatedUser.uid))
                            {
                                associatedUser.online = dictionary1[associatedUser.uid] ? 1 : 0;
                                associatedUser.online_mobile = dictionary2.ContainsKey(associatedUser.uid) ? 1 : 0;
                                flag = true;
                            }
                        }
                        if (flag)
                            updatedHeaders.Add(conversation);
                    }
                    if (updatedHeaders.Count > 0)
                        ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() => updatedHeaders.ForEach((Action<ConversationHeader>)(h => h.RefreshUIProperties(false)))));
                }
                if (newMessages.Count <= 0)
                    return;
                this.ApplyNewMessagesFromInstantUpdate(newMessages);
                ConversationsViewModel.CheckNewMessagesForStickersPack(newMessages);
            }));
        }

        private void ApplyNewMessagesFromInstantUpdate(List<Message> newMessages)
        {
            UsersService.Instance.GetUsers(Message.GetAssociatedUserIds(newMessages, true).Select<long, int>((Func<long, int>)(l => (int)l)).ToList<int>().Select<int, long>((Func<int, long>)(uid => (long)uid)).ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>)(usersResult => Execute.ExecuteOnUIThread((Action)(() =>
            {
                List<ConversationHeader> source1 = new List<ConversationHeader>();
                List<DialogHeaderInfo> dialogHeaderInfoList = new List<DialogHeaderInfo>();
                foreach (IGrouping<int, Message> source2 in newMessages.Where<Message>((Func<Message, bool>)(m => m.chat_id == 0)).GroupBy<Message, int>((Func<Message, int>)(m => m.uid)))
                {
                    Message userMessage = source2.LastOrDefault<Message>();
                    if (userMessage != null)
                    {
                        dialogHeaderInfoList.Add(new DialogHeaderInfo()
                        {
                            message = userMessage
                        });
                        ConversationHeader conversationHeader = this.Conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>)(c =>
                        {
                            if (c._message.uid == userMessage.uid)
                                return c._message.chat_id == 0;
                            return false;
                        }));
                        if (conversationHeader != null)
                        {
                            dialogHeaderInfoList.Last<DialogHeaderInfo>().unread = userMessage.@out == 0 ? conversationHeader.Unread + 1 : conversationHeader.Unread;
                            source1.Add(conversationHeader);
                        }
                    }
                }
                foreach (IGrouping<int, Message> source2 in newMessages.Where<Message>((Func<Message, bool>)(m => (uint)m.chat_id > 0U)).GroupBy<Message, int>((Func<Message, int>)(m => m.chat_id)))
                {
                    Message chatMessage = source2.LastOrDefault<Message>();
                    if (chatMessage != null)
                    {
                        dialogHeaderInfoList.Add(new DialogHeaderInfo()
                        {
                            message = chatMessage
                        });
                        ConversationHeader conversationHeader = this.Conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>)(c => c._message.chat_id == chatMessage.chat_id));
                        if (conversationHeader != null)
                        {
                            dialogHeaderInfoList.Last<DialogHeaderInfo>().unread = chatMessage.@out == 0 ? conversationHeader.Unread + 1 : conversationHeader.Unread;
                            source1.Add(conversationHeader);
                        }
                    }
                }
                List<ConversationHeader> conversationHeaders = ConversationsViewModel.GetConversationHeaders(dialogHeaderInfoList, usersResult.ResultData);
                Logger.Instance.Info("Applying new messages. Found {0} headers to remove, {1} to add.", (object)source1.Count, (object)conversationHeaders.Count);
                foreach (ConversationHeader conversationHeader1 in source1)
                {
                    UsersTypingHelper usersTypingHelper1 = conversationHeader1.UsersTypingHelper;
                    if ((usersTypingHelper1 != null ? (usersTypingHelper1.AnyTypingNow ? 1 : 0) : 0) != 0)
                    {
                        foreach (ConversationHeader conversationHeader2 in conversationHeaders)
                        {
                            if (conversationHeader1.IsChat == conversationHeader2.IsChat && conversationHeader1.UserOrChatId == conversationHeader2.UserOrChatId)
                            {
                                UsersTypingHelper usersTypingHelper2 = conversationHeader1.UsersTypingHelper;
                                usersTypingHelper2.SetUserIsNotTyping((long)conversationHeader2._message.user_id);
                                conversationHeader2.UsersTypingHelper = usersTypingHelper2;
                                usersTypingHelper2.Reinitialize(conversationHeader2);
                                break;
                            }
                        }
                    }
                }
                foreach (ConversationHeader conversationHeader in source1.Distinct<ConversationHeader>())
                    this.ConversationsGenCol.Delete(conversationHeader);
                int num = 0;
                foreach (ConversationHeader conversationHeader in (IEnumerable<ConversationHeader>)conversationHeaders.OrderByDescending<ConversationHeader, int>((Func<ConversationHeader, int>)(h => h._message.date)))
                    this.ConversationsGenCol.Insert(conversationHeader, num++);
            }))));
        }

        private static void CheckNewMessagesForStickersPack(List<Message> newMessages)
        {
            foreach (Message message in newMessages.Where<Message>((Func<Message, bool>)(m => m.@out == 0)))
            {
                List<Attachment> attachments = message.attachments;
                if (attachments == null || attachments.Count == 0)
                    break;
                Attachment attachment = attachments.FirstOrDefault<Attachment>((Func<Attachment, bool>)(attach =>
                {
                    if (attach.type == "gift" && attach.gift != null)
                        return (ulong)attach.gift.stickers_product_id > 0UL;
                    return false;
                }));
                long num1 = attachment != null ? attachment.gift.stickers_product_id : 0L;
                if (num1 != 0L)
                {
                    StoreService instance = StoreService.Instance;
                    int num2 = 0;
                    List<long> productIds = new List<long>();
                    productIds.Add(num1);
                    // ISSUE: variable of the null type
                    long purchaseForId = 0;
                    instance.GetStockItems((StoreProductType)num2, productIds, null, purchaseForId, (Action<BackendResult<VKList<StockItem>, ResultCode>>)(result =>
                    {
                        if (result.ResultCode != ResultCode.Succeeded)
                            return;
                        VKList<StockItem> resultData = result.ResultData;
                        StockItem stockItem1;
                        if (resultData == null)
                        {
                            stockItem1 = (StockItem)null;
                        }
                        else
                        {
                            List<StockItem> items = resultData.items;
                            stockItem1 = items != null ? items.FirstOrDefault<StockItem>() : (StockItem)null;
                        }
                        StockItem stockItem2 = stockItem1;
                        if (stockItem2 == null)
                            return;
                        EventAggregator.Current.Publish((object)new StickersPackPurchasedEvent(new StockItemHeader(stockItem2, false, 0, false))
                        {
                            IsGift = true
                        });
                    }));
                }
            }
        }

        internal void DeleteConversation(ConversationHeader conversation)
        {
            this.ConversationsGenCol.Delete(conversation);
            VKMessenger.Backend.BackendServices.MessagesService.DeleteDialog(conversation.UserOrChatId, conversation.IsChat, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => { }));
        }

        public static void Save()
        {
            CacheManager.TrySerialize((IBinarySerializable)ConversationsViewModel.Instance, "CNVRS2" + (object)AppGlobalStateManager.Current.LoggedInUserId, true, CacheManager.DataType.CachedData);
        }

        public void Handle(NotificationTurnOnOff message)
        {
            ConversationHeader conversationHeader = this.Conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>)(h =>
            {
                if (h.UserOrChatId == message.UserOrChatId)
                    return h.IsChat == message.IsChat;
                return false;
            }));
            if (conversationHeader == null)
                return;
            conversationHeader.RespondToSettingsChange();
        }

        public void GetData(GenericCollectionViewModel<MessageListResponse, ConversationHeader> caller, int offset, int count, Action<BackendResult<MessageListResponse, ResultCode>> callback)
        {
            VKMessenger.Backend.BackendServices.MessagesService.GetDialogs(new GetDialogsRequest(offset, count, 60), callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<MessageListResponse, ConversationHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.Conversations_NoDialogs;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.Conversations_OneDialogFrm, CommonResources.Conversations_TwoFourDialogsFrm, CommonResources.Conversations_FiveDialogsFrm, true, (string)null, false);
        }
    }
}
