using System.Collections.Generic;

namespace VKClient.Audio.Base.Events
{
  public class AdPixelEvent : StatEventBase
  {
    public List<string> UrlToLoad { get; set; }

    public AdPixelEvent()
    {
      this.UrlToLoad = new List<string>();
    }
  }
}
