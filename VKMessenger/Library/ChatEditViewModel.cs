using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class ChatEditViewModel : ViewModelStatefulBase
  {
    private bool _isTitleBoxEnabled = true;
    private long _chatId;
    private ChatInfo _chatInformation;
    private NavigationService _navigationService;
    private bool _isPhotoChanging;

    public ObservableCollection<ChatMember> Members = new ObservableCollection<ChatMember>();

    public long ChatId
    {
      get
      {
        return this._chatId;
      }
    }

    public long PeerId
    {
      get
      {
        return 2000000000L + this._chatId;
      }
    }

    public string Title
    {
      get
      {
        ChatInfo chatInformation = this._chatInformation;
        return (chatInformation != null ? Extensions.ForUI(chatInformation.chat.title) :  null) ?? "";
      }
    }

    public string Photo
    {
      get
      {
        ChatInfo chatInformation = this._chatInformation;
        return (chatInformation != null ? chatInformation.chat.photo_200 :  null) ?? "";
      }
    }

    public Visibility PhotoPlaceholderVisibility
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Photo))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool IsPhotoMenuEnabled
    {
      get
      {
          if (this.PhotoPlaceholderVisibility == Visibility.Collapsed)
          return !this.IsPhotoChanging;
        return false;
      }
    }

    public bool IsNotificationsSoundEnabled
    {
      get
      {
        ChatInfo chatInformation = this._chatInformation;
        if (chatInformation == null)
          return true;
        return !chatInformation.chat.push_settings.AreDisabledNow;
      }
    }

    public string NotificationsSoundMode
    {
      get
      {
        if (!this.IsNotificationsSoundEnabled)
          return CommonResources.NotificationsSound_Disabled;
        return CommonResources.NotificationsSound_Enabled;
      }
    }

    public bool IsTitleBoxEnabled
    {
      get
      {
        return this._isTitleBoxEnabled;
      }
      set
      {
        this._isTitleBoxEnabled = value;
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsTitleBoxEnabled));
      }
    }

    public bool IsPhotoChanging
    {
      get
      {
        return this._isPhotoChanging;
      }
      set
      {
        this._isPhotoChanging = value;
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsPhotoChanging));
        this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsPhotoMenuEnabled));
      }
    }

    public bool IsNotificationsSoundModeSwitching { get; private set; }

    public bool IsMemberAdding { get; private set; }

    public bool IsChatLeaving { get; private set; }

    public ChatEditViewModel(long chatId, NavigationService navigationService)
    {
      this._chatId = chatId;
      this._navigationService = navigationService;
    }

    public override void Load(Action<ResultCode> callback)
    {
      BackendServices.ChatService.GetChatInfo(this._chatId, (Action<BackendResult<ChatInfo, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this._chatInformation = result.ResultData;
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            long chatCreatorId = long.Parse(this._chatInformation.chat.admin_id);
            foreach (ChatUser chatParticipant in this._chatInformation.chat_participants)
            {
              ChatUser member = chatParticipant;
              User invitedByUser = this._chatInformation.invited_by_users.FirstOrDefault<User>((Func<User, bool>) (u => u.id == member.invited_by));
              if (invitedByUser != null)
                this.Members.Add(new ChatMember(member, invitedByUser, chatCreatorId));
            }
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Photo));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>) (() => this.PhotoPlaceholderVisibility));
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsPhotoMenuEnabled));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.NotificationsSoundMode));
          }));
        }
        callback(result.ResultCode);
      }));
    }

    public void ChangeTitle(string newTitle, Action errorAction)
    {
      if (string.IsNullOrWhiteSpace(newTitle) || newTitle.Length < 2)
      {
        errorAction();
      }
      else
      {
        this.SetInProgress(true, "");
        this.IsTitleBoxEnabled = false;
        BackendServices.ChatService.EditChat(this._chatId, newTitle, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this._chatInformation.chat.title = newTitle;
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Title));
          }
          else
          {
            errorAction();
            GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
          }
          this.SetInProgress(false, "");
          this.IsTitleBoxEnabled = true;
        }))));
      }
    }

    public void UpdatePhoto(Stream photoStream, Rect crop)
    {
      this.IsPhotoChanging = true;
      this.SetInProgress(true, "");
      ImagePreprocessor.PreprocessImage(photoStream, VKConstants.ResizedImageSize, true, (Action<ImagePreprocessResult>) (resized =>
      {
        Stream stream = resized.Stream;
        byte[] photoData = ImagePreprocessor.ReadFully(stream);
        stream.Close();
        BackendServices.MessagesService.UpdateChatPhoto(this.ChatId, photoData, ImagePreprocessor.GetThumbnailRect((double) resized.Width, (double) resized.Height, crop), (Action<BackendResult<ChatInfoWithMessageId, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this._chatInformation.chat.photo_200 = result.ResultData.chat.photo_200;
            this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Photo));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>) (() => this.PhotoPlaceholderVisibility));
            this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsPhotoMenuEnabled));
          }
          else
            GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
          this.SetInProgress(false, "");
          this.IsPhotoChanging = false;
        }))));
      }));
    }

    public void DeletePhoto()
    {
      this.IsPhotoChanging = true;
      this.SetInProgress(true, "");
      BackendServices.MessagesService.DeleteChatPhoto(this.ChatId, (Action<BackendResult<ChatInfoWithMessageId, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this._chatInformation.chat.photo_200 =  null;
          this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Photo));
          this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>) (() => this.PhotoPlaceholderVisibility));
          this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsPhotoMenuEnabled));
        }
        else
          GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        this.SetInProgress(false, "");
        this.IsPhotoChanging = false;
      }))));
    }

    public void SwitchNotificationsSoundMode()
    {
      this.IsNotificationsSoundModeSwitching = true;
      this.SetInProgress(true, "");
      AccountService.Instance.SetSilenceMode(AppGlobalStateManager.Current.GlobalState.NotificationsUri, this.IsNotificationsSoundEnabled ? -1 : 0, (Action<BackendResult<object, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this._chatInformation.chat.push_settings.disabled_until = this.IsNotificationsSoundEnabled ? -1 : 0;
          this.NotifyPropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsNotificationsSoundEnabled));
          this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.NotificationsSoundMode));
        }
        else
          GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        this.SetInProgress(false, "");
        this.IsNotificationsSoundModeSwitching = false;
      }))), this.ChatId, 0L);
    }

    public void AddMember(User user)
    {
      this.IsMemberAdding = true;
      this.SetInProgress(true, "");
      IChatService chatService = BackendServices.ChatService;
      long chatId = this._chatId;
      List<long> userIds = new List<long>();
      userIds.Add(user.id);
      Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          User loggedInUser = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
          this.Members.Add(new ChatMember(new ChatUser(user, loggedInUser.id), loggedInUser, long.Parse(this._chatInformation.chat.admin_id)));
        }
        else
          GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        this.SetInProgress(false, "");
        this.IsMemberAdding = false;
      })));
      chatService.AddChatUsers(chatId, userIds, callback);
    }

    public void ExcludeMember(ChatMember member)
    {
      this.SetInProgress(true, "");
      member.ExcludeButtonVisibility = Visibility.Collapsed;
      IChatService chatService = BackendServices.ChatService;
      long chatId = this._chatId;
      List<long> usersToBeRemoved = new List<long>();
      usersToBeRemoved.Add(member.Id);
      Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this.Members.Remove(member);
        }
        else
        {
          member.ExcludeButtonVisibility = Visibility.Visible;
          GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        }
        this.SetInProgress(false, "");
      })));
      chatService.RemoveChatUsers(chatId, usersToBeRemoved, callback);
    }

    public void LeaveChat()
    {
      this.IsChatLeaving = true;
      this.SetInProgress(true, "");
      IChatService chatService = BackendServices.ChatService;
      long chatId = this._chatId;
      List<long> usersToBeRemoved = new List<long>();
      usersToBeRemoved.Add(AppGlobalStateManager.Current.LoggedInUserId);
      Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> callback = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this._navigationService.RemoveBackEntrySafe();
          this._navigationService.GoBackSafe();
        }
        else
          GenericInfoUC.ShowBasedOnResult(result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        this.SetInProgress(false, "");
        this.IsChatLeaving = false;
      })));
      chatService.RemoveChatUsers(chatId, usersToBeRemoved, callback);
    }
  }
}
