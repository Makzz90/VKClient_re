namespace VKClient.Common.Backend.DataObjects
{
  public class City
  {
    private string _title = "";
    private string _area = "";
    private string _region = "";

    public long id { get; set; }

    public string name
    {
      get
      {
        return this.title;
      }
      set
      {
        this.title = value;
      }
    }

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

    public string area
    {
      get
      {
        return this._area;
      }
      set
      {
        this._area = (value ?? "").ForUI();
      }
    }

    public string region
    {
      get
      {
        return this._region;
      }
      set
      {
        this._region = (value ?? "").ForUI();
      }
    }

    public override string ToString()
    {
      return this.name;
    }
  }
}
