namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class CityItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public CityItem(string cityName)
      : base("/Resources/Profile/ProfileHome.png", cityName)
    {
      this.Data = (object) cityName;
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
