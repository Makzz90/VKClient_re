using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class TwitterSocialNetworkItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private readonly string _webLink;

    public TwitterSocialNetworkItem(string twitterName)
      : base("/Resources/Profile/Contacts/ProfileTwitter.png", twitterName)
    {
      this.Data = twitterName;
      if (string.IsNullOrEmpty(twitterName))
        return;
      this._webLink = "twitter.com/" + twitterName;
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
