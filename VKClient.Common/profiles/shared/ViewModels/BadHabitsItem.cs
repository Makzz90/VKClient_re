using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class BadHabitsItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public const int MaxValue = 5;

    public BadHabitsItem(string title, int id)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = title;
      this.Data = BadHabitsItem.GetById(id);
    }

    private static string GetById(int id)
    {
      switch (id)
      {
        case 1:
          return CommonResources.BadHabitsViews_VeryNegative;
        case 2:
          return CommonResources.BadHabitsViews_Negative;
        case 3:
          return CommonResources.BadHabitsViews_Compromisable;
        case 4:
          return CommonResources.BadHabitsViews_Neutral;
        case 5:
          return CommonResources.BadHabitsViews_Positive;
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
