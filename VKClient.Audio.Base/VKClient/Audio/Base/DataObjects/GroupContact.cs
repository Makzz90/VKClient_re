using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GroupContact
  {
    private string _phone;
    private string _email;
    private string _desc;

    public long user_id { get; set; }

    public string phone
    {
      get
      {
        return (this._phone ?? "").ForUI();
      }
      set
      {
        this._phone = value;
      }
    }

    public string email
    {
      get
      {
        return (this._email ?? "").ForUI();
      }
      set
      {
        this._email = value;
      }
    }

    public string desc
    {
      get
      {
        return (this._desc ?? "").ForUI();
      }
      set
      {
        this._desc = value;
      }
    }
  }
}
