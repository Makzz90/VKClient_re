using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class WorldViewItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public WorldViewItem(string worldView)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_WorldView;
      this.Data = worldView;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
