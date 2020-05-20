namespace VKClient.Common.Backend
{
  public class MoneyTransferAcceptedResponse
  {
    public bool IsSucceeded { get; set; }

    public long TransferId { get; set; }

    public long FromId { get; set; }

    public long ToId { get; set; }
  }
}
