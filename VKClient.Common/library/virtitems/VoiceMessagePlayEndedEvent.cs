using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.VirtItems
{
  public class VoiceMessagePlayEndedEvent
  {
      public Doc Doc { get; private set; }

    public VoiceMessagePlayEndedEvent(Doc doc)
    {
      this.Doc = doc;
    }
  }
}
