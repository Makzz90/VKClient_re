using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class PoliticalViewsItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public const int MaxValue = 9;

    public PoliticalViewsItem(int id)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_PoliticalViews;
      this.Data = (object) PoliticalViewsItem.GetById(id);
    }

    private static string GetById(int id)
    {
      switch (id)
      {
        case 1:
          return CommonResources.PoliticalViews_Communist;
        case 2:
          return CommonResources.PoliticalViews_Socialist;
        case 3:
          return CommonResources.PoliticalViews_Moderate;
        case 4:
          return CommonResources.PoliticalViews_Liberal;
        case 5:
          return CommonResources.PoliticalViews_Conservative;
        case 6:
          return CommonResources.PoliticalViews_Monarchist;
        case 7:
          return CommonResources.PoliticalViews_Ultraconservative;
        case 8:
          return CommonResources.PoliticalViews_Apathetic;
        case 9:
          return CommonResources.PoliticalViews_Libertarian;
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
