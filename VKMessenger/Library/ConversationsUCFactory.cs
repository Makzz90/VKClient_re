using VKClient.Audio.Base.DataObjects;
using VKMessenger.Views;

namespace VKMessenger.Library
{
  public class ConversationsUCFactory : IConversationsUCFactory
  {
    public ConversationsUCBase Create()
    {
      return (ConversationsUCBase) new ConversationsUC();
    }
  }
}
