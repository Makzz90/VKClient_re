using System;
using Windows.System;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
    public class SkypeSocialNetworkItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
    {
        public SkypeSocialNetworkItem(string skypeName)
            : base("/Resources/Profile/Contacts/ProfileSkype.png", skypeName)
        {
            this.NavigationAction = (Action)(async () => await Launcher.LaunchUriAsync(new Uri(string.Format("skype:{0}?call", skypeName))));
        }

        public string GetData()
        {
            return (this.Data ?? "").ToString();
        }
    }
}
