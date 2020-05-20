using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class UserOrGroupSource
  {
    public long id { get; set; }

    public string name { get; set; }

    public string type { get; set; }

    public int is_member { get; set; }

    public string activity { get; set; }

    public int is_closed { get; set; }

    public string photo_200 { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string photo_max { get; set; }

    public int verified { get; set; }

    public int friend_status { get; set; }

    public Occupation occupation { get; set; }

    public City city { get; set; }

    public Country country { get; set; }

    public User GetUser()
    {
      if (!(this.type == "profile"))
        return  null;
      return new User()
      {
        id = this.id,
        first_name = this.first_name,
        last_name = this.last_name,
        photo_max = this.photo_max,
        verified = this.verified,
        friend_status = this.friend_status,
        occupation = this.occupation,
        city = this.city,
        country = this.country
      };
    }

    public Group GetGroup()
    {
      if (!(this.type != "profile"))
        return  null;
      return new Group()
      {
        id = this.id,
        name = this.name,
        is_member = this.is_member,
        activity = this.activity,
        is_closed = this.is_closed,
        photo_200 = this.photo_200,
        verified = this.verified
      };
    }
  }
}
