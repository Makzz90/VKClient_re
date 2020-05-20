namespace VKClient.Audio.Base.DataObjects
{
  public class VotesPack
  {
    public int ProductId { get; set; }

    public string MerchantProductId { get; set; }

    public AccountPaymentType PaymentType { get; set; }

    public string Title { get; set; }

    public string PriceStr { get; set; }

    public int VotesCount { get; set; }
  }
}
