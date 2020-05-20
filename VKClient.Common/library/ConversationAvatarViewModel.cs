using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
    public class ConversationAvatarViewModel : ViewModelBase
    {
        private string _uiImageUrl = string.Empty;
        private string _uiChatImage1Url = string.Empty;
        private string _uiChatImage2Url = string.Empty;
        private string _uiChatImage3Url = string.Empty;
        private string _uiChatImage4Url = string.Empty;
        private Visibility _chatLeftImageVisibility;
        private Visibility _chatRightImageVisibility;
        private string _chatRightImageUrl;
        private string _chatLeftImageUrl;
        private bool _isChat;
        private string _defaultAvatar;

        public bool IsChat
        {
            get
            {
                return this._isChat;
            }
            set
            {
                if (this._isChat == value)
                    return;
                this._isChat = value;
                base.NotifyPropertyChanged<bool>(() => this.IsChat);
                base.NotifyPropertyChanged<Visibility>(() => this.IsChatVisibility);
                base.NotifyPropertyChanged<Visibility>(() => this.IsNotChatVisibility);
            }
        }

        public Visibility IsChatVisibility
        {
            get
            {
                if (!this.IsChat || !string.IsNullOrEmpty(this._defaultAvatar))
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

        public Visibility ChatLeftImageVisibility
        {
            get
            {
                return this._chatLeftImageVisibility;
            }
            set
            {
                if (this._chatLeftImageVisibility == value)
                    return;
                this._chatLeftImageVisibility = value;
                base.NotifyPropertyChanged<Visibility>(() => this.ChatLeftImageVisibility);
            }
        }

        public Visibility ChatRightImageVisibility
        {
            get
            {
                return this._chatRightImageVisibility;
            }
            set
            {
                if (this._chatRightImageVisibility == value)
                    return;
                this._chatRightImageVisibility = value;
                this.NotifyPropertyChanged<Visibility>(() => this.ChatRightImageVisibility);
            }
        }

        public string ChatLeftImageUrl
        {
            get
            {
                return this._chatLeftImageUrl;
            }
            set
            {
                if (!(this._chatLeftImageUrl != value))
                    return;
                this._chatLeftImageUrl = value;
                this.NotifyPropertyChanged<string>(() => this.ChatLeftImageUrl);
            }
        }

        public string ChatRightImageUrl
        {
            get
            {
                return this._chatRightImageUrl;
            }
            set
            {
                if (!(this._chatRightImageUrl != value))
                    return;
                this._chatRightImageUrl = value;
                this.NotifyPropertyChanged<string>(() => this.ChatRightImageUrl);
            }
        }

        public string UIImageUrl
        {
            get
            {
                return this._uiImageUrl;
            }
            set
            {
                if (!(value != this._uiImageUrl))
                    return;
                this._uiImageUrl = value ?? "";
                this.NotifyPropertyChanged<string>(() => this.UIImageUrl);
            }
        }

        public string UIChatImage1Url
        {
            get
            {
                return this._uiChatImage1Url;
            }
            set
            {
                if (!(this._uiChatImage1Url != value))
                    return;
                this._uiChatImage1Url = value;
                this.NotifyPropertyChanged<string>(() => this.UIChatImage1Url);
            }
        }

        public string UIChatImage2Url
        {
            get
            {
                return this._uiChatImage2Url;
            }
            set
            {
                if (!(this._uiChatImage2Url != value))
                    return;
                this._uiChatImage2Url = value;
                this.NotifyPropertyChanged<string>(() => this.UIChatImage2Url);
            }
        }

        public string UIChatImage3Url
        {
            get
            {
                return this._uiChatImage3Url;
            }
            set
            {
                if (!(this._uiChatImage3Url != value))
                    return;
                this._uiChatImage3Url = value;
                this.NotifyPropertyChanged<string>(() => this.UIChatImage3Url);
            }
        }

        public string UIChatImage4Url
        {
            get
            {
                return this._uiChatImage4Url;
            }
            set
            {
                if (!(this._uiChatImage4Url != value))
                    return;
                this._uiChatImage4Url = value;
                this.NotifyPropertyChanged<string>(() => this.UIChatImage4Url);
            }
        }

        public void Initialize(string defaultAvatar, bool isChat, List<long> chatParticipantsIds, List<User> associatedUsers)
        {
            // ISSUE: object of a compiler-generated type is created
            // ISSUE: variable of a compiler-generated type
            //      ConversationAvatarViewModel.<>c__DisplayClass45_0 cDisplayClass450 = new ConversationAvatarViewModel.<>c__DisplayClass45_0();
            // ISSUE: reference to a compiler-generated field
            //      cDisplayClass450.chatParticipantsIds = chatParticipantsIds;
            this._defaultAvatar = defaultAvatar;
            this.IsChat = isChat;
            // ISSUE: method reference
            this.NotifyPropertyChanged<Visibility>(() => this.IsChatVisibility);
            // ISSUE: method reference
            this.NotifyPropertyChanged<Visibility>(() => this.IsNotChatVisibility);
            if (isChat)
            {
                // ISSUE: reference to a compiler-generated field
                if (string.IsNullOrWhiteSpace(defaultAvatar) && chatParticipantsIds.Count <= 1)
                {
                    string str = "null";
                    // ISSUE: reference to a compiler-generated field
                    if (chatParticipantsIds.Count > 0)
                    {
                        // ISSUE: method pointer
                        User user = Enumerable.FirstOrDefault<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[0]);
                        if (user != null)
                            str = user.photo_max;
                    }
                    this._defaultAvatar = defaultAvatar = str;
                    // ISSUE: method reference
                    this.NotifyPropertyChanged<Visibility>(() => this.IsChatVisibility);
                    // ISSUE: method reference
                    this.NotifyPropertyChanged<Visibility>(() => this.IsNotChatVisibility);
                }
                if (!string.IsNullOrWhiteSpace(defaultAvatar))
                {
                    this.UIChatImage1Url = this.UIChatImage2Url = this.UIChatImage3Url = this.UIChatImage4Url = "";
                    this.ChatLeftImageVisibility = this.ChatRightImageVisibility = Visibility.Collapsed;
                    this.ChatRightImageUrl = string.Empty;
                    this.ChatRightImageUrl = string.Empty;
                    this.UIImageUrl = defaultAvatar;
                }
                else
                {
                    // ISSUE: reference to a compiler-generated field
                    if (chatParticipantsIds.Count >= 3)
                    {
                        // ISSUE: reference to a compiler-generated field
                        chatParticipantsIds.Remove((long)(int)AppGlobalStateManager.Current.LoggedInUserId);
                    }
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated field
                    if (chatParticipantsIds.Count == 1 || chatParticipantsIds.Count == 2)
                    {
                        this.ChatLeftImageVisibility = Visibility.Visible;
                        // ISSUE: method pointer
                        User user1 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[0]));
                        if (user1 != null)
                            this.ChatLeftImageUrl = user1.photo_max;
                        // ISSUE: reference to a compiler-generated field
                        if (chatParticipantsIds.Count == 1)
                        {
                            this.ChatRightImageVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            this.ChatRightImageVisibility = Visibility.Visible;
                            // ISSUE: method pointer
                            User user2 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[1]));
                            if (user2 != null)
                                this.ChatRightImageUrl = user2.photo_max;
                        }
                        this.UIChatImage1Url = this.UIChatImage2Url = this.UIChatImage3Url = this.UIChatImage4Url = "";
                    }
                    // ISSUE: reference to a compiler-generated field
                    if (chatParticipantsIds.Count == 3)
                    {
                        this.ChatLeftImageVisibility = Visibility.Visible;
                        User user4 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[0]));
                        if (user4 != null)
                        {
                            this.ChatLeftImageUrl = user4.photo_max;
                        }
                        this.ChatRightImageVisibility = Visibility.Collapsed;
                        User user5 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[1]));
                        if (user5 != null)
                        {
                            this.UIChatImage2Url = user5.photo_max;
                        }
                        User user6 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[2]));
                        if (user6 != null)
                        {
                            this.UIChatImage4Url = user6.photo_max;
                        }
                        this.UIChatImage1Url = this.UIChatImage3Url = "";
                    }
                    // ISSUE: reference to a compiler-generated field
                    if (chatParticipantsIds.Count < 4)
                        return;
                    User user7 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[0]));
                    if (user7 != null)
                    {
                        this.UIChatImage1Url = user7.photo_max;
                    }
                    user7 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[1]));
                    if (user7 != null)
                    {
                        this.UIChatImage2Url = user7.photo_max;
                    }
                    user7 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[2]));
                    if (user7 != null)
                    {
                        this.UIChatImage3Url = user7.photo_max;
                    }
                    user7 = Enumerable.FirstOrDefault<User>(Enumerable.Where<User>(associatedUsers, (User u) => u.uid == chatParticipantsIds[3]));
                    if (user7 != null)
                    {
                        this.UIChatImage4Url = user7.photo_max;
                    }
                    this.ChatRightImageVisibility = this.ChatLeftImageVisibility = Visibility.Collapsed;
                }
            }
            else
                this.UIImageUrl = defaultAvatar;
        }

        //
        internal double px_per_tick = 120.0 / 10.0 / 2.0;

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
