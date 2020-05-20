namespace VKClient.Common.Library
{
  public class ContactsSyncStatusChangedEvent
  {
    public bool IsSyncing { get; set; }

    public int Current { get; set; }

    public int Total { get; set; }
  }
}
