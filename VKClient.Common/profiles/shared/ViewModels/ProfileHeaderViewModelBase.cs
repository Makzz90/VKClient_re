using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public abstract class ProfileHeaderViewModelBase : ViewModelBase
  {
    private const string AVATAR_CAMERA = "vk.com/images/camera";
    private const string AVATAR_DEACTIVATED = "vk.com/images/deactivated";
    private const string AVATAR_COMMUNITY = "vk.com/images/community";

    public virtual bool HasAvatar { get; protected set; }

    public abstract string ProfileImageUrl { get; protected set; }

    public abstract string Name { get; protected set; }

    protected static bool IsValidAvatarUrl(string avatarUrl)
    {
      if (!string.IsNullOrWhiteSpace(avatarUrl) && !avatarUrl.Contains("vk.com/images/camera") && !avatarUrl.Contains("vk.com/images/deactivated"))
        return !avatarUrl.Contains("vk.com/images/community");
      return false;
    }
  }
}
