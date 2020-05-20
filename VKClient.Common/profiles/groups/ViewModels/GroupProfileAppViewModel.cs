using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Groups.ViewModels
{
  public class GroupProfileAppViewModel : ProfileAppViewModel
  {
    public GroupProfileAppViewModel(long groupId, AppButton appButton)
      : base(groupId, appButton)
    {
    }
  }
}
