namespace VKClient.Common.Backend.DataObjects
{
  public class PhotoVideoTag
  {
    private string _taggedName = "";

    public long uid
    {
      get
      {
        return this.user_id;
      }
      set
      {
        this.user_id = value;
      }
    }

    public long user_id { get; set; }

    public long tag_id { get; set; }

    public long placer_id { get; set; }

    public string tagged_name
    {
      get
      {
        return this._taggedName;
      }
      set
      {
        this._taggedName = (value ?? "").ForUI();
      }
    }

    public int date { get; set; }

    public double x { get; set; }

    public double y { get; set; }

    public double x2 { get; set; }

    public double y2 { get; set; }

    public int viewed { get; set; }
  }
}
