using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class UserNotificationNewsfeed
  {
    private string _title;
    private string _message;
    private string _usersDescription;
    private string _groupsDescription;

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

    public UserNotificationNewsfeedLayout layout { get; set; }

    public string users_description
    {
      get
      {
        return this._usersDescription;
      }
      set
      {
        this._usersDescription = (value ?? "").ForUI();
      }
    }

    public List<long> user_ids { get; set; }

    public string groups_description
    {
      get
      {
        return this._groupsDescription;
      }
      set
      {
        this._groupsDescription = (value ?? "").ForUI();
      }
    }

    public List<long> group_ids { get; set; }

    public List<UserNotificationImage> images { get; set; }
  }
}
