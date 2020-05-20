namespace VKClient.Common.Backend.DataObjects
{
  public class NewsActivityComment
  {
    private string _text;

    public long id { get; set; }

    public long from_id { get; set; }

    public long date { get; set; }

    public string text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = (value ?? "").ForUI();
      }
    }
  }
}
