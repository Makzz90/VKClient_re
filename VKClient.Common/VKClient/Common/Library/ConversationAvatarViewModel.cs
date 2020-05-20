using System;
using System.Collections.Generic;
using System.Linq;
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
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsChat));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsChatVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsNotChatVisibility));
      }
    }

    public Visibility IsChatVisibility
    {
      get
      {
        return !this.IsChat || !string.IsNullOrEmpty(this._defaultAvatar) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility IsNotChatVisibility
    {
      get
      {
        return this.IsChatVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
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
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ChatLeftImageVisibility));
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
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ChatRightImageVisibility));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ChatLeftImageUrl));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ChatRightImageUrl));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UIImageUrl));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UIChatImage1Url));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UIChatImage2Url));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UIChatImage3Url));
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
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.UIChatImage4Url));
      }
    }

    public void Initialize(string defaultAvatar, bool isChat, List<long> chatParticipantsIds, List<User> associatedUsers)
    {
      this._defaultAvatar = defaultAvatar;
      this.IsChat = isChat;
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsChatVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsNotChatVisibility));
      if (isChat)
      {
        if (string.IsNullOrWhiteSpace(defaultAvatar) && chatParticipantsIds.Count <= 1)
        {
          string str = "null";
          if (chatParticipantsIds.Count > 0)
          {
            User user = associatedUsers.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[0]));
            if (user != null)
              str = user.photo_max;
          }
          this._defaultAvatar = defaultAvatar = str;
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsChatVisibility));
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsNotChatVisibility));
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
          if (chatParticipantsIds.Count >= 3)
            chatParticipantsIds.Remove((long) (int) AppGlobalStateManager.Current.LoggedInUserId);
          if (chatParticipantsIds.Count == 1 || chatParticipantsIds.Count == 2)
          {
            this.ChatLeftImageVisibility = Visibility.Visible;
            User user1 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[0])).FirstOrDefault<User>();
            if (user1 != null)
              this.ChatLeftImageUrl = user1.photo_max;
            if (chatParticipantsIds.Count == 1)
            {
              this.ChatRightImageVisibility = Visibility.Collapsed;
            }
            else
            {
              this.ChatRightImageVisibility = Visibility.Visible;
              User user2 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[1])).FirstOrDefault<User>();
              if (user2 != null)
                this.ChatRightImageUrl = user2.photo_max;
            }
            this.UIChatImage1Url = this.UIChatImage2Url = this.UIChatImage3Url = this.UIChatImage4Url = "";
          }
          if (chatParticipantsIds.Count == 3)
          {
            this.ChatLeftImageVisibility = Visibility.Visible;
            User user1 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[0])).FirstOrDefault<User>();
            if (user1 != null)
              this.ChatLeftImageUrl = user1.photo_max;
            this.ChatRightImageVisibility = Visibility.Collapsed;
            User user2 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[1])).FirstOrDefault<User>();
            if (user2 != null)
              this.UIChatImage2Url = user2.photo_max;
            User user3 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[2])).FirstOrDefault<User>();
            if (user3 != null)
              this.UIChatImage4Url = user3.photo_max;
            this.UIChatImage1Url = this.UIChatImage3Url = "";
          }
          if (chatParticipantsIds.Count < 4)
            return;
          User user4 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[0])).FirstOrDefault<User>();
          if (user4 != null)
            this.UIChatImage1Url = user4.photo_max;
          User user5 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[1])).FirstOrDefault<User>();
          if (user5 != null)
            this.UIChatImage2Url = user5.photo_max;
          User user6 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[2])).FirstOrDefault<User>();
          if (user6 != null)
            this.UIChatImage3Url = user6.photo_max;
          User user7 = associatedUsers.Where<User>((Func<User, bool>) (u => u.uid == chatParticipantsIds[3])).FirstOrDefault<User>();
          if (user7 != null)
            this.UIChatImage4Url = user7.photo_max;
          this.ChatRightImageVisibility = this.ChatLeftImageVisibility = Visibility.Collapsed;
        }
      }
      else
        this.UIImageUrl = defaultAvatar;
    }
  }
}
