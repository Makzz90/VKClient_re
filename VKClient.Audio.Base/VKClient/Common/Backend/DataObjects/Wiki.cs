namespace VKClient.Common.Backend.DataObjects
{
  public class Wiki
  {
    private string _title = "";

    public long pid
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public long id { get; set; }

    public long gid
    {
      get
      {
        return this.group_id;
      }
      set
      {
        this.group_id = value;
      }
    }

    public long group_id { get; set; }

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

    public string view_url { get; set; }
  }
}
