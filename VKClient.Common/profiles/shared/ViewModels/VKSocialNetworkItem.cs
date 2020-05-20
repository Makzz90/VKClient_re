using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class VKSocialNetworkItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private readonly string _webLink;
    private readonly string _domain;

    public VKSocialNetworkItem(IProfileData profileData)
      : base("/Resources/Profile/Contacts/ProfileVK.png")
    {
      UserData userData = profileData as UserData;
      if (userData != null)
      {
        User user = userData.user;
        this._domain = string.IsNullOrEmpty(user.domain) ? string.Format("id{0}", user.id) : user.domain;
      }
      else
      {
        GroupData groupData = profileData as GroupData;
        if (groupData != null)
        {
          Group group = groupData.group;
          this._domain = string.IsNullOrEmpty(group.screen_name) ? string.Format("club{0}", group.id) : group.screen_name;
        }
      }
      this.Data = this._domain;
      if (string.IsNullOrEmpty(this._domain))
        return;
      this._webLink = "vk.com/" + this._domain;
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
