using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public class PrivacySettingsInfo
  {
    public List<PrivacySetting> settings { get; set; }

    public List<PrivacySection> sections { get; set; }
  }
}
