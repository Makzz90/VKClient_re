namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class CityItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public CityItem(string cityName)
      : base("/Resources/Profile/ProfileHome.png", cityName)
    {
      this.Data = cityName;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
