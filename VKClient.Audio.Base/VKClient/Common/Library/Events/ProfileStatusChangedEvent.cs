namespace VKClient.Common.Library.Events
{
  public class ProfileStatusChangedEvent
  {
    public long Id { get; private set; }

    public bool IsGroup { get; private set; }

    public string Status { get; private set; }

    public ProfileStatusChangedEvent(long id, bool isGroup, string status)
    {
      this.Id = id;
      this.IsGroup = isGroup;
      this.Status = status;
    }
  }
}
