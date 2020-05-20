namespace VKClient.Common.Backend.DataObjects
{
  public class AppButton
  {
    private string _title;

    public long app_id { get; set; }

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
