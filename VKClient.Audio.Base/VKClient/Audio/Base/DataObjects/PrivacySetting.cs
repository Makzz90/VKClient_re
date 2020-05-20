using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class PrivacySetting
  {
    public string key { get; set; }

    public string title { get; set; }

    public List<string> value { get; set; }

    public List<string> supported_values { get; set; }

    public string section { get; set; }
  }
}
