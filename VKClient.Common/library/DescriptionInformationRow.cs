using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class DescriptionInformationRow : InformationRow
  {
    public DescriptionInformationRow(string desc)
    {
      this.CanNavigate = false;
      this.Title = CommonResources.Description;
      this.Subtitle = desc;
    }
  }
}
