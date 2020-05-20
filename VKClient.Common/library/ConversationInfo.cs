using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public class ConversationInfo
  {
      public long UserOrChatId { get; private set; }

      public bool IsChat { get; private set; }

      public User User { get; private set; }

      public ChatExtended Chat { get; private set; }

    public string Title
    {
      get
      {
        if (this.User != null)
          return this.User.first_name;
        if (this.Chat == null)
          return "";
        return this.Chat.title;
      }
    }

    public ConversationInfo(bool isChat, long userOrChatId, User user, ChatExtended chat)
    {
      this.UserOrChatId = userOrChatId;
      this.IsChat = isChat;
      this.User = user;
      this.Chat = chat;
    }
  }
}
