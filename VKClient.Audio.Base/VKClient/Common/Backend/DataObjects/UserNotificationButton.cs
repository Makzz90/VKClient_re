namespace VKClient.Common.Backend.DataObjects
{
  public class UserNotificationButton
  {
    private string _title;

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

    public UserNotificationButtonAction action { get; set; }

    public UserNotificationButtonStyle style { get; set; }
  }
}
