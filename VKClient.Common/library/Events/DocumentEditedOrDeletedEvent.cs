namespace VKClient.Common.Library.Events
{
  public sealed class DocumentEditedOrDeletedEvent
  {
    public long OwnerId { get; set; }

    public long Id { get; set; }

    public string Title { get; set; }

    public string SizeString { get; set; }

    public bool IsEdited { get; set; }

    public int NewDocumentsCount { get; set; }

    public string NewFirstDocumentId { get; set; }
  }
}
