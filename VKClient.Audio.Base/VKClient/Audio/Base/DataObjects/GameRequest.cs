using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class GameRequest
  {
    public long id { get; set; }

    public string type { get; set; }

    public long app_id { get; set; }

    public string text { get; set; }

    public long date { get; set; }

    public long from_id { get; set; }

    public List<GameRequestFrom> from { get; set; }

    public string key { get; set; }

    public string name { get; set; }

    public int unread { get; set; }
  }
}
