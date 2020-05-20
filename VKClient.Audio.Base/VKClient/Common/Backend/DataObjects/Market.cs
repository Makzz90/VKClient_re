namespace VKClient.Common.Backend.DataObjects
{
  public class Market
  {
    public int enabled { get; set; }

    public long price_min { get; set; }

    public long price_max { get; set; }

    public int main_album_id { get; set; }

    public Wiki wiki { get; set; }

    public long contact_id { get; set; }

    public Currency currency { get; set; }
  }
}
