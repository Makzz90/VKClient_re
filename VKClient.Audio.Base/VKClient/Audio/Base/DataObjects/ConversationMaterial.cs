using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class ConversationMaterial
  {
    public long message_id { get; set; }

    public Attachment attachment { get; set; }
  }
}
