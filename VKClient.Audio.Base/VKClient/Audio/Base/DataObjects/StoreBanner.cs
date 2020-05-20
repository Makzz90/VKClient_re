namespace VKClient.Audio.Base.DataObjects
{
  public class StoreBanner
  {
    public string type { get; set; }

    public StockItem stock_item { get; set; }

    public StoreSection section { get; set; }

    public string photo_480 { get; set; }

    public string photo_640 { get; set; }

    public string photo_960 { get; set; }

    public string photo_1280 { get; set; }
  }
}
