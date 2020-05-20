using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileAppViewModel : ViewModelBase
  {
    private readonly long _ownerId;
    private readonly AppButton _appButton;

    public string Title
    {
      get
      {
        return this._appButton.title;
      }
    }

    protected ProfileAppViewModel(long groupId, AppButton appButton)
    {
      this._ownerId = -groupId;
      this._appButton = appButton;
    }

    public void NavigateToApp()
    {
      Navigator.Current.NavigateToProfileAppPage(this._appButton.app_id, this._ownerId, "");
    }
  }
}
