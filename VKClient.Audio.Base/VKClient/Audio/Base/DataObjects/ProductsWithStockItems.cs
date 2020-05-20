namespace VKClient.Audio.Base.DataObjects
{
  public class ProductsWithStockItems
  {
    public VKList<StoreProduct> products { get; set; }

    public VKList<StockItem> stockItems { get; set; }
  }
}
