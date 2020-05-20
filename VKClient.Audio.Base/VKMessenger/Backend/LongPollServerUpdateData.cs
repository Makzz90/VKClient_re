using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public class LongPollServerUpdateData
  {
    public LongPollServerUpdateType UpdateType { get; set; }

    public bool IsHistoricData { get; set; }

    public long user_id { get; set; }

    public long message_id { get; set; }

    public long chat_id { get; set; }

    public long timestamp { get; set; }

    public string text { get; set; }

    public bool @out { get; set; }

    public Message message { get; set; }

    public User user { get; set; }

    public int Platform { get; set; }

    public int Counter { get; set; }

    public Chat chat { get; set; }

    public bool isChat { get; set; }

    public bool hasAttachOrForward { get; set; }

    public override string ToString()
    {
      return string.Format("UpdateType = {0}, user_id = {1}, message_id= {2}, chat_id={3}", this.UpdateType, this.user_id, this.message_id, this.chat_id);
    }
  }
}
