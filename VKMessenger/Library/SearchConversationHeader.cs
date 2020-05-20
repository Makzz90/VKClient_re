using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class SearchConversationHeader : ConversationHeader
  {
    public string Title
    {
      get
      {
        return this.UITitle;
      }
    }

    public Visibility OnlineIconVisibility
    {
      get
      {
        return this.IsOnlineVisibility;
      }
    }

    public Visibility MobileOnlineIconVisibility
    {
      get
      {
        return this.IsOnlineMobileVisibility;
      }
    }

    public Visibility DisabledNotificationsIconVisibility
    {
      get
      {
        return this.NotificationsDisabledVisibility;
      }
    }

    public Visibility UserPhotoVisibility
    {
      get
      {
        return this.IsNotChatVisibility;
      }
    }

    public Visibility ChatPhotosVisibility
    {
      get
      {
        return this.IsChatVisibility;
      }
    }

    public string UserPhoto
    {
      get
      {
          if (this.UserPhotoVisibility != Visibility.Visible)
          return "";
        string photo200 = this._message.photo_200;
        if (string.IsNullOrEmpty(photo200))
          return this.User.photo_max;
        return photo200;
      }
    }

    public string ChatUserPhoto1
    {
      get
      {
        return this.GetChatUserPhoto(0);
      }
    }

    public string ChatUserPhoto2
    {
      get
      {
        return this.GetChatUserPhoto(1);
      }
    }

    public string ChatUserPhoto3
    {
      get
      {
        return this.GetChatUserPhoto(2);
      }
    }

    public string ChatUserPhoto4
    {
      get
      {
        return this.GetChatUserPhoto(3);
      }
    }

    public Visibility ChatLeftUserPhotoVisibility
    {
      get
      {
        if (!(this.GetChatUserPhoto(2) == ""))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility ChatRightUserPhotoVisibility
    {
      get
      {
        return this.ChatLeftUserPhotoVisibility;
      }
    }

    public SearchConversationHeader(Message message, List<User> associatedUsers)
      : base(message, associatedUsers, 0)
    {
    }

    private string GetChatUserPhoto(int userIndex)
    {
      List<User> associatedUsers = this._associatedUsers;
      if (associatedUsers.Count <= userIndex)
        return "";
      int num = 0;
      foreach (User user in associatedUsers)
      {
        if (user.id == AppGlobalStateManager.Current.LoggedInUserId && associatedUsers.Count > userIndex + 2)
          ++userIndex;
        if (userIndex == num)
          return user.photo_max;
        ++num;
      }
      return "";
    }
  }
}
