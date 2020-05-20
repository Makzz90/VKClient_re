using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ImportantInOthersItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public const int MaxValue = 6;

    public ImportantInOthersItem(int id)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_ImportantInOthers;
      this.Data = (object) ImportantInOthersItem.GetById(id);
    }

    private static string GetById(int id)
    {
      switch (id)
      {
        case 1:
          return CommonResources.ImportantInOthers_IntellectAndCreativity;
        case 2:
          return CommonResources.ImportantInOthers_KindnessAndHonesty;
        case 3:
          return CommonResources.ImportantInOthers_HealthAndBeauty;
        case 4:
          return CommonResources.ImportantInOthers_WealthAndPower;
        case 5:
          return CommonResources.ImportantInOthers_CourageAndPersistance;
        case 6:
          return CommonResources.ImportantInOthers_HumorAndLoveForLife;
        default:
          return "";
      }
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
