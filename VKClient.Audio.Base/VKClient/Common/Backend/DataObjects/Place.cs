namespace VKClient.Common.Backend.DataObjects
{
  public class Place
  {
    private string _title = "";
    private string _address = "";

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
      }
    }

    public string address
    {
      get
      {
        return this._address;
      }
      set
      {
        this._address = (value ?? "").ForUI();
      }
    }

    public double latitude { get; set; }

    public double longitude { get; set; }

    public string country { get; set; }

    public string city { get; set; }

    public string countryName { get; set; }

    public string cityName { get; set; }

    public long group_id { get; set; }

    public string group_photo { get; set; }

    public string country_name { get; set; }

    public string city_name { get; set; }

    public long country_id { get; set; }

    public long city_id { get; set; }
  }
}
