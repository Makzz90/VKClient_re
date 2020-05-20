namespace VKClient.Audio.Base.DataObjects
{
  public class MarketFeedResponse
  {
    public VKList<MarketAlbum> albums { get; set; }

    public VKList<Product> products { get; set; }

    public long priceFrom { get; set; }

    public long priceTo { get; set; }

    public int currencyId { get; set; }

    public string currencyName { get; set; }
  }
}
