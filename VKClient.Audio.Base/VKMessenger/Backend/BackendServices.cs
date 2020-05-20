using VKClient.Audio.Base;

namespace VKMessenger.Backend
{
  public class BackendServices
  {
    private static MessagesService _messagesService = new MessagesService();
    private static ILongPollServerService _longPollServerService = (ILongPollServerService) new VKMessenger.Backend.LongPollServerService();
    private static IChatService _chatService = (IChatService) new VKMessenger.Backend.ChatService();

    public static MessagesService MessagesService
    {
      get
      {
        return BackendServices._messagesService;
      }
    }

    public static ILongPollServerService LongPollServerService
    {
      get
      {
        return BackendServices._longPollServerService;
      }
    }

    public static IChatService ChatService
    {
      get
      {
        return BackendServices._chatService;
      }
    }
  }
}
