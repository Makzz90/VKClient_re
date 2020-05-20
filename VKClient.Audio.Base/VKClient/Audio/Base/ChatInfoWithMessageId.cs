using VKClient.Audio.Base.DataObjects;

namespace VKClient.Audio.Base
{
  public class ChatInfoWithMessageId
  {
    public long message_id { get; set; }

    public Chat chat { get; set; }
  }
}
