namespace VKClient.Common.Backend
{
  public class UserOrGroup
  {
    public string type { get; set; }

    public long id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string name { get; set; }

    public string screen_name { get; set; }

    public int is_closed { get; set; }

    public int is_admin { get; set; }

    public int admin_level { get; set; }

    public int is_member { get; set; }

    public string photo_50 { get; set; }

    public string photo_100 { get; set; }
  }
}
