using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class LanguagesItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public LanguagesItem(UserData profileData)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_Languages;
      User user = profileData.user;
      if (user.personal == null || user.personal.langs.IsNullOrEmpty())
        return;
      this.Data = (object) user.personal.langs.GetCommaSeparated(", ");
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
