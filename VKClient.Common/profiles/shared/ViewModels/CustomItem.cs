using System;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class CustomItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public CustomItem(string title, string description, Action navigationAction = null, ProfileInfoItemType type = ProfileInfoItemType.RichText)
      : base(type)
    {
      this.Title = title;
      this.Data = description;
      if (navigationAction == null)
        return;
      this.NavigationAction = navigationAction;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
