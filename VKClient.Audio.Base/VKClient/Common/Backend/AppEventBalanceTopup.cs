namespace VKClient.Common.Backend
{
  public class AppEventBalanceTopup : AppEventBase
  {
    public override string e
    {
      get
      {
        return "balance_topup";
      }
    }

    public string source { get; set; }

    public string action { get; set; }
  }
}
