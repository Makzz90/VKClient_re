namespace VKClient.Common.Library.VirtItems
{
  public class NewsFeedIgnoreItemData
  {
    public string Type { get; set; }

    public long OwnerId { get; set; }

    public long ItemId { get; set; }

    public NewsFeedIgnoreItemData(string type, long ownerId, long itemId)
    {
      this.Type = type;
      this.OwnerId = ownerId;
      this.ItemId = itemId;
    }
  }
}
