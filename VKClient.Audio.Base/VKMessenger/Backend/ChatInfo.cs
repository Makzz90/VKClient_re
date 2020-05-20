using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public class ChatInfo
  {
    public Chat chat { get; set; }

    public List<ChatUser> chat_participants { get; set; }

    public List<User> invited_by_users { get; set; }
  }
}
