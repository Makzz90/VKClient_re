namespace VKClient.Common.Backend.DataObjects
{
  public class Topic
  {
    private string _title = "";
    private string _first_comment = "";
    private string _last_comment = "";

    public long tid
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

    public int created { get; set; }

    public long created_by { get; set; }

    public long owner_id { get; set; }

    public int updated { get; set; }

    public long updated_by { get; set; }

    public int is_closed { get; set; }

    public int is_fixed { get; set; }

    public int comments { get; set; }

    public string first_comment
    {
      get
      {
        return this._first_comment;
      }
      set
      {
        this._first_comment = (value ?? "").ForUI();
      }
    }

    public string last_comment
    {
      get
      {
        return this._last_comment;
      }
      set
      {
        this._last_comment = (value ?? "").ForUI();
      }
    }

    public override string ToString()
    {
      return string.Format("topic{0}_{1}", this.owner_id, this.id);
    }
  }
}
