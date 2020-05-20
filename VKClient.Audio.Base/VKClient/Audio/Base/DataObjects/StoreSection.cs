namespace VKClient.Audio.Base.DataObjects
{
  public class StoreSection
  {
    public string name { get; set; }

    public string title { get; set; }

    public VKList<StockItem> stickers { get; set; }
  }
}
