namespace VKClient.Common.Library.Events
{
  public sealed class DocumentDeletedEvent
  {
    public long OwnerId { get; set; }

    public long Id { get; set; }
  }
}
