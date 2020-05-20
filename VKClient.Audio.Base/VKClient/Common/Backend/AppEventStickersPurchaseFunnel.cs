namespace VKClient.Common.Backend
{
  public class AppEventStickersPurchaseFunnel : AppEventBase
  {
    public override string e
    {
      get
      {
        return "stickers_purchase_funnel";
      }
    }

    public string source { get; set; }

    public string action { get; set; }
  }
}
