namespace VKClient.Common.Backend.DataObjects
{
  public class Military
  {
    public string unit { get; set; }

    public long unit_id { get; set; }

    public long country_id { get; set; }

    public int from { get; set; }

    public int until { get; set; }
  }
}
