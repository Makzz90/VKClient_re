using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class InstagramSocialNetworkItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private readonly string _webLink;

    public InstagramSocialNetworkItem(string instagramName)
      : base("/Resources/Profile/Contacts/ProfileInstagram.png", instagramName)
    {
      this.Data = instagramName;
      if (string.IsNullOrEmpty(instagramName))
        return;
      this._webLink = "instagram.com/" + instagramName;
      this.NavigationAction = (Action) (() => Navigator.Current.NavigateToWebUri(this._webLink, true, false));
    }

    public string GetData()
    {
      if (string.IsNullOrEmpty(this._webLink))
        return "";
      return this._webLink;
    }
  }
}
