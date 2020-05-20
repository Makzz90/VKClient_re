using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKMessenger.Backend;

namespace VKClient.Common.Graffiti.ViewModels
{
    public class GraffitiDrawViewModel : ViewModelBase
    {
        private readonly long _userOrChatId;
        private readonly bool _isChat;
        private bool _isBusyLoadingHeaderInfo;
        private User _user;
        private ChatExtended _chat;

        public ConversationAvatarViewModel ConversationAvatarVM { get; private set; }

        public GraffitiDrawViewModel(long userOrChatId, bool isChat, User user = null, ChatExtended chat = null)
        {
            this.ConversationAvatarVM = new ConversationAvatarViewModel();

            this._userOrChatId = userOrChatId;
            this._isChat = isChat;
            this._user = user;
            this._chat = chat;
        }

        public void LoadHeaderInfo()
        {
            if (this._isBusyLoadingHeaderInfo)
                return;
            if (this._isChat)
            {
                if (this._chat == null)
                {
                    this._isBusyLoadingHeaderInfo = true;
                    BackendServices.MessagesService.GetChat(this._userOrChatId, (Action<BackendResult<ChatExtended, ResultCode>>)(res =>
                    {
                        if (res.ResultCode == ResultCode.Succeeded)
                            this._chat = res.ResultData;
                        this.RefreshUIPropertiesSafe();
                        this._isBusyLoadingHeaderInfo = false;
                    }));
                }
                else
                    this.RefreshUIPropertiesSafe();
            }
            else if (this._user == null)
            {
                this._isBusyLoadingHeaderInfo = true;
                UsersService instance = UsersService.Instance;
                List<long> userIds = new List<long>();
                userIds.Add(this._userOrChatId);
                Action<BackendResult<List<User>, ResultCode>> callback = (Action<BackendResult<List<User>, ResultCode>>)(r =>
                {
                    if (r.ResultCode == ResultCode.Succeeded)
                    {
                        this._user = (User)Enumerable.First<User>(r.ResultData);
                        this.RefreshUIPropertiesSafe();
                        UsersService.Instance.GetStatus(this._userOrChatId, (Action<BackendResult<UserStatus, ResultCode>>)(res =>
                        {
                            if (res.ResultCode == ResultCode.Succeeded)
                                this.RefreshUIPropertiesSafe();
                            this._isBusyLoadingHeaderInfo = false;
                        }));
                    }
                    else
                        this._isBusyLoadingHeaderInfo = false;
                });
                instance.GetUsers(userIds, callback);
            }
            else
                this.RefreshUIPropertiesSafe();
        }

        private void RefreshUIPropertiesSafe()
        {
            if (this._isChat)
                Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ChatExtended chatExtended = this._chat;
                    if ((chatExtended != null ? chatExtended.users : null) == null)
                        return;
                    this.ConversationAvatarVM.Initialize(this._chat.photo_200, true, this._chat.users.Select<User, long>((Func<User, long>)(u => u.uid)).ToList<long>(), this._chat.users);
                }));
            else
                Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (this._user == null)
                        return;
                    this.ConversationAvatarVM.Initialize(this._user.photo_max, false, new List<long>(), new List<User>());
                }));
        }
    }
}
