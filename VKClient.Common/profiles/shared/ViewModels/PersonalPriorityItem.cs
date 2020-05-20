using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class PersonalPriorityItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public const int MaxValue = 8;

    public PersonalPriorityItem(int id)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_PersonalPriority;
      this.Data = PersonalPriorityItem.GetById(id);
    }

    private static string GetById(int id)
    {
      switch (id)
      {
        case 1:
          return CommonResources.PersonalPriority_FamilyAndChildren;
        case 2:
          return CommonResources.PersonalPriority_CareerAndMoney;
        case 3:
          return CommonResources.PersonalPriority_EntertainmentAndLeisure;
        case 4:
          return CommonResources.PersonalPriority_ScienceAndResearch;
        case 5:
          return CommonResources.PersonalPriority_ImprovingTheWorld;
        case 6:
          return CommonResources.PersonalPriority_PersonalDevelopment;
        case 7:
          return CommonResources.PersonalPriority_BeautyAndArt;
        case 8:
          return CommonResources.PersonalPriority_FameAndInfluence;
        default:
          return "";
      }
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
