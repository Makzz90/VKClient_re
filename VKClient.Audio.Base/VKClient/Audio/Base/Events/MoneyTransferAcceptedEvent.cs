namespace VKClient.Audio.Base.Events
{
  public class MoneyTransferAcceptedEvent
  {
      public long TransferId { get; private set; }

      public long FromId { get; private set; }

      public long ToId { get; private set; }

    public MoneyTransferAcceptedEvent(long transferId, long fromId, long toId)
    {
      this.TransferId = transferId;
      this.FromId = fromId;
      this.ToId = toId;
    }
  }
}
