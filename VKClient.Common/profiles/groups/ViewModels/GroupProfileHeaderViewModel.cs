using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Groups.ViewModels
{
    public sealed class GroupProfileHeaderViewModel : ProfileHeaderViewModelBase
    {
        public Visibility VerifiedVisibility { get; private set; }

        public Visibility CoverImageVisibility { get; private set; }

        public Visibility DataVisibility { get; private set; }

        public override string ProfileImageUrl { get; protected set; }

        public override string Name { get; protected set; }

        public string Description { get; private set; }

        public string CoverImageUrl { get; private set; }

        public GroupProfileHeaderViewModel(GroupData groupData)
        {
            this.VerifiedVisibility = Visibility.Collapsed;
            this.CoverImageVisibility = Visibility.Collapsed;
            this.DataVisibility = Visibility.Collapsed;

            Group group = groupData != null ? groupData.group : null;
            if (group == null)
                return;
            this.Name = group.name;
            this.Description = group.GroupType == GroupType.Event ? CommonResources.Event : group.activity;
            this.ProfileImageUrl = group.photo_200;
            this.VerifiedVisibility = groupData.IsVerified.ToVisiblity();
            CoverImage coverImage = groupData.CoverImage;
            if (coverImage != null)
            {
                this.CoverImageUrl = coverImage.url;
                if (!string.IsNullOrEmpty(this.CoverImageUrl))
                    this.CoverImageVisibility = Visibility.Visible;
            }
            this.HasAvatar = ProfileHeaderViewModelBase.IsValidAvatarUrl(group.photo_200);
            this.DataVisibility = Visibility.Visible;
        }

        public GroupProfileHeaderViewModel(string name)
        {
            this.VerifiedVisibility = Visibility.Collapsed;
            this.CoverImageVisibility = Visibility.Collapsed;
            this.DataVisibility = Visibility.Collapsed;

            this.Name = name;
        }
    }
}
