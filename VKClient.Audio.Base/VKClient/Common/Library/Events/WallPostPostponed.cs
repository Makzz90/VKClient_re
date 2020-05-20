namespace VKClient.Common.Library.Events
{
  public class WallPostPostponed
  {
    public long OwnerId { get; set; }

    public WallPostPostponed(long ownerId)
    {
      this.OwnerId = ownerId;
    }
  }
}
