using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Groups.ViewModels
{
  public sealed class GroupProfileHeaderViewModel : ProfileHeaderViewModelBase
  {
    private Visibility _verifiedVisibility = Visibility.Collapsed;
    private readonly bool _hasAvatar;

    public override bool HasAvatar
    {
      get
      {
        return this._hasAvatar;
      }
    }

    public override string ProfileImageUrl { get; protected set; }

    public override string Name { get; protected set; }

    public string Description { get; private set; }

    public Visibility VerifiedVisibility
    {
      get
      {
        return this._verifiedVisibility;
      }
      private set
      {
        this._verifiedVisibility = value;
      }
    }

    public GroupProfileHeaderViewModel(GroupData groupData)
    {
      if (groupData == null || groupData.group == null)
        return;
      Group group = groupData.group;
      this.Name = group.name;
      this.Description = group.GroupType == GroupType.Event ? CommonResources.Event : group.activity;
      this.ProfileImageUrl = group.photo_200;
      this.VerifiedVisibility = groupData.IsVerified ? Visibility.Visible : Visibility.Collapsed;
      this._hasAvatar = ProfileHeaderViewModelBase.IsValidAvatarUrl(group.photo_200);
    }

    public GroupProfileHeaderViewModel(string name)
    {
      this.Name = name;
    }
  }
}
