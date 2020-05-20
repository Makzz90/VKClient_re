using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public class ChatUser
  {
    private string _photo_max;

    public long uid
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

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string first_name_acc { get; set; }

    public string last_name_acc { get; set; }

    public int online { get; set; }

    public int online_mobile { get; set; }

    public string photo_rec { get; set; }

    public string photo_max
    {
      get
      {
        if (this.id < -2000000000L)
          return "/VKClient.Common;component/Resources/EmailUser.png";
        return this._photo_max ?? this.photo_200;
      }
      set
      {
        this._photo_max = value;
      }
    }

    public string photo_200 { get; set; }

    public string Name
    {
      get
      {
        return "" + this.first_name + " " + this.last_name;
      }
    }

    public string type { get; set; }

    public long invited_by { get; set; }

    public int sex { get; set; }

    public ChatUser(User user, long invitedById)
    {
      this.type = "profile";
      this.invited_by = invitedById;
      this.id = user.id;
      this.first_name = user.first_name;
      this.last_name = user.last_name;
      this.first_name_acc = user.first_name_acc;
      this.last_name_acc = user.last_name_acc;
      this.online = user.online;
      this.online_mobile = user.online_mobile;
      this.photo_rec = user.photo_rec;
      this.photo_max = user.photo_max;
      this.photo_200 = user.photo_200;
      this.sex = user.sex;
    }

    public ChatUser()
    {
    }
  }
}
