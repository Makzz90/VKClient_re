namespace VKClient.Common.Backend.DataObjects
{
  public class Career
  {
    public long group_id { get; set; }

    public string company { get; set; }

    public long city_id { get; set; }

    public string city_name { get; set; }

    public long from { get; set; }

    public long until { get; set; }

    public string position { get; set; }
  }
}
