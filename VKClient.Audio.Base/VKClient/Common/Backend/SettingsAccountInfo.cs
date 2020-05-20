using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class SettingsAccountInfo
  {
    public AccountInfo Account { get; set; }

    public ProfilesAndGroupsIds NewsBanned { get; set; }

    public ProfileInfo ProfileInfo { get; set; }
  }
}
