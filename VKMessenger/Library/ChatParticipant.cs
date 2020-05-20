using System.Windows;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class ChatParticipant
  {
    private ChatUser _chatUser;

    public bool IsUser
    {
      get
      {
        return this._chatUser.type == "profile";
      }
    }

    public string ImageUrl
    {
      get
      {
        if (!this.IsUser)
          return this._chatUser.photo_200;
        return this._chatUser.photo_max;
      }
    }

    public string FullName
    {
      get
      {
        if (!this.IsUser)
          return this._chatUser.name;
        return this._chatUser.Name;
      }
    }

    public Visibility IsOnline
    {
      get
      {
        return this._chatUser.online != 1 || this._chatUser.online_mobile != 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility IsOnlineMobile
    {
      get
      {
        return this._chatUser.online_mobile != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool CanRemove
    {
      get
      {
        return false;
      }
    }

    public long InvitedBy
    {
      get
      {
        return (long) this._chatUser.invited_by;
      }
    }

    public ChatUser ChatUser
    {
      get
      {
        return this._chatUser;
      }
    }

    public ChatParticipant(ChatUser chatUser)
    {
      this._chatUser = chatUser;
    }
  }
}
