using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library.Events
{
  public class DocumentUploadedEvent
  {
    public long OwnerId { get; set; }

    public Doc Document { get; set; }
  }
}
