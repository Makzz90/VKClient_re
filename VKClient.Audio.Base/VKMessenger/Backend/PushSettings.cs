using System;
using VKClient.Audio.Base.Utils;

namespace VKMessenger.Backend
{
  public class PushSettings
  {
    public int disabled_until { get; set; }

    public int sound { get; set; }

    public bool AreDisabledNow
    {
      get
      {
        if (this.disabled_until == -1)
          return true;
        if (this.disabled_until > 0)
          return ExtensionsBase.DateTimeToUnixTimestamp(DateTime.UtcNow, true) < this.disabled_until;
        return false;
      }
    }
  }
}
