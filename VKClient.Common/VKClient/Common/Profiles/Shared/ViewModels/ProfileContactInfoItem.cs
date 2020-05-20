namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public abstract class ProfileContactInfoItem : ProfileInfoItem
  {
    protected const string ICONS_ENDPOINT = "/Resources/Profile/Contacts/";

    public string Icon { get; private set; }

    protected ProfileContactInfoItem(string icon, string data)
      : base(ProfileInfoItemType.Contact)
    {
      this.Icon = icon;
      this.Data = (object) data;
    }

    protected ProfileContactInfoItem(string icon)
      : this(icon, "")
    {
      this.Icon = icon;
    }
  }
}
