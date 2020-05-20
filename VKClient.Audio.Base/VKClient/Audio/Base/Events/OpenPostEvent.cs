using System.Collections.Generic;

namespace VKClient.Audio.Base.Events
{
  public class OpenPostEvent : StatEventBase
  {
    public string PostId { get; set; }

    public List<string> CopyPostIds { get; set; }

    public OpenPostEvent()
    {
      this.CopyPostIds = new List<string>();
    }
  }
}
