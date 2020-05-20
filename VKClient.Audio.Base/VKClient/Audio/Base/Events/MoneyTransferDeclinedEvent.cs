using VKClient.Audio.Base.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public class MoneyTransferDeclinedEvent
  {
      public long TransferId { get; private set; }

      public long FromId { get; private set; }

      public long ToId { get; private set; }

    public MoneyTransferDeclinedEvent(MoneyTransfer transfer)
    {
      this.TransferId = transfer.id;
      this.FromId = transfer.from_id;
      this.ToId = transfer.to_id;
    }
  }
}
