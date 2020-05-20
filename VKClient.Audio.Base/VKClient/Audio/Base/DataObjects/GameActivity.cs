namespace VKClient.Audio.Base.DataObjects
{
  public class GameActivity
  {
    public string type { get; set; }

    public long app_id { get; set; }

    public long user_id { get; set; }

    public int level { get; set; }

    public string activity { get; set; }

    public int score { get; set; }

    public int date { get; set; }

    public string text { get; set; }
  }
}
