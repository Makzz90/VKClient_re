namespace VKClient.Common.Backend.DataObjects
{
  public class NewsFeedList
  {
    private string _title = "";

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
  }
}
