using System;
using Windows.System;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
    public class FacebookSocialNetworkItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
    {
        private readonly string _facebookId;

        public FacebookSocialNetworkItem(string facebookId, string facebookName) : base("/Resources/Profile/Contacts/ProfileFacebook.png", facebookName)
        {
            this.Data = (object)facebookName;
            this._facebookId = facebookId;
            this.NavigationAction = (Action)(() =>
            {
                if (string.IsNullOrEmpty(this._facebookId))
                    return;
                Launcher.LaunchUriAsync(new Uri(string.Format("fb:profile?id={0}", (object)this._facebookId)));
            });
        }

        public string GetData()
        {
            if (!string.IsNullOrEmpty(this._facebookId))
                return "facebook.com/profile.php?id=" + this._facebookId;
            return "";
        }
    }
}
