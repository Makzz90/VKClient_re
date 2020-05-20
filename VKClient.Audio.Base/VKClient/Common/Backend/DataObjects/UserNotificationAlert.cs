namespace VKClient.Common.Backend.DataObjects
{
  public class UserNotificationAlert
  {
    private string _title;
    private string _message;

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

    public string message
    {
      get
      {
        return this._message;
      }
      set
      {
        this._message = (value ?? "").ForUI();
      }
    }

    public UserNotificationButton button { get; set; }
  }
}
