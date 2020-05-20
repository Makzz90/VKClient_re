using System;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class SiteItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private readonly string _site;

    public SiteItem(string site)
      : base("/Resources/Profile/ProfileLink.png", site)
    {
      this._site = site;
      this.Data = this._site.StartsWith("http://") || this._site.StartsWith("https://") ? (object) this._site : (object) ("http://" + this._site);
      this.NavigationAction = (Action) (() => Navigator.Current.NavigateToWebUri(this._site, true, false));
    }

    public string GetData()
    {
      if (string.IsNullOrEmpty(this._site))
        return "";
      return this._site;
    }
  }
}
