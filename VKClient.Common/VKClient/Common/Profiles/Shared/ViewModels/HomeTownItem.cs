using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class HomeTownItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public HomeTownItem(string homeTown)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_Hometown;
      this.Data = (object) homeTown;
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
