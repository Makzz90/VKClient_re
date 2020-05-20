namespace VKClient.Audio.Base.DataObjects
{
  public class BalanceDataResponse
  {
    public VKList<StockItem> stockItems { get; set; }

    public int balance { get; set; }
  }
}
